namespace Filebase
{
	internal class LocalRecordCache<T> : IRecordCache<T> where T : class
	{
		private T _cache;
		
		public bool HasCachedData => _cache != null;

		public T GetCachedData()
		{
			return _cache;
		}

		public void UpdateCachedData(T data)
		{
			_cache = data;
		}

		public void ClearCache()
		{
			_cache = null;
		}
	}
}