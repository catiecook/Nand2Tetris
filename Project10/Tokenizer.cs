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
                keyword, symbol, identifier, int_const, string_const, COMMENT
            }

            public Token(Type t, string c)
            {
                type = t;
                context = c;
            }

            public readonly Type type;
            public readonly string context;
            public string xmlstring
            {
                get
                {
                    return "<" + type + ">" +
                        context.
                        Replace("&","&amp").
                        Replace("<","&lt").
                        Replace(">","&gt").
                        Replace("\"","&quot") +
                        "</" + type + ">"; } }
        }

        public Tokenizer(StreamReader ins, bool generateComments)
        {
            input = ins;
            genComments = generateComments;
            stringIndex = 0;
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
            if(CurrentLine == null || stringIndex >= CurrentLine.Length || CurrentLine.Length == 0) {
                string nonUgly;
                do {


                    //If we've reached the end of the file, return NULL:
                    if(input.EndOfStream) {
                        return null;
                    }

                    currentLine_ = input.ReadLine();
                    stringIndex = 0;

                    //to get rid of whitespace and comments
                    nonUgly = removeWhiteSpace();
                    //comments and whitespaces deleted
                    //now start to tokenize rest
                }
                while(currentLine_ == null || stringIndex >= CurrentLine.Length || CurrentLine.Length == 0);
                //Return a comment whenever a new line is pulled.
                if(genComments) return new Token(Token.Type.COMMENT,nonUgly);
            }

                //tegans code ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

                if(CurrentLine == null) {
                //If we've reached the end of the file, return NULL:
                if(input.EndOfStream) {
                    return null;
                }

                currentLine_ = input.ReadLine();
                stringIndex = 0;

                //Return a comment whenever a new line is pulled.
                if(genComments) return new Token(Token.Type.COMMENT,currentLine_);
            }
            if(symbols.Contains(CurrentLine[stringIndex]))  //~~~~~~~~~~~~~~~~~~~~~~~~check for symbols
            {
                Token tokenSymbol = new Token(Token.Type.symbol,currentLine_[stringIndex].ToString());
                stringIndex++;
                return tokenSymbol;
            }


            foreach(string value in keywords) //~~~~~~~~~~~~~~~~~~~~~~~~check for keywords
            {
                string possibleKeyword = null;
                if(stringIndex + value.Length <= CurrentLine.Length) {
                    possibleKeyword = CurrentLine.Substring(stringIndex,value.Length);
                }

                if(possibleKeyword == value) {
                    //keyword found!
                    Token tokenKeyword = new Token(Token.Type.keyword,possibleKeyword);
                    stringIndex += possibleKeyword.Length;
                    return tokenKeyword;
                }
                if(value == "return") {
                    //no keyword was found
                    break;
                }
            }

            if(CurrentLine[stringIndex] == '\"') //~~~~~~~~~~~~~~~~~~~~~~~~check for string_const
            {
                int quoteIndex = stringIndex + 1;
                //first quote of string_const found
                foreach(char character in CurrentLine) {
                    if(CurrentLine[quoteIndex] != '\"') //string_const are only found inside quotes! 
                    {
                        quoteIndex++;
                    }
                    else {
                        //second quote found
                        string stringConst = CurrentLine.Substring(stringIndex + 1,quoteIndex - 1 - stringIndex);
                        stringIndex = quoteIndex + 1;
                        Token tokenStringConst = new Token(Token.Type.string_const,stringConst);
                        return tokenStringConst;
                    }

                }
            }

            if(Char.IsDigit(CurrentLine[stringIndex])) //~~~~~~~~~~~~~~~~~~~~~~~~check for int_const
            {
                int digitIndex = stringIndex;
                foreach(char i in CurrentLine) {
                    if(Char.IsDigit(CurrentLine[digitIndex])) {
                        digitIndex++;
                    }
                }
                //return digit
                string stringDigit = CurrentLine.Substring(stringIndex,CurrentLine.Length - digitIndex);
                stringIndex = digitIndex + 1;
                Token tokenIntConst = new Token(Token.Type.int_const,stringDigit);
                return tokenIntConst;
            }


            if(Char.IsLetter(CurrentLine[stringIndex])) //~~~~~~~~~~~~~~~~~~~~~~~~check for identifier, already found all other cases. 
            {
                int letterIndex = stringIndex;
                foreach(char i in CurrentLine) {
                    if(CurrentLine.Length > letterIndex && symbols.Contains(CurrentLine[letterIndex + 1])) {
                        //end of identifier was found
                        break;
                    }
                    else {
                        letterIndex++;

                    }
                }

                string stringIdentifier = CurrentLine.Substring(stringIndex,letterIndex - stringIndex + 1);

                if(stringIdentifier.Length > 5) {

                    string arrayString = stringIdentifier.Substring(0,5); //when there is Arraya in CurrentLine, seperates them to Array a instead
                    if("Array" == arrayString) {
                        Token tokenArrayIdentifier = new Token(Token.Type.identifier,"Array");
                        stringIndex += 5;
                        return tokenArrayIdentifier;
                    }

                }


                stringIndex = letterIndex + 1;
                Token tokenIdentifier = new Token(Token.Type.identifier,stringIdentifier);
                return tokenIdentifier;
            }

            // For now, just create a new token that contains the character at stringIndex
            throw new FormatException("Syntax error: " + currentLine_);
        }




        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~REMOVE WHITESPACE/COMMENT FUNCTION~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        string removeWhiteSpace()
        {
            bool keepChecking = true;

            string nonUgly = currentLine_;

            while (keepChecking)
            {
                keepChecking = false;
                if (currentLine_.Length <= 1 || currentLine_[stringIndex] == '\n')
                {
                    currentLine_ = input.ReadLine();
                    keepChecking = true;
                }
                if (CurrentLine == null)
                {
                    stringIndex = 0;
                    break;
                }
                string pattern = "(?<!\")\\s+(?!\")";
                string replacement = "";
                Regex regex = new Regex(pattern);
 
                if ((stringIndex + 1) < (currentLine_.Length - 1) && currentLine_[stringIndex] == '/' && currentLine_[stringIndex] == '/') //takes out single-line comments
                {
                    // Read in new line and keep checking
                    currentLine_ = input.ReadLine();
                    stringIndex = 0;
                    keepChecking = true;
                    continue;
                }
                //check for /*....*/ multiple line comments
                if ((stringIndex + 1) < (currentLine_.Length - 1) && currentLine_[stringIndex] == '/' && currentLine_[stringIndex + 1] == '*') //takes out multi-line comments
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
                    continue;

                }

                nonUgly = CurrentLine;

                CurrentLine = regex.Replace(CurrentLine,replacement); //Takes out all whitespace!
            }

            return nonUgly;
        }
    }
}