namespace Filebase
{
	internal interface IRecordCache<T>
	{
		bool HasCachedData { get; }
		
		T GetCachedData();

		void UpdateCachedData(T data);

		void ClearCache();
	}
}
