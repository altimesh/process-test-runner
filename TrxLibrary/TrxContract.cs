using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Altimesh.TestRunner.TrxLibrary
{
    [Serializable]
    [XmlRoot(Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public class TestRun
    {
        public TestRun() { }

        public TestRun(string name, string runUser)
        {
            this.id = Guid.NewGuid().ToString();
            this.name = name;
            this.runUser = runUser;
            TestSettings = new TestSettings();
            Times = new Times();
            ResultSummary = new ResultSummary();
            Results = new List<UnitTestResult>();
            TestDefinitions = new List<UnitTest>();
            TestEntries = new List<TestEntry>();

            TestLists = new TestList[2];
            TestLists[0] = new TestList { id = "8c84fa94-04c1-424b-9868-57a2d4851a1d", name = "Results Not in a List" };
            TestLists[1] = new TestList { id = "19431567-8539-422a-85d7-44ee4e166bda", name = "All Loaded Results" };
        }

        [XmlAttribute]
        public string id { get; set; }

        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string runUser { get; set; }

        public TestSettings TestSettings { get; set; }

        public Times Times { get; set; }
        
        public ResultSummary ResultSummary { get; set; }
        
        [XmlArray]
        public List<UnitTestResult> Results;

        [XmlArray]
        public List<UnitTest> TestDefinitions;

        [XmlArray]
        public List<TestEntry> TestEntries;

        [XmlArray]
        public TestList[] TestLists { get; set; }
    }

    [Serializable]
    public class TestSettings
    {
        public TestSettings()
        {
            name = "Default Test Settings";
            id = "69ac05d6-3842-4155-93d8-999be006f23e";
            Deployment = new Deployment();
            Execution = new SettingsExecution();
        }

        [XmlAttribute]
        public string name { get; set; }
        [XmlAttribute]
        public string id { get; set; }

        public SettingsExecution Execution { get; set; }

        public Deployment Deployment { get; set; }
    }

    [Serializable]
    public class SettingsExecution
    {
        public SettingsExecution()
        {
            TestTypeSpecific = new TestTypeSpecific();
            AgentRule = new AgentRule();
        }

        public TestTypeSpecific TestTypeSpecific { get; set; }
        public AgentRule AgentRule { get; set; }
    }

    [Serializable]
    public class TestTypeSpecific
    {
        public TestTypeSpecific() { }
    }

    [Serializable]
    public class AgentRule
    {
        public AgentRule() { name = "Execution Agents"; }
        [XmlAttribute]
        public string name { get; set; }
    }

    [Serializable]
    public class Deployment
    {
        public Deployment() { runDeploymentRoot = "."; }
        [XmlAttribute]
        public string runDeploymentRoot { get; set; }
    }

    [Serializable]
    public class Times
    {
        public Times() { }

        [XmlAttribute]
        public string creation { get; set; }
        [XmlAttribute]
        public string queuing { get; set; }
        [XmlAttribute]
        public string start { get; set; }
        [XmlAttribute]
        public string finish { get; set; }
    }

    [Serializable]
    public class ResultSummary
    {
        public ResultSummary() { Counters = new Counters(); }

        [XmlAttribute]
        public string outcome { get; set; }

        public Counters Counters { get; set; }
    }

    [Serializable]
    public class Counters
    {
        public Counters()
        {
            executed = 0; passed = 0; error = 0; failed = 0; timeout = 0; aborted = 0; inconclusive = 0; passedButRunAborted = 0;
            notRunnable = 0; notExecuted = 0; disconnected = 0; warning = 0; completed = 0; inProgress = 0; pending = 0;
        }

        private int _total = -1;
        [XmlAttribute]
        public int total { get { if (_total == -1) { return executed + notExecuted; } else { return _total; } } set { _total = value; } }
        [XmlAttribute]
        public int passed { get; set; }
        [XmlAttribute]
        public int error { get; set; }
        [XmlAttribute]
        public int executed { get; set; }
        [XmlAttribute]
        public int failed { get; set; }
        [XmlAttribute]
        public int timeout { get; set; }
        [XmlAttribute]
        public int aborted { get; set; }
        [XmlAttribute]
        public int inconclusive { get; set; }
        [XmlAttribute]
        public int passedButRunAborted { get; set; }
        [XmlAttribute]
        public int notRunnable { get; set; }
        [XmlAttribute]
        public int notExecuted { get; set; }
        [XmlAttribute]
        public int disconnected { get; set; }
        [XmlAttribute]
        public int warning { get; set; }
        [XmlAttribute]
        public int completed { get; set; }
        [XmlAttribute]
        public int inProgress { get; set; }
        [XmlAttribute]
        public int pending { get; set; }
    }

    [Serializable]
    public class UnitTest
    {
        public UnitTest()
        {
            Execution = new UnitTestExecution();
            TestMethod = new TestMethod();
        }

        [XmlAttribute]
        public string name { get; set; }
        [XmlAttribute]
        public string storage { get; set; }
        [XmlAttribute]
        public string id { get; set; }

        public UnitTestExecution Execution { get; set; }

        public TestMethod TestMethod { get; set; }
    }

    [Serializable]
    public class UnitTestExecution
    {
        public UnitTestExecution() { }
        [XmlAttribute]
        public string id { get; set; }
    }

    [Serializable]
    public class TestMethod
    {
        public TestMethod()
        {
            // we don't care
            adapterTypeName = "Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
        }

        [XmlAttribute]
        public string codeBase { get; set; }
        [XmlAttribute]
        public string adapterTypeName { get; set; }
        [XmlAttribute]
        public string className { get; set; }
        [XmlAttribute]
        public string name { get; set; }
    }

    [Serializable]
    public class TestList
    {
        public TestList() { }

        [XmlAttribute]
        public string name { get; set; }
        [XmlAttribute]
        public string id { get; set; }
    }

    [Serializable]
    public class TestEntry
    {
        public TestEntry() { }

        [XmlAttribute]
        public string testId { get; set; }
        [XmlAttribute]
        public string executionId { get; set; }
        [XmlAttribute]
        public string testListId { get; set; }
    }

    [Serializable]
    public class UnitTestResult
    {
        public UnitTestResult()
        {
            testType = "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b"; // some guid
        }

        [XmlAttribute]
        public string executionId { get; set; }
        [XmlAttribute]
        public string testId { get; set; }
        [XmlAttribute]
        public string testListId { get; set; }
        [XmlAttribute]
        public string relativeResultsDirectory { get; set; }
        [XmlAttribute]
        public string testName { get; set; }
        [XmlAttribute]
        public string computerName { get; set; }
        [XmlAttribute]
        public string duration { get; set; }
        [XmlAttribute]
        public string startTime { get; set; }
        [XmlAttribute]
        public string endTime { get; set; }
        [XmlAttribute]
        public string testType { get; set; }
        [XmlAttribute]
        public string outcome { get; set; }

        public Output Output { get; set; }
    }

    [Serializable]
    public class Output
    {
        public Output() { }

        public ErrorInfo ErrorInfo { get; set; }
    }

    [Serializable]
    public class ErrorInfo
    {
        public ErrorInfo() { }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}