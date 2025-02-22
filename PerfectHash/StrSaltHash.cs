using System;

namespace PerfectHash
{
    /// <summary>
    /// Random hash function generator.
    /// Simple byte level hashing: each byte is multiplied to another byte from
    /// a random string of characters, summed up, and finally modulo NG is
    /// taken.
    /// </summary>
    public class StrSaltHash : SaltHash
    {
        public string salt;
            
        public string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private Random random;

        public override void Initialize(int N)
        {
            this.N = N;
            this.salt = "";
            random = new Random();
        }

        public void SetSalt(string salt)
        {
            this.salt = salt;
        }
            
        public override int Call(string key) {
            while (this.salt.Length < key.Length) {
                // add more salt as necessary
                this.salt += chars[random.Next(chars.Length)];
            }

            int hash = 0;

            for (int i = 0; i < key.Length; i++)
                hash += salt[i] * key[i];

            hash %= N;

            return hash;
        }

        public override int SaltLength => salt.Length;
        public override string GetFormattedSalt()
        {
            return salt;
        }

        public string template = @"
def hash_f(key, T):
    return sum(ord(T[i % $NS]) * ord(c) for i, c in enumerate(key)) % $NG

def perfect_hash(key):
    return (G[hash_f(key, ""$S1"")] +
            G[hash_f(key, ""$S2"")]) % $NG
";

        public override string Template => template;
    }
}