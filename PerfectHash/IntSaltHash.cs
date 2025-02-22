using System;
using System.Collections.Generic;

namespace PerfectHash
{
    /// <summary>
    /// Random hash function generator.
    /// Simple byte level hashing, each byte is multiplied in sequence to a table
    /// containing random numbers, summed tp, and finally modulo NG is taken.
    /// </summary>
    public class IntSaltHash : SaltHash
    {
        public List<int> salt;

        private Random random;

        public override void Initialize(int N)
        {
            this.N = N;
            this.salt = new List<int>();
            random = new Random();
        }

        public override int Call(string key) {
            while (this.salt.Count < key.Length) {
                // add more salt as necessary
                this.salt.Add(random.Next(1, this.N));
            }

            int hash = 0;
            for (int i = 0; i < key.Length; i++)
                hash += salt[i] * key[i];

            hash %= N;

            return hash;
        }

        public override int SaltLength => salt.Count;
        public override string GetFormattedSalt()
        {
            return "";
        }

        public string template = @"
    S1 = [$S1]
    S2 = [$S2]
    assert len(S1) == len(S2) == $NS
    
    def hash_f(key, T):
        return sum(T[i % $NS] * ord(c) for i, c in enumerate(key)) % $NG
    
    def perfect_hash(key):
        return (G[hash_f(key, S1)] + G[hash_f(key, S2)]) % $NG
    ";

        public override string Template => template;
    }
}