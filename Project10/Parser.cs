using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Project10
{
    class Parser
    {
        //global variables
        int listIndex = 0;
        int tabLines = 0;

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

        bool expect(string expect, List<Tokenizer.Token> list)
        {
            if (expect == list[listIndex].context)
            {
                return true;
            }
            else
            {
                Console.WriteLine("Was not passed in the expected : " + expect);
                return false;
            }
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