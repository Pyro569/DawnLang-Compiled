namespace DawnLangCompiler
{
    class ArgReader
    {
        public static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-b")
                    Builder.BuildFile(args[i + 1], args[i + 2]);
            }
        }
    }
}