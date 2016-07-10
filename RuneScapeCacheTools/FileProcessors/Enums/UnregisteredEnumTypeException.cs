using System;

namespace Villermen.RuneScapeCacheTools.FileProcessors.Enums
{
	public class UnregisteredEnumTypeException : Exception
	{
		public UnregisteredEnumTypeException()
		{
		}

		public UnregisteredEnumTypeException(string message) : base(message)
		{
		}

		public UnregisteredEnumTypeException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
