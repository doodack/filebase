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
			this._idExtractor = idExtractor;


			this._backingFilePath = Path.Combine(context.RootDirectory.FullName, name + ".json");
		}

		public async Task<IEnumerable<T>> GetAll()
		{
			if (!this.BackingFile.Exists)
			{
				return Enumerable.Empty<T>();
			}

			using (var reader = this.BackingFile.OpenText())
			{
				var json = await reader.ReadToEndAsync();
				var records = JsonConvert.DeserializeObject<Dictionary<string, T>>(json);

				return records != null ? records.Values : Enumerable.Empty<T>();
			}
		}

		public async Task<T> GetById(string id)
		{
			if (!this.BackingFile.Exists)
			{
				return null;
			}

			using (var reader = this.BackingFile.OpenText())
			{
				var json = await reader.ReadToEndAsync();
				var records = JsonConvert.DeserializeObject<Dictionary<string, T>>(json);

				T record;
				return records.TryGetValue(id, out record) ? record : null;
			}
		}

		public async Task AddOrUpdate(T record)
		{
			IDictionary<string, T> records;
			if (this.BackingFile.Exists)
			{
				using (var reader = this.BackingFile.OpenText())
				{
					var json = await reader.ReadToEndAsync();
					records = JsonConvert.DeserializeObject<Dictionary<string, T>>(json) ?? new Dictionary<string, T>();
				}
			}
			else
			{
				records = new Dictionary<string, T>();
			}

			var id = _idExtractor(record);
			if (records.ContainsKey(id))
			{
				records[id] = record;
			}
			else
			{
				records.Add(id, record);
			}

			var newJson = JsonConvert.SerializeObject(records, Formatting.Indented);

			using (var fs = this.BackingFile.Open(FileMode.Create))
			using (var writer = new StreamWriter(fs))
			{
				await writer.WriteAsync(newJson);
			}
		}

		public async Task Delete(string id)
		{
			if (!this.BackingFile.Exists)
			{
				await Task.FromResult(0);
				return;
			}

			IDictionary<string, T> records;
			using (var reader = this.BackingFile.OpenText())
			{
				var json = await reader.ReadToEndAsync();
				records = JsonConvert.DeserializeObject<Dictionary<string, T>>(json);
			}

			if (records == null)
			{
				await Task.FromResult(0);
				return;
			}

			if (records.ContainsKey(id))
			{
				records.Remove(id);
			}

			if (records.Count == 0)
			{
				this.BackingFile.Delete();
				await Task.FromResult(0);
			}
			else
			{
				var newJson = JsonConvert.SerializeObject(records, Formatting.Indented);
				using (var fs = this.BackingFile.Open(FileMode.Create))
				using (var writer = new StreamWriter(fs))
				{
					await writer.WriteAsync(newJson);
				}
			}
		}
	}
}
