using System;
using CommandLine;

namespace PerfectHash
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(options =>
                {
                    PerfectHash.Execute(options.KeysFilePath, options);
                })
                .WithNotParsed<Options>(errors =>
                {
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error);
                    }
                });
        }
    }
}
