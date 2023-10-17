namespace DawnLangCompiler
{
    class ErrorCodeIO
    {
        public static void ErrorCodeOutput()
        {
            switch (Tokenization.ErrorOpCode)
            {
                case "cl100":
                    Console.WriteLine("ERROR: Error has been encountered after compilation, DawnLang program is fine and usable");
                    break;
                case "cf100":
                    Console.WriteLine("ERROR: Error has occured while compiling the file");
                    break;
                case "cf200":
                    Console.WriteLine("ERROR: Error has occured during file compilation, this is most likely an error within your DawnLang code");
                    break;
                case "wc100":
                    Console.WriteLine("ERROR: Error has occured during pre-compilation");
                    break;
                case "c100":
                    Console.WriteLine("ERROR: Error has occured during file conversion");
                    break;
                case "fs100":
                    Console.WriteLine("ERROR: Error has occured during function parsing");
                    break;
                case "p100":
                    Console.WriteLine("ERROR: Error has occured during parsing of the file");
                    break;
                case "r100":
                    Console.WriteLine("ERROR: Error has occured during reading of file, most likely file has been moved/deleted or not accessible with current user permissions");
                    break;
            }
        }

        //TODO: System that is able to parse and search for errors in the DawnLang file if the error code is cf200, fs100 or wc100
    }
}