using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Altimesh.TestRunner.Library
{
    public class TestDescriptor
    {
        public const int DefaultSeqLength = -1;
        public Type DeclaringType { get; set; }
        public MethodInfo method { get; set; }

        private int _seqLength = DefaultSeqLength;
        public int SequenceLength
        {
            get
            {
                return _seqLength;
            }
            set
            {
                _seqLength = value;
            }
        }
    }

}
