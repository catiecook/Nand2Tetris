using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project10
{
    class Parser
    {
        //global variables
        int listIndex = 0;

        public static void Parse(List<Tokenizer.Token> list)
        {


        }

        //functions
        void CompileClass()
        {

        }

        void CompileClassVarDec()
        {

        }

        void CompileSubroutine()
        {

        }

        void CompileParameterList()
        {

        }

        void CompileVarDec()
        {

        }

        void CompileStatements()
        {

        }

        void CompileDo()
        {

        }

        void CompileLet()
        {

        }

        void CompileWhile()
        {

        }

        void CompileReturn()
        {

        }

        void CompileIf()
        {

        }

        void CompileExpression()
        {

        }

        void CompileTerm()
        {

        }

        void CompileExpressionList()
        {

        }

        void expect(string expect, List<Tokenizer.Token> list)
        {
            if (expect == list[listIndex].context)
            {

            }
        }

        void WriteXML()
        {

        }

        void WriteXMLTag()
        {

        }
    }//end of class
}//end of namespace