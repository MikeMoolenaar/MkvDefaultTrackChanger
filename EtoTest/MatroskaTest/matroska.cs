using NEbml.Core;
using System;

namespace NEbml.Matroska
{
	public static class ReaderExtensions
	{
		public static void LocateElement(this EbmlReader reader, ulong descriptor)
		{
			while (reader.ReadNext())
			{
				if (reader.ElementId.EncodedValue == descriptor)
				{
					reader.EnterContainer();
					return;
				}
			}

			throw new Exception($"Cannot find descriptor 0x{descriptor:X}");
		}

		public static bool TryLocateElement(this EbmlReader reader, ulong descriptor)
		{
			try
			{
				ReaderExtensions.LocateElement(reader, descriptor);
				return true;
			}
			catch (Exception _)
			{
				return false;
			}
		}
	}
}