using System.Diagnostics;

namespace DawnLangCompiler
{
    class Creation
    {
        public static void CreateCFile(string FileName)
        {
            Tokenization.ErrorOpCode = "wc100";       //wc for writing c, 100 for first potential error spot

            StreamWriter outputFile = new StreamWriter(FileName + ".cpp");

            //write each converted token to the c file
            foreach (string codeLines in Tokenization.ConvertedTokens)
                outputFile.WriteLine(codeLines);
            outputFile.Close();
        }

        public static void CompileCFile(string FileName, string OutputFileName)
        {
            Tokenization.ErrorOpCode = "cf100";              //cf for compile file, 100 for first potential error spot

            if (File.Exists(OutputFileName))    //if the compiled binary already exists with that name, delete it/overwrite it
                File.Delete(OutputFileName);

            Tokenization.ErrorOpCode = "cf200";              //cf for compile file, 200 for second potential error spot

            Process.Start("gcc", FileName + ".cpp -w -lstdc++");  //-lstdc++ every time in case if c++ code sections are included
            Thread.Sleep(50);                   //small micro sleep for program to not error moving file since it is so new
            File.Move("./a.out", "./" + OutputFileName);
        }

        public static void Cleanup(string FileName)
        {
            Tokenization.ErrorOpCode = "cl100";              //cl for cleanup, 100 for first potential error spot

            if (File.Exists("./" + FileName + ".cpp"))    //delete the TempFile.c if it still exists (which it still should, if not, error)
                File.Delete("./" + FileName + ".cpp");
            else
                System.Environment.Exit(1);
        }
    }
}