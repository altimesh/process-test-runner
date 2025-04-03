using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Altimesh.TestAdapter
{
    /// <summary>
    /// see https://blogs.msdn.microsoft.com/bhuvaneshwari/2012/03/13/authoring-a-new-visual-studio-unit-test-adapter/
    /// </summary>
    public class TestExecutor : ITestExecutor
    {
        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            throw new NotImplementedException();
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            throw new NotImplementedException();
        }
    }
}
