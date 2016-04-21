using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project10
{
    public class SymbolTable
    {
        //initializes variables

        int varIndex = 0;
        int staticIndex = 0;
        int argIndex = 0;
        int fieldIndex = 0;


        public static Dictionary<string, SymbolTableEntry> ClassSymbolTable = new Dictionary<string, SymbolTableEntry>();//for STATIC, FIELD

        public static Dictionary<string, SymbolTableEntry> SubRoutineSymbolTable = new SymbolTalbe<string, SymbolTableEntry>();//for ARG, VAR

        public static Dictionary<Symbol.KIND, int> indices = new Dictionary<Symbol.KIND, int>(); //new


        //no vectors in C# , List is better
        public SymbolTable() //creates a new empty symbol table, init all indices
        {
            ClassSymbolTable = new Dictionary<string, Symbol>();
            SubroutineSymbolTable = new Dictionary<string, Symbol>();
            indices = new Dictionary<Symbol.KIND, int>();

            indices.Add(Symbol.KIND.ARG, 0); //all initiated to 0 for now
            indices.Add(Symbol.KIND.FIELD, 0);
            indices.Add(Symbol.KIND.STATIC, 0);
            indices.Add(Symbol.KIND.VAR, 0);

        }

        //starts new subroutine scope, resets the subroutines symbol table
        public void startSubRoutine() // starts a new sub routine scope (resets the table)
        {
            SubRountineSymbolTable.clear();
            indicies.put(Symbol.KIND.VAR, 0);
            indices.put(Symbol.KIND.ARG, 0);
        }

        //define takes in the key and type, then enters it into a new Symbol Table, and test if for its kind
        public void define(string name, string type, Symbol.KIND kind)
        {
            if (kind == Symbol.KIND.ARG || kind == Symbol.KIND.VAR)
            {
                // insert the variables into a new symbol table  accordingly 
                int index = indices.get(kind);
                Symbol symbol = new Symbol(type, kind, index);
                indices.put(kind, index + 1);
                SubroutineSymbolTable.put(name, symbol);
            }

            else if (kind == Symbol.KIND.STATIC || kind == Symbol.KIND.FIELD)
            {
                //insert the variables into a new symbol table accordingly
                int index = indices.get(kind);
                Symbol symbol = new Symbol(type, kind, index);
                indices.put(kind, index + 1);
                ClassSymbolTable.put(name, symbol);
            }
        }

        //returns the # of variables of the given kind already in the current scope
        public int varCount(Symbol.KIND kind)
        {
            return indices.get(kind);
        }

        //returns the index assigned to the named identifier
        public int indexOf(string key)
        {
            Symbol symbol = lookUp(name);

            if (symbol != null) return symbol.getIndex();

            return -1;
        }

        //returns the kind of the named identifier in the current scope, 
        //if unknown identifier, the current scope returns NONE
        public Symbol.KIND kindOf(string name)
        {

            Symbol symbol = lookUp(name);

            if (symbol != null) return symbol.getKind();

            return Symbol.KIND.NONE;
        }

        //returns the type of the named identifier in the current scope
        public string typeOf(string name)
        {

            Symbol symbol = lookUp(name);

            if (symbol != null) return symbol.getType();

            return "";
        }

        //checks if the target symbol exists
        private Symbol lookUp(string name)
        {
            if (ClassSymbolTable.get(name) != null)
            {
                return ClassSymbolTable.get(name);
            }
            else if (SubRoutineSymbolTable.get(name) != null)
            {
                return SubRoutineSymbolTable.get(name);
            }
            else
            {
                return null;
            }

        }
    }//end of class SymbolTable

}//end of namespace

