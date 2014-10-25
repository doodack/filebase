using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Filebase;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tests
{
	public class FilebaseDatasetTests
	{
		private readonly string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData\\");

		internal class Entity
		{
			public string Id { get; set; }

			public int IntProp { get; set; }

			public Entity CompoundProp { get; set; }
		}

		[TearDown]
		public void Cleanup()
		{
			var dir = new DirectoryInfo(rootPath);
			if (dir.Exists)
			{
				dir.Delete(true);
			}
		}

		[Test]
		public async void GetAllAsync_when_file_doesnt_exist_should_return_empty_collection()
		{
			FilebaseContext ctx = new FilebaseContext(rootPath);
			FilebaseDataset<Entity> dataset = new FilebaseDataset<Entity>("entities", ctx, e => e.Id);

			IEnumerable<Entity> results = await dataset.GetAllAsync();
			Assert.AreEqual(0, results.Count());
		}

		[Test]
		public async void GetAllAsync_when_empty_file_exists_should_return_empty_collection()
		{
			FilebaseContext ctx = new FilebaseContext(rootPath);
			FilebaseDataset<Entity> dataset = new FilebaseDataset<Entity>("entities", ctx, e => e.Id);
			this.SetupFile(string.Empty);

			IEnumerable<Entity> results = await dataset.GetAllAsync();
			Assert.AreEqual(0, results.Count());
		}

		[Test]
		public async void GetAllAsync_when_file_exists_and_have_two_flat_objects_should_return_collection_with_this_objects()
		{
			FilebaseContext ctx = new FilebaseContext(rootPath);
			FilebaseDataset<Entity> dataset = new FilebaseDataset<Entity>("entities", ctx, e => e.Id);
			this.SetupFile(new[] 
			{ 
				new Entity { CompoundProp = null, Id = "one", IntProp = 1 },
				new Entity { CompoundProp = null, Id = "two", IntProp = 2 }
			});

			IEnumerable<Entity> results = await dataset.GetAllAsync();
			Assert.AreEqual(2, results.Count());

			var result1 = results.First();
			Assert.AreEqual("one", result1.Id);
			Assert.AreEqual(1, result1.IntProp);
			Assert.IsNull(result1.CompoundProp);

			var result2 = results.ElementAt(1);
			Assert.AreEqual("two", result2.Id);
			Assert.AreEqual(2, result2.IntProp);
			Assert.IsNull(result2.CompoundProp);
		}

		[Test]
		public async void GetByIdAsync_when_file_doesnt_exist_should_return_null()
		{
			FilebaseContext ctx = new FilebaseContext(rootPath);
			FilebaseDataset<Entity> dataset = new FilebaseDataset<Entity>("entities", ctx, e => e.Id);

			Entity result = await dataset.GetByIdAsync("0");
			Assert.IsNull(result);
		}

		[Test]
		public async void GetByIdAsync_when_record_doesnt_exist_should_return_null()
		{
			FilebaseContext ctx = new FilebaseContext(rootPath);
			FilebaseDataset<Entity> dataset = new FilebaseDataset<Entity>("entities", ctx, e => e.Id);
			this.SetupFile(new[] 
			{ 
				new Entity { CompoundProp = null, Id = "one", IntProp = 1 },
				new Entity { CompoundProp = null, Id = "two", IntProp = 2 }
			});

			Entity result = await dataset.GetByIdAsync("nonexistent");
			Assert.IsNull(result);
		}

		[Test]
		public async void GetByIdAsync_when_flat_record_exists_should_return_it()
		{
			FilebaseContext ctx = new FilebaseContext(rootPath);
			FilebaseDataset<Entity> dataset = new FilebaseDataset<Entity>("entities", ctx, e => e.Id);
			this.SetupFile(new[] 
			{ 
				new Entity { CompoundProp = null, Id = "one", IntProp = 1 },
				new Entity { CompoundProp = null, Id = "two", IntProp = 2 }
			});

			Entity result = await dataset.GetByIdAsync("two");
			Assert.AreEqual("two", result.Id);
			Assert.AreEqual(2, result.IntProp);
			Assert.IsNull(result.CompoundProp);
		}
		
		[Test]
		public async void GetByIdAsync_when_compound_record_exists_should_return_it()
		{
			FilebaseContext ctx = new FilebaseContext(rootPath);
			FilebaseDataset<Entity> dataset = new FilebaseDataset<Entity>("entities", ctx, e => e.Id);
			this.SetupFile(new[] 
			{ 
				new Entity { CompoundProp = null, Id = "one", IntProp = 1 },
				new Entity 
				{ 
					CompoundProp = new Entity { Id = "two-one", IntProp = 21, CompoundProp = null },
					Id = "two", 
					IntProp = 2 }
			});

			Entity result = await dataset.GetByIdAsync("two");
			Assert.AreEqual("two", result.Id);
			Assert.AreEqual(2, result.IntProp);
			Assert.AreEqual("two-one", result.CompoundProp.Id);
			Assert.AreEqual(21, result.CompoundProp.IntProp);
			Assert.IsNull(result.CompoundProp.CompoundProp);
		}

		[Test]
		public async void AddOrUpdateAsync_when_file_doesnt_exist_should_create_it()
		{
			FilebaseContext ctx = new FilebaseContext(rootPath);
			FilebaseDataset<Entity> dataset = new FilebaseDataset<Entity>("entities", ctx, e => e.Id);

			await dataset.AddOrUpdateAsync(new Entity { Id = "one", IntProp = 1 });
			var fileInfo = new FileInfo(Path.Combine(rootPath, "entities.json"));
			Assert.IsTrue(fileInfo.Exists);
		}

		[Test]
		public async void AddOrUpdateAsync_when_record_doesnt_exist_should_create_it()
		{
			FilebaseContext ctx = new FilebaseContext(rootPath);
			FilebaseDataset<Entity> dataset = new FilebaseDataset<Entity>("entities", ctx, e => e.Id);
			this.SetupFile(new[] 
			{ 
				new Entity { CompoundProp = null, Id = "one", IntProp = 1 },
			});

			await dataset.AddOrUpdateAsync(new Entity { Id = "new", IntProp = 42 });
			var result = await dataset.GetByIdAsync("new");
			Assert.AreEqual(2, (await dataset.GetAllAsync()).Count());
			Assert.AreEqual("new", result.Id);
			Assert.AreEqual(42, result.IntProp);
			Assert.IsNull(result.CompoundProp);
		}

		[Test]
		public async void AddOrUpdateAsync_when_record_exists_should_change_it()
		{
			FilebaseContext ctx = new FilebaseContext(rootPath);
			FilebaseDataset<Entity> dataset = new FilebaseDataset<Entity>("entities", ctx, e => e.Id);
			this.SetupFile(new[] 
			{ 
				new Entity { CompoundProp = null, Id = "one", IntProp = 1 },
			});

			await dataset.AddOrUpdateAsync(new Entity { Id = "one", IntProp = 111 });
			var result = await dataset.GetByIdAsync("one");
			Assert.AreEqual(1, (await dataset.GetAllAsync()).Count());
			Assert.AreEqual("one", result.Id);
			Assert.AreEqual(111, result.IntProp);
			Assert.IsNull(result.CompoundProp);
		}

		[Test]
		public async void DeleteAsync_when_two_items_exist_should_delete_one()
		{
			FilebaseContext ctx = new FilebaseContext(rootPath);
			FilebaseDataset<Entity> dataset = new FilebaseDataset<Entity>("entities", ctx, e => e.Id);
			this.SetupFile(new[] 
			{ 
				new Entity { CompoundProp = null, Id = "one", IntProp = 1 },
				new Entity { CompoundProp = null, Id = "two", IntProp = 2 }
			});

			await dataset.DeleteAsync("one");

			Assert.AreEqual(1, (await dataset.GetAllAsync()).Count());
			var remaining = await dataset.GetByIdAsync("two");
			Assert.IsNotNull(remaining);
		}

		[Test]
		public async void DeleteAsync_when_one_item_exists_should_delete_the_whole_file()
		{
			FilebaseContext ctx = new FilebaseContext(rootPath);
			FilebaseDataset<Entity> dataset = new FilebaseDataset<Entity>("entities", ctx, e => e.Id);
			this.SetupFile(new[] 
			{ 
				new Entity { CompoundProp = null, Id = "one", IntProp = 1 }
			});

			await dataset.DeleteAsync("one");

			Assert.AreEqual(0, (await dataset.GetAllAsync()).Count());

			var fileInfo = new FileInfo(Path.Combine(rootPath, "entities.json"));
			Assert.IsFalse(fileInfo.Exists);
		}

		private void SetupFile(string contents)
		{
			var fileInfo = new FileInfo(Path.Combine(rootPath, "entities.json"));
			using (var writer = fileInfo.CreateText())
			{
				writer.Write(contents);
			}
		}

		private void SetupFile(IEnumerable<Entity> contents)
		{
			var dict = contents.ToDictionary(e => e.Id);
			var json = JsonConvert.SerializeObject(dict);
			SetupFile(json);
		}
	}
}
