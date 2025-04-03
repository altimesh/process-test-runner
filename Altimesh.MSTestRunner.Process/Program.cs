using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TestAttributes;

namespace Altimesh.TestRunner.Process
{
    class Program
    {
        // arguments: dllPath, typeName, methodName, outputFilePath
        static void Main(string[] args)
        {
            List<string> output = new List<string>();

           
            // TODO: check errors
            try
            {
                Assembly loaded = Assembly.LoadFrom(args[0]); // we know it can be loaded

                Type currentType = loaded.GetTypes().Where((type) => type.FullName == args[1]).FirstOrDefault();

                ConstructorInfo defaultConstructor = currentType.GetConstructor(new Type[0]);
                MethodInfo[] allMethods = currentType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                MethodInfo testMethod = allMethods.Where((method) => method.Name == args[2]).FirstOrDefault();
                int seqIndex = int.Parse(args[4]);


                if (Utils.IsTestIgnored(testMethod))
                {
                    output.Add("Inconclusive");
                    output.Add("¤");
                    output.Add("");
                    output.Add("¤");
                    output.Add("");
                }
                else
                {
                    MethodInfo initializer = allMethods.Where((method) => Utils.IsTestInitialize(method)).FirstOrDefault();
                    MethodInfo cleaner = allMethods.Where((method) => Utils.IsTestCleanup(method)).FirstOrDefault();

                    RunTest(currentType, testMethod, defaultConstructor, initializer, cleaner, seqIndex);
                    output.Add("Passed");
                }
            }
            catch (Exception e)
            {   
                var inner = e.InnerException == null ? e : e.InnerException;
                var innerName = inner.GetType().Name.ToLowerInvariant();
                if (innerName.Contains("fail") || innerName.Contains("assertion"))
                {
                    output.Add("Failed");
                    output.Add("¤");
                    output.Add(inner.StackTrace);
                    output.Add("¤");
                    output.Add(inner.Message);
                }
                else if (innerName.Contains("inconclusive"))
                {
                    output.Add("Inconclusive");
                    output.Add("¤");
                    output.Add(inner.StackTrace);
                    output.Add("¤");
                    output.Add(inner.Message);
                }
                else
                {   
                    if (inner != null)
                    {
                        output.Add("Failed");
                        output.Add("¤");
                        output.Add(inner.StackTrace);
                        output.Add("¤");
                        output.Add(inner.Message);
                    }
                    else
                    {
                        output.Add("Failed");
                        output.Add("¤");
                        output.Add(e.StackTrace);
                        output.Add("¤");
                        output.Add(e.Message);
                    }
                }
            }

            File.WriteAllLines(args[3], output, Encoding.UTF8);
        }

        private static void RunTest(Type currentType, MethodInfo testMethod, ConstructorInfo defaultConstructor, MethodInfo initializer, MethodInfo cleaner, int seqIndex)
        {
            try
            {
                object testInstance;
                if (defaultConstructor != null)
                {
                    testInstance = Activator.CreateInstance(currentType);
                }
                else
                {
                    testInstance = FormatterServices.GetUninitializedObject(currentType);
                }

                if (initializer != default(MethodInfo))
                {
                    initializer.Invoke(testInstance, new object[0]);
                }

                InvokeTestMethod(testMethod, testInstance, seqIndex);

                if (cleaner != default(MethodInfo))
                {
                    cleaner.Invoke(testInstance, new object[0]);
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("ERROR: " + e.Message);
                Console.Out.WriteLine(e.StackTrace);
                Environment.Exit(0);
            }
        }

        private static void InvokeTestMethod(MethodInfo testMethod, object testInstance, int seqIndex)
        {
            if(seqIndex == -1)
                testMethod.Invoke(testInstance, new object[0]);
            else
            {
                object[] par = Utils.GetSequentialParameters(testMethod, seqIndex);
                testMethod.Invoke(testInstance, par);
            }
        }
    }
}
