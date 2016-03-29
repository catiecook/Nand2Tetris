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

        private static string generatePop(string segment, int index, bool isDirect)
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
                string gotoLine = "@" + name + "\n" + "0:JMP\n";
                return gotoLine;
            }

            public static string ifGoTo(string name)
            {
                //TODO
            }

            public static string function(string name, string nArgs)
            {
                //TODO
            }

            public static string call(string name)
            {
                //TODO
            }

            public static string fReturn()
            {
                //TODO
            }
        }

        public static void parse(StreamReader input, StreamWriter output, StreamWriter log)
        {
            Action<string, StreamWriter> writeTo = (s, dest) =>
            {
                dest.Write(s);
                dest.WriteLine();
            };
            Action<string> write = s =>
            {
                writeTo(s, output);
                writeTo(s, log);
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
                        case "add": write(Parse.add()); break;
                        case "sub": write(Parse.sub()); break;
                        case "neg": write(Parse.neg()); break;
                        case "eq": write(Parse.eq()); break;
                        case "gt": write(Parse.gt()); break;
                        case "lt": write(Parse.lt()); break;
                        case "and": write(Parse.and()); break;
                        case "or": write(Parse.or()); break;
                        case "not": write(Parse.not()); break;
                        case "push": write(Parse.push(subs[1], subs[2])); break;
                        case "pop": write(Parse.pop(subs[1], subs[2])); break;
                        case "label": write(Parse.label(subs[1])); break;
                        case "goto": write(Parse.goTo(subs[1])); break;
                        case "if-goto": write(Parse.ifGoTo(subs[1])); break;
                        case "function": write(Parse.function(subs[1], subs[2])); break;
                        case "call": write(Parse.call(subs[1])); break;
                        case "return": write(Parse.fReturn()); break;
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
