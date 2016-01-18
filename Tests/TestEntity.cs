namespace Filebase.Tests
{
	public class TestEntity
	{
		public string Id { get; set; }

		public int IntProp { get; set; }

		public TestEntity CompoundProp { get; set; }

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// true if the specified object  is equal to the current object; otherwise, false.
		/// </returns>
		/// <param name="obj">The object to compare with the current object. </param>
		public override bool Equals(object obj)
		{
			var other = obj as TestEntity;
			if (other == null)
			{
				return false;
			}

			return Id == other.Id && IntProp == other.IntProp && ((CompoundProp == null && other.CompoundProp == null) || CompoundProp.Equals(other.CompoundProp));
		}
	}
}