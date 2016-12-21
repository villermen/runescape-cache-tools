using System;

namespace Villermen.RuneScapeCacheTools.Extensions
{
    public class ExtendedProgress : Progress<string>
    {
        public int Total { get; set; }

        public int Current { get; private set; }

        public float Percentage => this.Total > 0 ? 100f / this.Total * this.Current : 100;

        public void Report(string value)
        {
            this.Current++;

            this.OnReport(value);
        }
    }
}