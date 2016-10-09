using System;

namespace Villermen.RuneScapeCacheTools.Extensions
{
    public class ExtendedProgress : Progress<string>
    {
        public int Total { get; set; } = -1;

        public int Current { get; private set; }

        public float Percentage => 100 / Total * Current;

        public void Report(string value)
        {
            if (Total == -1)
            {
                throw new InvalidOperationException("Total must be set before reporting progress.");
            }

            Current++;

            OnReport(value);
        }
    }
}