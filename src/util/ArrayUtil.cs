namespace VoidShips.Util;

using System;
using System.Collections.Generic;

public static class ArrayUtil
{
	public static void FilterNonReentrant<T>(this IList<T> target, Func<int, T, bool> shouldRemain)
	{
		var writeCursor = 0;

		for (var readCursor = 0; readCursor < target.Count; readCursor++)
		{
			var readValue = target[readCursor];
			if (shouldRemain(readCursor, readValue))
				target[writeCursor++] = readValue;
		}

		target.Truncate(writeCursor);
	}

	public static void Truncate<T>(this IList<T> target, int newLen)
	{
		while (target.Count > newLen)
		{
			target.Pop();
		}
	}

	public static void Pop<T>(this IList<T> target)
	{
		target.RemoveAt(target.Count - 1);
	}
}
