﻿using System;
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
            string genCommentInput = Console.ReadLine().ToLower();
            bool genComments;

            if (genCommentInput == "y" || genCommentInput == "yes")
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
                using (var output = new StreamWriter(dirName + ".xml"))
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
            string xmlFileNameString = new string(vmFileName);
            logFileNameString = string.Concat(vmFileNameString, "log"); //making a .log to fill with same as what we Console.WriteLine();
            xmlFileNameString = string.Concat(vmFileNameString, "xml");
            vmFileNameString = string.Concat(vmFileNameString, "vm");

            Console.WriteLine("\n");

            using (var input = new StreamReader(file))
            {
                Tokenizer tokenizeThis = new Tokenizer(input, genComments);

                //Output streams were passed to the function
                //(that means this is a file in a directory, and all the files use the same output)
                //if (output != null && log != null)
                {
                    Tokenizer.Token token;
                    do {
                        token = tokenizeThis.tokenize();
                        tokenList.Add(token);
                    }
                    while(token != null);
                }
            }//end of using

            CodeEngine codeEngine = new CodeEngine(vmFileNameString);
            Parser parser = new Parser(codeEngine);
            parser.Parse(tokenList);

            //foreach (Tokenizer.Token thisToken in tokenList)
            //{
            //    if (thisToken != null)
            //    {
            //        Console.WriteLine(thisToken.context + "   " + thisToken.type);
            //    }
            //}//end of writing out tokens to screen

            ////send tokenList to XMLGenerator and output file
            //if(output == null)
            //    using(var out2 = new StreamWriter(xmlFileNameString))
            //        XMLGenerator.generateXML(tokenList,out2);
            //else XMLGenerator.generateXML(tokenList,output);

        }

    }//end of class
}//end of namespace
