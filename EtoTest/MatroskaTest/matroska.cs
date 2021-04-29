using NEbml.Core;
using System;

namespace NEbml.Matroska
{
	internal static class ReaderExtensions
	{
		public static bool LocateElement(this EbmlReader reader, ulong descriptor)
		{
			while (reader.ReadNext())
			{
				if (reader.ElementId.EncodedValue == descriptor)
				{
					return true;
				}
			}
			return false;
		}
	}
}