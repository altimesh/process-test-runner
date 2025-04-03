using Altimesh.TestRunner.Library;
using Altimesh.TestRunner.TrxLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Altimesh.TrxDifference
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                return Usage();
            }
            if(!File.Exists(args[0]))
            {
                Console.WriteLine("Error: input file not found: " + args[0]);
                return 1;
            }
            if (!File.Exists(args[1]))
            {
                Console.WriteLine("Error: input file not found: " + args[1]);
                return 1;
            }

            FileStream fs0 = new FileStream(args[0], FileMode.Open);
            XmlReader reader0 = XmlReader.Create(fs0);
            XmlSerializer serializer0 = new XmlSerializer(typeof(TestRun));
            TestRun left = (TestRun)serializer0.Deserialize(reader0);


            FileStream fs1 = new FileStream(args[1], FileMode.Open);
            XmlReader reader1 = XmlReader.Create(fs1);
            XmlSerializer serializer1 = new XmlSerializer(typeof(TestRun));
            TestRun right = (TestRun)serializer1.Deserialize(reader1);

            List<string> output = new List<string>();
            foreach (UnitTestResult leftResult in left.Results)
            {
                string testname = leftResult.testName;
                string referenceOutcome = leftResult.outcome;
                foreach (UnitTestResult rightResult in right.Results)
                {
                    if (rightResult.testName == testname)
                    {
                        if (String.Compare(referenceOutcome, rightResult.outcome, StringComparison.InvariantCultureIgnoreCase) != 0)
                        {
                            output.Add(String.Format("[{0}] : reference: {1} != {2} other", testname, referenceOutcome, rightResult.outcome));
                        }
                        break;
                    }
                }
            }

            if (args.Length >= 3)
            {
                File.WriteAllLines(args[2], output);
            }
            else
            {
                FileInfo referenceInfo = new FileInfo(args[0]);
                FileInfo otherInfo = new FileInfo(args[1]);
                File.WriteAllLines(Path.GetFileNameWithoutExtension(args[0]) + "_" + Path.GetFileNameWithoutExtension(args[1]) + ".diff", output);
            }


            return 0;
        }


        static int Usage()
        {
            Console.WriteLine("trxpath1 trxpath2 <outfile>");
            return 1;
        }
    }
}
