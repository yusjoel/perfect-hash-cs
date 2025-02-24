using System.Collections.Generic;
using System.IO;

namespace PerfectHash
{
    public class CSharpCodeGenerator : CodeGenerator
    {
        private string template;
        private int hft;

        public override void LoadOptions(Options options)
        {
            delimiter = options.Delimiter;
            lendel = delimiter.Length;
            lineWidth = options.Width;
            indent = options.Indent;
            hft = options.Hft;
            template = null;
            if (File.Exists(options.TemplateFilePath))
            {
                template = File.ReadAllText(options.TemplateFilePath);
            }
        }

        public override string GenerateCode(List<string> keys, int[] G, SaltHash f1, SaltHash f2)
        {
            if (string.IsNullOrEmpty(template))
                template = builtin_template();

            return template.Replace("$NS", f1.SaltLength.ToString())
                .Replace("$S1", f1.GetFormattedSalt())
                .Replace("$S2", f2.GetFormattedSalt())
                .Replace("$NG", G.Length.ToString())
                .Replace("$G", Format(G))
                .Replace("$NK", keys.Count.ToString())
                .Replace("$K", Format(keys));
        }

        private string hash_template()
        {
            if (hft == 1)
                return @"
    private int GetHash(string key, string salt)
    {
        int hash = 0;
        for (int i = 0; i < key.Length; i++)
            hash += salt[i] * key[i];
        hash %= $NG;
        return hash;
    }

    public int GetPerfectHash(string key)
    {
        int hash1 = GetHash(key, ""$S1"");
        int hash2 = GetHash(key, ""$S2"");
        int perfectHash = (vertexValues[hash1] + vertexValues[hash2]) % $NG;
        return perfectHash;
    }
";
            
            throw new System.NotImplementedException();
        }

        private string builtin_template()
        {
            string hash = hash_template();
            
            return $@"
// =======================================================================
// =================  C # code for perfect hash function   ===============
// =======================================================================

using System.Diagnostics;

public class PerfectHash
{{
    private int[] vertexValues = 
    {{
$G
    }};
{hash}
// ============================ Sanity check =============================
    private string[] keys = 
    {{
$K
    }};

    private void SanityCheck()
    {{
        Debug.Assert(keys.Length == $NK);

        for (int h = 0; h < keys.Length; h++)
        {{
            Debug.Assert(GetPerfectHash(keys[h]) == h);
        }}
    }}

    public static void Main()
    {{
        var hash = new PerfectHash();
        hash.SanityCheck();
    }}
}}
";
        }
    }
}