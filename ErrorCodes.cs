using System.Collections.Generic;

namespace DawnLangCompiler
{
    class ErrorCodeIO
    {
        //TODO: Finish this up in the 1.1 release
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
                    cf200();
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
        private static void cf200()
        {
            Tokenization.LogTokens();

            List<string> ErrorCodedTokens = new List<string>();

            //read the Tokens.txt file
            if (File.Exists("Tokens.txt"))
            {
                StreamReader streamReader = new StreamReader("Tokens.txt");
                string line = streamReader.ReadLine();
                while (line != null)
                {
                    ErrorCodedTokens.Add(line);
                    line = streamReader.ReadLine();
                }
                streamReader.Close();
            }

            //tokenize the file
            List<string> Tokens = new List<string>();
            for (int i = 0; i < ErrorCodedTokens.Count; i++)
            {
                string TokenString = "";

                for (int j = 0; j < ErrorCodedTokens[i].Length; j++)
                    if (ErrorCodedTokens[i][j] == ';' || ErrorCodedTokens[i][j] == '{' || ErrorCodedTokens[i][j] == '}')
                    {
                        TokenString += ErrorCodedTokens[i][j];
                        Tokens.Add(TokenString);
                        TokenString = "";
                    }
                    else
                        TokenString += ErrorCodedTokens[i][j];
            }

            //ever growning list of C Keywords that I can think of
            //all keywords missing final letter in hopes that maybe, just maybe, the user error was forgetting the last character
            List<string> CKeywords = new List<string>(){
                "int",
                "char",
                "bool",
                "void",
                "printf",
                "#import",
                "#include",
                "puts",
                "scanf",
                "}",
                "int main(){",
            };

            //TODO: Engineer a good way to guess what the user was trying to type
            List<string> variableNames = new List<string>();

            for (int i = 1; i < Tokens.Count; i++)
            {
                bool foundKeyword = false;
                for (int k = 0; k < CKeywords.Count; k++)
                    if (Tokens[i] == CKeywords[k])
                        foundKeyword = true;
                    else
                    {
                        if (Tokens[i - 1] == "int" || Tokens[i - 1] == "char" || Tokens[i - 1] == "bool")
                            variableNames.Add(Tokens[i]);
                        for (int z = 0; z < variableNames.Count; z++)
                            if (Tokens[i] == variableNames[z])
                                foundKeyword = true;
                    }
                if (foundKeyword == false)
                    Console.WriteLine("Error appears to be regarding the " + Tokens[i] + " call");
            }


            File.Delete("Tokens.txt");
        }
    }
}