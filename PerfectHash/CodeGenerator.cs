using System;
using System.Collections.Generic;
using System.Text;

namespace PerfectHash
{
    public abstract class CodeGenerator
    {
        protected string delimiter;
        protected int lendel;
        protected int lineWidth;
        protected int indent;
        
        public abstract void LoadOptions(Options options);
        
        public abstract string GenerateCode(List<string> keys, int[] G, SaltHash f1, SaltHash f2);
        
        public string Format<T>(IList<T> list, int pos = -1)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (pos == -1)
            {
                for (int j = 0; j < indent; j++)
                    stringBuilder.Append(' ');
                pos = indent;
            }

            for (var i = 0; i < list.Count; i++)
            {
                var elt = list[i];
                bool last = i == list.Count - 1;
                string s = elt.ToString();
                if(elt is string)
                    s = $"\"{s}\"";
                
                if (pos + s.Length + lendel > lineWidth)
                {
                    stringBuilder.Append(Environment.NewLine);
                    for (int j = 0; j < indent; j++)
                        stringBuilder.Append(' ');
                    pos = indent;
                }

                stringBuilder.Append(s);
                pos += s.Length;
                if (!last)
                {
                    stringBuilder.Append(delimiter);
                    pos += lendel;
                }
            }

            return stringBuilder.ToString();
        }
    }
}