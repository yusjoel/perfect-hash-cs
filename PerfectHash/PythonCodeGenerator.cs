using System.Collections.Generic;
using System.IO;

namespace PerfectHash
{
    public class PythonCodeGenerator : CodeGenerator
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
                .Replace("$G", Format(G, 20))
                .Replace("$NK", keys.Count.ToString())
                .Replace("$K", Format(keys, 20));
        }

        private string hash_template()
        {
            if (hft == 1)
                return @"
def hash_f(key, T):
    return sum(ord(T[i % $NS]) * ord(c) for i, c in enumerate(key)) % $NG

def perfect_hash(key):
    return (G[hash_f(key, ""$S1"")] +
            G[hash_f(key, ""$S2"")]) % $NG
";
            
            
            return @"
S1 = [$S1]
S2 = [$S2]
assert len(S1) == len(S2) == $NS

def hash_f(key, T):
    return sum(T[i % $NS] * ord(c) for i, c in enumerate(key)) % $NG

def perfect_hash(key):
    return (G[hash_f(key, S1)] + G[hash_f(key, S2)]) % $NG
";
        }

        private string builtin_template()
        {
            string hash = hash_template();
            
            return $@"
# =======================================================================
# ================= Python code for perfect hash function ===============
# =======================================================================

G = [$G]
{hash}
# ============================ Sanity check =============================

K = [$K]
assert len(K) == $NK

for h, k in enumerate(K):
    assert perfect_hash(k) == h
";
        }
    }
}