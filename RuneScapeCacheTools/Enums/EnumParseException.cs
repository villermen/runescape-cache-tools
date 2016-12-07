﻿using System;

namespace Villermen.RuneScapeCacheTools.Enums
{
    public class EnumParseException : Exception
    {
        public EnumParseException()
        {
        }

        public EnumParseException(string message) : base(message)
        {
        }

        public EnumParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}