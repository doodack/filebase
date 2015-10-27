using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Filebase
{
	/// <summary>
	/// Provides methods for CRUD operations against a Filebase data.
	/// </summary>
	/// <typeparam name="T">Type of objects this data set is for.</typeparam>
	public class FilebaseDataset<T> where T : class
	{
		private readonly Func<T, string> _idExtractor;

		private readonly IRecordCache<IDictionary<string, T>> _localRecords;

		/// <summary>
		/// Initializes a new instance of the <see cref="FilebaseDataset{T}"/> class.
		/// </summary>
		/// <param name="name">The name of the data set.</param>
		/// <param name="context">The associated <see cref="FilebaseContext"/> instance.</param>
		/// <param name="idExtractor">The function that extracts an id from a record.</param>
		public FilebaseDataset(string name, FilebaseContext context, Func<T, string> idExtractor)
		{
			_idExtractor = idExtractor;
			_localRecords = new LocalRecordCache<IDictionary<string, T>>();

			var backingFilePath = Path.Combine(context.RootDirectory.FullName, name + ".json");
			FileStorageProvider = new FileStorageProvider<T>(new FileInfo(backingFilePath));
		}

		/// <summary>
		/// Gets or sets a value indicating whether this data set is volatile.
		/// If it is, a file will be opened and read each time a read operation is performed. 
		/// Otherwise, data will be cached locally.
		/// </summary>
		public bool IsVolatile { get; set; } = true;

		internal IFileStorageProvider<T> FileStorageProvider { get; set; }

		/// <summary>
		/// Gets all records from the set.
		/// </summary>
		public IEnumerable<T> GetAll()
		{
			var records = GetRecords();
			return records?.Values ?? Enumerable.Empty<T>();
		}

		/// <summary>
		/// Gets all records from the set. This method is asynchronous.
		/// </summary>
		public async Task<IEnumerable<T>> GetAllAsync()
		{
			var records = await GetRecordsAsync();
			return records?.Values ?? Enumerable.Empty<T>();
		}

		/// <summary>
		/// Gets the specified record from the set or null, if it does not exist.
		/// </summary>
		/// <param name="id">Record identifier.</param>
		public T GetById(string id)
		{
			var records = GetRecords();

			T record;
			return records.TryGetValue(id, out record) ? record : null;
		}

		/// <summary>
		/// Gets the specified record from the set or null, if it does not exist. This method is asynchronous.
		/// </summary>
		/// <param name="id">Record identifier.</param>
		public async Task<T> GetByIdAsync(string id)
		{
			var records = await GetRecordsAsync();

			T record;
			return records.TryGetValue(id, out record) ? record : null;
		}

		/// <summary>
		/// Adds the specified record to set or updates the existing one.
		/// </summary>
		/// <param name="record">Record to add or update.</param>
		public void AddOrUpdate(T record)
		{
			var records = GetRecords();
			var id = _idExtractor(record);
			records[id] = record;
			PersistRecords(records);
		}

		/// <summary>
		/// Adds the specified record to set or updates the existing one. This method is asynchronous.
		/// </summary>
		public async Task AddOrUpdateAsync(T record)
		{
			var records = await GetRecordsAsync();
			var id = _idExtractor(record);
			records[id] = record;
			await PersistRecordsAsync(records);
		}

		/// <summary>
		/// Deletes the specified record from the set. If the record does not exist, the method exits immediately.
		/// </summary>
		/// <param name="id">Id of the record to delete.</param>
		public void Delete(string id)
		{
			var records = GetRecords();
			if (records.ContainsKey(id))
			{
				records.Remove(id);
			}

			PersistRecords(records);
		}

		/// <summary>
		/// Deletes the specified record from the set. If the record does not exist, the method exits immediately. This method is asynchronous.
		/// </summary>
		/// <param name="id">Id of the record to delete.</param>
		public async Task DeleteAsync(string id)
		{
			var records = await GetRecordsAsync();
			if (records.ContainsKey(id))
			{
				records.Remove(id);
			}

			await PersistRecordsAsync(records);
		}

		private IDictionary<string, T> GetRecords()
		{
			if (!IsVolatile && _localRecords.HasCachedData)
			{
				return _localRecords.GetCachedData();
			}

			var records = FileStorageProvider.ReadFile();
			
			if (!IsVolatile)
			{
				_localRecords.UpdateCachedData(records);
			}

			return records;
		}

		private async Task<IDictionary<string, T>> GetRecordsAsync()
		{
			if (!IsVolatile && _localRecords.HasCachedData)
			{
				return _localRecords.GetCachedData();
			}

			var records = await FileStorageProvider.ReadFileAsync();

			if (!IsVolatile)
			{
				_localRecords.UpdateCachedData(records);
			}

			return records;
		}

		private void PersistRecords(IDictionary<string, T> records)
		{
			if (!IsVolatile)
			{
				_localRecords.UpdateCachedData(records);
			}

			FileStorageProvider.WriteFile(records);
		}

		private async Task PersistRecordsAsync(IDictionary<string, T> records)
		{
			if (!IsVolatile)
			{
				_localRecords.UpdateCachedData(records);
			}
			
			await FileStorageProvider.WriteFileAsync(records);
		}
	}
}
