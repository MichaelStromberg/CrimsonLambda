using System;
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
            long numBytes = 0;
            (string MegabytesPerSecond, double NumMilliseconds) results = ("", 0.0);

            try
            {
                var urls = new List<RemoteFile>
                {
                    new RemoteFile("http://illumina-annotation.s3.amazonaws.com/SA/44/GRCh37/chr11.nsa"),
                    new RemoteFile("http://illumina-annotation.s3.amazonaws.com/SA/44/GRCh37/chr12.nsa"),
                    new RemoteFile("http://illumina-annotation.s3.amazonaws.com/SA/44/GRCh37/chr13.nsa"),
                    new RemoteFile("http://illumina-annotation.s3.amazonaws.com/SA/44/GRCh37/chr14.nsa"),
                    new RemoteFile("http://illumina-annotation.s3.amazonaws.com/SA/44/GRCh37/chr15.nsa"),
                    new RemoteFile("http://illumina-annotation.s3.amazonaws.com/SA/44/GRCh37/chr16.nsa"),
                    new RemoteFile("http://illumina-annotation.s3.amazonaws.com/SA/44/GRCh37/chr17.nsa")
                };

                var benchmark = new Benchmark();

                urls.Execute(context.Logger, "downloads", file => file.Download(context.Logger), config.NumThreads);
                
                foreach (RemoteFile url in urls) numBytes += url.NumBytes;

                results = benchmark.GetMegabytesPerSecond(numBytes);
                context.Logger.LogLine(
                    $"speed with {config.NumThreads} threads: {results.MegabytesPerSecond}, total ms: {results.NumMilliseconds}");
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"EXCEPTION: {e.Message}");
                while (e.InnerException != null) e = e.InnerException;
                context.Logger.LogLine($"Inner exception: {e.Message}");
                context.Logger.LogLine(e.StackTrace);
            }


            return new LambdaResult { NumBytes = numBytes, MegabytesPerSecond = results.MegabytesPerSecond, NumMilliseconds = results.NumMilliseconds };
        }
    }
}
