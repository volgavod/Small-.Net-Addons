using System;
using System.Collections;

namespace SmallDotNetAddons.Collections
{
	public class EmptyCollectionException(string? message) : Exception(message)
	{
		public static void ThrowIfNullOrEmpty(IEnumerable collection, string message = "Collection can't be null or empty.")
		{
			if (collection == null || !collection.GetEnumerator().MoveNext())
				throw new EmptyCollectionException(message);
		}
	}
}
