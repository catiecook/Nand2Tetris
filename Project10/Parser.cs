using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using TList = System.Collections.Generic.List<Project10.Tokenizer.Token>;

namespace Project10
{
    class Parser
    {
        ICodeWriter codeWriter;
        private int tokenCount_;

        public class ClassEntry
        {
            public ClassEntry(string className)
            {
                context = className;
                exists = false;
                classSymbols = new Dictionary<string,SymbolEntry>();
            }

            public readonly string context;
            public bool exists;

            public Dictionary<string,SymbolEntry> classSymbols;
        }

        public class SymbolEntry
        {
            public enum Type
            {
                ARG,
                VAR,
                METHOD,
                FUNCTION,
                STATIC,
                FIELD

            }

            public enum SubType
            {
                INT,
                CHAR,
                STRING
            }

            public SymbolEntry() { exists = false; args = null; }

            public SymbolEntry(string context,Type type, SubType subType,Dictionary<string,SymbolEntry> args)
            {
                this.exists = true;
                this.context = context;
                this.type = type;
                this.subType = subType;

                this.args = args;
            }

            public string context;
            public Type type;
            public SubType subType;
            public bool exists;

            //For functions.
            public Dictionary<string,SymbolEntry> args;
        }

        Dictionary<string,ClassEntry> SymbolTable;

        public int TokenCount
        {
            get { return tokenCount_; }
        }

        public Parser(ICodeWriter writer)
        {
            tokenCount_ = 0;
            SymbolTable = new Dictionary<string,ClassEntry>();
            codeWriter = writer;
        }

        private void HandleComment(TList list)
        {
            var front = list.First();
            if(front.type == Tokenizer.Token.Type.COMMENT) {
                list.rmFront();
                codeWriter.writeComment(front.context);
            }
        }

        public void Parse(TList list)
        {
            HandleComment(list);
            int res = expect(list,"class");
            if(res == 0) {
                CompileClass(list);
                Parse(list);
            }
            else {//We've reached the end of the program, all classes parsed.

                //Check if there are any nonexistant symbols:
                foreach(var entry in SymbolTable.Values)
                {
                    if(!entry.exists) throw new FormatException("Error. Class \"" + entry.context + "\" was referenced but does not exist.");

                    foreach(var sym in entry.classSymbols.Values)
                    {
                        if(!sym.exists) throw new FormatException("Error. Symbol \"" + sym.context + "\" in class \"" + entry.context + "\" was referenced but does not exist.");
                    }
                }


            }
        }

        //functions
        void CompileClass(TList list)
        {
            string s = expect(list,Tokenizer.Token.Type.identifier);

            if(s == null) throw new FormatException("Syntax error: Expected class name.");

            if(!SymbolTable.ContainsKey(s))
                SymbolTable[s] = new ClassEntry(s);

            SymbolTable[s].exists = true;
            var localTable = SymbolTable[s].classSymbols;

            int res = expect(list,"{");
            if(res == 0) {

                CompileClassVarDec(list,localTable);
                CompileClassConstructorList(list,localTable);
                CompileClassSubroutineList(list,localTable);
            }
            else throw new FormatException("Syntax error: Expected \"{\"");

            res = expect(list,"}");
            if(res != 0) throw new FormatException("Syntax error: Expected \"}\"");
        }

        void CompileClassVarDec(TList list,Dictionary<string,SymbolEntry> localTable)
        {
            int exp = expect(list,"static","field");
            switch(exp)
            {
            case 0:
                CompileStatic(list,localTable);
                CompileClassVarDec(list,localTable);
                break;
            case 1:
                CompileField(list,localTable);
                CompileClassVarDec(list,localTable);
                break;
            default:
                break;
            }
        }

        void CompileStatic(TList list, Dictionary<string,SymbolEntry> localTable)
        {
            string exp = expect(list,Tokenizer.Token.Type.keyword);

            //Keyword type
            if(exp != null)
            {
                Action<SymbolEntry.SubType> staticType = ty => {

                    string iden = expect(list,Tokenizer.Token.Type.identifier);
                    if(iden == null) throw new FormatException("Syntax error. Expected identifier.");

                    if(localTable.ContainsKey(iden)) throw new FormatException("Identifier already defined.");

                    localTable[iden] = new SymbolEntry(iden,SymbolEntry.Type.STATIC,ty,null);
                };

                switch(exp)
                {
                case "int": staticType(SymbolEntry.SubType.INT); break;
                case "char": staticType(SymbolEntry.SubType.CHAR); break;
                case "string": staticType(SymbolEntry.SubType.STRING); break;
                default: throw new FormatException("Syntax error: Expected type keyword.");
                }
            }
            //Identifier for a class type
            else
            {

            }
        }

        void CompileField(TList list, Dictionary<string,SymbolEntry> localTable)
        {

        }

        void CompileClassConstructorList(TList list,Dictionary<string,SymbolEntry>localTable)
        {
            int exp = expect(list,"constructor");
            if(exp == 0) {
                CompileConstructor(list,localTable);
                CompileClassConstructorList(list,localTable);
            }
        }

        void CompileConstructor(TList list,Dictionary<string,SymbolEntry>localTable)
        {

        }

        void CompileClassSubroutineList(TList list,Dictionary<string,SymbolEntry>localTable)
        {
            int exp = expect(list,"function","method");
            switch(exp)
            {
            case 0:
                CompileFunction(list,localTable);
                CompileClassSubroutineList(list,localTable);
                break;
            case 1:
                CompileMethod(list,localTable);
                CompileClassSubroutineList(list,localTable);
                break;
            default:
                break;
            }
        }

        void CompileMethod(TList list,Dictionary<string,SymbolEntry>localTable)
        {

        }

        void CompileFunction(TList list,Dictionary<string,SymbolEntry>localTable)
        {

        }

        void CompileParameterList(TList list,Dictionary<string,SymbolEntry>argumentList)
        {
            int exp = expect(list,"int","char","string","array");

            switch(exp)
            {
            case 0:
                CompileArgument(list,"int",argumentList);
                break;
            case 1:
                CompileArgument(list,"char",argumentList);
                break;
            case 2:
                CompileArgument(list,"string",argumentList);
                break;
            case 3:
                CompileArrayArgument(list,argumentList);
                break;
            default:
                //Assume passing custom class type.
                string nme = expect(list,Tokenizer.Token.Type.identifier);
                //If the symbol table indicates that class does not exist:
                if(!SymbolTable.ContainsKey(nme))
                {
                    //Add the new class to the symbol table.
                    SymbolTable[nme] = new ClassEntry(nme);
                }
                CompileArgument(list,nme,argumentList);
                break;
            }

            exp = expect(list,")",",");

            switch(exp)
            {
            case 0:
                //end list
                break;
            case 1:
                CompileParameterList(list,argumentList);
                break;
            default:
                throw new FormatException("Syntax error. Expected: \")\" or \",\"");
            }
        }

        void CompileArgument(TList list,string type,Dictionary<string,SymbolEntry> argumentList)
        {

        }

        void CompileArrayArgument(TList list,Dictionary<string,SymbolEntry> argumentList)
        {

        }

        void CompileVarDec(TList list,Dictionary<string,SymbolEntry>scopeTable)
        {
            
        }

        void CompileStatements(TList list,Dictionary<string,SymbolEntry>scopeTable)
        {
            var front = list.First();
            switch (front.context)
            {
                case "let":
                    CompileLet(list);
                    break;
                case "if":
                    CompileIf(list);
                    break;
                case "while":
                    CompileWhile(list);
                    break;
                case "do":
                    CompileDo(list);
                    break;
                case "return":
                    CompileReturn(list);
                    break;
                default:
                    throw new FormatException("Syntax error. Expected: 'do', 'while', 'let', 'if', or 'return' statement");
            }
        }

        void CompileSubroutineCall(TList list)
        {

        }

        void CompileDo(TList list)
        {
            expect(list, Tokenizer.Token.Type.keyword);
            CompileSubroutineCall(list);
            expect(list, Tokenizer.Token.Type.symbol);
        }

        void CompileLet(TList list)
        {
            expect(list, Tokenizer.Token.Type.identifier); //not completed


        }

        void CompileWhile(TList list)
        {

        }

        void CompileReturn(TList list)
        {

        }

        void CompileIf(TList list)
        {

        }

        void CompileExpression(TList list)
        {

        }

        void CompileTerm(TList list)
        {

        }

        void CompileExpressionList(TList list,Dictionary<string,SymbolEntry>scopeTable)
        {
            
        }

        int expect(TList list,params string[] expect)
        {
            //first parse out all comments:
            HandleComment(list);

            var front = list.First();

            for(int i = 0; i < expect.Length; ++i)
                if(expect[i] == front.context) {
                    list.rmFront();//Expect removes the token upon success.
                    ++tokenCount_;
                    return i;
                }

            return -1;
        }

        string expect(TList list,Tokenizer.Token.Type expect)
        {
            var front = list.First();

            if(front.type == expect) {

                list.rmFront();
                ++tokenCount_;
                return front.context;
            }
            else return null;
        }



    }//end of class
}//end of namespace