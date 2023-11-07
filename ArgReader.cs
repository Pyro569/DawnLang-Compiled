using System.Diagnostics;

namespace DawnLangCompiler
{
    class ArgReader
    {
        private static List<string> PossibleArgs = new List<string>(){
            "-d - Debug Mode (For development use only)",
            "-b - Build File (input file name as next arg)",
            "-br - Build and Run File (Compiles and then runs the compiled binary)",
            "--version - Outputs the DawnLang version you have installed",
            "--h - Prints out a list of possible arguments for the compiler",
            "Example Compiler Usage: ./DawnLang \"-b\" \"hello_world.dl\" \"Hello_World\""
        };
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
                if (args[i] == "--version")
                    //TODO: Change this to DawnLang Version 1.0 for the full release
                    Console.WriteLine("DawnLang Version 1.0");
                if (args[i] == "--h")
                    for (int l = 0; l < PossibleArgs.Count; l++)
                        Console.WriteLine(PossibleArgs[l]);
            }
        }
    }
}