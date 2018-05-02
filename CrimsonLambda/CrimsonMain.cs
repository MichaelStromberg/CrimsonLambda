using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Compression.FileHandling;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace CrimsonLambda
{
    public class CrimsonMain
    {
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _bucketName;
        private readonly RegionEndpoint _bucketRegion;

        public CrimsonMain()
        {
            // deployment-specific details that should not be committed to version control
            _accessKey          = Environment.GetEnvironmentVariable("AccessKey");
            _secretKey          = Environment.GetEnvironmentVariable("SecretKey");
            _bucketName         = Environment.GetEnvironmentVariable("BucketName");
            string bucketRegion = Environment.GetEnvironmentVariable("BucketRegion");
            _bucketRegion       = RegionEndpoint.GetBySystemName(bucketRegion);
        }

        public LambdaResult FunctionHandler(LambdaConfiguration config, ILambdaContext context)
        {
            return new LambdaResult { NumVariants = GetNumVariantsAsync(config.VcfPath, context.Logger).Result };
        }

        private async Task<int> GetNumVariantsAsync(string vcfPath, ILambdaLogger logger)
        {
            var numVariants = 0;

            var request = WebRequest.Create("http://illumina-annotation.s3.amazonaws.com/Test/Mother.vcf.gz");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //using (var peekStream = new PeekStream(response.GetResponseStream(), 10_485_760))
            using (var gzipStream = new BlockGZipStream(response.GetResponseStream(), CompressionMode.Decompress))
            using (var reader = new StreamReader(gzipStream, Encoding.Default, true, 10_485_760))
            {
                while (true)
                {
                    string line = await reader.ReadLineAsync();
                    if (line == null) break;
                    numVariants++;
                }
            }
            //using (var client = new AmazonS3Client(_accessKey, _secretKey, _bucketRegion))
            //{
            //    GetObjectResponse response = await client.GetObjectAsync(_bucketName, vcfPath);

            //    //using (var peekStream = new PeekStream(response.ResponseStream, 10_485_760))
            //    using (var gzipStream = new BlockGZipStream(response.ResponseStream, CompressionMode.Decompress))
            //    using (var reader     = new StreamReader(gzipStream, Encoding.Default, true, 5_242_880))
            //    {
            //        while (true)
            //        {
            //            string line = await reader.ReadLineAsync();
            //            if (line == null) break;
            //            numVariants++;
            //        }
            //    }
            //}
            //}
            //catch (Exception)
            //{
            //}

            return numVariants;
        }
    }
}
