using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Filebase.Tests
{
	public class DatasetTests
	{
		private List<TestEntity> _testEntities = new List<TestEntity>()
		{
			new TestEntity { Id = "1", IntProp = 1 },
			new TestEntity { Id = "2", IntProp = 2, CompoundProp = new TestEntity { Id = "2.1", IntProp = 2 } },
			new TestEntity { Id = "3", IntProp = 3 }
		};

		private Dataset<TestEntity> _dataset;
		private Mock<IPersistentStorageProvider<TestEntity>> _storageMock;

		[SetUp]
		public void SetUp()
		{
			_storageMock = new Mock<IPersistentStorageProvider<TestEntity>>();
			_storageMock.Setup(s => s.ReadEntities()).Returns(_testEntities.ToDictionary(e => e.Id));
			_storageMock.Setup(s => s.ReadEntitiesAsync()).ReturnsAsync(_testEntities.ToDictionary(e => e.Id));

			_dataset = new Dataset<TestEntity>(_storageMock.Object, t => t.Id);
		}

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
	}
}
