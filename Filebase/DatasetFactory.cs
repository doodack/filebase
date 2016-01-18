using System;
using System.IO;

namespace Filebase
{
	public class DatasetFactory
	{
		public Dataset<T> Create<T>(string filePath, Func<T, string> idExtractor) where T : class
		{
			var fileStorageProvider = new FileStorageProvider<T>(new FileInfo(filePath));
			return new Dataset<T>(fileStorageProvider, idExtractor);
		}
	}
}