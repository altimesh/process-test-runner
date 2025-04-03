using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altimesh.TestRunner.Console
{
    internal class Arguments
    {
        public const string prefix = "-";
        public const string dllName = "dllName";
        public const string trxName = "trxName";
        public const string parallel = "parallel";
        public const string testList = "testList";
        public const string testListFile = "testListFile";

        public static Dictionary<string, string> ParseArguments(string[] rawArgs)
        {
            if (rawArgs.Length % 2 != 0)
            {
                Usage(); Environment.Exit(1);
            }

            Dictionary<string, string> result = [];

            for (int i = 0; i < rawArgs.Length; i += 2)
            {
                if (!rawArgs[i].StartsWith(prefix))
                {
                    Usage(); Environment.Exit(1);
                }
                switch (rawArgs[i].Substring(1))
                {
                    case parallel:
                        bool tmp;
                        if (!bool.TryParse(rawArgs[i + 1], out tmp))
                        {
                            Usage(); Environment.Exit(1);
                        }
                        result[parallel] = rawArgs[i + 1];
                        break;
                    case dllName:
                        result[dllName] = rawArgs[i + 1];
                        break;
                    case trxName:
                        result[trxName] = rawArgs[i + 1];
                        break;
                    case testList:
                        result[testList] = rawArgs[i + 1];
                        break;
                    case testListFile:
                        result[testListFile] = rawArgs[i + 1];
                        break;
                    default:
                        Usage(); Environment.Exit(1); break;
                }
            }

            if (result.ContainsKey(testList) && result.ContainsKey(testListFile))
            {
                Usage(); Environment.Exit(1);
            }

            return result;
        }

        public static Dictionary<string, string> WaitForArguments()
        {
            Dictionary<string, string> result = [];
            System.Console.WriteLine("type input dll path (absolute or relative):");
            do
            {
                string path = System.Console.ReadLine();
                if (!File.Exists(path))
                {
                    System.Console.WriteLine("dll does not exist - retry");
                }
                else
                {
                    result.Add(dllName, path);
                    break;
                }
            } while (true);
            System.Console.WriteLine("type output trx path (absolute or relative):");
            do
            {
                string name = System.Console.ReadLine();
                if (String.IsNullOrEmpty(name) || !name.EndsWith(".trx"))
                {
                    System.Console.WriteLine("please enter a valid trx file name: <non empty string>.trx");
                }
                else
                {
                    result.Add(trxName, name);
                    break;
                }
            } while (true);

            System.Console.WriteLine("type test list (semicolon separated test names) [empty]");
            string list = System.Console.ReadLine();
            if (!String.IsNullOrEmpty(list)) // no further validation
            {
                result.Add(testList, list);
                return result;
            }

            System.Console.WriteLine("type test list file path [empty]");
            do
            {
                string testFile = System.Console.ReadLine();
                if (!String.IsNullOrEmpty(testFile) && !File.Exists(testFile))
                {
                    System.Console.WriteLine("file does not exist");
                }
                else if (!String.IsNullOrEmpty(testFile))
                {
                    result.Add(testListFile, testFile);
                    break;
                }
                else
                {
                    break;
                }
            } while (true);
            System.Console.WriteLine("run in parallel [false]?");
            do
            {   
                string entry = System.Console.ReadLine();
                if (String.IsNullOrEmpty(entry))
                {
                    result.Add(parallel, "false");
                    return result;
                }
                bool b;
                if (bool.TryParse(entry, out b))
                {
                    result.Add(parallel, entry);
                    return result;
                }
                else
                {
                    System.Console.WriteLine("invalid boolean, retry");
                }
            } while (true);
        }

        public static void Usage()
        {
            System.Console.WriteLine("process-test-runner -dllName <DLLNAME> -trxName <TRXNAME> [optionalargs]");
            System.Console.WriteLine("[DLLNAME] path to the dll containing tests - relative or absolute");
            System.Console.WriteLine("          if relative the dll and this executable must be in the same directory");
            System.Console.WriteLine("[TRXNAME] path to the dll containing tests - relative or absolute");
            System.Console.WriteLine("          if relative, it will be created in the same directory as the input dll");
            System.Console.WriteLine("[optionalArgs] might be: ");
            System.Console.WriteLine("  -testList <semicolon separated test names>");
            System.Console.WriteLine("  -testListFile <path to a test list file : one test name per line>");
            System.Console.WriteLine("  -parallel [false]: true to run tests in parallel");
        }
    }
}
