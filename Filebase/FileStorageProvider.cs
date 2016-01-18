using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Filebase
{
	internal class FileStorageProvider<T> : IPersistentStorageProvider<T>
	{
		private static readonly TimeSpan FileAccessTimeout = TimeSpan.FromSeconds(30);

		private static readonly TimeSpan LockedFileRetryInterval = TimeSpan.FromMilliseconds(50);

		private readonly FileInfo _backingFile;

		public FileStorageProvider(FileInfo backingFile)
		{
			_backingFile = backingFile;
		}

		private bool BackingFileExists
		{
			get
			{
				_backingFile.Refresh();
				return _backingFile.Exists;
			}
		}

		public IDictionary<string, T> ReadEntities()
		{
			if (!BackingFileExists)
			{
				return new Dictionary<string, T>();
			}

			string json;
			using (var reader = OpenStreamReader())
			{
				json = reader.ReadToEnd();
			}

			return DeserializeRecords(json);
		}

		public async Task<IDictionary<string, T>> ReadEntitiesAsync()
		{
			if (!BackingFileExists)
			{
				return new Dictionary<string, T>();
			}

			string json;
			using (var reader = await OpenStreamReaderAsync())
			{
				json = await reader.ReadToEndAsync();
			}

			return DeserializeRecords(json);
		}

		public void WriteEntities(IDictionary<string, T> records)
		{
			var newJson = PrepareDataToWrite(records);
			if (newJson == null)
			{
				return;
			}

			using (var writer = OpenStreamWriter())
			{
				writer.Write(newJson);
			}
		}

		public async Task WriteEntitiesAsync(IDictionary<string, T> records)
		{
			var newJson = PrepareDataToWrite(records);
			if (newJson == null)
			{
				return;
			}

			using (var writer = await OpenStreamWriterAsync())
			{
				await writer.WriteAsync(newJson);
			}
		}

		private string PrepareDataToWrite(IDictionary<string, T> records)
		{
			if (!records.Any() && BackingFileExists)
			{
				_backingFile.Delete();
				return null;
			}

			return JsonConvert.SerializeObject(records, Formatting.Indented);
		}

		private async Task<StreamReader> OpenStreamReaderAsync()
		{
			var fs = await OpenFileAsync(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
			return new StreamReader(fs);
		}

		private StreamReader OpenStreamReader()
		{
			var fs = OpenFile(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
			return new StreamReader(fs);
		}

		private StreamWriter OpenStreamWriter()
		{
			var fs = OpenFile(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
			return new StreamWriter(fs);
		}

		private async Task<StreamWriter> OpenStreamWriterAsync()
		{
			var fs = await OpenFileAsync(FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
			return new StreamWriter(fs);
		}

		private static IDictionary<string, T> DeserializeRecords(string json)
		{
			var records = JsonConvert.DeserializeObject<Dictionary<string, T>>(json);
			if (records == null)
			{
				return new Dictionary<string, T>();
			}

			return records;
		}

		private FileStream OpenFile(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			var firstTryTime = DateTime.Now;
			while (true)
			{
				try
				{
					var fs = _backingFile.Open(fileMode, fileAccess, fileShare);
					return fs;
				}
				catch (IOException ioex)
				{
					if (!IsFileLocked(ioex) || IsTimeoutExceeded(firstTryTime))
					{
						throw;
					}
				}

				Thread.Sleep(LockedFileRetryInterval);
			}
		}

		private async Task<FileStream> OpenFileAsync(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			var firstTryTime = DateTime.Now;
			while (true)
			{
				try
				{
					var fs = _backingFile.Open(fileMode, fileAccess, fileShare);
					return fs;
				}
				catch (IOException ioex)
				{
					if (!IsFileLocked(ioex) || IsTimeoutExceeded(firstTryTime))
					{
						throw;
					}
				}

				await Task.Delay(LockedFileRetryInterval);
			}
		}

		private static bool IsFileLocked(IOException exception)
		{
			const int FileLockedErrorCode = -2147024864;

			return exception.HResult == FileLockedErrorCode;
		}

		private static bool IsTimeoutExceeded(DateTime startTime)
		{
			return DateTime.Now - startTime >= FileAccessTimeout;
		}
	}
}
