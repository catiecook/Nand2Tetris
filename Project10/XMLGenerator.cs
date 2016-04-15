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
            int indentation = 0;
            outputFile.WriteLine("<tokens>\n");

            while(tokenList.Count() > 0) {

                var tok = tokenList.First();
                if(tok == null) break;

                for(int i = 0; i < indentation; ++i) outputFile.Write(' ');
                string xml = tok.xmlstring;
                outputFile.WriteLine("\t" + xml);


                tokenList.RemoveAt(0);
            }
            outputFile.WriteLine("</tokens>\n");

            //TODO: Generate XML
            // "<" should equal &lt
            // ">" should equal &gt
            //"\"" should equal &quot
            //"&" should equal &amp
            
        }
    }
}
