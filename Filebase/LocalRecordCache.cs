namespace Filebase
{
	internal class LocalRecordCache<T> : IRecordCache<T> where T : class
	{
		private T cache;
		
		public bool HasCachedData
		{
			get { return cache != null; }
		}

		public T GetCachedData()
		{
			return cache;
		}

		public void UpdateCachedData(T data)
		{
			cache = data;
		}

		public void ClearCache()
		{
			cache = null;
		}
	}
}