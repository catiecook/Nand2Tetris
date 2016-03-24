using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Project7_8
{
    static class Parser
    {
        private static class Constants
        {
            private static int jmpIndex = 0;

            public static string jmp(string type)
            {
                string ret =
                        "@SP\nAM=M-1\nD=M\nA=A-1\nD=M-D\n@FALSE" +
                        jmpIndex + "\n" +
                        "D;" +
                        type +
                        "\n@SP\nA=M-1\nM=-1\n@CONTINUE" +
                        jmpIndex + "\n" +
                        "0;JMP\n(FALSE" +
                        jmpIndex +
                        ")\n@SP\nA=M-1\nM=0\n(CONTINUE" +
                        jmpIndex +
                        ")\n";
                ++jmpIndex;
                return ret;
            }

            //pops off the last 2 in the stack 
            public const string popStack2 = "@SP\nAM=M-1\nD=M\nA=A-1\n";
        }

        private static string pushCode1(string segment,int index,bool isDirect)
        {
            //if isDirect true = ""
            //if isDirect false = "@" + index + "\n" + "A=D+A\nD=M\n"
            string noPointerCode = (isDirect) ? "" : "@" + index + "\n" + "A=D+A\nD=M\n";

            return "@" + segment + "\n" +
                    "D=M\n" +
                    noPointerCode +
                    "@SP\n" +
                    "A=M\n" +
                    "M=D\n" +
                    "@SP\n" +
                    "M=M+1\n";
        }//end of pushTemplate1

        private static string popCode1(string segment,int index,bool isDirect)
        {
            //if isDirect true = "D=A\n"
            //if isDirect false = "D=M\n@" + index + "\nD=D+A\n"
            string noPointerCode = (isDirect) ? "D=A\n" : "D=M\n@" + index + "\nD=D+A\n";

            return "@" + segment + "\n" +
                    noPointerCode +
                    "@R13\n" +
                    "M=D\n" +
                    "@SP\n" +
                    "AM=M-1\n" +
                    "D=M\n" +
                    "@R13\n" +
                    "A=M\n" +
                    "M=D\n";
        }

        private static class Parse
        {
            public static string add()
            {
                return Constants.popStack2 + "\nM=M+D\n";
            }

            public static string sub()
            {
                return Constants.popStack2 + "\nM=M-D\n";
            }

            public static string neg()
            {
                return "D=0\n@SP\nA=M-1\nM=D-M\n";
            }

            public static string eq()
            {
                return Constants.jmp("JNE");
            }

            public static string gt()
            {
                return Constants.jmp("JLE");
            }

            public static string lt()
            {
                return Constants.jmp("JGE");
            }

            public static string and()
            {
                return Constants.popStack2 + "M=M&D\n";
            }

            public static string or()
            {
                return Constants.popStack2 + "M=M|D\n";
            }

            public static string not()
            {
                return "@SP\nA=M-1\nM=!M\n";
            }

            public static string push(string loc,string arg)
            {
                /*CONSTANT: "@" + arg + "\n" + "D=A\n@SP\nA=M\nM=D\n@SP\nM=M+1\n"; */

                switch(loc)
                {
                case "argument":
                    //TODO
                    break;
                case "local":
                    //TODO
                    break;
                case "static":
                    //TODO
                    break;
                case "constant":
                    //TODO
                    break;
                case "this":
                    //TODO
                    break;
                case "that":
                    //TODO
                    break;
                case "pointer":
                    //TODO
                    break;
                case "temp":
                    //TODO
                    break;
                default: throw new ArgumentException("Invalid memory segment [" + loc + "]");
                }
            }

            public static string pop(string loc,string arg)
            {
                //TODO
            }
        }


        public static void parse(StreamReader fileInput,StreamWriter fileOutput)
        {
            int lnnum = 0;
            do try {
                    Console.WriteLine("Parsing line [{0}]",lnnum);
                    string line = fileInput.ReadLine();

                    var subs = line.Split(' ');

                    string command = subs[0].ToLower();
                
                    switch(command)
                    {
                    case "add": fileOutput.Write(Parse.add()); fileOutput.WriteLine(); break;
                    case "sub": fileOutput.Write(Parse.sub()); fileOutput.WriteLine(); break;
                    case "neg": fileOutput.Write(Parse.neg()); fileOutput.WriteLine(); break;
                    case "eq":  fileOutput.Write(Parse.eq()); fileOutput.WriteLine(); break;
                    case "gt":  fileOutput.Write(Parse.gt()); fileOutput.WriteLine(); break;
                    case "lt":  fileOutput.Write(Parse.lt()); fileOutput.WriteLine(); break;
                    case "and": fileOutput.Write(Parse.and()); fileOutput.WriteLine(); break;
                    case "or": fileOutput.Write(Parse.or()); fileOutput.WriteLine(); break;
                    case "not": fileOutput.WriteLine(Parse.not()); fileOutput.WriteLine(); break;
                    case "push": fileOutput.WriteLine(Parse.push(subs[1],subs[2])); fileOutput.WriteLine(); break;
                    case "pop": fileOutput.WriteLine(Parse.pop(subs[1],subs[2])); fileOutput.WriteLine(); break;
                    default: throw new NotImplementedException("Operator \"" + command + "\" not found.");
                    }

                    ++lnnum;
                }
                catch(Exception err) {
                        Console.WriteLine("Error on line [{0}]: {1}",lnnum,err.Message);
            }
            while(!fileInput.EndOfStream);


            //only to take out the comments
            for(int i = 0; i < line.Length && keepGoing == true; i++) {
                if(line[i] == '/' && line[i + 1] == '/') {
                    //comment has been found! Don't copy any of the rest of the line
                    keepGoing = false; //set keepGoing to false and will fall out of for loop
                }
                else if(line[i] == '\n') {
                    keepGoing = false;
                }
                else if(char.IsLetterOrDigit(line[i]) || line[i] == '@' || line[i] == '.' || line[i] == '(' || line[i] == ')' || line[i] == '_' || line[i] == '-' || line[i] == '$' || line[i] == '+' || line[i] == ';' || line[i] == '*' || line[i] == '/' || line[i] == '=' || line[i] == '!' || line[i] == '|' || line[i] == '&' || line[i] == ' ') {
                    parsedLine[j] = line[i];
                    j++; //only increment j if [a-zA-Z0-9]*$ has been found in line[i]
                }
                else {
                    Console.WriteLine("ERROR: cannot parse line : " + line); //error checking
                    logOutput.WriteLine("ERROR: cannot parse line : " + line);
                    keepGoing = false;
                }
            }//end of for

            int howFull = 0;
            for(int i = 0; i < line.Length; i++) {
                if(parsedLine[i] == '\0') {
                    //don't add to howFull
                }
                else {
                    howFull += 1;
                }
            }
            char[] newResult = new char[howFull];
            //copy contents of parsedLine into newResult
            for(int i = 0; i < newResult.Length; i++) {
                newResult[i] = parsedLine[i];
            }

            string parsedString = new string(newResult);

            if(newResult.Length != 0) //if parsedLine.Length == 0 means it's an empty array! Just skip this line.
            {
                Console.WriteLine("After taking out comments : " + parsedString);
                logOutput.WriteLine("After taking out comments : " + parsedString);
                string[] splitted = parsedString.Split(' ');

                if(splitted.Length > 3) {
                    Console.WriteLine("ERROR: Too many arguments in the line : " + parsedString);
                }
                Console.WriteLine("number of strings in splitted :  " + splitted.Length);
                logOutput.WriteLine("number of strings in splitted :  " + splitted.Length);
                if(splitted[0] == "add" || splitted[0] == "sub" || splitted[0] == "neg" || splitted[0] == "eq" || splitted[0] == "gt" || splitted[0] == "lt" || splitted[0] == "and" || splitted[0] == "or" || splitted[0] == "not") {
                    argType = ARITHMETIC;
                    argument1 = splitted[0];
                }
                else if(splitted[0] == "return") {
                    argType = RETURN;
                    argument1 = splitted[0];
                }
                else {
                    argument1 = splitted[1];

                    if(splitted[0] == "push") {
                        argType = PUSH;
                    }
                    else if(splitted[0] == "pop") {
                        argType = POP;
                    }
                    else if(splitted[0] == "label") {
                        argType = LABEL;
                    }
                    else if(splitted[0] == "if") {
                        argType = IF;
                    }
                    else if(splitted[0] == "goto") {
                        argType = GOTO;
                    }
                    else if(splitted[0] == "function") {
                        argType = FUNCTION;
                    }
                    else if(splitted[0] == "call") {
                        argType = CALL;
                    }
                    else {
                        Console.WriteLine("ERROR: unknown command type on line : " + parsedString);
                        logOutput.WriteLine("ERROR: unknown command type on line : " + parsedString);
                    }

                    if(argType == PUSH || argType == POP || argType == FUNCTION || argType == CALL) {
                        if(int.TryParse(splitted[2],out argument2)) {
                            Console.WriteLine("argument2 was an integer.");
                            logOutput.WriteLine("argument2 was an integer.");
                        }
                        else {
                            Console.WriteLine("ERROR : argument2 was not an integer on line : " + parsedString);
                            logOutput.WriteLine("ERROR : argument2 was not an integer on line : " + parsedString);
                        }
                    }//end of if
                }//end of else

                Console.WriteLine("ArgType : " + argType);
                Console.WriteLine("argument1 : " + argument1);
                Console.WriteLine("argument2 : " + argument2);
                logOutput.WriteLine("ArgType : " + argType);
                logOutput.WriteLine("argument1 : " + argument1);
                logOutput.WriteLine("argument2 : " + argument2);

                if(argType == ARITHMETIC) {
                    if(splitted[0] == "add") {
                        logOutput.WriteLine(arithmeticCode1() + "M=M+D\n");
                        Console.WriteLine(arithmeticCode1() + "M=M+D\n");
                        fileOutput.WriteLine(arithmeticCode1() + "M=M+D\n");
                    }
                    else if(splitted[0] == "sub") {

                        logOutput.WriteLine(arithmeticCode1() + "M=M-D\n");
                        Console.WriteLine(arithmeticCode1() + "M=M-D\n");
                        fileOutput.WriteLine(arithmeticCode1() + "M=M-D\n");

                    }
                    else if(splitted[0] == "and") {

                        logOutput.WriteLine(arithmeticCode1() + "M=M&D\n");
                        Console.WriteLine(arithmeticCode1() + "M=M&D\n");
                        fileOutput.WriteLine(arithmeticCode1() + "M=M&D\n");

                    }
                    else if(splitted[0] == "or") {
                        logOutput.WriteLine(arithmeticCode1() + "M=M|D\n");
                        Console.WriteLine(arithmeticCode1() + "M=M|D\n");
                        fileOutput.WriteLine(arithmeticCode1() + "M=M|D\n");

                    }
                    else if(splitted[0] == "gt") {
                        logOutput.WriteLine(arithmeticCode2("JLE"));
                        Console.WriteLine(arithmeticCode2("JLE"));
                        fileOutput.WriteLine(arithmeticCode2("JLE"));
                        arthJumpFlag++;

                    }
                    else if(splitted[0] == "lt") {
                        logOutput.WriteLine(arithmeticCode2("JGE"));
                        Console.WriteLine(arithmeticCode2("JGE"));
                        fileOutput.WriteLine(arithmeticCode2("JGE"));
                        arthJumpFlag++;

                    }
                    else if(splitted[0] == "eq") {
                        logOutput.WriteLine(arithmeticCode2("JNE"));
                        Console.WriteLine(arithmeticCode2("JNE"));
                        fileOutput.WriteLine(arithmeticCode2("JNE"));
                        arthJumpFlag++;

                    }
                    else if(splitted[0] == "not") {
                        logOutput.WriteLine("@SP\nA=M-1\nM=!M\n");
                        Console.WriteLine("@SP\nA=M-1\nM=!M\n");
                        fileOutput.WriteLine("@SP\nA=M-1\nM=!M\n");
                    }
                    else if(splitted[0] == "neg") {
                        logOutput.WriteLine("D=0\n@SP\nA=M-1\nM=D-M\n");
                        Console.WriteLine("D=0\n@SP\nA=M-1\nM=D-M\n");
                        fileOutput.WriteLine("D=0\n@SP\nA=M-1\nM=D-M\n");
                    }
                    else {
                        Console.WriteLine("ERROR : called writeArithmetic for a non-arithmetic command");
                        logOutput.WriteLine("ERROR : called writeArithmetic for a non-arithmetic command");
                    }
                }
                else if(argType == POP || argType == PUSH) {
                    if(argType == PUSH) {

                        if(splitted[1] == "constant") {
                            logOutput.WriteLine("@" + argument2 + "\n" + "D=A\n@SP\nA=M\nM=D\n@SP\nM=M+1\n");
                            Console.WriteLine("@" + argument2 + "\n" + "D=A\n@SP\nA=M\nM=D\n@SP\nM=M+1\n");
                            fileOutput.WriteLine("@" + argument2 + "\n" + "D=A\n@SP\nA=M\nM=D\n@SP\nM=M+1\n");
                        }
                        else if(splitted[1] == "local") {
                            logOutput.WriteLine(pushCode1("LCL",argument2,false));
                            Console.WriteLine(pushCode1("LCL",argument2,false));
                            fileOutput.WriteLine(pushCode1("LCL",argument2,false));

                        }
                        else if(splitted[1] == "argument") {
                            logOutput.WriteLine(pushCode1("ARG",argument2,false));
                            Console.WriteLine(pushCode1("ARG",argument2,false));
                            fileOutput.WriteLine(pushCode1("ARG",argument2,false));
                        }
                        else if(splitted[1] == "this") {
                            logOutput.WriteLine(pushCode1("THIS",argument2,false));
                            Console.WriteLine(pushCode1("THIS",argument2,false));
                            fileOutput.WriteLine(pushCode1("THIS",argument2,false));

                        }
                        else if(splitted[1] == "that") {
                            logOutput.WriteLine(pushCode1("THAT",argument2,false));
                            Console.WriteLine(pushCode1("THAT",argument2,false));
                            fileOutput.WriteLine(pushCode1("THAT",argument2,false));

                        }
                        else if(splitted[1] == "temp") {
                            logOutput.WriteLine(pushCode1("R5",argument2 + 5,false));
                            Console.WriteLine(pushCode1("R5",argument2 + 5,false));
                            fileOutput.WriteLine(pushCode1("R5",argument2 + 5,false));

                        }
                        else if(splitted[1] == "pointer" && argument2 == 0) {
                            logOutput.WriteLine(pushCode1("THIS",argument2,true));
                            Console.WriteLine(pushCode1("THIS",argument2,true));
                            fileOutput.WriteLine(pushCode1("THIS",argument2,true));
                        }
                        else if(splitted[1] == "pointer" && argument2 == 1) {
                            logOutput.WriteLine(pushCode1("THAT",argument2,true));
                            Console.WriteLine(pushCode1("THAT",argument2,true));
                            fileOutput.WriteLine(pushCode1("THAT",argument2,true));

                        }
                        else if(splitted[1] == "static") {
                            string mystring = (16 + argument2).ToString();
                            logOutput.WriteLine(pushCode1(mystring,argument2,true));
                            Console.WriteLine(pushCode1(mystring,argument2,true));
                            fileOutput.WriteLine(pushCode1(mystring,argument2,true));
                        }

                    }
                    else if(argType == POP) {

                        if(splitted[1] == "local") {
                            logOutput.WriteLine(popCode1("LCL",argument2,false));
                            Console.WriteLine(popCode1("LCL",argument2,false));
                            fileOutput.WriteLine(popCode1("LCL",argument2,false));
                        }
                        else if(splitted[1] == "argument") {
                            logOutput.WriteLine(popCode1("ARG",argument2,false));
                            Console.WriteLine(popCode1("ARG",argument2,false));
                            fileOutput.WriteLine(popCode1("ARG",argument2,false));
                        }
                        else if(splitted[1] == "this") {
                            logOutput.WriteLine(popCode1("THIS",argument2,false));
                            Console.WriteLine(popCode1("THIS",argument2,false));
                            fileOutput.WriteLine(popCode1("THIS",argument2,false));
                        }
                        else if(splitted[1] == "that") {
                            logOutput.WriteLine(popCode1("THAT",argument2,false));
                            Console.WriteLine(popCode1("THAT",argument2,false));
                            fileOutput.WriteLine(popCode1("THAT",argument2,false));
                        }
                        else if(splitted[1] == "temp") {
                            logOutput.WriteLine(popCode1("R5",argument2 + 5,false));
                            Console.WriteLine(popCode1("R5",argument2 + 5,false));
                            fileOutput.WriteLine(popCode1("R5",argument2 + 5,false));
                        }
                        else if(splitted[1] == "pointer" && argument2 == 0) {
                            logOutput.WriteLine(popCode1("THIS",argument2,true));
                            Console.WriteLine(popCode1("THIS",argument2,true));
                            fileOutput.WriteLine(popCode1("THIS",argument2,true));
                        }
                        else if(splitted[1] == "pointer" && argument2 == 1) {
                            logOutput.WriteLine(popCode1("THAT",argument2,true));
                            Console.WriteLine(popCode1("THAT",argument2,true));
                            fileOutput.WriteLine(popCode1("THAT",argument2,true));
                        }
                        else if(splitted[1] == "static") {
                            string mystring = (16 + argument2).ToString();
                            logOutput.WriteLine(popCode1(mystring,argument2,true));
                            Console.WriteLine(popCode1(mystring,argument2,true));
                            fileOutput.WriteLine(popCode1(mystring,argument2,true));
                        }
                    }
                    else {

                        Console.WriteLine("ERROR : Call writePushPop() for a non-pushpop command line : " + parsedString);
                        logOutput.WriteLine("ERROR : Call writePushPop() for a non-pushpop command line : " + parsedString);

                    }
                }
            }//end of if
        }//end of void parser
    }
}
