using System;

namespace CrimsonLambda
{
    public sealed class Benchmark
    {
        private readonly DateTime _startTime;

        public Benchmark() => _startTime = DateTime.Now;

        public (string MegabytesPerSecond, double NumMilliseconds) GetMegabytesPerSecond(long numBytes)
        {
            DateTime stopTime         = DateTime.Now;
            var timespan              = new TimeSpan(stopTime.Ticks - _startTime.Ticks);
            double numSeconds         = timespan.TotalSeconds;
            double megaBytes          = numBytes / 1024.0 / 1024.0;
            double megaBytesPerSecond = megaBytes / numSeconds;
            return ($"{megaBytesPerSecond:0.000} MB/s", timespan.TotalMilliseconds);
        }
    }
}