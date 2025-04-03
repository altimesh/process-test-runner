using Altimesh.TestRunner.Library;
using System.Collections.Generic;
using System.Linq;

namespace Altimesh.TestRunner.Console
{
    class Program
    {
        static void Main(params string[] args)
        {
            Dictionary<string, string> arguments = null;
            if (args.Length == 0)
            {
                arguments = Arguments.WaitForArguments();
            }
            else
            {
                arguments = Arguments.ParseArguments(args);
            }
            
            List<string> testlist = new List<string>();
            if (arguments.ContainsKey(Arguments.testList))
            {
                testlist = arguments[Arguments.testList].Split(';', ',', ':').ToList();
            }
            else if (arguments.ContainsKey(Arguments.testListFile))
            {
                testlist = System.IO.File.ReadAllLines(arguments[Arguments.testListFile]).ToList();
            }
            
            if (!arguments.ContainsKey(Arguments.parallel))
            {
                arguments.Add(Arguments.parallel, "false");
            }

            DllRunner.Test(arguments[Arguments.dllName], arguments[Arguments.trxName], testlist, bool.Parse(arguments[Arguments.parallel]));
        }
    }
}
