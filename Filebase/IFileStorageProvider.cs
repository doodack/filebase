using System.Collections.Generic;
using System.Threading.Tasks;

namespace Filebase
{
	internal interface IFileStorageProvider<T>
	{
		IDictionary<string, T> ReadFile();

		Task<IDictionary<string, T>> ReadFileAsync();

		void WriteFile(IDictionary<string, T> records);

		Task WriteFileAsync(IDictionary<string, T> records);
	}
}