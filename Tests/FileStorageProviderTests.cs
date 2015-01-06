using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

using NUnit.Framework;

namespace Filebase.Tests
{
	public class FileStorageProviderTests
	{
		private readonly static string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData\\");
		private readonly static string filePath = Path.Combine(rootPath, "test.json");

		private readonly FileStorageProvider<TestEntity> fileStorageProvider = new FileStorageProvider<TestEntity>(new FileInfo(filePath));

		[SetUp]
		public void Setup()
		{
			var dir = new DirectoryInfo(rootPath);
			if (!dir.Exists)
			{
				dir.Create();
				dir.Refresh();
			}
		}

		[TearDown]
		public void Cleanup()
		{
			var dir = new DirectoryInfo(rootPath);
			if (dir.Exists)
			{
				dir.Delete(true);
				dir.Refresh();
			}
		}

		#region ReadFile tests

		[Test]
		public void ReadFile_when_file_doesnt_exist_should_return_empty_collection()
		{
			var results = fileStorageProvider.ReadFile();
			Assert.AreEqual(0, results.Count());
		}

		[Test]
		public async void ReadFileAsync_when_file_doesnt_exist_should_return_empty_collection()
		{
			var results = await fileStorageProvider.ReadFileAsync();
			Assert.AreEqual(0, results.Count());
		}

		[Test]
		public void ReadFile_when_empty_file_exists_should_return_empty_collection()
		{
			SetupFile(string.Empty);

			var results = fileStorageProvider.ReadFile();
			Assert.AreEqual(0, results.Count());
		}

		[Test]
		public async void ReadFileAsync_when_empty_file_exists_should_return_empty_collection()
		{
			SetupFile(string.Empty);

			var results = await fileStorageProvider.ReadFileAsync();
			Assert.AreEqual(0, results.Count());
		}

		[Test]
		public void ReadFile_when_file_exists_and_have_two_flat_objects_should_return_collection_with_this_objects()
		{
			SetupFile(new[] 
			{ 
				new TestEntity { CompoundProp = null, Id = "one", IntProp = 1 },
				new TestEntity { CompoundProp = null, Id = "two", IntProp = 2 }
			});

			var results = fileStorageProvider.ReadFile();
			Assert.AreEqual(2, results.Count());

			Assert.IsTrue(results.ContainsKey("one"));
			Assert.IsTrue(results.ContainsKey("two"));

			var result1 = results["one"];
			Assert.AreEqual("one", result1.Id);
			Assert.AreEqual(1, result1.IntProp);
			Assert.IsNull(result1.CompoundProp);

			var result2 = results["two"];
			Assert.AreEqual("two", result2.Id);
			Assert.AreEqual(2, result2.IntProp);
			Assert.IsNull(result2.CompoundProp);
		}

		[Test]
		public async void ReadFileAsync_when_file_exists_and_have_two_flat_objects_should_return_collection_with_this_objects()
		{
			SetupFile(new[] 
			{ 
				new TestEntity { CompoundProp = null, Id = "one", IntProp = 1 },
				new TestEntity { CompoundProp = null, Id = "two", IntProp = 2 }
			});

			var results = await fileStorageProvider.ReadFileAsync();
			Assert.AreEqual(2, results.Count());

			Assert.IsTrue(results.ContainsKey("one"));
			Assert.IsTrue(results.ContainsKey("two"));

			var result1 = results["one"];
			Assert.AreEqual("one", result1.Id);
			Assert.AreEqual(1, result1.IntProp);
			Assert.IsNull(result1.CompoundProp);

			var result2 = results["two"];
			Assert.AreEqual("two", result2.Id);
			Assert.AreEqual(2, result2.IntProp);
			Assert.IsNull(result2.CompoundProp);
		}

		#endregion

		#region WriteFile tests

		[Test]
		public void WriteFile_when_file_doesnt_exist_should_create_it()
		{
			var records = new Dictionary<string, TestEntity> { { "one", new TestEntity { Id = "one", IntProp = 1 } } };

			fileStorageProvider.WriteFile(records);
			var fileInfo = new FileInfo(filePath);
			Assert.IsTrue(fileInfo.Exists);
		}

		[Test]
		public async void WriteFileAsync_when_file_doesnt_exist_should_create_it()
		{
			var records = new Dictionary<string, TestEntity> { { "one", new TestEntity { Id = "one", IntProp = 1 } } };

			await fileStorageProvider.WriteFileAsync(records);
			var fileInfo = new FileInfo(filePath);
			Assert.IsTrue(fileInfo.Exists);
		}

		[Test]
		public void WriteFile_should_save_serialized_records()
		{
			var records = new Dictionary<string, TestEntity>
				{
					{ "one", new TestEntity { Id = "one", IntProp = 1 } },
					{ "two", new TestEntity { Id = "two", IntProp = 2, CompoundProp = new TestEntity { Id = "two-one", IntProp = 21 } } }
				};

			fileStorageProvider.WriteFile(records);

			var fileInfo = new FileInfo(filePath);

			Dictionary<string, TestEntity> savedRecords;
			using (var reader = fileInfo.OpenText())
			{
				var json = reader.ReadToEnd();
				savedRecords = JsonConvert.DeserializeObject<Dictionary<string, TestEntity>>(json);
			}

			CollectionAssert.AreEqual(records, savedRecords);
		}

		[Test]
		public async void WriteFileAsync_should_save_serialized_records()
		{
			var records = new Dictionary<string, TestEntity>
				{
					{ "one", new TestEntity { Id = "one", IntProp = 1 } },
					{ "two", new TestEntity { Id = "two", IntProp = 2, CompoundProp = new TestEntity { Id = "two-one", IntProp = 21 } } }
				};

			await fileStorageProvider.WriteFileAsync(records);

			var fileInfo = new FileInfo(filePath);

			Dictionary<string, TestEntity> savedRecords;
			using (var reader = fileInfo.OpenText())
			{
				var json = reader.ReadToEnd();
				savedRecords = JsonConvert.DeserializeObject<Dictionary<string, TestEntity>>(json);
			}

			CollectionAssert.AreEqual(records, savedRecords);
		}

		[Test]
		public void WriteFile_when_empty_dictionary_provided_should_delete_the_file()
		{
			SetupFile(new[] 
			{ 
				new TestEntity { CompoundProp = null, Id = "one", IntProp = 1 }
			});

			var fileInfo = new FileInfo(filePath);
			Assert.IsTrue(fileInfo.Exists);

			fileStorageProvider.WriteFile(new Dictionary<string, TestEntity>());

			fileInfo.Refresh();
			Assert.IsFalse(fileInfo.Exists);
		}

		[Test]
		public async void WriteFileAsync_when_empty_dictionary_provided_should_delete_the_file()
		{
			SetupFile(new[] 
			{ 
				new TestEntity { CompoundProp = null, Id = "one", IntProp = 1 }
			});

			var fileInfo = new FileInfo(filePath);
			Assert.IsTrue(fileInfo.Exists);

			await fileStorageProvider.WriteFileAsync(new Dictionary<string, TestEntity>());

			fileInfo.Refresh();
			Assert.IsFalse(fileInfo.Exists);
		}

		#endregion

		[Test]
		public async void WriteFileAsync_when_read_is_in_progress_should_wait()
		{
			var records = new[]
				{
					new TestEntity { CompoundProp = null, Id = "one", IntProp = 1 },
					new TestEntity { CompoundProp = null, Id = "two", IntProp = 2 }
				};
			
			SetupFile(records);

			var recordsDict = records.ToDictionary(r => r.Id);

			var t1 = fileStorageProvider.ReadFileAsync().ContinueWith(_ => Debug.WriteLine("t1 completed"));
			var t2 = fileStorageProvider.WriteFileAsync(recordsDict).ContinueWith(_ => Debug.WriteLine("t2 completed"));
			var t3 = fileStorageProvider.WriteFileAsync(recordsDict).ContinueWith(_ => Debug.WriteLine("t3 completed"));
			var t4 = fileStorageProvider.WriteFileAsync(recordsDict).ContinueWith(_ => Debug.WriteLine("t4 completed"));

			await Task.WhenAll(t1, t2, t3, t4);

			Assert.Pass();
		}

		[Test]
		public async void WriteFile_when_read_is_in_progress_should_wait()
		{
			var records = new[]
				{
					new TestEntity { CompoundProp = null, Id = "one", IntProp = 1 },
					new TestEntity { CompoundProp = null, Id = "two", IntProp = 2 }
				};

			SetupFile(records);

			var recordsDict = records.ToDictionary(r => r.Id);

			var t1 = HoldFileOpen(FileAccess.Read, FileShare.Read).ContinueWith(_ => Debug.WriteLine("t1 completed"));
			var t2 = Task.Run(() => fileStorageProvider.WriteFile(recordsDict)).ContinueWith(_ => Debug.WriteLine("t2 completed"));
			var t3 = Task.Run(() => fileStorageProvider.WriteFile(recordsDict)).ContinueWith(_ => Debug.WriteLine("t3 completed"));
			var t4 = Task.Run(() => fileStorageProvider.WriteFile(recordsDict)).ContinueWith(_ => Debug.WriteLine("t4 completed"));

			await Task.WhenAll(t1, t2);

			Assert.Pass();
		}

		private void SetupFile(string contents)
		{
			var fileInfo = new FileInfo(filePath);
			using (var writer = fileInfo.CreateText())
			{
				writer.Write(contents);
			}
		}

		private void SetupFile(IEnumerable<TestEntity> contents)
		{
			var dict = contents.ToDictionary(e => e.Id);
			var json = JsonConvert.SerializeObject(dict);
			SetupFile(json);
		}

		private async Task HoldFileOpen(FileAccess fileAccess, FileShare fileShare)
		{
			using (File.Open(filePath, FileMode.Open, fileAccess, fileShare))
			{
				await Task.Delay(500);
			}
		}
	}
}
