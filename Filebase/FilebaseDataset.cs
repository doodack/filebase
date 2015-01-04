using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Filebase
{
	public class FilebaseDataset<T> where T : class
	{
		private readonly Func<T, string> _idExtractor;

		private readonly string _backingFilePath;

		private FileInfo BackingFile
		{
			get
			{
				return new FileInfo(_backingFilePath);
			}
		}

		public FilebaseDataset(string name, FilebaseContext context, Func<T, string> idExtractor)
		{
			_idExtractor = idExtractor;
			_backingFilePath = Path.Combine(context.RootDirectory.FullName, name + ".json");
		}

		public async Task<IEnumerable<T>> GetAllAsync()
		{
			if (!BackingFile.Exists)
			{
				return Enumerable.Empty<T>();
			}

			var records = await ReadFileAsync();
			return records != null ? records.Values : Enumerable.Empty<T>();
		}

		public IEnumerable<T> GetAll()
		{
			if (!BackingFile.Exists)
			{
				return Enumerable.Empty<T>();
			}

			var records = ReadFile();
			return records != null ? records.Values : Enumerable.Empty<T>();
		}

		public async Task<T> GetByIdAsync(string id)
		{
			if (!BackingFile.Exists)
			{
				return null;
			}

			var records = await ReadFileAsync();

			T record;
			return records.TryGetValue(id, out record) ? record : null;
		}

		public T GetById(string id)
		{
			if (!BackingFile.Exists)
			{
				return null;
			}

			var records = ReadFile();

			T record;
			return records.TryGetValue(id, out record) ? record : null;
		}

		public async Task AddOrUpdateAsync(T record)
		{
			IDictionary<string, T> records = BackingFile.Exists ? await ReadFileAsync() : new Dictionary<string, T>();
			var id = _idExtractor(record);
			records[id] = record;
			await WriteFileAsync(records);
		}

		public void AddOrUpdate(T record)
		{
			IDictionary<string, T> records = BackingFile.Exists ? ReadFile() : new Dictionary<string, T>();
			var id = _idExtractor(record);
			records[id] = record;
			WriteFile(records);
		}

		public async Task DeleteAsync(string id)
		{
			if (!BackingFile.Exists)
			{
				return;
			}

			IDictionary<string, T> records = await ReadFileAsync();

			if (records == null)
			{
				return;
			}

			if (records.ContainsKey(id))
			{
				records.Remove(id);
			}

			if (records.Count == 0)
			{
				BackingFile.Delete();
			}
			else
			{
				await WriteFileAsync(records);
			}
		}

		public void Delete(string id)
		{
			if (!BackingFile.Exists)
			{
				return;
			}

			IDictionary<string, T> records = ReadFile();

			if (records == null)
			{
				return;
			}

			if (records.ContainsKey(id))
			{
				records.Remove(id);
			}

			if (records.Count == 0)
			{
				BackingFile.Delete();
			}
			else
			{
				WriteFile(records);
			}
		}

		private Dictionary<string, T> ReadFile()
		{
			string json;
			using (var reader = BackingFile.OpenText())
			{
				json = reader.ReadToEnd();
			}

			return JsonConvert.DeserializeObject<Dictionary<string, T>>(json);
		}

		private async Task<Dictionary<string, T>> ReadFileAsync()
		{
			string json;
			using (var reader = BackingFile.OpenText())
			{
				json = await reader.ReadToEndAsync();
			}

			return JsonConvert.DeserializeObject<Dictionary<string, T>>(json);
		}

		private void WriteFile(IDictionary<string, T> records)
		{
			var newJson = JsonConvert.SerializeObject(records, Formatting.Indented);
			using (var fs = BackingFile.Open(FileMode.Create))
			using (var writer = new StreamWriter(fs))
			{
				writer.Write(newJson);
			}
		}

		private async Task WriteFileAsync(IDictionary<string, T> records)
		{
			var newJson = JsonConvert.SerializeObject(records, Formatting.Indented);
			using (var fs = BackingFile.Open(FileMode.Create))
			using (var writer = new StreamWriter(fs))
			{
				await writer.WriteAsync(newJson);
			}
		}
	}
}
