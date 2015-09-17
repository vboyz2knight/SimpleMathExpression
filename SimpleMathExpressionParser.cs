using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleMathExpression
{
    //Format of expression when something like (9)2 have to explictly put as (9)*2
    //Double type will be use in all calculation
    public class SimpleExpressionParser : IMathExpressionParser
    {
        readonly char[] allowedOperatorArray = { '-', '+', '/', '*', '%', '^' };
        readonly string allowedOperatorSeperator = @"(\+)|(\-)|(/)|(\^)|(\*)|(%)|(\()|(\))";

        private string infixExpression = string.Empty;
        private string postfixExpression = string.Empty;
        double myAnswer = 0;
        string myError = string.Empty;

        public double MyAnswer
        {
            get { return this.myAnswer; }
            private set { this.myAnswer = value;}
        }

        public string MyError
        {
            get { return this.myError; }
            private set { this.myError = value; }
        }

        private List<string> _infixExpressionList = new List<string>();
        private List<string> _RPNList = new List<string>();

        enum associative { Left_Associative, Right_Associative };

        Dictionary<string, int> operatorsAssociative = new Dictionary<string, int> {
            {"-",(int)associative.Left_Associative},
            {"+",(int)associative.Left_Associative},
            {"/",(int)associative.Left_Associative},
            {"*",(int)associative.Left_Associative},
            {"%",(int)associative.Left_Associative},
            {"^",(int)associative.Left_Associative}
        };

        public bool SolveExpression(string expression, out double answer, out string error)
        {
            bool bReturn = false;

            if (expression.Length > 0)
            {
                try
                {
                    parseExpressionToInfixList(expression);
                    infixExpression = expression;

                    if (isValidExpression())
                    {
                        postfixExpression = ToRPNOrder().ToString();
                        myAnswer = RPNEval();

                        bReturn = true;
                    }
                    else
                    {
                        myError = "Unsupported data in expression.  Unable to evaluate expression.";
                    }
                }
                catch( Exception ex)
                {
                    myError += ex.Message;
                    bReturn = false;
                }
            }
            else
            {
                myError = "Expression is empty!";
            }

            if(bReturn)
            {
                answer = myAnswer;
            }
            else
            {
                answer = 0;
            }

            error = myError;

            return bReturn;
        }

        private bool isAssociative(string token, associative assoc)
        {
            if (!isOperator(token))
            {
                throw new ArgumentException(string.Format("Invalid token as operator: {0}", token));
            }
            else if (operatorsAssociative[token] == (int)assoc)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private double RPNEval()
        {
            /*
            Initialize(Stack S)
            x = ReadToken();  // Read Token
            while(x)
            {
                    if ( x is Operand )
                        Push ( x ) Onto Stack S.

                    if ( x is Operator )
                        {
                        Operand2 = Pop(Stack S);
                        Operand1 = Pop(Stack S);
                        Evaluate (Operand1,Operand2,Operator x);
                        ??Push answer back to stack s??
                        }

                    x = ReadNextToken();  // Read Token
            }
             * */

            Stack<double> stack = new Stack<double>();
            string myAnswer = string.Empty;
            double operand1 = 0;
            double operand2 = 0;

            if (_RPNList.Count > 2)
            {
                foreach (string token in _RPNList)
                {
                    if (isOperand(token))
                    {
                        stack.Push(double.Parse(token));
                    }
                    else if (isOperator(token))
                    {
                        //at least 2 operand
                        if (_RPNList.Count >= 2)
                        {
                            operand2 = stack.Pop();
                            operand1 = stack.Pop();
                        }
                        else
                        {
                            throw new ArgumentException("Expression list does not have enough arguments to solve!");
                        }

                        stack.Push(Evaluate(operand1, operand2, char.Parse(token)));
                    }
                }
            }
            else
            {
                throw new ArgumentException("Expression list does not have enough arguments!");
            }

            if (stack.Count() >= 2)
            {
                throw new ArgumentException("The result stack have more than 2 items!");
            }

            return stack.Pop();
        }

        /*
         * Shunting-yard algorithm from 
         * https://en.wikipedia.org/wiki/Shunting-yard_algorithm#The_algorithm_in_detail
         * 
         *  While there are tokens to be read:
                Read a token.
                If the token is a number, then add it to the output queue.
                If the token is a function token, then push it onto the stack.
                If the token is a function argument separator (e.g., a comma):
                    Until the token at the top of the stack is a left parenthesis, pop operators off the stack onto the output queue. If no left parentheses are encountered, either the separator was misplaced or parentheses were mismatched.
                If the token is an operator, o1, then:
                    while there is an operator token, o2, at the top of the operator stack, and either
                        o1 is left-associative and its precedence is less than or equal to that of o2, or
                        o1 is right associative, and has precedence less than that of o2,
                        then pop o2 off the operator stack, onto the output queue;
                    push o1 onto the operator stack.
                If the token is a left parenthesis (i.e. "("), then push it onto the stack.
                If the token is a right parenthesis (i.e. ")"):
                    Until the token at the top of the stack is a left parenthesis, pop operators off the stack onto the output queue.
                    Pop the left parenthesis from the stack, but not onto the output queue.
                    If the token at the top of the stack is a function token, pop it onto the output queue.
                    If the stack runs out without finding a left parenthesis, then there are mismatched parentheses.
            When there are no more tokens to read:
                While there are still operator tokens in the stack:
                    If the operator token on the top of the stack is a parenthesis, then there are mismatched parentheses.
                    Pop the operator onto the output queue.
            Exit.
         */

        private string ToRPNOrder()
        {
            Stack<char> operatorsStack = new Stack<char>();

            foreach (string token in _infixExpressionList)
            {
                if (isOperand(token))
                {
                    _RPNList.Add(token);
                }
                else if (isOperator(token))
                {
                    while (operatorsStack.Count > 0 && isOperator(operatorsStack.Peek().ToString()))
                    {
                        if ((isAssociative(token, associative.Left_Associative) && (precedenceNum(token) <= precedenceNum(operatorsStack.Peek().ToString()))) |
                            (isAssociative(token, associative.Right_Associative) && precedenceNum(token) < precedenceNum(operatorsStack.Peek().ToString())))
                        {
                            _RPNList.Add(operatorsStack.Pop().ToString());
                            continue;
                        }
                        break;
                    }

                    operatorsStack.Push(char.Parse(token));
                }
                else if (token == "(")
                {
                    operatorsStack.Push('(');
                }
                else if (token == ")")
                {
                    while ((operatorsStack.Count() > 0) && (operatorsStack.Peek() != '('))
                    {
                        _RPNList.Add(operatorsStack.Pop().ToString());
                    }

                    operatorsStack.Pop();
                }
            }

            while (operatorsStack.Count > 0)
            {
                _RPNList.Add(operatorsStack.Pop().ToString());
            }

            return _RPNList.ToString();
            /*
             *  For all the input tokens [S1]:
                    Read the next token [S2];
                    If token is an operator (x) [S3]:
                        While there is an operator (y) at the top of the operators stack and either (x) is
                        left-associative and its precedence is less or equal to that of (y), or (x) is right-associative
                        and its precedence is less than (y) [S4]:
                            Pop (y) from the stack [S5];
                            Add (y) output buffer [S6];
                        Push (x) on the stack [S7];
                    Else If token is left parenthesis, then push it on the stack [S8];
                    Else If token is a right parenthesis [S9]:
                        Until the top token (from the stack) is left parenthesis, pop from the stack to the output buffer [S10];
                        Also pop the left parenthesis but don’t include it in the output buffer [S11];
                    Else add token to output buffer [S12].
                While there are still operator tokens in the stack, pop them to output [S13]
             * */
        }
        private double Evaluate(double operand1, double operand2, char p)
        {
            double myEAnswer = 0;

            switch (p)
            {
                case '%':
                    return operand1 % operand2;
                case '^':
                    return Math.Pow(operand1, operand2);
                case '/':
                    if (operand2 != 0)
                    {
                        myEAnswer = operand1 / operand2;
                    }
                    else
                    {
                        throw new DivideByZeroException();
                    }
                    break;
                case '+':
                    myEAnswer = operand1 + operand2;
                    break;
                case '-':
                    myEAnswer = operand1 - operand2;
                    break;
                case '*':
                    myEAnswer = operand1 * operand2;
                    break;
                default:
                    throw new ArgumentException(string.Format("Error! Unsuport operation when trying to evaluate {0} {1} {2}.", operand1, p, operand2));
            }

            return myEAnswer;
        }
        private short precedenceNum(string oper)
        {
            char op;

            try
            {
                op = char.Parse(oper);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            switch (op)
            {
                case '^':
                    return 5;
                case '/':
                case '*':
                case '%':
                    return 4;
                case '+':
                case '-':
                    return 1;
                default:
                    throw new ArgumentException(string.Format("Unsupport operation detected: {0}", oper));
            }
        }
        private bool isOperand(string tmpInput)
        {
            bool bReturn = false;
            double outdouble = 0;

            if (double.TryParse(tmpInput, out outdouble))
            {
                bReturn = true;
            }

            return bReturn;
        }

        private bool isOperator(string oper)
        {
            try
            {
                return allowedOperatorArray.Any(x => x == char.Parse(oper));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("{0}", ex));
            }
        }

        private void parseExpressionToInfixList(string expression)
        {
            try
            {
                _infixExpressionList.AddRange(Regex.Split(expression, allowedOperatorSeperator));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("{0}", ex));
            }

            //remove empty items
            _infixExpressionList = _infixExpressionList.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }
        private bool isValidExpression()
        {
            bool bValid = true;

            //only certain char allowed
            foreach (string s in _infixExpressionList)
            {
                //is it a decimal?
                if (Regex.IsMatch(s, @"\d"))
                {
                    continue;
                }
                else if (Regex.IsMatch(s, allowedOperatorSeperator))
                {
                    continue;
                }
                else
                {
                    bValid = false;
                    break;
                }
            }

            //count of open ( must match count of close )
            if (_infixExpressionList.Count((a) => a == "(") != _infixExpressionList.Count((a) => a == ")"))
            {
                bValid = false;
            }

            return bValid;
        }

        
    }
}
