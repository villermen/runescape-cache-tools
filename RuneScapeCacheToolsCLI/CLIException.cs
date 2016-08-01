using System;

namespace Villermen.RuneScapeCacheTools.CLI
{
	public class CLIException : Exception
	{
		public CLIException()
		{
		}

		public CLIException(string message) : base(message)
		{
		}

		public CLIException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}