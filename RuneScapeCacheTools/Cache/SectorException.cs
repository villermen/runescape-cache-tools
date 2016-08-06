using System;

namespace Villermen.RuneScapeCacheTools.Cache
{
	public class SectorException : Exception
	{
		public SectorException()
		{
		}

		public SectorException(string message) : base(message)
		{
		}

		public SectorException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}