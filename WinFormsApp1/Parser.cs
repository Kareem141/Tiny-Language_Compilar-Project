using System;
using System.Collections.Generic;
using System.Linq;

namespace Tiny_Language
{
    internal class Parser
    {
        private List<Token> tokens;
        private int index = 0;

        private Token Current => index < tokens.Count ? tokens[index] : null;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens.Where(t => t.Type != "COMMENT").ToList();
        }

        private void Advance() => index++;

        private void Match(string expectedValue)
        {
            if (Current != null && Current.Value == expectedValue)
                Advance();
            else
                throw new Exception(
                    $"Expected '{expectedValue}' but found " +
                    $"'{Current?.Value ?? "end of input"}' ({Current?.Type ?? "—"})");
        }

        private void MatchType(string expectedType)
        {
            if (Current != null && Current.Type == expectedType)
                Advance();
            else
                throw new Exception(
                    $"Expected {expectedType} but found " +
                    $"'{Current?.Value ?? "end of input"}' ({Current?.Type ?? "—"})");
        }


        public void ParseProgram()
        {
 
            if (Current != null && Current.Value == "main")
            {
                Advance();       
                Match("{");
                ParseStatements();
                Match("}");
            }
            else
            {
                ParseStatements();
            }


            if (Current != null)
                throw new Exception(
                    $"Unexpected token '{Current.Value}' ({Current.Type}) " +
                    $"after the program ended. " +
                    $"Check for a missing semicolon or an extra token.");
        }


        private void ParseStatements()
        {
            while (Current != null
           && Current.Value != "}"
           && Current.Value != "until"
           && Current.Value != "elseif"
           && Current.Value != "else"
           && Current.Value != "end")      
            {
                ParseStatement();

                if (Current != null && Current.Value == ";")
                    Advance();
            }
        }

        private void ParseStatement()
        {
            if (Current == null) return;

            switch (Current.Type)
            {
                case "COMMENT":
                    Advance();                     
                    break;

                case "DATATYPE":
                    ParseDeclaration();            
                    break;

                case "ID":
                    ParseAssignment();
                    break;

                case "FUNC_CALL":
                    ParseFuncCallStatement();       
                    break;

                default:
                    switch (Current.Value)
                    {
                        case "if": ParseIf(); break;
                        case "repeat": ParseRepeat(); break;
                        case "write": ParseWrite(); break;
                        case "read": ParseRead(); break;
                        case "return": ParseReturn(); break;
                        default:
                            throw new Exception(
                                $"A statement cannot start with " +
                                $"'{Current.Value}' ({Current.Type}).");
                    }
                    break;
            }
        }


        private void ParseDeclaration()
        {
            MatchType("DATATYPE");         


            if (Current?.Type == "FUNC_CALL")
            {
                ParseFunctionDefinition();
                return;
            }

            if (Current?.Type != "ID")
                throw new Exception(
                    $"Expected an identifier after datatype, " +
                    $"found '{Current?.Value}'.");

            MatchType("ID");

            if (Current?.Value == ":=")
            {
                Advance();
                ParseExpression();
            }

            while (Current?.Value == ",")
            {
                Advance();                 

                if (Current?.Type != "ID")
                    throw new Exception(
                        $"Expected an identifier after ',' in declaration, " +
                        $"found '{Current?.Value}'.");

                MatchType("ID");
                if (Current?.Value == ":=")
                {
                    Advance();
                    ParseExpression();
                }
            }
        }


        private void ParseFunctionDefinition()
        {
            MatchType("FUNC_CALL");    
            Match("(");
            ParseParamList();
            Match(")");
            Match("{");
            ParseStatements();
            Match("}");
        }

        private void ParseParamList()
        {
            if (Current?.Value == ")") return;     

            MatchType("DATATYPE");
            MatchType("ID");

            while (Current?.Value == ",")
            {
                Advance();
                MatchType("DATATYPE");
                MatchType("ID");
            }
        }


        private void ParseAssignment()
        {
            MatchType("ID");
            Match(":=");
            ParseExpression();
        }


        private void ParseIf()
        {
            Match("if");

            bool hasParen = Current?.Value == "(";
            if (hasParen) Advance();

            ParseCondition();

            if (hasParen) Match(")");

            Match("then");

            ParseStatements();

            while (Current?.Value == "elseif")
            {
                Advance();

                bool elseifHasParen = Current?.Value == "(";
                if (elseifHasParen) Advance();

                ParseCondition();

                if (elseifHasParen) Match(")");

                Match("then");

                ParseStatements();   
            }

            if (Current?.Value == "else")
            {
                Advance();
                ParseStatements();   
            }

            if (Current?.Value == "end")
                Advance();
        }


        private void ParseRepeat()
        {
            Match("repeat");


            if (Current?.Value == "{")
            {
                Advance();
                ParseStatements();
                Match("}");
            }
            else
            {
                ParseStatements();
            }

            Match("until");

            if (Current?.Value == "(")
            {
                Advance();
                ParseCondition();
                Match(")");
            }
            else
            {
                ParseCondition();
            }
        }

        private void ParseWrite()
        {
            Match("write");

            if (Current?.Value == "endl")
            {
                Advance();
            }
            else if (Current?.Value == "(")
            {
                Advance();
                ParseExpression();
                Match(")");
            }
            else
            {
                ParseExpression();
            }
        }

        private void ParseRead()
        {
            Match("read");

            if (Current?.Value == "(")
            {
                Advance();
                MatchType("ID");
                Match(")");
            }
            else
            {
                MatchType("ID");
            }
        }


        private void ParseReturn()
        {
            Match("return");
            ParseExpression();
        }

        private void ParseFuncCallStatement()
        {
            MatchType("FUNC_CALL");
            Match("(");
            ParseArgList();
            Match(")");
        }

        private void ParseBlock()
        {
            if (Current?.Value == "{")
            {
                Advance();
                ParseStatements();
                Match("}");
            }
            else
            {
                ParseStatement();
                if (Current?.Value == ";") Advance();
            }
        }


        private void ParseCondition()
        {
            ParseRelationalExpr();

            while (Current?.Type == "OP_BOOL")     
            {
                Advance();
                ParseRelationalExpr();
            }
        }

        private void ParseRelationalExpr()
        {
            ParseExpression();

            if (Current?.Type == "OP_COND")       
                Advance();
            else
                throw new Exception(
                    $"Expected a relational operator (<, >, =, <>, <=, >=) " +
                    $"but found '{Current?.Value ?? "end of input"}'.");

            ParseExpression();
        }


        private void ParseExpression()
        {
            ParseTerm();

            while (Current?.Type == "OP_ARITH"
                   && (Current.Value == "+" || Current.Value == "-"))
            {
                Advance();
                ParseTerm();
            }
        }

        private void ParseTerm()
        {
            ParseFactor();

            while (Current?.Type == "OP_ARITH"
                   && (Current.Value == "*" || Current.Value == "/"))
            {
                Advance();
                ParseFactor();
            }
        }


        private void ParseFactor()
        {
            if (Current == null)
                throw new Exception("Unexpected end of input inside an expression.");

            if (Current.Type == "ID" || Current.Type == "NUM" || Current.Type == "STRING")
            {
                Advance();
            }
            else if (Current.Value == "(")
            {
                Advance();
                ParseExpression();
                Match(")");
            }
            else if (Current.Type == "FUNC_CALL")
            {
                Advance();
                Match("(");
                ParseArgList();
                Match(")");
            }
            else
            {
                throw new Exception(
                    $"Expected an identifier, number, or expression " +
                    $"but found '{Current.Value}' ({Current.Type}).");
            }
        }

        private void ParseArgList()
        {
            if (Current?.Value == ")") return;   

            ParseExpression();
            while (Current?.Value == ",")
            {
                Advance();
                ParseExpression();
            }
        }
    }
}