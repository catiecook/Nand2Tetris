using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Project10
{
    class CodeEngine : ICodeWriter , IDisposable
    {
        private StreamWriter output;

        public CodeEngine(string fileName)
        {
            StreamWriter output = new StreamWriter(fileName); 
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

        }

        public void writeArgDeclaration(string type, string name)
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
    }
}
