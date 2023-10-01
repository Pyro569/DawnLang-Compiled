using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DawnLangCompiler
{
    class Builder
    {
        private static List<string> Lines = new List<string>();
        private static List<string> Tokens = new List<string>();
        private static List<string> ConvertedTokens = new List<string>();
        private static string ErrorOpCode = "a000";

        public static void BuildFile(string FilePath, string OutputFileName)
        {
            try
            {
                ReadFile(FilePath);
                ConvertTokens();
                CreateCFile();
                CompileCFile(OutputFileName);
                Cleanup();
            }
            catch
            {
                Console.WriteLine("ERROR CODE: " + ErrorOpCode);
            }
        }
        public static void ReadFile(string FilePath)
        {
            Lines.Clear();
            Tokens.Clear();

            ErrorOpCode = "r100";    //r for read, 100 for first potential error spot

            //read the file's lines and add to the lines list
            StreamReader streamReader = new StreamReader(FilePath);
            String line = streamReader.ReadLine();
            while (line != null)
            {
                Lines.Add(line);
                line = streamReader.ReadLine();
            }
            streamReader.Close();

            ErrorOpCode = "p100";    //p for parsing, 100 for first potential error spot


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
                    else if (Lines[i][j] != ';' && Lines[i][j] != ' ' && Lines[i][j] != '(' && Lines[i][j] != ')')
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

            ErrorOpCode = "c100";       //c for conversion, 100 for first potential error spot

            for (int i = 0; i < Tokens.Count; i++)
            {
                switch (Tokens[i])
                {
                    case "print":
                        ConvertedTokens.Add("printf(" + Tokens[i + 1] + ");");
                        break;
                    case "int":
                        ConvertedTokens.Add("int " + Tokens[i + 1] + Tokens[i + 2] + Tokens[i + 3] + ";");
                        break;
                    case "printvar":
                        ConvertedTokens.Add("printf(\"%d\\n\"," + Tokens[i + 1] + ");");
                        break;
                    default:
                        break;
                }
            }

            //debug purposes only
            foreach (string tokens in ConvertedTokens)
                System.Console.WriteLine(tokens);
        }

        private static void CreateCFile()
        {
            ErrorOpCode = "wc100";       //w for writing c, 100 for first potential error spot

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
            ErrorOpCode = "cf100";              //cf for compile file, 100 for first potential error spot

            if (File.Exists(OutputFileName))
                File.Delete(OutputFileName);

            ErrorOpCode = "cf200";              //cf for compile file, 200 for second potential error spot

            Process.Start("gcc", "TempFile.c");
            Thread.Sleep(50);                   //small micro sleep for program to not error moving file since it is so new
            File.Move("./a.out", "./" + OutputFileName);
        }

        private static void Cleanup()
        {
            ErrorOpCode = "cl100";              //cl for cleanup, 100 for first potential error spot

            if (File.Exists("./TempFile.c"))
            {
                File.Delete("./TempFile.c");
            }
        }
    }
}