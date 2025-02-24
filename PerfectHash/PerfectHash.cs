/*
Generate a minimal perfect hash function for the keys in a file,
desired hash values may be specified within this file as well.
A given code template is filled with parameters, such that the
output is code which implements the hash function.
Templates can easily be constructed for any programming language.

The code is based on an a program A.M. Kuchling wrote:
http://www.amk.ca/python/code/perfect-hash

The algorithm the program uses is described in the paper
'Optimal algorithms for minimal perfect hashing',
Z. J. Czech, G. Havas and B.S. Majewski.
http://citeseer.ist.psu.edu/122364.html

The algorithm works like this:

1.  You have K keys, that you want to perfectly hash against some
    desired hash values.

2.  Choose a number N larger than K.  This is the number of
    vertices in a graph G, and also the size of the resulting table G.

3.  Pick two random hash functions f1, f2, that return values from 0..N-1.

4.  Now, for all keys, you draw an edge between vertices f1(key) and f2(key)
    of the graph G, and associate the desired hash value with that edge.

5.  If G is cyclic, go back to step 2.

6.  Assign values to each vertex such that, for each edge, you can add
    the values for the two vertices and get the desired (hash) value
    for that edge.  This task is easy, because the graph is acyclic.
    This is done by picking a vertex, and assigning it a value of 0.
    Then do a depth-first search, assigning values to new vertices so that
    they sum up properly.

7.  f1, f2, and vertex values of G now make up a perfect hash function.


For simplicity, the implementation of the algorithm combines steps 5 and 6.
That is, we check for loops in G and assign the vertex values in one procedure.
If this procedure succeeds, G is acyclic and the vertex values are assigned.
If the procedure fails, G is cyclic, and we go back to step 2, replacing G
with a new graph, and thereby discarding the vertex values from the failed
attempt.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PerfectHash
{
    public static class PerfectHash
    {
        public static bool verbose = false;

        public static int trials = 5;

        ///<summary>
        /// Return hash functions f1 and f2, and G for a perfect minimal hash.
        /// Input is an iterable of 'keys', whose indices are the desired hash values.
        /// 'Hash' is a random hash function generator, that means Hash(N) returns a
        /// returns a random hash function which returns hash values from 0..N-1.
        /// </summary>
        public static (T, T, int[]) generate_hash<T>(List<string> keys) where T : SaltHash, new()
        {
            var NK = keys.Count;
            var hashSet = new HashSet<string>(keys);
            if (NK != hashSet.Count) {
                throw new Exception("duplicate keys");
            }

            if (NK > 10000 && typeof(T) == typeof(StrSaltHash)) {
                Console.WriteLine(@"\
    WARNING: You have {0} keys.
             Using --hft=1 is likely to fail for so many keys.
             Please use --hft=2 instead.
    ", NK);
            }
            // the number of vertices in the graph G
            var NG = NK + 1;
            if (verbose) {
                Console.WriteLine("NG = {0}", NG);
            }
            var trial = 0;
            Graph G;
            T f1;
            T f2;
            while (true) {
                if (trial % trials == 0) {
                    // trials failures, increase NG slightly
                    if (trial > 0) {
                        NG = Math.Max(NG + 1, Convert.ToInt32(1.05 * NG));
                    }
                    if (verbose) {
                        Console.WriteLine("\nGenerating graphs NG = {0} ", NG);
                    }
                }
                trial += 1;
                if (NG > 100 * (NK + 1)) {
                    throw new Exception($"{NK} keys");
                }
                if (verbose) {
                    Console.WriteLine(".");
                }
                G = new Graph(NG);
                f1 = new T();
                f1.Initialize(NG);
                f2 = new T();
                f2.Initialize(NG);

                // Connect vertices given by the values of the two hash functions
                // for each key.  Associate the desired hash value with each edge.
                for (int i = 0; i < keys.Count; i++)
                {
                    string key = keys[i];
                    int hashval = i;
                    G.connect(f1.Call(key), f2.Call(key), hashval);
                }

                // Try to assign the vertex values.  This will fail when the graph
                // is cyclic.  But when the graph is acyclic it will succeed and we
                // break out, because we're done.
                if (G.assign_vertex_values()) {
                    break;
                }
            }
            if (verbose) {
                Console.WriteLine("\nAcyclic graph found after {0} trials.", trial);
                Console.WriteLine("NG = {0}", NG);
            }
            // Sanity check the result by actually verifying that all the keys
            // hash to the right value.
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                int hashval = i;
                Debug.Assert(hashval == (G.vertex_values[f1.Call(key)] + G.vertex_values[f2.Call(key)]) % NG);
            }
            if (verbose) {
                Console.WriteLine("OK");
            }
            return (f1, f2, G.vertex_values);
        }


        ///<summary>
        /// Return hash functions f1 and f2, and G for a perfect minimal hash.
        /// Input is an iterable of 'keys', whose indices are the desired hash values.
        /// 'Hash' is a random hash function generator, that means Hash(N) returns a
        /// returns a random hash function which returns hash values from 0..N-1.
        /// </summary>
        public static int[] generate_hash<T>(List<string> keys, int NG, T f1, T f2) where T : SaltHash, new()
        {
            var NK = keys.Count;

            Graph G = new Graph(NG);

            // Connect vertices given by the values of the two hash functions
            // for each key.  Associate the desired hash value with each edge.
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                int hashval = i;
                G.connect(f1.Call(key), f2.Call(key), hashval);
            }

            // Try to assign the vertex values.  This will fail when the graph
            // is cyclic.  But when the graph is acyclic it will succeed and we
            // break out, because we're done.
            bool result = G.assign_vertex_values();
            Debug.Assert(result);

            // Sanity check the result by actually verifying that all the keys
            // hash to the right value.
            for (int i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                int hashval = i;
                Debug.Assert(hashval == (G.vertex_values[f1.Call(key)] + G.vertex_values[f2.Call(key)]) % NG);
            }

            return G.vertex_values;
        }

        ///<summary>
        /// Takes a list of key value pairs and inserts the generated parameter
        /// lists into the 'template' string.  'Hash' is the random hash function
        /// generator, and the optional keywords are formating options.
        /// The return value is the substituted code template.
        ///</summary>
        public static string generate_code<T>(List<string> keys, Options options) where T:SaltHash, new()
        {
            T f1;
            T f2;
            int[] G;
            if (string.IsNullOrEmpty(options.TestData))
            {
                (f1, f2, G) = generate_hash<T>(keys);
            }
            else
            {
                var substrings = options.TestData.Split(";");
                int NG = int.Parse(substrings[0]);
                f1 = new T();
                f1.Initialize(NG);
                if (f1 is StrSaltHash sf1)
                    sf1.SetSalt(substrings[1]);
                f2 = new T();
                f2.Initialize(NG);
                if (f2 is StrSaltHash sf2)
                    sf2.SetSalt(substrings[2]);
                G = generate_hash(keys, NG, f1, f2);
            }

            Debug.Assert(f1.N == f2.N);
            Debug.Assert(f1.N == G.Length);
            int salt_len = f1.SaltLength;
            Debug.Assert(salt_len == f2.SaltLength);

            var codeGenerator = GetCodeGenerator(options);
            string result = codeGenerator.GenerateCode(keys, G, f1, f2);
            return result;
        }

        private static CodeGenerator GetCodeGenerator(Options options)
        {
            CodeGenerator codeGenerator = null;
            if (options.Language == "py")
            {
                codeGenerator = new PythonCodeGenerator();
            }
            else if (options.Language == "cs")
            {
                codeGenerator = new CSharpCodeGenerator();
            }
            else
            {
                throw new Exception("Unknown language: " + options.Language);
            }
            
            codeGenerator.LoadOptions(options);

            return codeGenerator;
        }

        ///<summary>
        /// Reads keys and desired hash value pairs from a file.  If no column
        /// for the hash value is specified, a sequence of hash values is generated,
        /// from 0 to N-1, where N is the number of rows found in the file.
        /// </summary>
        public static List<string> read_table(string filename)
        {
            if (verbose)
            {
                Console.WriteLine($"Reading table from file {filename} to extract keys.");
            }

            var keys = new List<string>();
            using (var streamReader = new StreamReader(File.OpenRead(filename)))
            {
                string line = streamReader.ReadLine();
                while (line != null)
                {
                    line = line.Trim();
                    keys.Add(line);
                    line = streamReader.ReadLine();
                }
            }

            return keys;
        }
        
        public static void Execute(string keys_file, Options options)
        {
            verbose = options.Verbose;
            var keys = read_table(keys_file);
            string code;
            if(options.Hft == 1)
            {
                code = generate_code<StrSaltHash>(keys, options);
            }
            else
            {
                code = generate_code<IntSaltHash>(keys, options);
            }

            if (string.IsNullOrEmpty(options.Output))
            {
                Console.WriteLine(code);
            }
            else
            {
                if (File.Exists(options.Output))
                    File.Delete(options.Output);
                File.WriteAllText(options.Output, code);
            }
        }
    }
}