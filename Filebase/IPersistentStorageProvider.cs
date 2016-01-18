using System.Collections.Generic;
using System.Threading.Tasks;

namespace Filebase
{
	internal interface IPersistentStorageProvider<T>
	{
		IDictionary<string, T> ReadEntities();

		Task<IDictionary<string, T>> ReadEntitiesAsync();

		void WriteEntities(IDictionary<string, T> records);

		Task WriteEntitiesAsync(IDictionary<string, T> records);
	}
}