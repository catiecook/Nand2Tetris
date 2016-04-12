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
        private static String keyWordReg;
        private List<string> keywords = new List<string>();
        private List<char> symbols = new List<char>();


        public class Token
        {
            public enum Type
            {
                //Token types;
                keyword, symbol, identifier, int_const, string_const, COMMENT  // ? we should just throw comments out
            }

            public Token(Type t, string c)
            {
                type = t;
                context = c;
                string xmlstring = "<" + type + ">" + context + "</" + type + ">";
            }

            public readonly Type type;
            public readonly string context;
            public readonly string xmlstring;
        }

        public Tokenizer(StreamReader ins, bool generateComments)
        {
            input = ins;
            genComments = generateComments;
            stringIndex = 1;
            currentLine_ = "";
            keywords.Add("class");
            keywords.Add("constructor");
            keywords.Add("function");
            keywords.Add("method");
            keywords.Add("field");
            keywords.Add("static");
            keywords.Add("var");
            keywords.Add("int");
            keywords.Add("char");
            keywords.Add("boolean");
            keywords.Add("void");
            keywords.Add("true");
            keywords.Add("false");
            keywords.Add("null");
            keywords.Add("this");
            keywords.Add("let");
            keywords.Add("do");
            keywords.Add("if");
            keywords.Add("else");
            keywords.Add("while");
            keywords.Add("return");

            symbols.Add('{');
            symbols.Add('}');
            symbols.Add('(');
            symbols.Add(')');
            symbols.Add('[');
            symbols.Add(']');
            symbols.Add('.');
            symbols.Add(',');
            symbols.Add(';');
            symbols.Add('+');
            symbols.Add('-');
            symbols.Add('*');
            symbols.Add('/');
            symbols.Add('&');
            symbols.Add('|');
            symbols.Add('<');
            symbols.Add('>');
            symbols.Add('=');
            symbols.Add('~');
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

                currentLine_ = input.ReadLine();
                stringIndex = 0;

                //Return a comment whenever a new line is pulled.
                if (genComments) return new Token(Token.Type.COMMENT, currentLine_);
            }

            //TODO: Extract the next token from the line and return.
            //      Increment the stringIndex variable whenever a character is read.
            //      Use a statement like: char currentChar = currentLine_[stringIndex++];
            //      Use the private currentLine_, which is a field. It is directly manipulable.
            //      You can't use some operations on the public 'CurrentLine' property.

            //tegans code ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            //to get rid of whitespace and comments
            removeWhiteSpace();
            //comments and whitespaces deleted
            //now start to tokenize rest

            Console.WriteLine(currentLine_);

            if (symbols.Contains(CurrentLine[stringIndex]))  //~~~~~~~~~~~~~~~~~~~~~~~~check for symbols
            {            
                Token tokenSymbol = new Token(Token.Type.symbol, currentLine_[stringIndex].ToString());
                stringIndex++;
                return tokenSymbol;
            }


            foreach (string value in keywords) //~~~~~~~~~~~~~~~~~~~~~~~~check for keywords
            {
                string possibleKeyword = null;
                if(stringIndex + value.Length <= CurrentLine.Length)
                {
                    possibleKeyword = CurrentLine.Substring(stringIndex, value.Length);
                }
                else
                {
                    //no keyword possible
                    break;
                }

                if (possibleKeyword == value)
                {
                    //keyword found!
                    Token tokenKeyword = new Token(Token.Type.keyword, possibleKeyword);
                    stringIndex += possibleKeyword.Length;
                    Console.WriteLine("String index at : " + stringIndex);
                    Console.WriteLine("String index at character : " + CurrentLine[stringIndex]);
                    return tokenKeyword;
                }
                if(value == "return")
                {
                    //no keyword was found
                    break;
                }
            }
            
            if(CurrentLine[stringIndex] == '\"') //~~~~~~~~~~~~~~~~~~~~~~~~check for string_const
            {
                int quoteIndex = stringIndex + 1;
                //first quote of string_const found
                foreach (char character in CurrentLine)
                {
                    if (CurrentLine[quoteIndex] != '\"')
                    {
                        quoteIndex++;
                    }
                    else
                    {
                        //second quote found
                        string stringConst = CurrentLine.Substring(stringIndex + 1, quoteIndex - 1 - stringIndex);
                        stringIndex = quoteIndex + 1;
                        Token tokenStringConst = new Token(Token.Type.string_const, stringConst);
                        return tokenStringConst;
                    }

                }
            }

            if (Char.IsDigit(CurrentLine[stringIndex])) //~~~~~~~~~~~~~~~~~~~~~~~~check for int_const
            {
                int digitIndex = stringIndex;
                foreach (char i in CurrentLine)
                {
                    if (Char.IsDigit(CurrentLine[digitIndex]))
                    {
                        digitIndex++;
                    }
                }
                //return digit
                string stringDigit = CurrentLine.Substring(stringIndex, CurrentLine.Length - digitIndex);
                stringIndex = digitIndex + 1;
                Token tokenIntConst = new Token(Token.Type.int_const, stringDigit);
                return tokenIntConst;
            }
            

            if (Char.IsLetter(CurrentLine[stringIndex])) //~~~~~~~~~~~~~~~~~~~~~~~~check for identifier, already found all other cases. 
            {
                int letterIndex = stringIndex;
                foreach (char i in CurrentLine)
                {
                    if (CurrentLine.Length >= letterIndex && symbols.Contains(CurrentLine[letterIndex+1]))
                    {
                            //end of identifier was found
                        break;
                    }
                    else
                    {
                        Console.WriteLine(CurrentLine[letterIndex]);
                        letterIndex++;

                    }
                }
                //return digit
                string stringIdentifier= CurrentLine.Substring(stringIndex,  letterIndex - stringIndex + 1);
                stringIndex = letterIndex + 1;
                Console.WriteLine("String index at : " + stringIndex);
                Console.WriteLine("String index at character : " + CurrentLine[stringIndex]);
                Token tokenIdentifier = new Token(Token.Type.identifier, stringIdentifier);
                return tokenIdentifier;
            }



            // For now, just create a new token that contains the character at stringIndex
            Token token = new Token(Token.Type.COMMENT, currentLine_[stringIndex].ToString());
            stringIndex = 0;
            return token;
       }




 //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~REMOVE WHITESPACE/COMMENT FUNCTION~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        void removeWhiteSpace()
        {
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
            }
        }
    }
}
