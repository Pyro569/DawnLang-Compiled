using System.Diagnostics;

namespace DawnLangCompiler
{
    class ArgReader
    {
        public static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                //arg 0 must be -d so that way the Tokenization stuff actually runs with debug mode 
                if (args[0] == "-d")
                    Tokenization.DebugMode = true;
                if (args[i] == "-b")//-b = build
                    Tokenization.BuildFile(args[i + 1], args[i + 2]);
                if (args[i] == "-br")//-br = buld/run
                {
                    Tokenization.BuildFile(args[i + 1], args[i + 2]);
                    Process.Start("./" + args[i + 2]);//is this platform independent?
                }
            }
        }
    }
}