using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;




using TList = System.Collections.Generic.List<Project10.Tokenizer.Token>;

using TTable = System.Collections.Generic.Dictionary<string, Project10.Parser.SymbolEntry>;

namespace Project10
{
    interface ICodeWriter
    {
        class CodeEngine : ICodeWriter, IDisposable
        {
            private StreamWriter output;
            private SymbolTable symbolTable;
            private XMLWriter writer;
            private Tokenizer token;
            private string currentName;
            private int currentKind;
            private string currentType;
            private string tokenType;


            public void compileClass()
            {
                //compiles class *className*, open bracket *{* class variable declarations & subroutine declarations *currentType* 
                symbolTable.startClass();

                if (token) //if it is a token fall into here
                {
                    WriteXMLTag("token"); //***WriteXML may not be the right choice... 

                    while (token.advance())
                    {
                        WriteXML(convert(token.tokenType), token.currentType);
                    }
                }
                else
                {
                    WriteXMLTag("class");
                    expect("class", "keyword", "identifier");
                    compileClassVarDec(/*"class", GLOBAL FOR KIND OF VAR, "program ", Program.fileName*/);
                    expect("symbol");

                    while (checkIdentifier(currentType)) //took this name from tokenizer.cs
                    {
                        if (token.currentType == "static")
                        {
                            compileClassVarDec(/*parameter for KIND.STATIC*/);
                            continue;
                        }

                        else if (token.currentType == "field")
                        {
                            compileClassVarDec(/*parameter for KIND.FIELD*/);
                            continue;
                        }

                        else if (token.currentType == "constructor")
                        {
                            writeSubroutineHead(/*parameter for KIND.FIELD*/);
                            continue;
                        }

                        else if (token.currentType == "}")
                        {
                            WriteXML("symbol", "}");
                            break;
                        }

                        else
                        {
                            Console.WriteLine("Something isn't right");
                            break;
                        }

                    }


                    WriteXMLTag("/class");
                  //  PrintClassSymbolTable(Working_Class);
                }


            }

            public void compileClassVarDec() //compiles the class variables/parameters at declarations
            {
                string tokenType = token.currentType;
                WriteXMLTag("classVarDec"); // field or static
                WriteXML("keyword", token.currentType);
      
                while (checkIdentifier(currentType))
                {

                    if (token.tokenType == ",") //this means theres more to read, keep going
                    {
                        WriteXML("symbol", ", ");
                        
                        expect("identifier");
                        CompileVarDefinition(/*tokenType, currentType, ???*/);
                        continue;
                    }

                    else if (token.tokenType == ";")
                    {
                        WriteXML("symbol", ";"); //done wth declaration 
                        break;
                    }

                    else
                    {
                        Console.WriteLine("Something isn't right");
                        break;
                    }
                }
                WriteXMLTag("classVarDec"); 
            }

            public void CompileVarDefinition()
            {
                while (checkIdentifier(currentType))
                {
                    if (token.currentType == "let")
                    {
                        compileLet();
                        continue;
                    }

                    else if (token.currentType == "if")
                    {
                        compileIf();
                        continue;
                    }

                    else if (token.currentType == "do")
                    {
                        compileDo();
                        continue;
                    }
                    else if (token.currentType == "return")
                    {
                        compileReturn();
                        break;
                    }
                }
            }

        public bool getNextToken()
            {
                if (token) //if there are tokens to read
                {
                    return true;
                }
                else
                    return false; 
            }
            
            public void compileIf()
            {
                token.advance();
                if (!checkIdentifier(currentType))
                {
                    Console.WriteLine("Illegal identifier");
                    return;
                }

                string var = currentName;
                bool isArray = false;
                string kind = symbolTable.kindOf(var);
                string type = symbolTable.typeOf(var);
                int index = symbolTable.indexOf(var);

                token.advance();
                if (checkSymbol("["))
                {
                    compileArrayTerm();
                    isArray = true;
                    // if has [], advance and next should be =

                    // the top of stack should be the index
                    writer.writePush(kind, index);
                    writer.writeXML("add");
                    writer.writePop("temp", 2);
                    //writer.writePop("pointer", 1);

                    token.advance();
                }

                if (!checkSymbol("="))
                {
                    Console.WriteLine("No = found");
                    return;
                }

                token.advance();
                compileExpression();
            }

            public void compileLet()
            {
                token.advance();
                if (!checkIdentifier())
                {
                    Console.WriteLine("Illegal identifier");
                    return;
                }

                string var = currentName;
                bool isArray = false;
                string kind = symbolTable.kindOf(var);
                string type = symbolTable.typeOf(var);
                int index = symbolTable.indexOf(var);

                token.advance();
                if (checkSymbol("["))
                {
                    compileArrayTerm();
                    isArray = true;
                    // if has [], advance and next should be =

                    // the top of stack should be the index
                    writer.writePush(kind, index);
                    writer.writeXML("add");
                    writer.writePop("temp", 2);
                    //writer.writePop("pointer", 1);

                    token.advance();
                }

                if (!checkSymbol("="))
                {
                    Console.WriteLine("No = found");
                    return;
                }

                token.advance();
                compileExpression();

                // the result should be on the top of stack now
                // write out pop code
                if (isArray)
                {
                   
                    writer.writePush("temp", 2);
                    writer.writePop("pointer", 1);
                    writer.writePop("that", 0);
                }
                else {
                    writer.writePop(kind, index);
                }

                // No need to advance because compileExpression does one token look ahead
                if (!checkSymbol(";"))
                {
                    Console.WriteLine("No ; found at the end of statement");
                    return;
                }
            }
        

            public void compileReturn()
            {
                token.advance();
                // if the following is not ; then try to parse argument
                if (!checkSymbol(";"))
                {
                    compileExpression();
              
                    if (!checkSymbol(";"))
                    {
                        Console.WriteLine("return statement not ending with ;");
                        return;
                    }
                }
                else {
                    writer.writePush("constant", 0);
                }
                
                writer.writeReturn();
            }
        

            public void compileDo()
            {
                token.advance();
                // first check if the current
                // token is valid identifier. Then advance again and check if the it is . or (
                if (checkIdentifier(currentType))
                {
                    string firstHalf = currentType;
                    token.advance();
                    if (checkSymbol(".") || checkSymbol("("))
                    {
                        // if it's ".", means currentName now is other class names
                        // else, we are calling self methods.
                        // if the current symbol is (
                        writeSubRoutineHead(firstHalf); //idk idk
                    }
                    else
                    {
                        Console.WriteLine("Not valid subroutine call");
                        return;
                    }


                }
                else
                {
                    Console.WriteLine("%s is not a valid identifier for do statement\n", token.Token());
                    return;
                }

                token.advance();

                if (!checkSymbol(";"))
                {
                    Console.WriteLine("No closing ;");
                    return;
                }
                // pop the 0 from void return function
                writer.writePop("temp", 0);
            }

        
            
            public void compileArrayTerm()
            {
                token.advance();
                compileExpression();

                if (!checkSymbol("]"))
                {
                    Console.WriteLine("No closing ] for the array expression");
                }
            }

           public void compileExpression()
            {
                compileTerm();

                // compileTerm needs to do one token look ahead, so no advance here.
                while (checkSymbol("+") || checkSymbol("-") || checkSymbol("*") || checkSymbol("/") ||
                       checkSymbol("&") || checkSymbol("|") || checkSymbol("<") || checkSymbol(">") ||
                       checkSymbol("="))
                {
                    string localSymbol = token.symbol();
                    token.advance();
                    compileTerm();
                    // write op vm code here
                    if (localSymbol.Equals("+"))
                    {
                        writer.writeXML("add");
                    }
                    else if (localSymbol.Equals("-"))
                    {
                        writer.writeXML("sub");
                    }
                    else if (localSymbol.Equals("*"))
                    {
                        // TODO: make sure the correctness of this line.  Need to push the arguments?
                        writer.writeArithmetic("call Math.multiply 2");
                    }
                    else if (localSymbol.Equals("/"))
                    {
                        writer.writeXML("call Math.divide 2");
                    }
                    else if (localSymbol.Equals("&"))
                    {
                        writer.writeXML("and");
                    }
                    else if (localSymbol.Equals("|"))
                    {
                        writer.writeXML("or");
                    }
                    else if (localSymbol.Equals("<"))
                    {
                        writer.writeXML("lt");
                    }
                    else if (localSymbol.Equals(">"))
                    {
                        writer.writeXML("gt");
                    }
                    else if (localSymbol.Equals("="))
                    {
                        writer.writeXML("eq");
                    }
                    // no advance here, because compileTerm needs to do one token look ahead
                }
            }

           public void compileTerm()
            {
                if (token.tokenType() == Tokenizer.INT_CONST)
                {
                    // push n
                    writer.writePush("constant", token.intVal());
                    token.advance();
                }
                else if (token.tokenType() == Tokenizer.STRING_CONST)
                {
                    // Need to create a string object here
                    String strLiteral = token.stringVal();
                    // push strLiteral.length()
                    writer.writePush("constant", strLiteral.Length());

                    Console.WriteLine("here: " + strLiteral + " " + Integer.toString(strLiteral.Length()));

                    // call String.new 1
                    writer.writeCall("String.new", 1);
                    // pop temp 0
                    //writer.writePop("temp", 0);
                    for (int i = 0; i < strLiteral.Length(); i++)
                    {
                        //      push temp 0
                        //writer.writePush("temp", 0);
                        //      push int i
                        //writer.writePush("constant", i);
                        //      push char c
                        writer.writePush("constant", (int)strLiteral.charAt(i));
                        //      call String.setCharAt 3
                        writer.writeCall("String.appendChar", 2);
                        //      pop temp 1
                        //writer.writePop("temp", 1);
                    }
                    //now on top of the stack should be the address of an intialized string
                    //writer.writePush("temp", 0);

                    token.advance();
                }
                else if (checkKeyword("true") || checkKeyword("false") || checkKeyword("null") ||
                         checkKeyword("this"))
                {
                    if (checkKeyword("null") || checkKeyword("false"))
                    {
                        writer.writePush("constant", 0);
                    }
                    else if (checkKeyword("true"))
                    {
                        writer.writePush("constant", 1);
                        writer.writeArithmetic("neg");
                    }
                    else if (checkKeyword("this"))
                    {
                        writer.writePush("pointer", 0);
                    }
                    token.advance();
                }
                else if (checkSymbol("-") || checkSymbol("~"))
                {
                    token.advance();
                    string localSymbol = token.symbol();
                    compileTerm();

                    // output op
                    if (localSymbol.Equals("-"))
                    {
                        writer.writeArithmetic("neg");
                    }
                    else {
                        writer.writeArithmetic("not");
                    }
                }
                else if (checkIdentifier())
                {
                    string firstHalf = currentName;
                    token.advance();
                    if (checkSymbol("["))
                    {
                        // push the array base address
                        writer.writePush(symbolTable.kindOf(firstHalf), symbolTable.indexOf(firstHalf));
                        compileArrayTerm();
                        writer.writeArithmetic("add");
                        writer.writePop("pointer", 1);
                        writer.writePush("that", 0);
                        token.advance();
                    }
                    else if (checkSymbol("(") || checkSymbol("."))
                    {
                        writeSubroutineHead(firstHalf);
                        token.advance();
                    }
                    else {
                        // if doesn't match [, (, or ., it is a normal identifier
                        writer.writePush(symbolTable.kindOf(firstHalf), symbolTable.indexOf(firstHalf));
                    }
                }
                else if (token.tokenType() == Tokenizer.SYMBOL)
                {
                    if (checkSymbol("("))
                    {
                        token.advance();
                        compileExpression();
                        if (checkSymbol(")"))
                        {
                            token.advance();
                        }
                        else {
                            Console.WriteLine("no closing bracket for term");
                        }
                    }

                }
                else {
                    Console.WriteLine("illegal varName: %s\n", token.token());
                    return;
                }
            }


            public void Dispose()
            {
                output.Close();
            }

            public void writeComment(string comment)
            {
                output.WriteLine("//" + comment);
            }


            public bool checkIdentifier(string currentId) 
            {
                if (token.Token() == Tokenizer.IDENTIFIER) //syntax error
                {
                    currentName = token.id(); //idk idk
                    return true;
                }

                else
                {
                    return false; 
                }
           
            }

            public bool checkSymbol(string currentSymbol)
            {
                if (token.Token() == Tokenizer.SYMBOL) //syntax error
                {
                    return true;
                }

                else
                {
                    return false;
                }
            }

            public bool checkKeyword(string currentKey)
            {
                if (token.Token() == Tokenizer.KEYWORD) //syntax error
                {
                    return true;
                }

                else
                {
                    return false;
                }
            }


            public void writeClassSymbolTable(string className, Dictionary<string, Parser.SymbolEntry> symbols)
            {
                output.WriteLine(className);
                output.WriteLine(symbols);
            }
            
            public void writeConstructorHead(string className, int numArgs)
            {

            }



            public void writeSubroutineHead(string className, string subName, int numArgs) 
            {
                // clear the previous subroutine symbol table
                symbolTable.startSubroutine();
                //isVoid = false;

                // already know that the current token start with constructor, function or method
                int subRoutineKind = token.keyword();  // is it a function or method or constructor

                // match return type
                token.advance();
                //*******************************************
                //Don't know how to handle the "expects" 
                //*******************************************
                if (!expect())//looking for writen type
                {
                    output.WriteLine("Illegal type name for subroutine");
                    return;
                }

                string currentSubName = null;

                // match subroutine identifier
                token.advance();
                if (expect()) //check for identifier
                {
                    currentSubName = className + "." + currentName;
                }
                else {
                    output.WriteLine("illegal subroutine name");
                    return;
                }

                // if it is a method, the first argument is self
                if (subRoutineKind == Tokenizer.METHOD)
                {
                    symbolTable.define("this", className, Tokenizer.ARG);
                }

                // match parameter list
                tokenizer.advance();    
                if (expect("(")) //look for symbol "("
                {
                    compileClassVarDec();
                }


                else {
                    output.WriteLine("no () after function name");
                    return;
                }

                // match the closing ) for the paramater list
                if (!expect(")")) //if ")" does not exist
                {
                    output.WriteLine("no () after function name");
                    return;
                }
            }

            public void writeSubroutineEnd()
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

            int expect(TList list, params string[] expect)
            {

                var front = list.First();

                //shouldn't need this? vv

                for (int i = 0; i < expect.Length; ++i)
                    if (expect[i] == front.context)
                    {
                        list.rmFront();//Expect removes the token upon success.
                        ++tokenCount_;
                        return i;
                    }

                return -1;
            }

            string expect(TList list, Tokenizer.Token.Type expect)
            {
                var front = list.First();

                if (front.type == expect)
                {

                    //  list.rmFront();
                    //  ++tokenCount_;
                    return front.context;
                }
                else return null;
            }
        }
    }
}
