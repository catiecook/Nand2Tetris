using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Project10
{
    class XMLWriter
    {
        public XMLWriter()
        {

        }

        int tabLines = 0;

        void WriteXML(string xmlString, StreamWriter outputFile)
        {
            for (int i = 0; i < tabLines; i++)
            {
                outputFile.Write("\t");
            }
            outputFile.WriteLine("<" + xmlString + ">");
        }

        void WriteXMLTag(Tokenizer.Token xmlTag, StreamWriter outputFile)
        {
            for (int i = 0; i < tabLines; i++)
            {
                outputFile.Write("\t");
            }
            outputFile.WriteLine("<" + xmlTag.type + ">" + xmlTag.context + "</" + xmlTag.type + ">");
        }
    }
}
