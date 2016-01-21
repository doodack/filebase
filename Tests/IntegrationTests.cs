using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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

		[Test]
		public void Given_multiple_datasets_sync_methods_should_work_properly()
		{ 
			var result = Parallel.For(0, 100, DoWork);
			
			Assert.True(result.IsCompleted);
			var parsed = GetParsedFile();
			Assert.AreEqual(100, parsed.Count);
		}

		[Test]
		public async Task Given_multiple_datasets_async_methods_should_work_properly()
		{
			var tasks = new List<Task>(100);

			for (int i = 0; i < 100; ++i)
			{
				var workerIndex = i;
				tasks.Add(DoWorkAsync(workerIndex));
			}

			await Task.WhenAll(tasks);

			var parsed = GetParsedFile();
			Assert.AreEqual(100, parsed.Count);
		}

		private void DoWork(int workerIndex)
		{
			var factory = new DatasetFactory();
			var dataset = factory.Create<TestEntity>(filePath, t => t.Id);
			dataset.IsVolatile = true;

			var tempItemId = workerIndex + "-toDelete";

			dataset.AddOrUpdate(new TestEntity(tempItemId, workerIndex));
			var tempItem = dataset.GetById(tempItemId);
			Assert.NotNull(tempItem);
			dataset.AddOrUpdate(new TestEntity(workerIndex.ToString(), workerIndex));
			dataset.Delete(tempItemId);
			tempItem = dataset.GetById(tempItemId);
			Assert.Null(tempItem);
		}

		private async Task DoWorkAsync(int workerIndex)
		{
			Debug.WriteLine($"{DateTime.Now.ToLongTimeString()} WORKER {workerIndex}: starting");

			var factory = new DatasetFactory();
			var dataset = factory.Create<TestEntity>(filePath, t => t.Id);
			dataset.IsVolatile = true;

			var tempItemId = workerIndex + "-toDelete";

			Debug.WriteLine($"{DateTime.Now.ToLongTimeString()} WORKER {workerIndex}: adding test entity");
			await dataset.AddOrUpdateAsync(new TestEntity(tempItemId, workerIndex));

			Debug.WriteLine($"{DateTime.Now.ToLongTimeString()} WORKER {workerIndex}: getting test entity");
			var tempItem = dataset.GetById(tempItemId);
			Assert.NotNull(tempItem);

			Debug.WriteLine($"{DateTime.Now.ToLongTimeString()} WORKER {workerIndex}: adding second entity");
			await dataset.AddOrUpdateAsync(new TestEntity(workerIndex.ToString(), workerIndex));

			Debug.WriteLine($"{DateTime.Now.ToLongTimeString()} WORKER {workerIndex}: deleting test entity");
			await dataset.DeleteAsync(tempItemId);

			Debug.WriteLine($"{DateTime.Now.ToLongTimeString()} WORKER {workerIndex}: getting deleted test entity");
			tempItem = await dataset.GetByIdAsync(tempItemId);
			Assert.Null(tempItem);

			Debug.WriteLine($"{DateTime.Now.ToLongTimeString()} WORKER {workerIndex}: done");
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