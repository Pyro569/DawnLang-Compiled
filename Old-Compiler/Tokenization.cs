using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        private static List<string> ConstVars = new List<string>();
        private static List<string> FloatVars = new List<string>();
        private static List<string> FunctionNames = new List<string>();     //names of created functions
        private static List<string> IntListNames = new List<string>();      //list of int list names
        private static List<string> StringListNames = new List<string>();
        private static List<string> BoolListNames = new List<string>();
        public static bool DebugMode = false;
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
                if (!DebugMode)
                    Creation.Cleanup("Main");              //cleanup all of the leftovers
            }
            catch
            {   //print out the error code and do some cleanup if there is an error
                //ErrorCodeIO.ErrorCodeOutput();
                if (!DebugMode)
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
                     && Lines[i][j] != '}' && Lines[i][j] != '[' && Lines[i][j] != ']' && Lines[i][j] != '=' && Lines[i][j] != '+' && Lines[i][j] != '-'
                     && Lines[i][j] != '<' && Lines[i][j] != '>')
                        TokenString += Lines[i][j];
                    else
                    //add the string to the tokens and set string to blank
                    {
                        Tokens.Add(TokenString);
                        if (Lines[i][j] == ',' || Lines[i][j] == ')' || Lines[i][j] == '}' || Lines[i][j] == '[' || Lines[i][j] == ']' || Lines[i][j] == ';' ||
                         Lines[i][j] == '=' || Lines[i][j] == '+' || Lines[i][j] == '-' || Lines[i][j] == '<' || Lines[i][j] == '>')
                            Tokens.Add(Lines[i][j] + "");
                        TokenString = "";
                    }

                    for (int z = 0; z < Tokens.Count; z++)
                        if (Tokens[z] == "")
                            Tokens.Remove(Tokens[z]);
                }
            }

            //debug purposes only
            //foreach (string token in Tokens)
            //    System.Console.WriteLine(token);
        }

        //remove tokens from the token list, method inputs list to remove multi-line clutter
        public static void RemoveToken(List<int> index)
        {
            foreach (int indexNum in index)
                Tokens.Remove(Tokens[indexNum]);
        }

        private static void SearchForFunctions()
        {
            ErrorOpCode = "fs100"; //fs for function search, 100 for operation spot
                                   //loop through every function and check if it contains function declaration
            for (int i = 0; i < Tokens.Count; i++)
                if (Tokens[i] == "function" && Tokens[i + 1] != "main")
                    FunctionNames.Add(Tokens[i + 1]);
            //if function is declared other than main, add to function name list
        }

        private static void ConvertTokens()
        {
            int location = 0;
            //convert tokens to c code
            ErrorOpCode = "c100";       //c for conversion, 100 for first potential error spot

            for (int i = 0; i < Tokens.Count; i++)
            {
                switch (Tokens[i])
                {
                    case "print":
                        ConvertedTokens.Add("printf(" + Tokens[i + 1] + ");");              //print("hello, world"); comes out to printf("hello, world");
                        break;
                    case "print.str":
                        ConvertedTokens.Add("printf(\"%s\\n\"," + Tokens[i + 1] + ");");    //print_str(hello); comes out to printf("%s", hello);
                        break;
                    case "int":
                        if (Tokens[i + 1] != "<")
                        {
                            ConvertedTokens.Add("int " + Tokens[i + 1] + Tokens[i + 2]);  //int a = 17;
                            if (Tokens[i + 3] == "input.int.last")
                                ConvertedTokens[ConvertedTokens.Count - 1] += "intinput";
                            if (Tokens[i + 3] == "list.size")
                                ConvertedTokens[ConvertedTokens.Count - 1] += "arraySize";
                            else
                                ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[i + 3];
                            ConvertedTokens[ConvertedTokens.Count - 1] += ";";
                            IntVars.Add(Tokens[i + 1]);
                        }
                        break;
                    case "bool":
                        ConvertedTokens.Add("bool " + Tokens[i + 1] + " = " + Tokens[i + 3] + ";");
                        BoolVars.Add(Tokens[i + 1]);
                        break;
                    case "string":
                        ConvertedTokens.Add("char " + Tokens[i + 1] + "[]" + Tokens[i + 2] + Tokens[i + 3] + ";");  //string hello = "hello world!"; comes out to char[] hello = "hello world!';
                        StringVars.Add(Tokens[i + 1]);
                        break;
                    case "print.int":
                        ConvertedTokens.Add("printf(\"%d\\n\"," + Tokens[i + 1] + ");");    //print_int(a); comes out to printf("%d\n",a);
                        break;
                    case "print.float":
                        ConvertedTokens.Add("printf(\"%2.6f\\n\"," + Tokens[i + 1] + ");");    //print_float(a); comes out to printf("%d\n",a);
                        break;
                    case "for":
                        ConvertedTokens.Add("for(int " + Tokens[i + 2] + " = " + Tokens[i + 4] + "; ");
                        int stopZone = 0;
                        for (int z = i + 6; z < Tokens.Count; z++)
                            if (Tokens[z] != ";")
                                ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[z];
                            else
                            {
                                ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[z];
                                stopZone = z;
                                break;
                            }
                        ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[stopZone + 1] + Tokens[stopZone + 2] + Tokens[stopZone + 3];
                        if (Tokens[stopZone + 4] == "+" || Tokens[stopZone + 4] == "-")
                            ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[stopZone + 4];
                        ConvertedTokens[ConvertedTokens.Count - 1] += " ){";    //create a for loop
                        for (int z = i; z < stopZone; z++)
                            Tokens.Remove(Tokens[z]);
                        break;
                    case "function":
                        if (Tokens[i + 1] == "main")
                        {
                            ConvertedTokens.Add("int main(");
                            if (Tokens[i + 2] == "args")
                            {
                                ConvertedTokens[ConvertedTokens.Count - 1] += "int argc, char** argv";
                                IntVars.Add("argc");
                                StringListNames.Add("argv");
                            }
                            ConvertedTokens[ConvertedTokens.Count - 1] += "){";
                        }
                        else
                        {
                            ConvertedTokens.Add("void " + Tokens[i + 1] + "("); //function declaration with function name
                            for (int j = i; j < Tokens.Count; j++)
                            {
                                if (Tokens[j] == "int")
                                {
                                    ConvertedTokens.Add("int " + Tokens[j + 1]);    //add int and int name as parameter
                                    if (Tokens[j + 2] != ")")                       //if the third token is not a left bracket, add a comma
                                        ConvertedTokens[ConvertedTokens.Count - 1] += ",";
                                    IntVars.Add(Tokens[j + 1]);                     //add the int variable name to int vars list
                                    RemoveToken(new List<int> { j, j + 1 });
                                }
                                if (Tokens[j] == "string")
                                {
                                    ConvertedTokens.Add("char " + Tokens[j + 1] + "[]");
                                    if (Tokens[j + 2] != ")")
                                        ConvertedTokens[ConvertedTokens.Count - 1] += ",";
                                    StringVars.Add(Tokens[j + 1]);
                                    RemoveToken(new List<int> { j, j + 1 });
                                }
                                else if (Tokens[j] == "}")
                                    break;
                            }
                            ConvertedTokens.Add("){");
                        }
                        break;
                    case "if":
                        ConvertedTokens.Add("if(" + Tokens[i + 1] + Tokens[i + 2] + Tokens[i + 3] + Tokens[i + 4] + "){");
                        break;
                    case "line":
                        ConvertedTokens.Add("printf(\"\\n\");");
                        break;
                    case "input.str":
                        ConvertedTokens.Add("char strinput[255];\nscanf(\"%s\", strinput);");
                        break;
                    case "input.int":
                        ConvertedTokens.Add("int intinput;\nscanf(\"%d\", &intinput);");
                        break;
                    case "input.int.last":
                        ConvertedTokens.Add("printf(\"%d\", intinput);");
                        break;
                    case "input.str.last":
                        ConvertedTokens.Add("printf(\"%s\", strinput);");
                        break;
                    case "else":
                        ConvertedTokens.Add("else");
                        if (Tokens[i + 1] != "if")
                            ConvertedTokens[ConvertedTokens.Count - 1] += "{";
                        break;
                    case "}":
                        ConvertedTokens.Add("}");
                        break;
                    case "while":
                        int stopPoint = 0;
                        ConvertedTokens.Add("while(");
                        for (int k = i + 1; k < Tokens.Count; k++)
                            if (Tokens[k] != ")")
                                ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[k];
                            else
                            {
                                ConvertedTokens[ConvertedTokens.Count - 1] += ")";
                                stopPoint = k;
                                break;
                            }
                        ConvertedTokens[ConvertedTokens.Count - 1] += "{";
                        for (int l = i + 1; l < stopPoint; l++)
                            Tokens.Remove(Tokens[l]);
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
                                ConvertedTokens[ConvertedTokens.Count - 1] += "<stdio.h>";
                                break;
                            case "dawnlang.data.types":
                                ConvertedTokens[ConvertedTokens.Count - 1] += "<stdbool.h>";
                                break;
                            case "dawnlang.io.args":
                                ConvertedTokens[ConvertedTokens.Count - 1] += "<string.h>";
                                break;
                        }
                        break;
                    case "List":
                        List<string> TokensToRemove = new List<string>();
                        switch (Tokens[i + 2])
                        {
                            case "int":
                                IntListNames.Add(Tokens[i + 4]);
                                ConvertedTokens.Add("int " + Tokens[i + 4] + "[] = {");
                                for (int z = i + 7; z < Tokens.Count; z++)
                                    if (Tokens[z] != "]")
                                    {
                                        ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[z];
                                        TokensToRemove.Add(Tokens[z]);
                                    }
                                    else
                                    {
                                        ConvertedTokens[ConvertedTokens.Count - 1] += "};";
                                        TokensToRemove.Add(Tokens[z]);
                                        break;
                                    }
                                break;
                            case "string":
                                StringListNames.Add(Tokens[i + 4]);
                                ConvertedTokens.Add("char " + Tokens[i + 4] + "[][255] = {");
                                for (int z = i + 7; z < Tokens.Count; z++)
                                    if (Tokens[z] != "]")
                                        ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[z];
                                    else
                                    {
                                        ConvertedTokens[ConvertedTokens.Count - 1] += "};";
                                        TokensToRemove.Add(Tokens[z]);
                                        break;
                                    }
                                break;
                            case "bool":
                                BoolListNames.Add(Tokens[i + 4]);
                                ConvertedTokens.Add("bool " + Tokens[i + 4] + "[] = {");
                                for (int z = i + 7; z < Tokens.Count; z++)
                                    if (Tokens[z] != "]")
                                        ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[z];
                                    else
                                    {
                                        ConvertedTokens[ConvertedTokens.Count - 1] += "};";
                                        TokensToRemove.Add(Tokens[z]);
                                        break;
                                    }
                                break;
                        }
                        for (int l = 0; l < TokensToRemove.Count; l++)
                            Tokens.Remove(Tokens[l]);
                        break;
                    case "list.size":
                        if (!ConvertedTokens.Contains("int arraySize"))
                            ConvertedTokens.Add("int");
                        ConvertedTokens[ConvertedTokens.Count - 1] += " arraySize = sizeof(";
                        ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[i + 1] + ")/4;";
                        break;
                    case "list.element.add":
                        if (!ConvertedTokens.Contains("int arraySize"))
                            ConvertedTokens.Add("int");
                        ConvertedTokens[ConvertedTokens.Count - 1] += " arraySize = sizeof(";
                        ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[i + 1] + ")/4;";
                        ConvertedTokens.Add(Tokens[i + 1] + "[arraySize] = " + Tokens[i + 3] + ";");
                        break;
                    case "print.list.element":
                        if (IntListNames.Contains(Tokens[i + 1]))
                            ConvertedTokens.Add("printf(\"%d\\n\", " + Tokens[i + 1] + "[" + Tokens[i + 3] + "]);");
                        if (BoolListNames.Contains(Tokens[i + 1]))
                            ConvertedTokens.Add("printf(\"%d\\n\", " + Tokens[i + 1] + "[" + Tokens[i + 3] + "]);");
                        if (StringListNames.Contains(Tokens[i + 1]))
                            ConvertedTokens.Add("printf(\"%s\\n\", " + Tokens[i + 1] + "[" + Tokens[i + 3] + "]);");
                        break;
                    case "switch":
                        if (IntVars.Contains(Tokens[i + 1]) || BoolVars.Contains(Tokens[i + 1]) || StringVars.Contains(Tokens[i + 1]))
                            ConvertedTokens.Add("switch(" + Tokens[i + 1] + "){");
                        RemoveToken(new List<int> { i, i + 1 });
                        break;
                    case "case":
                        ConvertedTokens.Add("case " + Tokens[i + 1]);
                        RemoveToken(new List<int> { i + 1 });
                        break;
                    case "break":
                        ConvertedTokens.Add("break;");
                        break;
                    case "C":
                        if (Tokens[i + 1] == "-" && Tokens[i + 2] == "Code" && Tokens[i + 3] == "[")
                        {
                            for (int l = location; l < Lines.Count; l++)
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
                                if (Tokens[n] == "]" && Tokens[n + 1] == "-" && Tokens[n + 2] == "End")
                                    break;
                                else if (Tokens[n] == "int")
                                {
                                    IntVars.Add(Tokens[n + 1]);
                                    RemoveToken(new List<int> { n, n + 1 });
                                }
                                else if (Tokens[n] == "char[]")
                                {
                                    StringVars.Add(Tokens[n + 1]);
                                    RemoveToken(new List<int> { n, n + 1 });
                                }
                                else if (Tokens[n] == "bool")
                                {
                                    BoolVars.Add(Tokens[n + 1]);
                                    RemoveToken(new List<int> { n, n + 1 });
                                }
                                else if (Tokens[n] == "void")
                                {
                                    FunctionNames.Add(Tokens[n + 1]);
                                    RemoveToken(new List<int> { n, n + 1 });
                                }
                            }
                        }
                        else if (Tokens[i + 1] == "+" && Tokens[i + 2] == "+" && Tokens[i + 3] == "-" && Tokens[i + 4] == "Code" && Tokens[i + 5] == "[")
                        {
                            for (int l = location; l < Lines.Count; l++)
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
                                if (Tokens[n] == "]" && Tokens[n + 1] == "-" && Tokens[n + 2] == "End")
                                    break;
                                else if (Tokens[n] == "int")
                                {
                                    IntVars.Add(Tokens[n + 1]);
                                    RemoveToken(new List<int> { n, n + 1 });
                                }
                                else if (Tokens[n] == "std::string" || Tokens[n] == "string")
                                {
                                    StringVars.Add(Tokens[n + 1]);
                                    RemoveToken(new List<int> { n, n + 1 });
                                }
                                else if (Tokens[n] == "bool")
                                {
                                    BoolVars.Add(Tokens[n + 1]);
                                    RemoveToken(new List<int> { n, n + 1 });
                                }
                                else if (Tokens[n] == "void")
                                {
                                    FunctionNames.Add(Tokens[n + 1]);
                                    RemoveToken(new List<int> { n, n + 1 });
                                }
                            }
                        }
                        for (int k = i; k < Tokens.Count; k++)
                            if (Tokens[k] == "]" && Tokens[k + 1] == "-" && Tokens[k + 2] == "End")
                                break;
                            else
                                RemoveToken(new List<int> { k });
                        break;
                    case "return":
                        ConvertedTokens.Add("return ");
                        for (int k = i + 1; k < Tokens.Count; k++)
                            if (Tokens[k] != ";")
                                ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[k];
                            else
                            {
                                ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[k];
                                break;
                            }
                        break;
                    case "constant.int":
                        ConvertedTokens.Add("const int ");
                        for (int k = i + 1; k < Tokens.Count; k++)
                        {
                            if (Tokens[k] != ";")
                                ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[k];
                            else
                            {
                                ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[k];
                                break;
                            }
                        }
                        break;
                    case "float":
                        ConvertedTokens.Add("float ");
                        for (int k = i + 1; k < Tokens.Count; k++)
                        {
                            if (Tokens[k] != ";")
                                ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[k];
                            else
                            {
                                ConvertedTokens[ConvertedTokens.Count - 1] += Tokens[k];
                                break;
                            }
                        }
                        FloatVars.Add(Tokens[i + 1]);
                        break;
                    default:
                        //change the value of an int variable
                        if (IntVars.Contains(Tokens[i]) && Tokens[i - 1] != "int" && Tokens[i + 1] == "=")
                        {
                            if (Tokens[i + 2] != "=" && Tokens[i + 2] != "<" && Tokens[i + 2] != ">" && Tokens[i + 2] != "list.size")
                                ConvertedTokens.Add(Tokens[i] + " " + Tokens[i + 1] + " " + Tokens[i + 2] + ";");
                        }
                        else if (IntVars.Contains(Tokens[i]) && Tokens[i - 1] != "int" && Tokens[i + 1] == "+" && Tokens[i + 2] == "=")
                        {
                            if (Tokens[i + 3] != "=" && Tokens[i + 3] != "<" && Tokens[i + 3] != ">")
                                ConvertedTokens.Add(Tokens[i] + " " + Tokens[i + 1] + Tokens[i + 2] + " " + Tokens[i + 3] + ";");
                        }
                        else if (IntVars.Contains(Tokens[i]) && Tokens[i + 2] == "input.int.last")
                        {
                            ConvertedTokens.Add(Tokens[i] + " = intinput;");
                            RemoveToken(new List<int> { i, i + 1, i + 2 });
                        }
                        else if (IntVars.Contains(Tokens[i]) && Tokens[i + 2] == "list.size")
                        {
                            if (!ConvertedTokens.Contains("int arraySize"))
                                ConvertedTokens.Insert(ConvertedTokens.Count - 1, "int");
                            ConvertedTokens[ConvertedTokens.Count - 2] += " arraySize = sizeof(";
                            ConvertedTokens[ConvertedTokens.Count - 2] += Tokens[i + 3] + ")/4;";
                            ConvertedTokens.Add(Tokens[i] + " = arraySize;");
                            RemoveToken(new List<int> { i, i + 1, i + 2 });
                        }
                        else if (StringVars.Contains(Tokens[i]) && Tokens[i + 2] == "input.str.last")
                        {
                            ConvertedTokens.Add(Tokens[i] + " " + Tokens[i + 1] + " strinput;");
                            RemoveToken(new List<int> { i, i + 1, i + 2 });
                        }
                        else if (BoolVars.Contains(Tokens[i]) && Tokens[i - 1] != "bool")
                        {
                            if (Tokens[i + 1] == "=")
                                ConvertedTokens.Add(Tokens[i] + " " + Tokens[i + 1] + " " + Tokens[i + 2] + ";");
                        }
                        else if (FunctionNames.Contains(Tokens[i]) && Tokens[i - 1] != "function")
                        {
                            ConvertedTokens.Add(Tokens[i] + "(");
                            for (int j = i; j < Tokens.Count; j++)
                                if (IntVars.Contains(Tokens[j + 1]))
                                {
                                    ConvertedTokens.Add(Tokens[j + 1]);
                                    if (Tokens[j + 2] == ",")
                                        ConvertedTokens[ConvertedTokens.Count - 1] += ",";
                                    Tokens.Remove(Tokens[j + 1]);
                                }
                                else if (StringVars.Contains(Tokens[j + 1]))
                                {
                                    ConvertedTokens.Add(Tokens[j + 1]);
                                    if (Tokens[j + 2] == ",")
                                        ConvertedTokens[ConvertedTokens.Count - 1] += ",";
                                    Tokens.Remove(Tokens[j + 1]);
                                }
                                else if (Tokens[j] == ")")
                                    break;
                            ConvertedTokens[ConvertedTokens.Count - 1] += ");";
                        }
                        break;
                }
            }

            //debug purposes only
            //foreach (string tokens in ConvertedTokens)
            //    System.Console.WriteLine(tokens);
        }

        public static void LogTokens()  //write all tokens to a file in order to analyze them to potentially find the error
        {
            using (StreamWriter streamWriter = new StreamWriter("Tokens.txt"))
                for (int i = 0; i < ConvertedTokens.Count; i++)
                    streamWriter.Write(ConvertedTokens[i] + "\n");
        }
    }
}