﻿using System;
using System.Collections.Generic;
using System.IO;

namespace CrimsonLauncher
{
    public static class CrimsonLauncherMain
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("USAGE: {0} <num lambdas>", Path.GetFileName(Environment.GetCommandLineArgs()[0]));
                Environment.Exit(1);
            }

            const string jsonConfiguration = "{\"NumThreads\":1}";
            int numLambdas = Convert.ToInt32(args[0]);

            var lambdas = new List<Lambda>(numLambdas);
            for (var lambdaId = 1; lambdaId <= numLambdas; lambdaId++) lambdas.Add(new Lambda(lambdaId));

            var benchmark = new Benchmark();
            lambdas.Execute("downloads", lambda => lambda.Execute(jsonConfiguration));

            long numBytes = 0;
            foreach (Lambda lambda in lambdas) numBytes += lambda.Result.NumBytes;

            (string MegabytesPerSecond, double NumMilliseconds) results = benchmark.GetMegabytesPerSecond(numBytes);
            Console.WriteLine($"speed with {numLambdas} lambdas: {results.MegabytesPerSecond}, total ms: {results.NumMilliseconds}");
        }
    }
}