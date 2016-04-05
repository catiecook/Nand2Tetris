using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

//R0 = SP
//R1 = LCL
//R2 = ARG
//R3 = THIS
//R4 = THAT
//R5 - R12 = holds the contents of the temp segment
//R13 - R15 = can be used by the VM implementation as general purpose registers 


namespace Project7_8
{
    static class Parser
    {
        private static class Constants
        {
            private static int jmpIndex = 0;

            private static int returnIndex = 0;

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

            public static string genReturn()
            {
                string ret = "return_" + returnIndex.ToString();
                ++returnIndex;
                return ret;
            }

            //pops off the last 2 in the stack 
            public const string popStack2 = "@SP\nAM=M-1\nD=M\nA=A-1\n";
        }

        private static string generatePush(string segment, int index, bool isDirect)
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
        }


        private static string generatePop(string segment, int index = 0, bool isDirect = false)
        {
            //if isDirect true = "D=A\n"
            //if isDirect false = "D=M\n@" + index + "\nD=D+A\n"
            string noPointerCode = (isDirect) ? "D=A\n" : "D=M\n@" + index.ToString() + "\nD=D+A\n";

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

            public static string push(string loc, string arg)
            {
                int index = Int32.Parse(arg);

                switch (loc)
                {
                    case "argument": return generatePush("ARG", index, false);
                    case "local": return generatePush("LCL", index, false);
                    case "static": return generatePush((index + 16).ToString(), index, true);
                    case "constant": return "@" + index.ToString() + "\n" + "D=A\n@SP\nA=M\nM=D\n@SP\nM=M+1\n";
                    case "this": return generatePush("THIS", index, false);
                    case "that": return generatePush("THAT", index, false);
                    case "pointer":
                        switch (index)
                        {
                            case 0: return generatePush("THIS", index, true);
                            case 1: return generatePush("THAT", index, true);
                            default: throw new ArgumentException("Invalid pointer.");
                        }
                    case "temp": return generatePush("R5", index + 5, false);
                    default: throw new ArgumentException("Invalid memory segment [" + loc + "]");
                }
            }

            public static string pop(string loc, string arg)
            {
                int index = Int32.Parse(arg);

                switch (loc)
                {
                    case "argument": return generatePop("ARG", index, false);
                    case "local": return generatePop("LCL", index, false);
                    case "static": return generatePop((index + 16).ToString(), index, true);
                    case "this": return generatePop("THIS", index, false);
                    case "that": return generatePop("THAT", index, false);
                    case "pointer":
                        switch (index)
                        {
                            case 0: return generatePop("THIS", index, true);
                            case 1: return generatePop("THAT", index, true);
                            default: throw new ArgumentException("Invalid pointer.");
                        }
                    case "temp": return generatePush("R5", index + 5, false);
                    default: throw new ArgumentException("Invalid memory segment [" + loc + "]");

                }
            }

            public static string label(string name)
            {
                //TODO --- tegan
                string labelLine = "(" + name + ")";
                return labelLine;
            }

            public static string goTo(string name)
            {
                //TODO -- tegan
                string gotoLine = "@" + name + "\n" + "0;JMP\n";
                return gotoLine;
            }

            public static string ifGoTo(string name)
            {
                //TODO -- tegan
                string ifGotoLine = "@SP\nAM=M-1\nD=M\nA=A-1\n@" + name + "\nD;JNE\n";
                return ifGotoLine;
            }

            public static string function(string name, string nArgs)
            {
                //TODO
                string functionLine = "(" + name + ")\n"; //write label
                foreach (int element in nArgs)
                {
                    functionLine = functionLine + "@0\n"
                                                + "D=A\n"
                                                + "@SP\n"
                                                + "A=M\n"
                                                + "M=D\n";
                }
                return functionLine;
            }

            public static string call(string nam, string nArgs)
            {
                Int32.Parse(nArgs);
                StringBuilder builder = new StringBuilder();

                string retAddr = Constants.genReturn();

                //Function address to R15
                builder.AppendLine("@" + nam);
                builder.AppendLine("D=A");
                builder.AppendLine("@R14");
                builder.AppendLine("M=D");

                //Number args into R14
                builder.AppendLine("@" + nArgs);
                builder.AppendLine("D=A");
                builder.AppendLine("@R15");
                builder.AppendLine("M=D");

                //Push return address onto stack
                builder.AppendLine("@" + retAddr);
                builder.AppendLine("D=A");
                builder.AppendLine("@SP");
                builder.AppendLine("M=D");

                //Save local, arg, this, and that
                Action<string> saveState = s => {

                    builder.AppendLine("@" + s);
                    builder.AppendLine("D=A");
                    builder.AppendLine("@SP");
                    builder.AppendLine("M=D");
                };

                saveState("LCL");
                saveState("ARG");
                saveState("THIS");
                saveState("THAT");

                //Reposition local, M[LCL] = M[SP], M[SP] = D
                builder.AppendLine("@SP");
                builder.AppendLine("D=M");
                builder.AppendLine("@LCL");
                builder.AppendLine("M=D");

                //Reposition arg, M[SP] is in D, num args in R14
                builder.AppendLine("@R14");
                builder.AppendLine("D=D-M");
                builder.AppendLine("@5");//5 because thats how much space we took to save state
                builder.AppendLine("D=D-A");
                builder.AppendLine("@ARG");
                builder.AppendLine("M=D");

                //Put result in args
                builder.AppendLine("@R15");
                builder.AppendLine("A=M");
                builder.AppendLine("0;JMP");

                builder.AppendLine("(" + retAddr + ")");

                return builder.ToString();
            }

            public static string fReturn()
            {

                //TODO -- tegan -- if numargs == 0, then ARG and return address point are at the same place
                string fReturnLine = "@LCL\n"
                                    + "D=M\n" //?
                                    + "@R6\n" //maybe temp
                                    + "M=D\n" //maybe
                                    + "@5\n"
                                    + "A=D-A\n" //RET = *(FRAME - 5)
                                    + "D=M\n"
                                    + "@R7\n" //R7 return value
                                    + "M=D\n" //hold return address in register temporarily

                                    + generatePop("ARG")

                                    /*
                                    + "@ARG\n" //pop arg 0         
                                    + "D=M\n"  // ARG is in D
                                    + "@0\n"
                                    + "D=D+A\n"
                                    + "@R5\n"
                                    + "M=D\n"
                                    + "@SP\n"
                                    + "A=M-1\n"
                                    + "D=M\n"
                                    + "@R5\n"
                                    + "A=M\n"
                                    + "M=D\n"
                                    */

                                    + "@SP\n"
                                    + "M=M-1\n"
                                    + "@ARG\nD=M\n@1\nD=D+A\n@SP\nM=D\n"

                                    + "@R6\nD=M\n@1\nA=D-A\nD=M\n@THAT\nM=D\n" //restore THAT to the caller
                                    + "@R6\nD=M\n@2\nA=D-A\nD=M\n@THIS\nM=D\n" //restore THIS to the caller
                                    + "@R6\nD=M\n@3\nA=D-A\nD=M\n@ARG\nM=D\n"  //restore ARG to the caller
                                    + "@R6\nD=M\n@4\nA=D-A\nD=M\n@LCL\nM=D\n" //restore LCL to the caller
                                    + "@R7\nA=M\n0;JMP\n";  // jump to the return value (held in R7)
                return fReturnLine;

            }
        }

        public static void parse(StreamReader input, StreamWriter output, StreamWriter log)
        {

            Action<string, StreamWriter> writeTo = (s, dest) =>
            {
                dest.Write(s);
                dest.WriteLine();
            };
            Action<string,string> write = (s,comment) =>
            {
                string str = comment == null ? s : "//" + comment + "\n" + s;

                writeTo(str, output);
                writeTo(str, log);
            };

            int lnnum = 0;
            do try
                {
                    Console.WriteLine("Parsing line [{0}]", lnnum);
                    string line = input.ReadLine();

                    //Remove comments:
                    if (line.Length >= 2)
                        if (line[0] == '/' && line[1] == '/')
                            continue;

                    //Skip empty lines:
                    if (line.Trim().Length == 0) continue;

                    var subs = line.Split(' ');
                    string command = subs[0].ToLower();

                    switch (command)
                    {
                        case "add": write(Parse.add(),line); break;
                        case "sub": write(Parse.sub(),line); break;
                        case "neg": write(Parse.neg(),line); break;
                        case "eq": write(Parse.eq(),line); break;
                        case "gt": write(Parse.gt(),line); break;
                        case "lt": write(Parse.lt(),line); break;
                        case "and": write(Parse.and(),line); break;
                        case "or": write(Parse.or(),line); break;
                        case "not": write(Parse.not(),line); break;
                        case "push": write(Parse.push(subs[1], subs[2]),line); break;
                        case "pop": write(Parse.pop(subs[1], subs[2]),line); break;
                        case "label": write(Parse.label(subs[1]),line); break;
                        case "goto": write(Parse.goTo(subs[1]),line); break;
                        case "if-goto": write(Parse.ifGoTo(subs[1]),line); break;
                        case "function": write(Parse.function(subs[1], subs[2]),line); break;
                        case "call": write(Parse.call(subs[1],subs[2]),line); break;
                        case "return": write(Parse.fReturn(),line); break;
                        default: throw new NotImplementedException("Operator \"" + command + "\" not found.");
                    }

                    ++lnnum;
                }
                catch (Exception err)
                {
                    Console.WriteLine("Error on line [{0}]: {1}", lnnum, err.Message);
                    log.WriteLine("Error on line [{0}]: {1}", lnnum, err.Message);
                }
            while (!input.EndOfStream);
        }
    }
}