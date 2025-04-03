using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;

namespace Altimesh.TestRunner.TrxLibrary
{
    public class TestRunFactory
    {
        public TestRun currentRun { get; private set; }
        string testListId;

        public TestRunFactory()
        {
            string user = WindowsIdentity.GetCurrent().Name;

            string name = Environment.UserName + "@" + Environment.MachineName + " " + DateTime.UtcNow.ToString();
            currentRun = new TestRun(name, user);
            testListId = Guid.NewGuid().ToString();
            currentRun.TestLists[0].id = this.testListId;
            currentRun.ResultSummary.outcome = "Completed";
        }

        public static object o_sync = new object();
        public void AddTest(Test test, int seqIndex = -1)
        {
            var result = new UnitTestResult();
            result.startTime = ReportGenerator.DateToString(test.start);
            result.endTime = ReportGenerator.DateToString(test.stop);
            TimeSpan duration = (test.stop - test.start);
            result.duration = duration.ToString("hh\\:mm\\:ss");
            var definition = new UnitTest();
            var entry = new TestEntry();

            lock (o_sync)
            {
                switch (test.outcome)
                {
                    case TestOutcome.Failed:
                        currentRun.ResultSummary.Counters.failed++;
                        result.outcome = "Failed";
                        currentRun.ResultSummary.outcome = "Failed";
                        break;
                    case TestOutcome.Inconclusive:
                        currentRun.ResultSummary.Counters.inconclusive++;
                        result.outcome = "Inconclusive";
                        break;
                    case TestOutcome.Passed:
                        currentRun.ResultSummary.Counters.passed++;
                        result.outcome = "Passed";
                        break;
                    default:
                        throw new NotImplementedException("this type of test outcome is not yet supported");
                }

                if (!String.IsNullOrEmpty(test.Message) || !String.IsNullOrEmpty(test.StackTrace))
                {
                    Output output = new Output();
                    output.ErrorInfo = new ErrorInfo { Message = test.Message, StackTrace = test.StackTrace };
                    result.Output = output;
                }

                currentRun.ResultSummary.Counters.executed++;

                // test definitions
                string displayName = GetDisplayName(test.method, seqIndex);
                definition.name = displayName != null ? displayName : test.methodName;
                definition.id = test.id;
                definition.TestMethod.name = test.methodName;
                definition.TestMethod.className =
                test.declaringTypeName + ", " +
                test.assemblyName;
                definition.TestMethod.codeBase = test.codeBase;
                definition.Execution.id = test.executionId;

                // test entries
                entry.testId = test.id;
                entry.executionId = test.executionId;
                entry.testListId = this.testListId;

                // results
                result.relativeResultsDirectory = test.executionId;
                result.executionId = test.executionId;
                result.testId = test.id;
                result.testListId = this.testListId;
                result.testName = test.methodName;
                result.computerName = Environment.MachineName;


                currentRun.Results.Add(result);
                currentRun.TestDefinitions.Add(definition);
                currentRun.TestEntries.Add(entry);
            }
        }
        
        private static string GetDisplayName(MethodInfo mi, int seqIndex)
        {
            if(mi == null)
            {
                return null;
            }

            object[] seqParams = GetSequentialParameters(mi, seqIndex);
            string displayName = mi.Name;

            // TODO: display better test name for sequences --- (testName(val0.toString(), ...) for example
            if (seqParams.Length > 0)
                displayName += "(" + String.Join(",", seqParams.Select((o) => o == null ? "null" : o.ToString())) + ")";
            return displayName;
        }


        public static object[] GetSequentialParameters(MethodInfo testMethod, int seqIndex)
        {
            List<object> parameters = new List<object>();
            foreach (ParameterInfo pi in testMethod.GetParameters())
            {
                Attribute valatt = pi.GetCustomAttributes().Where((att) => IsValuesAttribute(att.GetType())).FirstOrDefault();
                if (valatt == default(Attribute))
                    parameters.Add(null);
                FieldInfo datafield = valatt.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where((fi) => fi.Name.ToLowerInvariant().Contains("data")).FirstOrDefault();
                if (datafield == null || datafield.FieldType != typeof(object).MakeArrayType())
                    parameters.Add(null);
                object[] data = (object[])datafield.GetValue(valatt);
                if (seqIndex < data.Length)
                    parameters.Add(data[seqIndex]);
                else
                    parameters.Add(null);
            }
            object[] par = parameters.ToArray();
            return par;
        }

        public static bool IsValuesAttribute(Type t)
        {
            Type toto = t;
            while (toto != null && toto.Name.ToLowerInvariant().Contains("valuesattribute"))
            {
                toto = toto.BaseType;
            }

            return toto != null;
        }
    }

    public enum TestOutcome
    {
        Passed,
        Inconclusive,
        Failed
    }

    public class Test
    {
        public Test(MethodInfo method)
        {
            this.method = method;
            this.id = Guid.NewGuid().ToString();
            this.executionId = Guid.NewGuid().ToString();
            this.methodName = method.Name;
            this.declaringTypeName = method.DeclaringType.FullName;
            this.assemblyName = method.DeclaringType.Assembly.GetName().ToString();
            this.codeBase = method.DeclaringType.Assembly.CodeBase.Substring("file:///".Length);
        }

        public Test(string methodName, string declaringTypeName, string assemblyName, string codeBase)
        {
            this.id = Guid.NewGuid().ToString();
            this.executionId = Guid.NewGuid().ToString();
            this.methodName = methodName;
            this.declaringTypeName = declaringTypeName;
            this.assemblyName = assemblyName;
            this.codeBase = codeBase;
        }

        public DateTime start { get; set; }
        public DateTime stop { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public TestOutcome outcome { get; set; }
        public string id { get; private set; }
        public string executionId { get; private set; }
        public string methodName { get; private set; }
        public string declaringTypeName { get; private set; }
        public string assemblyName { get; private set; }
        public string codeBase { get; private set; }
        public MethodInfo method { get; set; }
    }
}
