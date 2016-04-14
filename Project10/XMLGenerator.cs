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
                outputFile.WriteLine(xml);

                tokenList.RemoveAt(0);
            }
            outputFile.WriteLine("</tokens>\n");

            //TODO: Generate XML
            // "<" should equal &lt
            // ">" should equal &gt
            //"\"" should equal &quot
            //"&" should equal &amp


            //foreach (Tokenizer.Token token in tokenList)
            //{
            //    if(token != null)
            //    {

            //        if(token.context == "<") {
            //            outputFile.WriteLine("\t<" + token.type + ">&lt</" + token.type + ">\n");
            //        }
            //        else if(token.context == ">") {
            //            outputFile.WriteLine("\t<" + token.type + ">&gt</" + token.type + ">\n");
            //        }
            //        else if(token.context == "\"") {
            //            outputFile.WriteLine("\t<" + token.type + ">&quot</" + token.type + ">\n");
            //        }
            //        else if(token.context == "&") {
            //            outputFile.WriteLine("\t<" + token.type + ">&amp</" + token.type + ">\n");
            //        }
            //        else //write out the xml with normal token context
            //        {
            //            outputFile.WriteLine("\t<" + token.type + ">" + token.context + "</" + token.type + ">\n");
            //        }
            //    }
            //}
            

        }
    }
}
