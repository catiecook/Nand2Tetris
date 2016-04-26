using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project10
{
    //Interface for both XML writer and Code Engine
    interface ICodeWriter
    {
        void writeComment(string comment);

        void writeArgDeclaration(string type,string name);

        //TODO: Write more 'write___' functions for different token situations that might need various arguments.

        void writePush(string push);

        void writePop(string pop);


    }
}
