using System.Collections.Generic;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace CrimsonLambda
{
    public class CrimsonMain
    {
        public LambdaResult FunctionHandler(LambdaConfiguration config, ILambdaContext context)
        {
            var urls = new List<RemoteFile>
            {
                new RemoteFile("http://illumina-annotation.s3.amazonaws.com/SA/44/GRCh37/chr16.nsa")
            };

            var benchmark = new Benchmark();

            urls.Execute(context.Logger, "downloads", file => file.Download(context.Logger), config.NumThreads);

            long numBytes = 0;
            foreach (RemoteFile url in urls) numBytes += url.NumBytes;

            (string MegabytesPerSecond, double NumMilliseconds) results = benchmark.GetMegabytesPerSecond(numBytes);
            context.Logger.LogLine($"speed with {config.NumThreads} threads: {results.MegabytesPerSecond}, total ms: {results.NumMilliseconds}");

            return new LambdaResult { NumBytes = numBytes, MegabytesPerSecond = results.MegabytesPerSecond, NumMilliseconds = results.NumMilliseconds };
        }
    }
}
