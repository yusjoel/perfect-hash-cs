namespace PerfectHash
{
    public abstract class SaltHash
    {
        public abstract string Template { get; }

        public abstract void Initialize(int N);

        public int N { get; set; }

        public abstract int Call(string key);

        public abstract int SaltLength { get; }

        public abstract string GetFormattedSalt();
    }
}