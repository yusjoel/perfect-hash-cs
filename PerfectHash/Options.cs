using CommandLine;

namespace PerfectHash
{
    public class Options
    {
        [Value(1)]
        public string KeysFilePath { get; set; }

        [Option("delimiter", Default = ", ", HelpText = "Delimiter for list items used in output, the default delimiter is ', '")]
        public string Delimiter { get; set; }

        [Option("indent", Default = 4, HelpText = "Make INT spaces at the beginning of a new line when generated list is wrapped. Default is 4")]
        public int Indent { get; set; }

        [Option("width", Default = 76, HelpText = "Maximal width of generated list when wrapped. Default width is 76")]
        public int Width { get; set; }

        [Option("comment", Default = "#", HelpText = "STR is the character, or sequence of characters, which marks the beginning of a comment (which runs till the end of the line), in the input KEYS_FILE. Default is '#'")]
        public string Comment { get; set; }

        [Option("splitby", Default = ",", HelpText = "STR is the character by which the columns in the input KEYS_FILE are split. Default is ','")]
        public string SplitBy { get; set; }

        [Option("keycol", Default = 1, HelpText = "Specifies the column INT in the input KEYS_FILE which contains the keys. Default is 1, i.e. the first column.")]
        public int KeyCol { get; set; }

        [Option("trials", Default = 5, HelpText = "Specifies the number of trials before NG is increased. A small INT will give compute faster, but the array G will be large. A large INT will take longer to compute but G will be smaller. Default is 5")]
        public int Trials { get; set; }

        [Option("hft", Default = 1, HelpText = "Hash function type INT. Possible values are 1 (StrSaltHash) and 2 (IntSaltHash). The default is 1")]
        public int Hft { get; set; }

        [Option('e', "execute", HelpText = "Execute the generated code within the Python interpreter.")]
        public bool Execute { get; set; }

        [Option('o', "output", HelpText = "Specify output FILE explicitly. `-o std' means standard output. `-o no' means no output. By default, the file name is obtained from the name of the template file by substituting `tmpl' to `code'.")]
        public string Output { get; set; }

        [Option('v', "verbose", HelpText = "verbosity")]
        public bool Verbose { get; set; }

        [Option('t', "test", HelpText = "给出指定的NG, salt1, salt2，计算G")]
        public string TestData { get; set; }
    }
}