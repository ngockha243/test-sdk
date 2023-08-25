using System;
using System.Diagnostics;

namespace Extensions
{
	public static class UnityObjectExt
	{
		[Conditional("ENABLE_THROWS")]
		public static void ThrowIfNull(this UnityEngine.Object o)
		{
			if (o == null) throw new ArgumentNullException();
		}
	}
}