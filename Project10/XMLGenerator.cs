using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Project10
{
    class XMLGenerator
    {
        public static void generateXML(List<Tokenizer.Token> tokenList, StreamWriter outputFile)
        {
            //TODO: Generate XML
            // "<" should equal &lt
            // ">" should equal &gt
            //"\"" should equal &quot
            //"&" should equal &amp

            outputFile.WriteLine("<tokens>\n");
            foreach (Tokenizer.Token token in tokenList)
            {
                if(token != null)
                {
                    outputFile.WriteLine("\t<" + token.type + ">" + token.context + "</" + token.type + ">\n");
                }
            }
            outputFile.WriteLine("</tokens>\n");

        }
    }
}
