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

        void writeConstructorHead(string className,int numArgs);

        void writeSubroutineHead(string className,string subName,int numArgs);

        void writeSubroutineEnd();

        /// <summary>
        /// Writes the code for setting a variable.
        /// </summary>
        /// <param name="destinationIndex">Index of the destination.</param>
        /// <param name="destinationType">Memory segment of destination.</param>
        /// <param name="sourceIndex">Index of source.</param>
        /// <param name="sourceType">Memory segment of source.</param>
        /// <remarks>
        /// The possible values for the memory segment chars are:
        ///     a : argument
        ///     l : local
        ///     s : static
        ///     c : constant
        /// </remarks>
        void writeVariableSet(int destinationIndex,char destinationType,int sourceIndex,char sourceType);

        

        //TODO: Write more 'write___' functions for different token situations that might need various arguments.
    }
}
