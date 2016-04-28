using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using TList = System.Collections.Generic.List<Project10.Tokenizer.Token>;

using TTable = System.Collections.Generic.Dictionary<string,Project10.Parser.SymbolEntry>;

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
                classSymbols = new TTable();
            }

            public readonly string context;
            public bool exists;

            public TTable classSymbols;
        }

        public class SymbolEntry
        {
            private static int globalStaticIndex = 0;
            private static int nextStaticIndex
            {
                get
                {
                    return globalStaticIndex++;
                }
            }

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
                STRING,
                CLASS,
                A_INT,
                A_CHAR,
                A_STRING,
                A_CLASS
            }

            public SymbolEntry() { exists = false; args = null; }

            public SymbolEntry(string context,string subContext,Type type, SubType subType,TTable args)
            {
                this.exists = true;
                this.context = context;
                this.subContext = subContext;
                this.type = type;
                this.subType = subType;

                if(this.type == Type.STATIC) this.index = nextStaticIndex;

                this.args = args;
            }

            public string context;
            public string subContext;
            public Type type;
            public SubType subType;
            public bool exists;
            public int index;

            //For functions.
            public TTable args;
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
                CompileClassConstructorList(list,localTable,s);
                CompileClassSubroutineList(list,localTable,s);
            }
            else throw new FormatException("Syntax error: Expected \"{\"");

            res = expect(list,"}");
            if(res != 0) throw new FormatException("Syntax error: Expected \"}\"");
        }

        void CompileClassVarDec(TList list,TTable localTable)
        {
            int exp = expect(list,"static","field");
            switch(exp)
            {
            case 0:
                CompileMemberVariable(list,localTable,SymbolEntry.Type.STATIC);
                CompileClassVarDec(list,localTable);
                break;
            case 1:
                CompileMemberVariable(list,localTable,SymbolEntry.Type.FIELD);
                CompileClassVarDec(list,localTable);
                break;
            default:
                break;
            }
        }

        void CompileMemberVariable(TList list, TTable localTable,SymbolEntry.Type supType)
        {
            string exp = expect(list,Tokenizer.Token.Type.keyword);

            //Keyword type
            if(exp != null)
            {
                switch(exp)
                {
                case "int": CompileIdentifier(list,localTable,supType,SymbolEntry.SubType.INT,null); break;
                case "char": CompileIdentifier(list,localTable,supType,SymbolEntry.SubType.CHAR,null); break;
                case "string": CompileIdentifier(list,localTable,supType,SymbolEntry.SubType.STRING,null); break;
                default: throw new FormatException("Syntax error: Expected type keyword.");
                }
            }
            //Identifier for a class type
            else
            {
                string iden = expect(list,Tokenizer.Token.Type.identifier);

                if(iden == null) throw new FormatException("Syntax error. Expected type.");

                if(!SymbolTable.ContainsKey(iden)) SymbolTable[iden] = new ClassEntry(iden);

                CompileIdentifier(list,localTable,supType,SymbolEntry.SubType.CLASS,iden);
            }
        }

        void CompileClassConstructorList(TList list,TTable localTable, string name)
        {
            int exp = expect(list,"constructor");
            if(exp == 0) {
                CompileConstructor(list,localTable,name);
                CompileClassConstructorList(list,localTable,name);
            }
        }

        void CompileConstructor(TList list,TTable localTable,string name)
        {
            string nm = expect(list,Tokenizer.Token.Type.identifier);
            if(nm != name) throw new FormatException("Error. Constructor in class must share class name.");

            TTable argTable = new TTable();
            CompileParameterList(list,argTable);

            codeWriter.writeConstructorHead(name,argTable.Count());

            CompileSubroutineBody(list,localTable,argTable);

            codeWriter.writeSubroutineEnd();
        }

        void CompileClassSubroutineList(TList list,TTable localTable,string name)
        {
            int exp = expect(list,"function","method");
            switch(exp)
            {
            case 0:
                CompileSubroutine(list,localTable,name,SymbolEntry.Type.FUNCTION);
                CompileClassSubroutineList(list,localTable,name);
                break;
            case 1:
                CompileSubroutine(list,localTable,name,SymbolEntry.Type.METHOD);
                CompileClassSubroutineList(list,localTable,name);
                break;
            default:
                break;
            }
        }

        void CompileSubroutine(TList list,TTable localTable,string classname,SymbolEntry.Type supType)
        {
            string exp = expect(list,Tokenizer.Token.Type.keyword);

            Func<SymbolEntry.SubType,string,Tuple<string,TTable>> CIden = (ST,S) => {

                string nm = expect(list,Tokenizer.Token.Type.identifier);
                if(nm == null) throw new FormatException("Syntax error: Expected identifier.");

                return CompileIdentifier(list,localTable,supType,ST,S,true);
            };

            Tuple<string,TTable> fInfo;

            //Keyword type
            if(exp != null)
            {
                switch(exp)
                {
                case "int":  fInfo = CIden(SymbolEntry.SubType.INT,null); break;
                case "char": fInfo = CIden(SymbolEntry.SubType.CHAR,null); break;
                case "string": fInfo = CIden(SymbolEntry.SubType.STRING,null); break;
                default: throw new FormatException("Syntax error: Expected type keyword.");
                }
            }
            //Identifier for a class type
            else
            {
                string iden = expect(list,Tokenizer.Token.Type.identifier);

                if(iden == null) throw new FormatException("Syntax error. Expected type.");

                if(!SymbolTable.ContainsKey(iden)) SymbolTable[iden] = new ClassEntry(iden);

                fInfo = CIden(SymbolEntry.SubType.CLASS,iden);
            }

            string fName = fInfo.Item1;
            var fArgs = fInfo.Item2;

            codeWriter.writeSubroutineHead(classname,fName,fArgs.Count());
            CompileSubroutineBody(list,localTable,fArgs);
            codeWriter.writeSubroutineEnd();
        }

        Tuple<string,TTable> CompileIdentifier(TList list,TTable localTable,SymbolEntry.Type supType,SymbolEntry.SubType ty,string s,bool fun = false)
        {
            string iden = expect(list,Tokenizer.Token.Type.identifier);
            if(iden == null) throw new FormatException("Syntax error. Expected identifier.");

            if(localTable.ContainsKey(iden)) throw new FormatException("Identifier already defined.");

            Dictionary<string,SymbolEntry> args = null;
            Tuple<string,Dictionary<string,SymbolEntry>> fInfo = null;

            if(fun)
            {
                args = new Dictionary<string,SymbolEntry>();
                fInfo = new Tuple<string,TTable>(iden,args);
                CompileParameterList(list,args);
            }

            localTable[iden] = new SymbolEntry(iden,s,supType,ty,args);

            return fInfo;
        }

        void CompileParameterList(TList list,TTable argumentList)
        {
            int exp = expect(list,")");
            if(exp == 0) return;//Empty parameter list.

            exp = expect(list,"int","char","string","array");
            string nme;

            switch(exp)
            {
            case 0:
                CompileArgument(list,SymbolEntry.SubType.INT,null,argumentList);
                break;
            case 1:
                CompileArgument(list,SymbolEntry.SubType.CHAR,null,argumentList);
                break;
            case 2:
                CompileArgument(list,SymbolEntry.SubType.STRING,null,argumentList);
                break;
            case 3:
                exp = expect(list,"int","char","string");
                switch(exp)
                {
                case 0:
                    CompileArgument(list,SymbolEntry.SubType.A_INT,null,argumentList);
                    break;
                case 1:
                    CompileArgument(list,SymbolEntry.SubType.A_CHAR,null,argumentList);
                    break;
                case 2:
                    CompileArgument(list,SymbolEntry.SubType.A_STRING,null,argumentList);
                    break;
                default:
                    //Assume passing custom class type.
                    nme = expect(list,Tokenizer.Token.Type.identifier);
                    //If the symbol table indicates that class does not exist:
                    if(!SymbolTable.ContainsKey(nme))
                    {
                        //Add the new class to the symbol table.
                        SymbolTable[nme] = new ClassEntry(nme);
                    }
                    CompileArgument(list,SymbolEntry.SubType.A_CLASS,nme,argumentList);
                    break;
                }
                break;
            default:
                //Assume passing custom class type.
                nme = expect(list,Tokenizer.Token.Type.identifier);
                //If the symbol table indicates that class does not exist:
                if(!SymbolTable.ContainsKey(nme))
                {
                    //Add the new class to the symbol table.
                    SymbolTable[nme] = new ClassEntry(nme);
                }
                CompileArgument(list,SymbolEntry.SubType.CLASS,nme,argumentList);
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

        void CompileArgument(TList list,SymbolEntry.SubType type,string subC,TTable argumentList)
        {
            string exp = expect(list,Tokenizer.Token.Type.identifier);

            if(exp == null) throw new FormatException("Syntax error: Expected identifier.");

            if(argumentList.ContainsKey(exp)) throw new FormatException("Error: Identical identifiers in argument list.");

            argumentList[exp] = new SymbolEntry(exp,subC,SymbolEntry.Type.ARG,type,null);
        }

        void CompileSubroutineBody(TList list,Dictionary<string,SymbolEntry>localTable,TTable argumentTable)
        {
            TTable scopeTable = new TTable();

            CompileVarDecList(list,scopeTable);
            CompileStatementList(list,localTable,argumentTable,scopeTable);

        }

        void CompileVarDecList(TList list,TTable scopeTable)
        {
            int exp = expect(list,"var");

            if(exp == 0)
            {
                CompileVarDec(list,scopeTable);
                CompileVarDecList(list,scopeTable);
            }
        }

        void CompileVarDec(TList list,TTable scopeTable)
        {
            string exp = expect(list,Tokenizer.Token.Type.keyword);

            //Keyword type
            if(exp != null)
            {
                switch(exp)
                {
                case "int": CompileIdentifier(list,scopeTable,SymbolEntry.Type.VAR,SymbolEntry.SubType.INT,null); break;
                case "char": CompileIdentifier(list,scopeTable,SymbolEntry.Type.VAR,SymbolEntry.SubType.CHAR,null); break;
                case "string": CompileIdentifier(list,scopeTable,SymbolEntry.Type.VAR,SymbolEntry.SubType.STRING,null); break;
                default: throw new FormatException("Syntax error: Expected type keyword.");
                }
            }
            //Identifier for a class type
            else
            {
                string iden = expect(list,Tokenizer.Token.Type.identifier);

                if(iden == null) throw new FormatException("Syntax error. Expected type.");

                if(!SymbolTable.ContainsKey(iden)) SymbolTable[iden] = new ClassEntry(iden);

                CompileIdentifier(list,scopeTable,SymbolEntry.Type.VAR,SymbolEntry.SubType.CLASS,iden);
            }
        }

        void CompileStatementList(TList list,TTable localTable,TTable argumentTable,TTable scopeTable)
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