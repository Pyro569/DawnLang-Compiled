using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DawnLangCompiler
{
    class Builder
    {
        private static List<string> Lines = new List<string>();     //lines in the original dawnlang file
        private static List<string> Tokens = new List<string>();    //tokens from the original dawnlang file
        private static List<string> ConvertedTokens = new List<string>();   //the C version of the tokens from the dawnlang file
        private static List<string> IntVars = new List<string>();           //names of integer variables stored in the program
        private static List<string> StringVars = new List<string>();        //names of string variables stored in the program
        private static List<string> RequiredImports = new List<string>();   //imports required to make the C code work
        private static List<string> FunctionNames = new List<string>();     //names of created functions
        private static string ErrorOpCode = "a000";                         //random junk output for errors that actually has a meaning once you look at the source code

        public static void BuildFile(string FilePath, string OutputFileName)
        {
            try
            {
                ReadFile(FilePath);     //get the tokens of the file
                SearchForFunctions();   //search for functions initialized in file and add to FunctionNames list
                CheckForImports();      //check for required imports to make it work in C
                ConvertTokens();        //convert the code to C
                CreateCFile();          //write the C code into a file
                CompileCFile(OutputFileName);   //compile the C file hopefully
                Cleanup();              //cleanup all of the leftovers
            }
            catch
            {   //print out the error code and do some cleanup if there is an error
                Console.WriteLine("ERROR CODE: " + ErrorOpCode);
                Cleanup();
                if (File.Exists(OutputFileName))    //remove the probably fucked binary if it exists and compiled
                    File.Delete(OutputFileName);
            }
        }
        public static void ReadFile(string FilePath)
        {
            ErrorOpCode = "r100";    //r for read, 100 for first potential error spot

            //read the file's lines and add to the lines list
            StreamReader streamReader = new StreamReader(FilePath);
            string line = streamReader.ReadLine();
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
                //add semicolon to end to allow for easier breaking of lines in parsing
                if (!Lines[i].EndsWith(";"))
                    Lines[i] += ";";

                string TokenString = "";
                bool Quotation = false;

                for (int j = 0; j < Lines[i].Length; j++)
                {//loop through length of string in Lines[i]

                    //if quotation marks, mark the beginning of quotation for tokens to be 1 token
                    if (Lines[i][j] == '"' && Quotation == false)   //begin quotation
                        Quotation = true;
                    else if (Lines[i][j] == '"' && Quotation == true)   //end quotation
                        Quotation = false;

                    //add space to string if in quote and space
                    if (Lines[i][j] == ' ' && Quotation == true)
                        TokenString += " ";
                    //if not semicolon space or parenthesis then add current char to string
                    else if (Lines[i][j] != ';' && Lines[i][j] != ' ' && Lines[i][j] != '(' && Lines[i][j] != ')' && Lines[i][j] != ',')
                        TokenString += Lines[i][j];
                    else
                    //add the string to the tokens and set string to blank
                    {
                        Tokens.Add(TokenString);
                        if (Lines[i][j] == ',')
                            Tokens.Add(",");
                        TokenString = "";
                    }
                }
            }

            //debug purposes only
            //foreach (string token in Tokens)
            //   System.Console.WriteLine(token);
        }

        private static void SearchForFunctions()
        {
            ErrorOpCode = "fs100"; //fs for function search, 100 for operation spot
                                   //loop through every function and check if it contains function declaration
            for (int i = 0; i < Tokens.Count; i++)
                if (Tokens[i] == "function")
                    if (Tokens[i + 1] != "main")
                        FunctionNames.Add(Tokens[i + 1]);
            //if function is declared other than main, add to function name list
        }

        private static void CheckForImports()
        {
            //list of dawnlang functions that will require some kind of import in C code
            List<string> FunctionsRequiringImports = new List<string>(){
                "print",
                "print_int",
                "print_str",
            };

            //the needed import for said dawnlang function
            List<string> CorrespondingImport = new List<string>(){
                "<stdio.h>",    //print
                "<stdio.h>",    //print_int
                "<stdio.h>",    //print_str
            };

            ErrorOpCode = "is100";  //is for import search

            for (int i = 0; i < FunctionsRequiringImports.Count; i++)   //loop through each function in the FunctionsRequiringImports list
                if (Tokens.Contains(FunctionsRequiringImports[i]) && !RequiredImports.Contains(CorrespondingImport[i]))
                    //if the tokens list contains said function and the requiredimports list does not already have it, add it
                    RequiredImports.Add(CorrespondingImport[i]);
        }

        private static void ConvertTokens()
        {
            //convert tokens to c code
            ErrorOpCode = "c100";       //c for conversion, 100 for first potential error spot

            for (int i = 0; i < Tokens.Count; i++)
            {
                switch (Tokens[i])
                {
                    case "print":
                        ConvertedTokens.Add("printf(" + Tokens[i + 1] + ");");              //print("hello, world"); comes out to printf("hello, world");
                        break;
                    case "print_str":
                        ConvertedTokens.Add("printf(\"%s\\n\"," + Tokens[i + 1] + ");");    //print_str(hello); comes out to printf("%s", hello);
                        break;
                    case "int":
                        ConvertedTokens.Add("int " + Tokens[i + 1] + Tokens[i + 2] + Tokens[i + 3] + ";");  //int a = 17;
                        IntVars.Add(Tokens[i + 1]);
                        break;
                    case "string":
                        ConvertedTokens.Add("char " + Tokens[i + 1] + "[]" + Tokens[i + 2] + Tokens[i + 3] + ";");  //string hello = "hello world!"; comes out to char[] hello = "hello world!';
                        StringVars.Add(Tokens[i + 1]);
                        break;
                    case "print_int":
                        ConvertedTokens.Add("printf(\"%d\\n\"," + Tokens[i + 1] + ");");    //print_int(a); comes out to printf("%d\n",a);
                        break;
                    case "for":
                        ConvertedTokens.Add("for(int i = " + Tokens[i + 3] + "; i <= " + Tokens[i + 5] + "; i++){");    //create a for loop
                        break;
                    case "then":
                        if (Tokens[i + 1] == "end")         //end a for loop with a };
                            ConvertedTokens.Add("};");
                        break;
                    case "function":
                        if (Tokens[i + 1] == "main")
                            ConvertedTokens.Add("int main(){");
                        else
                        {
                            ConvertedTokens.Add("void " + Tokens[i + 1] + "(");
                            for (int j = i; j < Tokens.Count; j++)
                            {
                                if (Tokens[j] == "int")
                                {
                                    ConvertedTokens.Add("int " + Tokens[j + 1]);
                                    if (Tokens[j + 2] != "{")
                                        ConvertedTokens[ConvertedTokens.Count - 1] += ",";
                                    IntVars.Add(Tokens[j + 1]);
                                    Tokens.Remove(Tokens[j]);
                                    Tokens.Remove(Tokens[j + 1]);
                                }
                                if (Tokens[j] == "string")
                                {
                                    ConvertedTokens.Add("string " + Tokens[j + 1]);
                                    if (Tokens[j + 2] != "{")
                                        ConvertedTokens[ConvertedTokens.Count - 1] += ",";
                                    StringVars.Add(Tokens[j + 1]);
                                    Tokens.Remove(Tokens[j]);
                                    Tokens.Remove(Tokens[j + 1]);
                                }
                                else if (Tokens[j] == "}")
                                    break;
                            }
                            ConvertedTokens.Add("){");
                        }
                        break;
                    case "}":
                        ConvertedTokens.Add("}");
                        break;
                    default:
                        //change the value of an int variable
                        if (IntVars.Contains(Tokens[i]) && Tokens[i - 1] != "int")
                            if (Tokens[i + 1] == "=")
                                ConvertedTokens.Add(Tokens[i] + " " + Tokens[i + 1] + " " + Tokens[i + 2] + ";");
                        if (FunctionNames.Contains(Tokens[i]) && Tokens[i - 1] != "function")
                        {
                            ConvertedTokens.Add(Tokens[i] + "(");
                            for (int j = i; j < Tokens.Count; j++)
                            {
                                if (IntVars.Contains(Tokens[j + 1]))
                                {
                                    ConvertedTokens.Add(Tokens[j + 1]);
                                    if (Tokens[j + 2] == ",")
                                    {
                                        ConvertedTokens.Add(",");
                                        ConvertedTokens.Add(Tokens[j + 4]);
                                    }
                                }
                                else if (StringVars.Contains(Tokens[j + 1]))
                                {
                                    ConvertedTokens.Add(Tokens[j + 1]);
                                    if (Tokens[j + 2] == ",")
                                    {
                                        ConvertedTokens.Add(",");
                                        ConvertedTokens.Add(Tokens[j + 4]);
                                    }
                                }
                                if (!IntVars.Contains(Tokens[j + 1]) && !StringVars.Contains(Tokens[j + 1]))
                                    break;
                            }
                            ConvertedTokens[ConvertedTokens.Count - 1] += ");";
                        }
                        break;
                }
            }

            //debug purposes only
            //foreach (string tokens in ConvertedTokens)
            //    System.Console.WriteLine(tokens);
        }

        private static void CreateCFile()
        {
            ErrorOpCode = "wc100";       //wc for writing c, 100 for first potential error spot

            StreamWriter outputFile = new StreamWriter("TempFile.c");

            //add the required imports to the top of the c file
            for (int i = 0; i < RequiredImports.Count; i++)
                outputFile.WriteLine("#include " + RequiredImports[i] + "\n");
            //write each converted token to the c file
            foreach (string codeLines in ConvertedTokens)
                outputFile.WriteLine(codeLines);
            outputFile.Close();
        }

        private static void CompileCFile(string OutputFileName)
        {
            ErrorOpCode = "cf100";              //cf for compile file, 100 for first potential error spot

            if (File.Exists(OutputFileName))    //if the compiled binary already exists with that name, delete it/overwrite it
                File.Delete(OutputFileName);

            ErrorOpCode = "cf200";              //cf for compile file, 200 for second potential error spot

            Process.Start("gcc", "TempFile.c");
            Thread.Sleep(50);                   //small micro sleep for program to not error moving file since it is so new
            File.Move("./a.out", "./" + OutputFileName);
        }

        private static void Cleanup()
        {
            ErrorOpCode = "cl100";              //cl for cleanup, 100 for first potential error spot

            if (File.Exists("./TempFile.c"))    //delete the TempFile.c if it still exists (which it still should, if not, error)
                File.Delete("./TempFile.c");
        }
    }
}