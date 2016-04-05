// VMtranslator 
// Takes in VM files, translates them into Hack assembly code
// Translates the push and pop arguments

//TEGAN STRALEY & CATIE COOK & IAN WIGGINS & LUCIA GUATNEY
//FILE: VMtranslator.cs
//PROJECT: created for project 7 & 8 of NAND2Tetris course

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace Project7_8
{
    class Program
    {
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~MAIN~~~~~~~
        public static void Main(string[] args)
        {
            Console.WriteLine("Enter in the .vm file/directory you wish to convert to .asm : ");
            string vmFileDirName = Console.ReadLine();

            if(File.Exists(vmFileDirName)) //Single file, just process.
            {
                ProcessFile(vmFileDirName);
            }
            else if(Directory.Exists(vmFileDirName)) //Directory, process each file in it.
            {
                int lastSlash = vmFileDirName.LastIndexOf("\\",0);
                string dirName = vmFileDirName.Substring(lastSlash + 1);

                //Using a single output and single log file for all the files in the directory.
                using(var output = new StreamWriter(dirName + ".asm"))
                using(var log = new StreamWriter(dirName + ".log")) {

                    string[] fileEntries = Directory.GetFiles(vmFileDirName,"*.vm",SearchOption.TopDirectoryOnly);
                    foreach(string fileName in fileEntries)
                        ProcessFile(fileName,output,log);
                }
            }

            else{
        	Console.WriteLine("ERROR : no file/directory was found by that name...");
            }

            Console.WriteLine("Translation complete. . .");
            Console.ReadKey();
        }

        static void ProcessFile(string file,StreamWriter output = null,StreamWriter log = null)
        {
            //change .asm to .hack
            char[] asmFileName = new char[file.Length - 2];
            for(int i = 0; i < file.Length - 2; i++) {
                asmFileName[i] = file[i];
            }
            Console.WriteLine("Processing file: {0}",file);
            string asmFileNameString = new string(asmFileName);
            string logFileNameString = new string(asmFileName);
            logFileNameString = string.Concat(asmFileNameString,"log"); //making a .log to fill with same as what we Console.WriteLine();
            asmFileNameString = string.Concat(asmFileNameString,"asm"); //.asm is now .hack

            using(var input = new StreamReader(file)) {

                    //Output streams were passed to the function
                    //(that means this is a file in a directory, and all the files use the same output)
                    if(output != null && log != null) Parser.parse(input,output,log);

                    //Output streams were not passed
                    //(This is a single file beng parsed, generate the output files now)
                    else
                        using(var noutput = new StreamWriter(asmFileNameString))
                            using(var nlog = new StreamWriter(logFileNameString))
                                Parser.parse(input,noutput,nlog);
            }
        }

    }
}



