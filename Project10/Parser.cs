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

        public class ClassEntry
        {
            public ClassEntry()
            {
                exists = false;
                classSymbols = new Dictionary<string,SymbolEntry>();
            }

            public string context;
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

            public SymbolEntry() { exists = false; }

            public string context;
            public Type type;
            public bool exists;
        }

        Dictionary<string,ClassEntry> SymbolTable;

        public Parser(ICodeWriter writer)
        {
            SymbolTable = new Dictionary<string,ClassEntry>();
            codeWriter = writer;
            tabLines = 0;
        }

        //global variables
        int tabLines;

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
        }

        //functions
        void CompileClass(TList list)
        {
            int res = expect(list,"{");
            if(res == 0) {

                CompileClassVarDec(list);
                CompileClassConstructorList(list);
                CompileClassSubroutineList(list);
            }
            else throw new FormatException("Syntax error: Expected \"{\"");

            res = expect(list,"}");
            if(res != 0) throw new FormatException("Syntax error: Expected \"}\"");
        }

        void CompileClassVarDec(TList list)
        {
            int exp = expect(list,"static","field");
            switch(exp)
            {
            case 0:
                CompileStatic(list);
                CompileClassVarDec(list);
                break;
            case 1:
                CompileField(list);
                CompileClassVarDec(list);
                break;
            default:
                break;
            }
        }

        void CompileStatic(TList list, Dictionary<string,SymbolEntry> localTable)
        {

        }

        void CompileField(TList list, Dictionary<string,SymbolEntry> localTable)
        {

        }

        void CompileClassConstructorList(TList list)
        {
            int exp = expect(list,"constructor");
            if(exp == 0) {
                CompileConstructor(list);
                CompileClassConstructorList(list);
            }
        }

        void CompileConstructor(TList list)
        {

        }

        void CompileClassSubroutineList(TList list)
        {
            int exp = expect(list,"function","method");
            switch(exp)
            {
            case 0:
                CompileFunction(list);
                CompileClassSubroutineList(list);
                break;
            case 1:
                CompileMethod(list);
                CompileClassSubroutineList(list);
                break;
            default:
                break;
            }
        }

        void CompileMethod(TList list)
        {

        }

        void CompileFunction(TList list)
        {

        }

        void CompileParameterList(TList list)
        {
            CompileExpressionList(list);
            int exp = expect(list,")");

            if(exp != 0) throw new FormatException("Syntax error. Expected: \")\"");
        }

        void CompileVarDec(TList list,string className,Dictionary<string,SymbolEntry>localTable, bool top = true)
        {
            int exp = expect(list,"static","field");

            switch(exp)
            {
            case 0:
                CompileStatic(list,localTable);
                CompileVarDec(list,className,localTable,false);
                break;
            case 1:
                CompileField(list,localTable);
                CompileVarDec(list,className,localTable,false);
                break;
            default:
                if(top)
                    codeWriter.writeVariableDeclarations(className,localTable);
                break;
            }
        }

        void CompileStatements(TList list,Dictionary<string,SymbolEntry>symbolTable)
        {

        }

        void CompileDo(TList list)
        {

        }

        void CompileLet(TList list)
        {

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

        void CompileExpressionList(TList list)
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
                    return i;
                }

            return -1;
        }

        int expect(TList list,params Tokenizer.Token.Type[] expect)
        {
            var front = list.First();

            for(int i = 0; i < expect.Length; ++i)
                if(expect[i] == front.type) {
                    list.rmFront();
                    return i;
                }

            return -1;
        }

        void WriteXML(string xmlString, StreamWriter outputFile)
        {
            for (int i = 0; i < tabLines; i++)
            {
                outputFile.Write("\t");
            }
            outputFile.WriteLine("<"+ xmlString + ">");
        }

        void WriteXMLTag(Tokenizer.Token xmlTag, StreamWriter outputFile)
        {
            for (int i = 0; i < tabLines; i++)
            {
                outputFile.Write("\t");
            }
            outputFile.WriteLine("<" + xmlTag.type + ">" + xmlTag.context + "</" + xmlTag.type + ">");
        }
    }//end of class
}//end of namespace