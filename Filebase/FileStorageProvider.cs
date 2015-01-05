using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Filebase
{
	internal class FileStorageProvider<T> : IFileStorageProvider<T>
	{
		private readonly FileInfo _backingFile;

		public FileStorageProvider(FileInfo backingFile)
		{
			_backingFile = backingFile;
		}

		public IDictionary<string, T> ReadFile()
		{
			string json;
			
			if (!BackingFileExists)
			{
				return new Dictionary<string, T>();
			}

			using (var reader = _backingFile.OpenText())
			{
				json = reader.ReadToEnd();
			}
			
			var records = JsonConvert.DeserializeObject<Dictionary<string, T>>(json);
			if (records == null)
			{
				return new Dictionary<string, T>();
			}

			return records;
		}

		public async Task<IDictionary<string, T>> ReadFileAsync()
		{
			string json;
			if (!BackingFileExists)
			{
				return new Dictionary<string, T>();
			}

			using (var reader = _backingFile.OpenText())
			{
				json = await reader.ReadToEndAsync();
			}

			var records = JsonConvert.DeserializeObject<Dictionary<string, T>>(json);
			if (records == null)
			{
				return new Dictionary<string, T>();
			}

			return records;
		}

		public void WriteFile(IDictionary<string, T> records)
		{
			if (!records.Any() && BackingFileExists)
			{
				_backingFile.Delete();
				return;
			}

			var newJson = JsonConvert.SerializeObject(records, Formatting.Indented);
			using (var fs = _backingFile.Open(FileMode.Create))
			using (var writer = new StreamWriter(fs))
			{
				writer.Write(newJson);
			}
		}

		public async Task WriteFileAsync(IDictionary<string, T> records)
		{
			if (!records.Any() && BackingFileExists)
			{
				_backingFile.Delete();
				return;
			}

			var newJson = JsonConvert.SerializeObject(records, Formatting.Indented);
			using (var fs = _backingFile.Open(FileMode.Create))
			using (var writer = new StreamWriter(fs))
			{
				await writer.WriteAsync(newJson);
			}
		}

		private bool BackingFileExists 
		{ 
			get
			{
				_backingFile.Refresh();
				return _backingFile.Exists;
			}
		}
	}
}
