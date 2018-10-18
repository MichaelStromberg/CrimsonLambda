using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace CrimsonLambda
{
    public sealed class RemoteFile
    {
        private readonly string _url;
        public long NumBytes;

        private static readonly HttpClient HttpClient = new HttpClient();
        static RemoteFile() => ServicePointManager.DefaultConnectionLimit = int.MaxValue;

        public RemoteFile(string url) => _url = url;

        public void Download(ILambdaLogger logger)
        {
            logger.LogLine($"- start download: {_url}");
            var benchmark = new Benchmark();

            while (!SuccessfulDownloadAsync().Result) logger.LogLine($"- requeueing download of the {_url}");

            (string MegabytesPerSecond, double NumMilliseconds) results = benchmark.GetMegabytesPerSecond(NumBytes);
            logger.LogLine($"- finished download: {_url}, throughput: {results.MegabytesPerSecond} total ms: {results.NumMilliseconds}");
        }

        private async Task<bool> SuccessfulDownloadAsync()
        {
            try
            {
                long numBytes = 0;
                var buffer    = new byte[8 * 1024 * 1024];

                using (var stream = await HttpClient.GetStreamAsync(_url))
                {
                    while (true)
                    {
                        int numBytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (numBytesRead == 0) break;
                        numBytes += numBytesRead;
                    }
                }

                NumBytes = numBytes;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}