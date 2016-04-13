using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Project10
{
    class Tokenizer
    {
        private StreamReader input;
        private bool genComments;

        //A function such as this optimizes better than using a data structure in C#
        //Since the HashSet and Dictionary classes use the same hasing algorithms as the Switch statement,
        //declaring the function is more efficient because it uses less memory and less redirection
        //to call the same end functionality.
        bool isSymbol(char c)
        {
            switch(c)
            {
            case '{':
            case '}':
            case '(':
            case ')':
            case '[':
            case ']':
            case '.':
            case ',':
            case ';':
            case '+':
            case '-':
            case '*':
            case '/':
            case '&':
            case '|':
            case '<':
            case '>':
            case '=':
            case '~':
                return true;
            default:
                return false;
            }
        }

        bool isKeyword(string s)
        {
            switch(s)
            {
            case "class":
            case "constructor":
            case "function":
            case "method":
            case "field":
            case "static":
            case "var":
            case "int":
            case "char":
            case "boolean":
            case "void":
            case "true":
            case "false":
            case "null":
            case "this":
            case "let":
            case "do":
            case "if":
            case "else":
            case "while":
            case "return":
                return true;
            default:
                return false;
        }
        }

        bool isStringConst(string s)
        {
            if(s.Length < 2) return false;
            return s.First() == '"' && s.Last() == '"';
        }

        bool isIntConst(string s)
        {
            return s.All(c => Char.IsDigit(c));
        }

        bool isIdentifier(string s)
        {
            if(s.Length == 0) return false;

            if(!Char.IsLetter(s.First())) return false;

            return s.Skip(1).All(c => Char.IsLetterOrDigit(c) || c == '_');
        }

        public class Token
        {
            public enum Type
            {
                //Token types;
                keyword,
                symbol,
                identifier,
                int_const,
                string_const,
                COMMENT  
                //The COMMENT type isn't for comments in the existing code,
                //instead, it is for the comments we generate in the scanner.
                //These comments are the contents of the line currently being scanned.
                //This makes debugging the generated code later significantly easier.

            }

            public Token(Type t, string c)
            {
                type = t;
                context = c;
            }

            public readonly Type type;
            public readonly string context;
        }

        public Tokenizer(StreamReader ins, bool generateComments)
        {
            input = ins;
            genComments = generateComments;
            stringIndex = 1;
            currentLine_ = "";            
        }

        private string currentLine_;
        public string CurrentLine
        {
            get { return currentLine_; }
            private set { currentLine_ = value; }
        }
        private int stringIndex;

        /// <summary>
        /// Reads the next token from the stream and returns it.
        /// </summary>
        /// <returns>
        /// A token if there is another token in the stream.
        /// NULL if the end of the stream has been reached.
        /// </returns>
        public Token tokenize()
        {
            //If we've reached the end of a line, get a new line.
            if (stringIndex >= CurrentLine.Length || CurrentLine.Length == 0 || CurrentLine == null) {

                //If we've reached the end of the file, return NULL:
                if (input.EndOfStream) return null;

                currentLine_ = input.ReadLine().Trim();

                //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~start of removing whitespace
                bool keepChecking = true;

                while (keepChecking)
                {
                    keepChecking = false;
                    if (currentLine_.Length <= 1 || currentLine_[stringIndex] == '\n')
                    {
                        currentLine_ = input.ReadLine();
                        keepChecking = true;
                    }

                    string pattern = @"\s+";
                    string replacement = "";
                    Regex regex = new Regex(pattern);
                    CurrentLine = regex.Replace(CurrentLine, replacement); //Takes out all whitespace!

                    string result = null;
                    for (int i = 0; i < 200; i++)
                    {
                        result = Regex.Replace(CurrentLine, @"\s+", "");
                    }

                    if ((stringIndex + 1) < (currentLine_.Length - 1) && currentLine_[stringIndex] == '/' && currentLine_[stringIndex] == '/')
                    {
                        // Read in new line and keep checking
                        currentLine_ = input.ReadLine();
                        stringIndex = 0;
                        keepChecking = true;
                    }
                    //check for /*....*/ multiple line comments
                    if ((stringIndex + 1) < (currentLine_.Length - 1) && currentLine_[stringIndex] == '/' && currentLine_[stringIndex + 1] == '*')
                    {
                        bool multiLineCommentFound = false;
                        while (multiLineCommentFound == false)
                        {
                            stringIndex++;
                            if (stringIndex >= currentLine_.Length)
                            {
                                currentLine_ = input.ReadLine();
                                stringIndex = 0;
                            }
                            if (currentLine_[stringIndex] == '*' && currentLine_[stringIndex + 1] == '/')
                            {
                                stringIndex += 2;
                                multiLineCommentFound = true;
                                if (stringIndex == currentLine_.Length)
                                {
                                    currentLine_ = input.ReadLine();
                                    stringIndex = 0;
                                }
                            }

                        }//end of while
                        keepChecking = true;

                    }

                }//end of removing whitespace while
                stringIndex = 0;

                //Return a comment whenever a new line is pulled.
                if (genComments) return new Token(Token.Type.COMMENT, currentLine_);
            }

            //TODO: Extract the next token from the line and return.
            //      Increment the stringIndex variable whenever a character is read.
            //      Use a statement like: char currentChar = currentLine_[stringIndex++];
            //      Use the private currentLine_, which is a field. It is directly manipulable.
            //      You can't use some operations on the public 'CurrentLine' property.

            Console.WriteLine(currentLine_);

            if(isSymbol(currentLine_[stringIndex]))
                return new Token(
                    Token.Type.symbol,
                    currentLine_[stringIndex++].ToString());

            //Since it isn't a single char symbol,
            //parse to the first instance of whitespace or the end of the line.
            string tok = "";
            for(int i = 0; stringIndex < currentLine_.Length; ++i)
            {
                char c = currentLine_[stringIndex++];
                if(Char.IsWhiteSpace(c) || isSymbol(c)) {
                    --stringIndex;//So if a symbol was found it isn't skipped.
                    break;
                }
                tok += c;
            }

            if(isKeyword(tok)) return new Token(Token.Type.keyword,tok);

            if(isStringConst(tok)) return new Token(Token.Type.string_const,tok);

            if(isIntConst(tok)) return new Token(Token.Type.int_const,tok);

            if(isIdentifier(tok)) return new Token(Token.Type.identifier,tok);

            throw new ArgumentException("token \"" + tok + "\" is invalid.");


            //foreach (string value in keywords) //~~~~~~~~~~~~~~~~~~~~~~~~check for keywords
            //{
            //    string possibleKeyword = null;
            //    if(stringIndex + value.Length <= CurrentLine.Length)
            //    {
            //        possibleKeyword = CurrentLine.Substring(stringIndex, value.Length);
            //    }
            //    else
            //    {
            //        //no keyword possible
            //        break;
            //    }

            //    if (possibleKeyword == value)
            //    {
            //        //keyword found!
            //        Token tokenKeyword = new Token(Token.Type.keyword, possibleKeyword);
            //        stringIndex += possibleKeyword.Length;
            //        Console.WriteLine("String index at : " + stringIndex);
            //        Console.WriteLine("String index at character : " + CurrentLine[stringIndex]);
            //        return tokenKeyword;
            //    }
            //    if(value == "return")
            //    {
            //        //no keyword was found
            //        break;
            //    }
            //}
            
            //if(CurrentLine[stringIndex] == '\"') //~~~~~~~~~~~~~~~~~~~~~~~~check for string_const
            //{
            //    int quoteIndex = stringIndex + 1;
            //    //first quote of string_const found
            //    foreach (char character in CurrentLine)
            //    {
            //        if (CurrentLine[quoteIndex] != '\"')
            //        {
            //            quoteIndex++;
            //        }
            //        else
            //        {
            //            //second quote found
            //            string stringConst = CurrentLine.Substring(stringIndex + 1, quoteIndex - 1 - stringIndex);
            //            stringIndex = quoteIndex + 1;
            //            Token tokenStringConst = new Token(Token.Type.string_const, stringConst);
            //            return tokenStringConst;
            //        }

            //    }
            //}

            //if (Char.IsDigit(CurrentLine[stringIndex])) //~~~~~~~~~~~~~~~~~~~~~~~~check for int_const
            //{
            //    int digitIndex = stringIndex;
            //    foreach (char i in CurrentLine)
            //    {
            //        if (Char.IsDigit(CurrentLine[digitIndex]))
            //        {
            //            digitIndex++;
            //        }
            //    }
            //    //return digit
            //    string stringDigit = CurrentLine.Substring(stringIndex, CurrentLine.Length - digitIndex);
            //    stringIndex = digitIndex + 1;
            //    Token tokenIntConst = new Token(Token.Type.int_const, stringDigit);
            //    return tokenIntConst;
            //}
            

            //if (Char.IsLetter(CurrentLine[stringIndex])) //~~~~~~~~~~~~~~~~~~~~~~~~check for identifier, already found all other cases. 
            //{
            //    int letterIndex = stringIndex;
            //    foreach (char i in CurrentLine)
            //    {
            //        if (CurrentLine.Length >= letterIndex && symbols.Contains(CurrentLine[letterIndex+1]))
            //        {
            //                //end of identifier was found
            //            break;
            //        }
            //        else
            //        {
            //            Console.WriteLine(CurrentLine[letterIndex]);
            //            letterIndex++;

            //        }
            //    }
            //    //return digit
            //    string stringIdentifier= CurrentLine.Substring(stringIndex,  letterIndex - stringIndex + 1);
            //    stringIndex = letterIndex + 1;
            //    Console.WriteLine("String index at : " + stringIndex);
            //    Console.WriteLine("String index at character : " + CurrentLine[stringIndex]);
            //    Token tokenIdentifier = new Token(Token.Type.identifier, stringIdentifier);
            //    return tokenIdentifier;
            //}



            //// For now, just create a new token that contains the character at stringIndex
            //Token token = new Token(Token.Type.COMMENT, currentLine_[stringIndex].ToString());
            //stringIndex = 0;
            //return token;
       }

         


 //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~REMOVE WHITESPACE/COMMENT FUNCTION~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 
            //This function isn't needed.
            //C# has an existing function to do this: string.trim()
            //It removes all the leading and trailing whitespace.
        //void removeWhiteSpace()
        //{
        //    bool keepChecking = true;

        //    while (keepChecking)
        //    {
        //        keepChecking = false;
        //        if (currentLine_.Length <= 1 || currentLine_[stringIndex] == '\n')
        //        {
        //            currentLine_ = input.ReadLine();
        //            keepChecking = true;
        //        }

        //        string pattern = @"\s+";
        //        string replacement = "";
        //        Regex regex = new Regex(pattern);
        //        CurrentLine = regex.Replace(CurrentLine, replacement); //Takes out all whitespace!

        //        string result = null;
        //        for (int i = 0; i < 200; i++)
        //        {
        //            result = Regex.Replace(CurrentLine, @"\s+", "");
        //        }

        //        if ((stringIndex + 1) < (currentLine_.Length - 1) && currentLine_[stringIndex] == '/' && currentLine_[stringIndex] == '/')
        //        {
        //            // Read in new line and keep checking
        //            currentLine_ = input.ReadLine();
        //            stringIndex = 0;
        //            keepChecking = true;
        //        }
        //        //check for /*....*/ multiple line comments
        //        if ((stringIndex + 1) < (currentLine_.Length - 1) && currentLine_[stringIndex] == '/' && currentLine_[stringIndex + 1] == '*')
        //        {
        //            bool multiLineCommentFound = false;
        //            while (multiLineCommentFound == false)
        //            {
        //                stringIndex++;
        //                if (stringIndex >= currentLine_.Length)
        //                {
        //                    currentLine_ = input.ReadLine();
        //                    stringIndex = 0;
        //                }
        //                if (currentLine_[stringIndex] == '*' && currentLine_[stringIndex + 1] == '/')
        //                {
        //                    stringIndex += 2;
        //                    multiLineCommentFound = true;
        //                    if (stringIndex == currentLine_.Length)
        //                    {
        //                        currentLine_ = input.ReadLine();
        //                        stringIndex = 0;
        //                    }
        //                }

        //            }//end of while
        //            keepChecking = true;

        //        }
        //    }
        //}
    }
}
