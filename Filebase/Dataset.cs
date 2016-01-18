using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Filebase
{
	/// <summary>
	/// Provides methods for CRUD operations against a Filebase data.
	/// </summary>
	/// <typeparam name="T">Type of objects this data set is for.</typeparam>
	public class Dataset<T> where T : class
	{
		private readonly Func<T, string> _idExtractor;

		private readonly IRecordCache<IDictionary<string, T>> _localRecords;

		/// <summary>
		/// Initializes a new instance of the <see cref="Dataset{T}"/> class.
		/// </summary>
		/// <param name="storageProvider">The persistent storage provider.</param>
		/// <param name="idExtractor">The function that extracts an id from a record.</param>
		internal Dataset(IPersistentStorageProvider<T> storageProvider, Func<T, string> idExtractor)
		{
			_idExtractor = idExtractor;
			_localRecords = new LocalRecordCache<IDictionary<string, T>>();
			FileStorageProvider = storageProvider;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this data set is volatile.
		/// If it is, a file will be opened and read each time a read operation is performed. 
		/// Otherwise, data will be cached locally.
		/// </summary>
		public bool IsVolatile { get; set; } = true;

		internal IPersistentStorageProvider<T> FileStorageProvider { get; }

		/// <summary>
		/// Gets all records from the set.
		/// </summary>
		public IReadOnlyCollection<T> GetAll()
		{
			var records = GetRecords();
			return records?.Values.ToArray() ?? new T[0];
		}

		/// <summary>
		/// Gets all records from the set. This method is asynchronous.
		/// </summary>
		public async Task<IReadOnlyCollection<T>> GetAllAsync()
		{
			var records = await GetRecordsAsync();
			return records?.Values.ToArray() ?? new T[0];
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

			var records = FileStorageProvider.ReadEntities();
			
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

			var records = await FileStorageProvider.ReadEntitiesAsync();

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

			FileStorageProvider.WriteEntities(records);
		}

		private async Task PersistRecordsAsync(IDictionary<string, T> records)
		{
			if (!IsVolatile)
			{
				_localRecords.UpdateCachedData(records);
			}
			
			await FileStorageProvider.WriteEntitiesAsync(records);
		}
	}
}
