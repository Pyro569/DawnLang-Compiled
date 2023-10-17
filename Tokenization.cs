using System;
using System.Collections.Generic;
using System.IO;

namespace DawnLangCompiler
{
    class Tokenization
    {
        private static List<string> Lines = new List<string>();     //lines in the original dawnlang file
        private static List<string> Tokens = new List<string>();    //tokens from the original dawnlang file
        public static List<string> ConvertedTokens = new List<string>();   //the C version of the tokens from the dawnlang file
        private static List<string> IntVars = new List<string>();           //names of integer variables stored in the program
        private static List<string> StringVars = new List<string>();        //names of string variables stored in the program
        private static List<string> BoolVars = new List<string>();          //names of boolean variables
        private static List<string> FunctionNames = new List<string>();     //names of created functions
        private static List<string> IntListNames = new List<string>();      //list of int list names
        private static List<string> StringListNames = new List<string>();
        private static List<string> BoolListNames = new List<string>();
        public static string ErrorOpCode = "a000";                         //random junk output for errors that actually has a meaning once you look at the source code

        public static void BuildFile(string FilePath, string OutputFileName)
        {
            try
            {
                ReadFile(FilePath);     //get the tokens of the file
                SearchForFunctions();   //search for functions initialized in file and add to FunctionNames list
                ConvertTokens();        //convert the code to C
                Creation.CreateCFile("Main");          //write the C code into a file
                Creation.CompileCFile("Main", OutputFileName);   //compile the C file hopefully
                Creation.Cleanup("Main");              //cleanup all of the leftovers
            }
            catch
            {   //print out the error code and do some cleanup if there is an error
                ErrorCodeIO.ErrorCodeOutput();
                Creation.Cleanup("Main");
                if (File.Exists(OutputFileName))    //remove the probably fucked binary if it exists and compiled
                    File.Delete(OutputFileName);
                System.Environment.Exit(1);
            }
        }
        public static void ReadFile(string FilePath)
        {
            Lines.Clear();
            ErrorOpCode = "r100";    //r for read, 100 for first potential error spot

            //read the file's lines and add to the lines list, if it exists
            if (File.Exists(FilePath))
            {
                StreamReader streamReader = new StreamReader(FilePath);
                string line = streamReader.ReadLine();
                while (line != null)
                {
                    Lines.Add(line);
                    line = streamReader.ReadLine();
                }
                streamReader.Close();
            }
            else
            {
                Console.WriteLine("ERROR: File cannot be found");
                System.Environment.Exit(1);
            }

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
                    if (Lines[i].Contains("//"))
                        i += 1;
                    if (Lines[i][j] == '"' && Quotation == false)   //begin quotation
                        Quotation = true;
                    else if (Lines[i][j] == '"' && Quotation == true)   //end quotation
                        Quotation = false;

                    //add space to string if in quote and space
                    if (Lines[i][j] == ' ' && Quotation == true)
                        TokenString += " ";
                    //add comma to string if in quote and space
                    else if (Lines[i][j] == ',' && Quotation == true)
                        TokenString += ",";
                    //if not semicolon space or parenthesis then add current char to string
                    else if (Lines[i][j] != ';' && Lines[i][j] != ' ' && Lines[i][j] != '(' && Lines[i][j] != ')' && Lines[i][j] != ',' && Lines[i][j] != '{'
                     && Lines[i][j] != '}' && Lines[i][j] != '[' && Lines[i][j] != ']')
                        TokenString += Lines[i][j];
                    else
                    //add the string to the tokens and set string to blank
                    {
                        Tokens.Add(TokenString);
                        if (Lines[i][j] == ',')
                            Tokens.Add(",");
                        if (Lines[i][j] == ')')
                            Tokens.Add(")");
                        if (Lines[i][j] == '}')
                            Tokens.Add("}");
                        if (Lines[i][j] == '[')
                            Tokens.Add("[");
                        if (Lines[i][j] == ']')
                            Tokens.Add("]");
                        if (Lines[i][j] == ';')
                            Tokens.Add(";");
                        TokenString = "";
                    }
                }
            }

            //debug purposes only
            //foreach (string token in Tokens)
            //    System.Console.WriteLine(token);
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
                    case "bool":
                        ConvertedTokens.Add("bool " + Tokens[i + 1] + " = " + Tokens[i + 3] + ";");
                        BoolVars.Add(Tokens[i + 1]);
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
                            ConvertedTokens.Add("}");
                        break;
                    case "function":
                        if (Tokens[i + 1] == "main")            //search for main function and add it
                            ConvertedTokens.Add("int main(){"); //add main function to the c file
                        else
                        {
                            ConvertedTokens.Add("void " + Tokens[i + 1] + "("); //function declaration with function name
                            for (int j = i; j < Tokens.Count; j++)
                            {
                                if (Tokens[j] == "int")
                                {
                                    ConvertedTokens.Add("int " + Tokens[j + 1]);    //add int and int name as parameter
                                    if (Tokens[j + 3] != "{")                       //if the third token is not a left bracket, add a comma
                                        ConvertedTokens[ConvertedTokens.Count - 1] += ",";
                                    IntVars.Add(Tokens[j + 1]);                     //add the int variable name to int vars list
                                    Tokens.Remove(Tokens[j]);                       //remove tokens[j] and tokens[j+1]
                                    Tokens.Remove(Tokens[j + 1]);
                                }
                                if (Tokens[j] == "string")
                                {
                                    ConvertedTokens.Add("char " + Tokens[j + 1] + "[]");
                                    if (Tokens[j + 3] != "{")
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
                    case "if":
                        ConvertedTokens.Add("if(" + Tokens[i + 1] + Tokens[i + 2] + Tokens[i + 3] + "){");
                        break;
                    case "else":
                        ConvertedTokens.Add("else");
                        if (Tokens[i + 1] != "if")
                            ConvertedTokens[ConvertedTokens.Count - 1] += "{";
                        break;
                    case "}":
                        ConvertedTokens.Add("}");
                        break;
                    case "#include":
                        ReadFile(Tokens[i + 1]);
                        SearchForFunctions();
                        break;
                    case "#import":
                        ConvertedTokens.Add("#include ");
                        switch (Tokens[i + 1])
                        {
                            case "dawnlang.io":
                                ConvertedTokens[ConvertedTokens.Count - 1] += "<stdio.h>\n#include <iostream>";
                                break;
                            case "dawnlang.data.types":
                                ConvertedTokens[ConvertedTokens.Count - 1] += "<stdbool.h>";
                                break;
                        }
                        break;
                    case "List<int>":
                        IntListNames.Add(Tokens[i + 1]);
                        ConvertedTokens.Add("int " + Tokens[i + 1] + "[] = {");
                        for (int z = i + 5; z < Tokens.Count; z++)
                            if (Tokens[z] != "]")
                                ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[z];
                            else
                            {
                                ConvertedTokens[ConvertedTokens.Count - 1] += "};";
                                break;
                            }
                        break;
                    case "List<string>":
                        StringListNames.Add(Tokens[i + 1]);
                        ConvertedTokens.Add("char " + Tokens[i + 1] + "[][255] = {");
                        for (int z = i + 5; z < Tokens.Count; z++)
                            if (Tokens[z] != "]")
                                ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[z];
                            else
                            {
                                ConvertedTokens[ConvertedTokens.Count - 1] += "};";
                                break;
                            }
                        break;
                    case "List<bool>":
                        BoolListNames.Add(Tokens[i + 1]);
                        ConvertedTokens.Add("bool " + Tokens[i + 1] + "[] = {");
                        for (int z = i + 5; z < Tokens.Count; z++)
                            if (Tokens[z] != "]")
                                ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[z];
                            else
                            {
                                ConvertedTokens[ConvertedTokens.Count - 1] += "};";
                                break;
                            }
                        break;
                    case "print_list_element":
                        if (IntListNames.Contains(Tokens[i + 1]))
                            ConvertedTokens.Add("printf(\"%d\\n\", " + Tokens[i + 1] + "[" + Tokens[i + 4] + "]);");
                        if (BoolListNames.Contains(Tokens[i + 1]))
                            ConvertedTokens.Add("printf(\"%d\\n\", " + Tokens[i + 1] + "[" + Tokens[i + 4] + "]);");
                        if (StringListNames.Contains(Tokens[i + 1]))
                            ConvertedTokens.Add("printf(\"%s\\n\", " + Tokens[i + 1] + "[" + Tokens[i + 4] + "]);");
                        break;
                    case "C-Code":
                        if (Tokens[i + 1] == "[")
                        {
                            int location = 0;
                            for (int l = 0; l < Lines.Count; l++)
                                if (Lines[l].Contains("C-Code["))
                                {
                                    location = l;
                                    break;
                                }
                            for (int b = location + 1; b < Lines.Count; b++)
                                if (!Lines[b].Contains("]-End"))
                                    ConvertedTokens.Add(Lines[b]);
                                else
                                    break;
                            for (int n = i; n < Tokens.Count; n++)
                            {
                                if (Tokens[n] == "]-End")
                                    break;
                                else if (Tokens[n] == "int")
                                {
                                    IntVars.Add(Tokens[n + 1]);
                                    Tokens.Remove(Tokens[n]);
                                    Tokens.Remove(Tokens[n + 1]);
                                }
                                else if (Tokens[n] == "char[]")
                                {
                                    StringVars.Add(Tokens[n + 1]);
                                    Tokens.Remove(Tokens[n]);
                                    Tokens.Remove(Tokens[n + 1]);
                                }
                                else if (Tokens[n] == "bool")
                                {
                                    BoolVars.Add(Tokens[n + 1]);
                                    Tokens.Remove(Tokens[n]);
                                    Tokens.Remove(Tokens[n + 1]);
                                }
                            }
                        }
                        break;
                    case "C++-Code":
                        if (Tokens[i + 1] == "[")
                        {
                            int location = 0;
                            for (int l = 0; l < Lines.Count; l++)
                                if (Lines[l].Contains("C++-Code["))
                                {
                                    location = l;
                                    break;
                                }
                            for (int b = location + 1; b < Lines.Count; b++)
                                if (!Lines[b].Contains("]-End"))
                                    ConvertedTokens.Add(Lines[b]);
                                else
                                    break;
                            for (int n = i; n < Tokens.Count; n++)
                            {
                                if (Tokens[n] == "]-End")
                                    break;
                                else if (Tokens[n] == "int")
                                {
                                    IntVars.Add(Tokens[n + 1]);
                                    Tokens.Remove(Tokens[n]);
                                    Tokens.Remove(Tokens[n + 1]);
                                }
                                else if (Tokens[n] == "std::string" || Tokens[n] == "string")
                                {
                                    StringVars.Add(Tokens[n + 1]);
                                    Tokens.Remove(Tokens[n]);
                                    Tokens.Remove(Tokens[n + 1]);
                                }
                                else if (Tokens[n] == "bool")
                                {
                                    BoolVars.Add(Tokens[n + 1]);
                                    Tokens.Remove(Tokens[n]);
                                    Tokens.Remove(Tokens[n + 1]);
                                }
                            }
                        }
                        break;
                        break;
                    default:
                        //change the value of an int variable
                        if (IntVars.Contains(Tokens[i]) && Tokens[i - 1] != "int")
                            if (Tokens[i + 1] == "=" || Tokens[i + 1] == "+=")
                                ConvertedTokens.Add(Tokens[i] + " " + Tokens[i + 1] + " " + Tokens[i + 2] + ";");
                            else if (BoolVars.Contains(Tokens[i]) && Tokens[i - 1] != "bool")
                                if (Tokens[i + 1] == "=")
                                    ConvertedTokens.Add(Tokens[i] + " " + Tokens[i + 1] + " " + Tokens[i + 2] + ";");
                                else if (FunctionNames.Contains(Tokens[i]) && Tokens[i - 1] != "function")
                                {
                                    ConvertedTokens.Add(Tokens[i] + "(");
                                    for (int j = i; j < Tokens.Count; j++)
                                    {
                                        if (IntVars.Contains(Tokens[j + 1]))
                                        {
                                            ConvertedTokens.Add(Tokens[j + 1]);
                                            if (Tokens[j + 2] == ",")
                                            {
                                                ConvertedTokens[ConvertedTokens.Count - 1] += ",";
                                            }
                                            Tokens.Remove(Tokens[j + 1]);
                                        }
                                        else if (StringVars.Contains(Tokens[j + 1]))
                                        {
                                            ConvertedTokens.Add(Tokens[j + 1]);
                                            if (Tokens[j + 2] == ",")
                                            {
                                                ConvertedTokens[ConvertedTokens.Count - 1] += ",";
                                            }
                                            Tokens.Remove(Tokens[j + 1]);
                                        }
                                        else if (Tokens[j] == ")")
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
    }
}