// VMtranslator 
// Takes in VM files, translates them 
// Translates the push and pop arguments

//~~~~~~~~~~~~~ PARSER MODULE ~~~~~~~~~~~~~~

//Call Constructor
	//open .asm file
		//creat array of strings with fule data

	//error message if no file present that works

//Call Parseer function
		//remove all white space and comments 
		
//Bool moreCommands() that constantly checks if there are more commands present
	//while(moreCommands();
		//Read next commmand from input file and make current command
		//untill the boolean function moreCommands() is false
		//meaning there are no more commands

		//While there ARE still commands
			//call commandType() to return the type of command encountered. 
					//examples: C_ARITHEMETIC, C_PUSH, C_POP, C_LABEL, C_GOTO, C_IF, C_FUNCTION, C_RETURN, C_CALL
			//call string arg1()
					//return the first argument of current command
					//not called if the current command is C_RETURN because that means its the end 
			// call int arg2()
					//returns second argument of the current command. 
					//Called only if current command is C_PUSH, C_POP, C_FUNCTION, C_CALL 

//~~~~~~~~~~~~~ CODEWRITER ~~~~~~~~~~~~~~

//Call constructor (ostream out)
		//opens output file/stream and prepares to write to it 	
	//setFileName(string fileName) 
			//iterate through data array?
			//Call writeArithmetic (strong command) to write assembly code translated 
			//from given arithemetic commmand
			//Call WritePushPop(pop/push command, string segment, int index)
				//writes assembly code translated from current command, either C_PUSH or C_POP
		//When all lines have been read out
			//close file


//TEGAN STRALEY & CATIE COOK
//FILE: VMtranslator.cs
//PROJECT: created for project 7 of NAND2Tetris course

//File converts .vm input file to hack assmebly code. The resulting code
//in displayed out to user and also written to a corresponding output file. 

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
                        ProcessFile(fileName);
                }
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



