using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Filebase.Tests
{
	public class DatasetTests
	{
		private List<TestEntity> _testEntities;

		private Dataset<TestEntity> _dataset;
		private Mock<IPersistentStorageProvider<TestEntity>> _storageMock;

		[SetUp]
		public void SetUp()
		{
			_testEntities = new List<TestEntity>()
			{
				new TestEntity { Id = "1", IntProp = 1 },
				new TestEntity { Id = "2", IntProp = 2, CompoundProp = new TestEntity { Id = "2.1", IntProp = 2 } },
				new TestEntity { Id = "3", IntProp = 3 }
			};

			_storageMock = new Mock<IPersistentStorageProvider<TestEntity>>();
			_storageMock.Setup(s => s.ReadEntities()).Returns(_testEntities.ToDictionary(e => e.Id));
			_storageMock.Setup(s => s.ReadEntitiesAsync()).ReturnsAsync(_testEntities.ToDictionary(e => e.Id));

			_dataset = new Dataset<TestEntity>(_storageMock.Object, t => t.Id);
		}

		#region GetAll

		[Test]
		public void GetAll_should_return_all_objects_from_storage()
		{
			var objects = _dataset.GetAll();
			CollectionAssert.AreEquivalent(objects, _testEntities);
		}

		[Test]
		public async Task GetAllAsync_should_return_all_objects_from_storage()
		{
			var objects = await _dataset.GetAllAsync();
			CollectionAssert.AreEquivalent(objects, _testEntities);
		}

		[Test]
		public void Given_empty_storage_GetAll_should_return_empty_collection()
		{
			_storageMock.Setup(s => s.ReadEntities()).Returns(new Dictionary<string, TestEntity>());

			var objects = _dataset.GetAll();
			Assert.IsEmpty(objects);
		}

		[Test]
		public async Task Given_empty_storage_GetAllAsync_should_return_empty_collection()
		{
			_storageMock.Setup(s => s.ReadEntitiesAsync()).ReturnsAsync(new Dictionary<string, TestEntity>());

			var objects = await _dataset.GetAllAsync();
			Assert.IsEmpty(objects);
		}

		#endregion

		#region GetById

		[Test]
		public void Given_valid_id_GetById_should_return_matching_entity()
		{
			var obj = _dataset.GetById("3");

			Assert.AreEqual("3", obj.Id);
			Assert.AreEqual(3, obj.IntProp);
			Assert.IsNull(obj.CompoundProp);
		}

		[Test]
		public async Task Given_valid_id_GetByIdAsync_should_return_matching_entity()
		{
			var obj = await _dataset.GetByIdAsync("3");

			Assert.AreEqual("3", obj.Id);
			Assert.AreEqual(3, obj.IntProp);
			Assert.IsNull(obj.CompoundProp);
		}

		[Test]
		public void Given_invalid_id_GetById_should_return_null()
		{
			var obj = _dataset.GetById("42");
			Assert.IsNull(obj);
		}

		[Test]
		public async Task Given_invalid_id_GetByIdAsync_should_return_null()
		{
			var obj = await _dataset.GetByIdAsync("42");
			Assert.IsNull(obj);
		}

		#endregion

		#region Caching

		[Test]
		public void Given_nonvolatile_dataset_GetAll_should_return_cached_data()
		{
			_dataset.IsVolatile = false;
			_dataset.GetAll();
			var newEntity = new TestEntity("42", 42);
			_testEntities.Add(newEntity);
			_storageMock.Setup(s => s.ReadEntities()).Returns(_testEntities.ToDictionary(e => e.Id));

			var objects = _dataset.GetAll();
			Assert.AreEqual(3, objects.Count);
			CollectionAssert.DoesNotContain(objects, newEntity);
		}

		[Test]
		public void Given_volatile_dataset_GetAll_should_return_fresh_data()
		{
			_dataset.IsVolatile = true;
			_dataset.GetAll();
			var newEntity = new TestEntity("42", 42);
			_testEntities.Add(newEntity);
			_storageMock.Setup(s => s.ReadEntities()).Returns(_testEntities.ToDictionary(e => e.Id));

			var objects = _dataset.GetAll();
			Assert.AreEqual(4, objects.Count);
			CollectionAssert.Contains(objects, newEntity);
		}

		[Test]
		public async Task Given_nonvolatile_dataset_GetAllAsync_should_return_cached_data()
		{
			_dataset.IsVolatile = false;
			_dataset.GetAll();
			var newEntity = new TestEntity("42", 42);
			_testEntities.Add(newEntity);
			_storageMock.Setup(s => s.ReadEntities()).Returns(_testEntities.ToDictionary(e => e.Id));

			var objects = await _dataset.GetAllAsync();
			Assert.AreEqual(3, objects.Count);
			CollectionAssert.DoesNotContain(objects, newEntity);
		}

		[Test]
		public async Task Given_volatile_dataset_GetAllAsync_should_return_fresh_data()
		{
			_dataset.IsVolatile = true;
			await _dataset.GetAllAsync();
			var newEntity = new TestEntity("42", 42);
			_testEntities.Add(newEntity);
			_storageMock.Setup(s => s.ReadEntities()).Returns(_testEntities.ToDictionary(e => e.Id));

			var objects = await _dataset.GetAllAsync();
			Assert.AreEqual(4, objects.Count);
			CollectionAssert.Contains(objects, newEntity);
		}

		#endregion
	}
}
