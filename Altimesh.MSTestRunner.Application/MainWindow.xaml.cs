using Altimesh.TestRunner.Library;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Altimesh.MSTestRunner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel VM;
        public MainWindow()
        {
            InitializeComponent();
            this.VM = new MainViewModel();
            this.DataContext = this.VM;
            this.TestList.ItemsSource = this.VM.Tests;
        }

        private void LoadDLLOnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.CheckFileExists = true;
            o.CheckPathExists = true;
            o.Filter = "Dll files (*.dll)|*.dll";
            o.Title = "Select the test dll";
            o.AutoUpgradeEnabled = true;
            var result = o.ShowDialog();
            string fileName = null;
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    fileName = o.FileName;
                    break;
                default:
                    break;
            }

            this.VM.dllName = o.FileName;

            Dictionary<Type, List<MethodInfo>> alltests = DllImporter.GetAllMethod(this.VM.dllName);
            this.VM.Tests.Clear();
            foreach(var kvpType in alltests) {
                foreach (MethodInfo mi in alltests[kvpType.Key])
                {
                    int timeout = Timeout(mi);
                    this.VM.Tests.Add(new TestViewModel { className = kvpType.Key.Name, result = "unknown", testName = mi.Name, timeout = timeout });
                }
            }
        }

        private static int Timeout(MethodInfo mi)
        {
            bool has = HasAttribute(mi, "TimeoutAttribute");
            if (has)
            {
                dynamic attribute = mi.GetCustomAttributes(true).First((attr) => attr.GetType().Name == "TimeoutAttribute");
                return attribute.Timeout;
            }

            return -1;
        }

        private static bool HasAttribute(MethodInfo mi, string attrName)
        {
            return mi.GetCustomAttributes(true).FirstOrDefault((attr) => attr.GetType().Name == attrName) != null;
        }

        private void OnRunTests(object sender, RoutedEventArgs e)
        {
            if (this.VM.Tests.Count > 0)
            {
                foreach (TestViewModel test in this.VM.Tests)
                {
                    test.result = "unknown";
                    TestOutcome outcome = DllImporter.RunTest(this.VM.dllName, test.className, test.testName, test.timeout);
                    switch(outcome)
                    {
                        case TestOutcome.Passed:
                            test.result = "passed";
                            break;
                        case TestOutcome.Inconclusive:
                            test.result = "inconclusive";
                            break;
                        case TestOutcome.Failed:
                            test.result = "failed";
                            break;
                    }
                }
            }
        }

        private void RunSingleTest(object sender, RoutedEventArgs r)
        {
            System.Windows.Controls.Button button = (r.OriginalSource as System.Windows.Controls.Button);
            TestViewModel test = button.DataContext as TestViewModel;
            test.result = "unknown";
            TestOutcome outcome = DllImporter.RunTest(this.VM.dllName, test.className, test.testName, test.timeout);
            switch (outcome)
            {
                case TestOutcome.Passed:
                    test.result = "passed";
                    break;
                case TestOutcome.Inconclusive:
                    test.result = "inconclusive";
                    break;
                case TestOutcome.Failed:
                    test.result = "failed";
                    break;
            }
        }
    }

    class MainViewModel: ObservableObject
    {
        public MainViewModel()
        {
            Tests = new ObservableCollection<TestViewModel>();
        }
        private string _dllName;
        public string dllName
        {
            get { return _dllName; }
            set
            {
                _dllName = value;
                RaisePropertyChangedEvent("dllName");
            }
        }

        public ObservableCollection<TestViewModel> Tests;
    }


    class TestViewModel : ObservableObject
    {
        private string _testName;
        private string _className;
        private string _result;
        private string _color;
        private int _timeout;

        public string className
        {
            get { return _className; }
            set
            {
                _className = value;
                RaisePropertyChangedEvent("className");
            }
        }

        public int timeout
        {
            get { return _timeout; }
            set
            {
                _timeout = value;
            }
        }

        public string testName
        {
            get { return _testName; }
            set
            {
                _testName = value;
                RaisePropertyChangedEvent("testName");
            }
        }

        public string result
        {
            get { return _result; }
            set
            {
                _result = value;
                RaisePropertyChangedEvent("result");
                RaisePropertyChangedEvent("color");
            }
        }

        public string color
        {
            get
            {
                if (result == "passed")
                    return "green";
                else if (result == "inconclusive")
                    return "yellow";
                else if (result == "failed")
                    return "red";
                else
                    return "blue";
            }
        }
    }

    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DelegateCommand : ICommand
    {
        private readonly Action _action;

        public DelegateCommand(Action action)
        {
            _action = action;
        }

        public void Execute(object parameter)
        {
            _action();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        #pragma warning disable 67
        public event EventHandler CanExecuteChanged;
        #pragma warning restore 67
    }
}
