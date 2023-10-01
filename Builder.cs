using System.Diagnostics;

namespace DawnLangCompiler
{
    class Builder
    {
        private static List<string> Lines = new List<string>();
        private static List<string> Tokens = new List<string>();
        private static List<string> ConvertedTokens = new List<string>();

        public static void BuildFile(string FilePath, string OutputFileName)
        {
            ReadFile(FilePath);
            ConvertTokens();
            CreateCFile();
            CompileCFile(OutputFileName);
            //System.Threading.Thread.Sleep(1000);
            Cleanup();
        }
        public static void ReadFile(string FilePath)
        {
            Lines.Clear();
            Tokens.Clear();

            //read the file's lines and add to the lines list
            StreamReader streamReader = new StreamReader(FilePath);
            String line = streamReader.ReadLine();
            while (line != null)
            {
                Lines.Add(line);
                line = streamReader.ReadLine();
            }
            streamReader.Close();

            //run through each line and parse tokens then add to the tokens list
            for (int i = 0; i < Lines.Count; i++)
            {
                if (!Lines[i].EndsWith(";"))
                {
                    Lines[i] += ";";
                }

                string TokenString = "";
                bool Quotation = false;

                for (int j = 0; j < Lines[i].Length; j++)
                {//loop through length of string in Lines[i]

                    //if quotation marks, mark the beginning of quotation for tokens to be 1 token
                    if (Lines[i][j] == '"' && Quotation == false)
                    {
                        Quotation = true;
                    }
                    else if (Lines[i][j] == '"' && Quotation == true)
                    {
                        Quotation = false;
                    }

                    if (Lines[i][j] == ' ' && Quotation == true)
                    {
                        TokenString += " ";
                    }
                    else if (Lines[i][j] != ';' && Lines[i][j] != ' ')
                    {
                        TokenString += Lines[i][j];
                    }
                    else
                    {
                        Tokens.Add(TokenString);
                        TokenString = "";
                    }
                }
            }

            //debug purposes only
            //foreach (string token in Tokens)
            //    System.Console.WriteLine(token);
        }

        private static void ConvertTokens()
        {
            ConvertedTokens.Clear();

            //convert tokens to c code

            for (int i = 0; i < Tokens.Count; i++)
            {
                switch (Tokens[i])
                {
                    case "print":
                        ConvertedTokens.Add("printf(");
                        ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[i + 1]; //add next token to the end of the print command
                        ConvertedTokens[ConvertedTokens.Count - 1] += ");";
                        break;
                    default:
                        break;
                }
            }

            //debug purposes only
            //foreach (string tokens in ConvertedTokens)
            //    System.Console.WriteLine(tokens);
        }

        private static void CreateCFile()
        {
            StreamWriter outputFile = new StreamWriter("TempFile.c");
            outputFile.WriteLine("#include <stdio.h>\nint main() {");
            foreach (string codeLines in ConvertedTokens)
            {
                outputFile.WriteLine(codeLines);
            }
            outputFile.WriteLine("return 0;\n}");
            outputFile.Close();
        }

        private static void CompileCFile(string OutputFileName)
        {
            Process.Start("gcc", "TempFile.c");
            System.Threading.Thread.Sleep(1000);
            System.IO.File.Move("./a.out", "./" + OutputFileName);
        }

        private static void Cleanup()
        {
            if (File.Exists("./TempFile.c"))
            {
                File.Delete("./TempFile.c");
            }
        }
    }
}