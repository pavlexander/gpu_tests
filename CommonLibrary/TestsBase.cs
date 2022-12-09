using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public abstract class TestsBase : ITestsBase
    {
        public bool[] results;
        public byte[] resultsBytes;
        public double[] calculatables;
        protected double calculatable = 0;

        private static bool[] baselineResults;
        private static double baselineCalculatable;

        private static bool _isPreviouslyProcessed = false;
        private string _testName;
        private bool _isByteResults;

        public TestsBase(string name, bool isByteResults)
        {
            this._testName = name;
            this._isByteResults = isByteResults;
            results = new bool[DataGenerator.InputCount];
            resultsBytes = new byte[DataGenerator.InputCount];
            calculatables = new double[DataGenerator.InputCount];
        }

        public abstract void Init();

        public abstract void Dispose();

        public void RunMainProccessor()
        {
            Init();

            Console.WriteLine();
            Console.WriteLine($"{DateTime.Now}: Starting '{this._testName}' test.");
            var watch = System.Diagnostics.Stopwatch.StartNew();

            Proccess();

            watch.Stop();
            double elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"{DateTime.Now}: Elapsed time: {elapsedMs} ms");

            NormalizeResults();
            SetBaselineResults();
            VerifyResults();

            Dispose();
        }

        public abstract void Proccess();

        private void NormalizeResults()
        {
            if (_isByteResults)
            {
                for (int i = 0; i < results.Length; i++)
                {
                    results[i] = resultsBytes[i] == 1;
                }
            }
        }

        private void SetBaselineResults()
        {
            calculatable = calculatables.Sum();

            if (!_isPreviouslyProcessed)
            {
                baselineResults = new bool[results.Length];
                Array.Copy(results, baselineResults, results.Length);

                baselineCalculatable = calculatable;

                _isPreviouslyProcessed = true;
                Console.WriteLine($"Baseline calculatable is: {calculatable}");
            }
        }

        private bool VerifyResults()
        {
            Console.Write($"Verifying '{this._testName}' results. ");

            if (results == null || baselineResults == null)
            {
                Console2.WriteLineRed($"Null array!");
                return false;
            }

            if (results.Length != baselineResults.Length)
            {
                Console2.WriteLineRed($"Lengths are not the same!");
                return false;
            }
            
            for (int i = 0; i < baselineResults.Length; i++)
            {
                if (results[i] != baselineResults[i])
                {
                    Console2.WriteLineRed($"Values are not the same!");
                    return false;
                }
            }

            if (calculatable == 0 || baselineCalculatable != calculatable)
            {
                Console2.WriteLineRed($"Calculatable value is not correct!");
                return false;
            }

            Console.WriteLine($"OK.");
            return true;
        }
    }
}
