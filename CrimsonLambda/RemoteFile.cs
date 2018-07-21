using System;
using System.IO;
using System.Net;
using Amazon.Lambda.Core;

namespace CrimsonLambda
{
    public sealed class RemoteFile
    {
        public readonly string Url;
        public long NumBytes;

        static RemoteFile() => ServicePointManager.DefaultConnectionLimit = int.MaxValue;

        public RemoteFile(string url) => Url = url;

        public void Download(ILambdaLogger logger)
        {
            logger.LogLine($"- start download: {Url}");
            var benchmark = new Benchmark();

            while (!SuccessfulDownload()) logger.LogLine($"- requeueing download of the {Url}");

            (string MegabytesPerSecond, double NumMilliseconds) results = benchmark.GetMegabytesPerSecond(NumBytes);
            logger.LogLine($"- finished download: {Url}, throughput: {results.MegabytesPerSecond} total ms: {results.NumMilliseconds}");
        }

        private bool SuccessfulDownload()
        {
            try
            {
                WebRequest request = WebRequest.Create(Url);
                var response = (HttpWebResponse)request.GetResponse();

                long numBytes = 0;
                var buffer    = new byte[8 * 1024 * 1024];

                using (Stream stream = response.GetResponseStream())
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