using System.Diagnostics;

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
                if (args[i] == "-br")
                {
                    Builder.BuildFile(args[i + 1], args[i + 2]);
                    Process.Start("./" + args[i + 2]);
                }
            }
        }
    }
}