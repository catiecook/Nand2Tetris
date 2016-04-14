using System;
using System.Collections.Generic;
using System.Linq;
//Program: Jack compiler for the Nand2tetris coursework


using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace Project10
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter in the .jack file/directory you wish to convert to .vm : ");
            string jackFileDirName = Console.ReadLine();
            Console.WriteLine("Would you like the compiler to generate XML comments as well? (y/n) : ");
            string genCommentInput = Console.ReadLine();
            bool genComments;

            if (genCommentInput == "y" || genCommentInput == "Y" || genCommentInput == "yes" || genCommentInput == "Yes" || genCommentInput == "YES")
            {
                genComments = true;
            }
            else
            {
                genComments = false;
            }

            if (File.Exists(jackFileDirName)) //Single file, just process.
            {
                ProcessFile(jackFileDirName, genComments);
                Console.WriteLine("found file");
            }
            else if (Directory.Exists(jackFileDirName)) //Directory, process each file in it.
            {
                Console.WriteLine("found directory");
                int lastSlash = jackFileDirName.LastIndexOf("\\", 0);
                string dirName = jackFileDirName.Substring(lastSlash + 1);

                //Using a single output and single log file for all the files in the directory.
                using (var output = new StreamWriter(dirName + ".vm"))
                using (var log = new StreamWriter(dirName + ".log"))
                {

                    string[] fileEntries = Directory.GetFiles(jackFileDirName, "*.jack", SearchOption.TopDirectoryOnly);
                    foreach (string fileName in fileEntries)
                        ProcessFile(fileName, genComments, output, log);
                }
            }

            else {
                Console.WriteLine("ERROR : no file/directory was found by that name...");
            }

            Console.WriteLine("Translation complete. . .");
            Console.ReadKey();
        }//end of Main



        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~PROCESS FILE FUNCTION~~~~~~~~~~~~~~~~~~

        static void ProcessFile(string file, bool genComments, StreamWriter output = null, StreamWriter log = null)
        {
            //create List of tokens
            List<Tokenizer.Token> tokenList = new List<Tokenizer.Token>();

            //change .jack to .vm
            char[] vmFileName = new char[file.Length - 4];
            for (int i = 0; i < file.Length - 4; i++)
            {
                vmFileName[i] = file[i];
            }
            Console.WriteLine("Processing file: {0}", file);
            string vmFileNameString = new string(vmFileName);
            string logFileNameString = new string(vmFileName);
            logFileNameString = string.Concat(vmFileNameString, "log"); //making a .log to fill with same as what we Console.WriteLine();
            vmFileNameString = string.Concat(vmFileNameString, "vm");

            Console.WriteLine("\n");

            using (var input = new StreamReader(file))
            {
                Tokenizer tokenizeThis = new Tokenizer(input, genComments);

                //Output streams were passed to the function
                //(that means this is a file in a directory, and all the files use the same output)
                if (output != null && log != null)
                {
                    Tokenizer.Token token = tokenizeThis.tokenize();
                    while (token != null)
                    {
                        token = tokenizeThis.tokenize();
                        tokenList.Add(token);
                    }

                }

            }

            foreach (Tokenizer.Token thisToken in tokenList)
            {
                if (thisToken != null)
                {
                    Console.WriteLine(thisToken.context);
                }

            }

        }



    }//end of class
}//end of namespace
