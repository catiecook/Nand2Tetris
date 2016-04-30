using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using TList = System.Collections.Generic.List<Project10.Tokenizer.Token>;

using TTable = System.Collections.Generic.Dictionary<string, Project10.Parser.SymbolEntry>;

namespace Project10
{
    class CodeEngine : ICodeWriter, IDisposable
    {
        private StreamWriter output;
        private SymbolTable table;
        private XMLWriter writer;
        private string className;
        private string currentName;
        private Tokenizer token;
        private String className;
        private String currentName;
        private int currentKind;
        private String currentType;

        public CodeEngine(string fileName)
        {
            token output = new Tokenizer(fileName);
            table = new SymbolTable();
            string outputName = Parser.filename;
            writer = new VMWriter(outputName);
        }

        public void Dispose()
        {
            output.Close();
        }

        public void writeComment(string comment)
        {
            output.WriteLine("//" + comment);
        }

        public void writeClassSymbolTable(string className, Dictionary<string, Parser.SymbolEntry> symbols)
        {
            output.WriteLine(className);
            output.WriteLine(symbols);
        }

        public void writeArgDeclaration(string type, string name)
        {
            output.WriteLine(type + name);
           
            // set kind here
            currentKind = tokenizer.keyword();

            // match type
            tokenizer.advance();

           
            if (!expect())//add in the parameters to check for var dec
            {
                output.WriteLine("illegal type for class var dec");
                return;
            }

            // match varName
            tokenizer.advance();

         
            if (expect()) //add in the parameters to check for identifier
            {
                addToTable();
            }
            else {
                output.WriteLine("illegal classVar identifier");
                return;
            }


            // match potential ", varName" part
            tokenizer.advance();
            while (tokenizer.symbol().equals(","))
            {
                tokenizer.advance();
                if (expect()) //add in the parameters to check for class identifier
                {
                    addToTable();
                }
                else {
                    output.WriteLine("illegal classVar identifier");
                    return;
                }
                tokenizer.advance();
            }

            // match ;
            if (!expect(";")) //looking for an instruction ending
            {
                output.WriteLine("no ending ;");
                return;
            }
        }

    

        public void writeConstructorHead(string className, int numArgs)
        {

        }



        public void writeSubroutineHead(string className, string subName, int numArgs)
        {
        // clear the previous subroutine symbol table
        table.startSubroutine();
        //isVoid = false;

        // already know that the current token start with constructor, function or method
        int subRoutineKind = tokenizer.keyword();  // is it a function or method or constructor

        // match return type
        tokenizer.advance();

        if (!expect())//looking for writen type
        {
            output.WriteLine("Illegal type name for subroutine");
            return;
        }

        String currentSubName = null;

        // match subroutine identifier
        tokenizer.advance();
        if (expect()) //check for identifier
        {
            currentSubName = className + "." + currentName;
        }
        else {
            output.WriteLine("illegal subroutine name");
            return;
        }

        // if it is a method, the first argument is self
        if (subRoutineKind == JackTokens.METHOD)
        {
            table.define("this", className, JackTokens.ARG);
        }

        // match parameter list
        tokenizer.advance();
        if (expect("(")) //look for symbol "("
        {
            compileParameterList();
        }
        else {
            output.WriteLine("no () after function name");
            return;
        }

        // match the closing ) for the paramater list
        if (!expect(")")) //if ")" does not exist
        {
            output.WriteLine("no () after function name");
            return;
        }
    }

        public void writeSubroutineEnd()
        {

        }


        public void writePush(string toPush)
        {
            output.WriteLine("push " + toPush);
        }

        public void writePop(string toPop)
        {
            output.WriteLine("pop " + toPop);
        }

        int expect(TList list, params string[] expect)
        {
           
            var front = list.First();

            //shouldn't need this? vv

           /* for (int i = 0; i < expect.Length; ++i)
                if (expect[i] == front.context)
                {
                    list.rmFront();//Expect removes the token upon success.
                    ++tokenCount_;
                    return i;
                }*/

            return -1;
        }

        string expect(TList list, Tokenizer.Token.Type expect)
        {
            var front = list.First();

            if (front.type == expect)
            {

                list.rmFront();
                ++tokenCount_;
                return front.context;
            }
            else return null;
        }
    }
}
