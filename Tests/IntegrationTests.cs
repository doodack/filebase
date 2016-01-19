using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Filebase.Tests
{
	public class IntegratonTests
	{
		private static readonly string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData\\");
		private static readonly string filePath = Path.Combine(rootPath, "integration.json");

		private Dataset<TestEntity> _dataset;

		[SetUp]
		public void Setup()
		{
			var factory = new DatasetFactory();
			_dataset = factory.Create<TestEntity>(filePath, t => "id:" + t.Id);
			_dataset.IsVolatile = true;
		}

		[TearDown]
		public void Cleanup()
		{
			var directory = new DirectoryInfo(rootPath);
			directory.Delete(true);
		}

		[Test]
		public void Given_no_file_saving_new_entity_should_create_valid_JSON_file()
		{
			_dataset.AddOrUpdate(new TestEntity("42", 4242));
			var parsed = GetParsedFile();
			Assert.True(parsed.ContainsKey("id:42"));
		}

		[Test]
		public void Given_existing_data_saving_new_entity_should_update_JSON_file()
		{
			_dataset.AddOrUpdate(new TestEntity("42", 4242));
			_dataset.AddOrUpdate(new TestEntity("88", 8888));
			_dataset.AddOrUpdate(new TestEntity("21", 2100));

			var parsed = GetParsedFile();

			Assert.AreEqual(3, parsed.Count);
			Assert.True(parsed.ContainsKey("id:42"));
			Assert.True(parsed.ContainsKey("id:88"));
			Assert.True(parsed.ContainsKey("id:21"));
			Assert.AreEqual(4242, parsed["id:42"].IntProp);
			Assert.AreEqual("42", parsed["id:42"].Id);
		}

		private string GetFileContents()
		{
			var jsonFile = new FileInfo(filePath);

			using (var reader = jsonFile.OpenText())
			{
				return reader.ReadToEnd();
			}
		}

		private Dictionary<string, TestEntity> GetParsedFile()
		{
			var json = GetFileContents();
			return JsonConvert.DeserializeObject<Dictionary<string, TestEntity>>(json);
		} 
	}
}