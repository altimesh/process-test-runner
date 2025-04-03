using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TestAttributes;

namespace Altimesh.TestRunner.Library
{
    public class DllLoader
    {
        public static List<TestDescriptor> GetAllMethod(string dllPath, List<string> testlist, ref int testCount)
        {
            testCount = 0;
            List<TestDescriptor> result = new List<TestDescriptor>();
            Dictionary<Type, List<MethodInfo>> tmp = new Dictionary<Type, List<MethodInfo>>();
            Assembly loaded;
            try
            {
                loaded = Assembly.LoadFrom(dllPath);
            }
            catch (Exception)
            {
                Console.WriteLine("could not load assembly: " + dllPath);
                return result;
            }

            Type[] types = loaded.GetTypes().Where((type) => Utils.IsTestClass(type)).OrderBy((type) => type.Name).ToArray();
            
            // count test methods
            for (int i = 0; i < types.Length; ++i)
            {
                Type currentType = types[i];
                MethodInfo[] allMethods = currentType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                MethodInfo[] tests = allMethods.Where((method) => Utils.IsTestMethod(method) && (!testlist.Any() || testlist.Contains(method.Name))).OrderBy((method) => method.Name).ToArray();
            }


            for (int i = 0; i < types.Length; ++i)
            {
                Type currentType = types[i];
                ConstructorInfo defaultConstructor = currentType.GetConstructor(new Type[0]);
                MethodInfo[] allMethods = currentType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                MethodInfo[] tests = allMethods.Where((method) => Utils.IsTestMethod(method) && (!testlist.Any() || testlist.Contains(method.Name))).ToArray();

                tmp.Add(currentType, new List<MethodInfo>());
                for (int j = 0; j < tests.Length; ++j)
                {
                    tmp[currentType].Add(tests[j]);
                }
            }
            
            foreach(var kvp in tmp)
            {
                foreach (var mi in kvp.Value)
                {
                    int seqLength = Utils.GetSequenceLength(mi);
                    result.Add(new TestDescriptor { DeclaringType = kvp.Key, method = mi, SequenceLength = seqLength });
                    testCount += Math.Abs((int)Utils.GetSequenceLength(mi)); // -1 means 1 test, any positive value means that value
                }
            }

            return result;
        }
    }
}
