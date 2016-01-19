using System;
using System.IO;

namespace Filebase
{
	public class DatasetFactory
	{
		public Dataset<T> Create<T>(string filePath, Func<T, string> idExtractor) where T : class
		{
			var directoryPath = Path.GetDirectoryName(filePath);
			var directory = new DirectoryInfo(directoryPath);
			if (!directory.Exists)
			{
				directory.Create();
			}

			var fileStorageProvider = new FileStorageProvider<T>(new FileInfo(filePath));
			return new Dataset<T>(fileStorageProvider, idExtractor);
		}
	}
}