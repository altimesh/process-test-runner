using Altimesh.TestRunner.TrxLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TestAttributes;

namespace Altimesh.TestRunner.Library
{
    public class DllRunner
    {
        public static unsafe void Test(string dllPath, string trxPath, List<string> testlist, bool parallel)
        {
            DateTime globalStart = DateTime.Now;
            Assembly loaded;
            try
            {
                loaded = Assembly.LoadFrom(dllPath);
            }
            catch (Exception)
            {
                Console.WriteLine("could not load assembly: " + dllPath);
                return;
            }

			Type setupType = loaded.GetTypes().Where((t) => String.IsNullOrEmpty(t.Namespace) && Utils.HasAttribute(t, "SetUpFixtureAttribute")).FirstOrDefault();
            if (setupType != null)
            {
                MethodInfo assemblyInit = setupType.GetMethods().Where((mi) => Utils.HasAttribute(mi, "SetUpAttribute")).FirstOrDefault();
                assemblyInit.Invoke(null, new object[0]);
            }

            int testCount = 0;
            List<TestDescriptor> allDescriptorsList = DllLoader.GetAllMethod(dllPath, testlist, ref testCount);

            TestDescriptor[] allDescriptors = allDescriptorsList.ToArray();

            TestRunFactory runFactory = new TestRunFactory();
            int v_passed = 0, v_failed = 0, v_inconclusive = 0, v_index = 0;
            int* passed = &v_passed;
            int* failed = &v_failed;
            int* inconclusive = &v_inconclusive;
            int* index = &v_index;

            int threadCount = parallel ?  Environment.ProcessorCount : 1;

            TestDescriptor[] SingleProcessTests = allDescriptors.Where((t) => t.method.GetCustomAttributes().Where((att) => att.GetType().Name == "SingleProcessAttribute").Any()).OrderBy((t) => t.method.Name).ToArray();
            TestDescriptor[] MultiProcessTests = allDescriptors.Where((t) => !t.method.GetCustomAttributes().Where((att) => att.GetType().Name == "SingleProcessAttribute").Any()).OrderBy((t) => t.method.Name).ToArray();

            int slice = MultiProcessTests.Length / threadCount;
            // parallel region
            Parallel.For(0, threadCount, (i) =>
            {
                int start = i * slice;
                for (int k = start; k < start + slice; ++k)
                {
                    RunOneTest(dllPath, testCount, runFactory, passed, failed, inconclusive, index, MultiProcessTests[k]);
                }
            });

            // tail
            for (int k = threadCount * slice; k < MultiProcessTests.Length; ++k)
            {
                RunOneTest(dllPath, testCount, runFactory, passed, failed, inconclusive, index, MultiProcessTests[k]);
            }

            // single process tests
            if(SingleProcessTests.Length > 0) 
            {
                Console.WriteLine("Done with multi process tests - starting single (openmp) tests");
            }

            for (int k = 0; k < SingleProcessTests.Length; ++k)
            {
                RunOneTest(dllPath, testCount, runFactory, passed, failed, inconclusive, index, SingleProcessTests[k]);
            }

            DateTime globalStop = DateTime.Now;
            runFactory.currentRun.Times.creation = ReportGenerator.DateToString(globalStart);
            runFactory.currentRun.Times.finish = ReportGenerator.DateToString(globalStop);
            runFactory.currentRun.Times.queuing = ReportGenerator.DateToString(globalStart);
            runFactory.currentRun.Times.start = ReportGenerator.DateToString(globalStart);
            ReportGenerator generator = new ReportGenerator();
            generator.WriteToFile(trxPath, runFactory.currentRun, dllPath);
            Console.WriteLine("{0} : Report written to {1}", ((*failed > 0 || *inconclusive > 0) ? "FAILED" : "PASSED"), trxPath);
            Console.WriteLine("Total: {0} Failed: {1} Passed: {2} Inconclusive: {3}", (*failed + *passed + *inconclusive), *failed, *passed, *inconclusive);
        }

        private static object o_sync = new object();

        private static unsafe void RunOneTest(string dllPath, int testCount, TestRunFactory runFactory, int* passed, int* failed, int* inconclusive, int* index, TestDescriptor desc)
        {
            int seqLength = Utils.GetSequenceLength(desc.method);
            
            if (seqLength != TestDescriptor.DefaultSeqLength)
            {
                for (int i = 0; i < seqLength; ++i)
                {
                    RunOneSequenceValue(dllPath, testCount, runFactory, passed, failed, inconclusive, index, desc, i);
                }
            }
            else
            {
                RunOneSequenceValue(dllPath, testCount, runFactory, passed, failed, inconclusive, index, desc, -1);
            }
        }
        
        private static unsafe void RunOneSequenceValue(string dllPath, int testCount, TestRunFactory runFactory, int* passed, int* failed, int* inconclusive, int* index, TestDescriptor desc, int seqIndex)
        {
            string displayName = Utils.GetDisplayName(desc.method, seqIndex);

            lock (o_sync)
            {
                Console.Out.WriteLine("RUNNING {0}.{1}: {2}/{3}", desc.DeclaringType.Name, displayName, (*index), testCount);
                *index = *index + 1;
            }

            Test test = new Test(desc.method);

            int timeout = Utils.Timeout(desc.method);
            TestOutcome outcome = TestOutcome.Inconclusive;
            try
            {
                outcome = RunTest(test, desc, runFactory, dllPath, timeout, seqIndex);
            }
            catch { }
            switch (outcome)
            {
                case TestOutcome.Passed:
                    lock (o_sync)
                    {
                        *passed = *passed + 1;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("[PASSED]      :" + displayName);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    break;
                case TestOutcome.Failed:
                    lock (o_sync)
                    {
                        *failed = *failed + 1;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[FAILED]      :" + displayName);
                        Console.WriteLine(test.Message);
                        Console.WriteLine(test.StackTrace);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    break;
                case TestOutcome.Inconclusive:
                    lock (o_sync)
                    {
                        *inconclusive = *inconclusive + 1;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[INCONCLUSIVE]:" + displayName);
                        Console.WriteLine(test.Message);
                        Console.WriteLine(test.StackTrace);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    break;
                default:
                    lock (o_sync)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("[UNKNOWN]     :" + displayName);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    break;
            }
        }

        private static TestOutcome RunTest(Test test, TestDescriptor desc, TestRunFactory runFactory, string dllPath, int timeout, int seqIndex)
        {
            if (Utils.IsIgnored(desc.method))
            {
                test.outcome = TestOutcome.Inconclusive;
                test.StackTrace = "empty stack trace";
                string reason = Utils.GetIgnoreReason(desc.method);
                test.Message = "[IGNORED]" + (String.IsNullOrEmpty(reason) ? "" : (": " + reason));
                runFactory.AddTest(test, seqIndex);
                return TestOutcome.Inconclusive;
            }
            if (Utils.HasAttribute(desc.method, "DeploymentItemAttribute"))
            {
                test.outcome = TestOutcome.Inconclusive;
                test.Message = "deployment item is not yet supported";
                test.StackTrace = "empty stacktrace";
                runFactory.AddTest(test, seqIndex);
                return TestOutcome.Inconclusive;
            }

            string outputFileName = Path.GetTempFileName();

            test.start = DateTime.Now;
            RunTestInSeparateProcess(dllPath, desc, outputFileName, timeout, seqIndex);
            test.stop = DateTime.Now;
            test.outcome = TestOutcome.Passed;

            string[] output = File.ReadAllLines(outputFileName).ToArray();
            if (output.Length == 0)
            {
                test.outcome = TestOutcome.Failed;
                test.Message = "Something wrong happened in the test runner";
                runFactory.AddTest(test, seqIndex);
                return TestOutcome.Failed;
            }
            string outcome, message, stacktrace;
            ReadOutput(output, out outcome, out message, out stacktrace);

            if (outcome != "passed")
            {
                if (outcome == "inconclusive")
                {
                    test.outcome = TestOutcome.Inconclusive;
                }
                else if (outcome == "failed")
                {
                    test.outcome = TestOutcome.Failed;
                }
                test.Message = message;
                test.StackTrace = stacktrace;
            }

            runFactory.AddTest(test, seqIndex);
            File.Delete(outputFileName); // ensure no memory leak on disk
            return test.outcome;
        }

        public static void ReadOutput(string[] raw, out string outcome, out string message, out string stacktrace)
        {
            // TODO: messages should be serialized objects
            const string separator = "¤";
            outcome = raw[0].ToLowerInvariant();
            int i = 2; // skip first ¤
            message = "";
            stacktrace = "";
            for (i = 2; i < raw.Length; ++i)
            {
                stacktrace += raw[i];
                if (raw[i] == separator)
                {
                    break;
                }
            }
            ++i;
            for (; i < raw.Length; ++i)
            {
                message += raw[i];
                if (raw[i] == separator)
                {
                    break;
                }
            }

            message.Replace(separator, "");
        }

        public static void RunTestInSeparateProcess(string dllPath, TestDescriptor desc, string tmpFile, int timeout, int seqIndex)
        {
            FileInfo dll = new FileInfo(dllPath);
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.WorkingDirectory = dll.DirectoryName;
            p.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Altimesh.TestRunner.Process.exe");
            p.StartInfo.Arguments = "\"" + dll.FullName + "\" \"" + desc.DeclaringType.FullName + "\" \"" + desc.method.Name + "\" \"" + tmpFile + "\" " + seqIndex;
            //Console.WriteLine("starting satellite with args: " + p.StartInfo.Arguments);


            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
            using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
            {
                p.OutputDataReceived += (sender, e) =>
                {
                    try
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            //Console.Out.WriteLine(e.Data);
                        }
                    }
                    catch { }
                };
                p.ErrorDataReceived += (sender, e) =>
                {
                    try
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            //Console.Error.WriteLine(e.Data);
                        }
                    }
                    catch { }
                };

                p.Start();

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                // TODO: specific handling for timeouts. 
                if (timeout > 0)
                {
                   bool exited = p.WaitForExit(timeout);
                   if (!exited)
                   {
                       try
                       {
                           errorWaitHandle.Set();
                           outputWaitHandle.Set();
                       }
                       catch { }
                       try
                       {
                           p.Kill();
                       }
                       catch { }

                       List<string> output = new List<string>();

                       output.Add("Failed");
                       output.Add("¤");
                       output.Add("empty stack trace");
                       output.Add("¤");
                       output.Add("Timeout after " + timeout + " milliseconds");
                       File.WriteAllLines(tmpFile, output);
                   }
                }
                else
                {
                    p.WaitForExit();
                }

                outputWaitHandle.WaitOne();
                errorWaitHandle.WaitOne();

                if (p.ExitCode == 6) // aborted process
                {
                    List<string> output = new List<string>();

                    output.Add("Failed");
                    output.Add("¤");
                    output.Add("empty stack trace");
                    output.Add("¤");
                    output.Add("Aborted");
                    File.WriteAllLines(tmpFile, output);
                }
                else if (p.ExitCode == 0) 
                { 
                    // when exit (0) has been called from the test
                    if (!File.Exists(tmpFile) || new FileInfo(tmpFile).Length == 0)
                    {
                        File.WriteAllText(tmpFile, "Passed" + Environment.NewLine);
                    }
                }
            }
        }
    }
}
