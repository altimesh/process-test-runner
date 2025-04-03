using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestAttributes
{
    public static class Utils
    {
        public static bool IsTestClass(Type t)
        {
            return HasAttribute(t, "TestClassAttribute") || HasAttribute(t, "TestFixtureAttribute");
        }

        public static bool IsTestSequential(MethodInfo mi)
        {
            return HasAttribute(mi, "SequentialAttribute");
        }

        public static int GetSequenceLength(MethodInfo mi)
        {
            int result = -1;
            if (!IsTestSequential(mi))
            {
                return result;
            }

            ParameterInfo[] ps = mi.GetParameters();
            if (!ps.Any())
            {
                return result;
            }

            foreach (ParameterInfo pi in ps)
            {
                Attribute valatt = pi.GetCustomAttributes().Where((att) => IsValuesAttribute(att.GetType())).FirstOrDefault();
                if (valatt == default(Attribute))
                    continue;
                FieldInfo datafield = valatt.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where((fi) => fi.Name.ToLowerInvariant().Contains("data")).FirstOrDefault();
                if (datafield == null || datafield.FieldType != typeof(object).MakeArrayType())
                    continue;
                object[] data = (object[])datafield.GetValue(valatt);
                if (data.Length > result)
                    result = (int)data.Length;
            }

            return result;
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

        public static bool IsTestMethod(MethodInfo mi)
        {
            return HasAttribute(mi, "TestMethodAttribute") || HasAttribute(mi, "TestAttribute");
        }

        public static bool IsTestInitialize(MethodInfo mi)
        {
            return HasAttribute(mi, "TestInitializeAttribute") || HasAttribute(mi, "SetUpAttribute");
        }

        public static bool IsIgnored(MethodInfo mi)
        {
            return HasAttribute(mi, "IgnoreAttribute");
        }
        
        public static string GetDisplayName(MethodInfo testMethod, int seqIndex)
        {
            object[] seqParams = GetSequentialParameters(testMethod, seqIndex);
            string displayName = testMethod.Name;

            // TODO: display better test name for sequences --- (testName(val0.toString(), ...) for example
            if (seqParams.Length > 0)
                displayName += "(" + String.Join(",", seqParams.Select((o) => o == null ? "null" : o.ToString())) + ")";
            return displayName;
        }

        public static string GetIgnoreReason(MethodInfo mi)
        {
            Attribute at = mi.GetCustomAttributes().Where((att) => att.GetType().Name.ToLowerInvariant().Contains("ignore")).FirstOrDefault();
            PropertyInfo reason = at == default(Attribute) ? default(PropertyInfo) : at.GetType().GetProperties().FirstOrDefault((p) => p.Name.ToLowerInvariant().Contains("reason"));
            if (reason != default(PropertyInfo))
            {
                return (string)reason.GetValue(at, null);
            }

            return "";
        }

        public static bool IsTestCleanup(MethodInfo mi)
        {
            return HasAttribute(mi, "TestCleanupAttribute") || HasAttribute(mi, "TearDownAttribute");
        }

        public static bool IsTestIgnored(MethodInfo mi)
        {
            return HasAttribute(mi, "IgnoreAttribute");
        }

        // GetCustomAttribute won't work if Test libraries versions do not match
        public static bool HasAttribute(Type t, string attrName)
        {
            return t.GetCustomAttributes(true).FirstOrDefault((attr) => attr.GetType().Name == attrName) != null;
        }

        public static bool HasAttribute(MethodInfo mi, string attrName)
        {
            return mi.GetCustomAttributes(true).FirstOrDefault((attr) => attr.GetType().Name == attrName) != null;
        }

        public static int Timeout(MethodInfo mi)
        {
            bool has = HasAttribute(mi, "TimeoutAttribute");
            if (has)
            {
                Attribute attribute = mi.GetCustomAttributes().First((attr) => attr.GetType().Name == "TimeoutAttribute");
                if (attribute == default(Attribute))
                    return -1;

                PropertyInfo pi = attribute.GetType().GetProperty("Timeout");
                if (pi != null)
                    return (int)pi.GetValue(attribute, null);

                pi = attribute.GetType().GetProperties().FirstOrDefault((p) => p.Name.ToLowerInvariant().Contains("properties"));
                if (pi == default(PropertyInfo))
                    return -1;

                IDictionary val = (IDictionary)pi.GetValue(attribute, null);
                return (int)val["Timeout"];
            }

            return -1;
        }

    }
}
