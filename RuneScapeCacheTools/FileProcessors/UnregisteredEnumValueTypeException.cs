using System;

namespace Villermen.RuneScapeCacheTools.FileProcessors
{
	public class UnregisteredEnumValueTypeException : Exception
	{
		public UnregisteredEnumValueTypeException()
		{
		}

		public UnregisteredEnumValueTypeException(string message) : base(message)
		{
		}

		public UnregisteredEnumValueTypeException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
