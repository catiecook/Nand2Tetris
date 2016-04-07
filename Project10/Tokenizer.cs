using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Project10
{
    class Tokenizer
    {
        private StreamReader input;
        private bool genComments;

        public class Token
        {
            public enum Type
            {
                //TODO: Token types;
                Comment,
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
            if(stringIndex > CurrentLine.Length) {

                //If we've reached the end of the file, return NULL:
                if(input.EndOfStream) return null;

                currentLine_ = input.ReadLine();
                stringIndex = 0;

                //Return a comment whenever a new line is pulled.
                if(genComments) return new Token(Token.Type.Comment,currentLine_);
            }

            //TODO: Extract the next token from the line and return.
            //      Increment the stringIndex variable whenever a character is read.
            //      Use a statement like: char currentChar = currentLine_[stringIndex++];
            //      Use the private currentLine_, which is a field. It is directly manipulable.
            //      You can't use some operations on the public 'CurrentLine' property.
        }

        
    }
}
