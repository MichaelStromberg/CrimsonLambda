using System;
using System.Collections.Generic;
using System.IO;
using Amazon;

namespace CrimsonLauncher
{
    public static class CrimsonLauncherMain
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("USAGE: {0} <region> <num lambdas>", Path.GetFileName(Environment.GetCommandLineArgs()[0]));
                Environment.Exit(1);
            }

            const string jsonConfiguration = "{\"NumThreads\":1}";
            RegionEndpoint regionEndpoint = RegionEndpoint.GetBySystemName(args[0]);
            var numLambdas = Convert.ToInt32(args[1]);

            if (regionEndpoint.DisplayName == "Unknown")
            {
                Console.WriteLine($"ERROR: An unknown region name was provided ({regionEndpoint.SystemName}). Please specify something like: us-east-1");
                Environment.Exit(1);
            }
            
            try
            {
                var benchmark = new Benchmark();

                var lambdas = new List<Lambda>(numLambdas);
                for (var lambdaId = 1; lambdaId <= numLambdas; lambdaId++) lambdas.Add(new Lambda(lambdaId));
                
                lambdas.Execute("downloads", lambda => lambda.Execute(jsonConfiguration, regionEndpoint));

                long numBytes = 0;
                foreach (Lambda lambda in lambdas) numBytes += lambda.Result.NumBytes;

                (string MegabytesPerSecond, double NumMilliseconds) results = benchmark.GetMegabytesPerSecond(numBytes);
                Console.WriteLine($"speed with {numLambdas} lambdas: {results.MegabytesPerSecond}, total ms: {results.NumMilliseconds}");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\nERROR: ");
                Console.ResetColor();

                while (e.InnerException != null) e = e.InnerException;
                Console.WriteLine("{0}", e.Message);

                // print the stack trace
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nStack trace:");
                Console.ResetColor();
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}