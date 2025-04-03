using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Altimesh.TestRunner.Library;

namespace Altimesh.TestAdapter
{
    /// <summary>
    /// see https://blogs.msdn.microsoft.com/bhuvaneshwari/2012/03/13/authoring-a-new-visual-studio-unit-test-adapter/
    /// </summary>
    public class TestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            List<TestDescriptor> allTests = new List<TestDescriptor>();
            int testCount = 0;
            foreach (string source in sources) {
                allTests.AddRange(DllLoader.GetAllMethod(source, null, ref testCount));
            }
        }
    }
}
