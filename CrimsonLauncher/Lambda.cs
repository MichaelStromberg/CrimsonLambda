using System;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Lambda.Serialization.Json;
using Amazon.Runtime;

namespace CrimsonLauncher
{
    public sealed class Lambda
    {
        public readonly int Id;
        public LambdaResult Result;

        private readonly JsonSerializer _serializer = new JsonSerializer();

        public Lambda(int id) => Id = id;

        public void Execute(string jsonConfiguration)
        {
            Console.WriteLine($"- execute: {Id}");
            var client = new AmazonLambdaClient(new StoredProfileAWSCredentials("Michael"), RegionEndpoint.USEast1);

            InvokeResponse response = client.InvokeAsync(new InvokeRequest
            {
                FunctionName   = "MSCrimsonLambda",
                Payload        = jsonConfiguration,
                InvocationType = "RequestResponse"
            }).Result;

            Console.WriteLine($"Response {Id} status code: {response.StatusCode}");
            Result = _serializer.Deserialize<LambdaResult>(response.Payload);

            Console.Out.Flush();
            Console.WriteLine($"- finished: {Id}. {Result.NumBytes:N0}, {Result.MegabytesPerSecond}, {Result.NumMilliseconds:N1}");
        }
    }
}