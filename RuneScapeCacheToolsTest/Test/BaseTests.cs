using System;
using Xunit;

namespace Villermen.RuneScapeCacheTools.Test
{
    public abstract class BaseTests
    {
        private DateTimeOffset _startTime = DateTimeOffset.UtcNow;

        /// <summary>
        /// Asserts that the given file was modified by this test as it could just be a leftover from the previous one.
        /// </summary>
        protected void AssertFileExistsAndModified(string filePath)
        {
            Assert.True(System.IO.File.Exists(filePath));

            DateTimeOffset modifiedTime = System.IO.File.GetLastAccessTimeUtc(filePath);
            Assert.False(
                modifiedTime.ToUnixTimeSeconds() < this._startTime.ToUnixTimeSeconds(),
                $"File modified time ({modifiedTime:u}) was less than test start time ({this._startTime:u})."
            );
        }
    }
}
