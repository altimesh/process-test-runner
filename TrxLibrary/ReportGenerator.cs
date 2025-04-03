using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Altimesh.TestRunner.TrxLibrary
{
    public class ReportGenerator
    {
        /// <summary>
        /// save test run
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="run"></param>
        /// <param name="directoryFullPath">must be an absolute path to an existing directory if fileName is relative</param>
        public void WriteToFile(string fileName, TestRun run, string directoryFullPath = "")
        {
            if (!Path.IsPathRooted(fileName) && !String.IsNullOrEmpty(directoryFullPath) && Directory.Exists(directoryFullPath))
            {
                fileName = Path.Combine(directoryFullPath, fileName);
            }

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");

            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, Encoding.UTF8))
                {
                    XmlSerializer x = new XmlSerializer(run.GetType());
                    x.Serialize(sw, run, ns);
                    File.WriteAllBytes(fileName, ms.ToArray());
                }
            }
        }
        
        public static string DateToString(DateTime date)
        {
            return date.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
        }
    }
}
