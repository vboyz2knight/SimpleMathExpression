using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleMathExpression
{
    //only numbers and + - * / ( ) are allows
    //precision to 0.0001
    public class SimpleExpressionParser
    {
        private string InfixExpression = "";
        private string PostfixExpression = "";
        private string Answer = "";
        bool parserError = false;

        public bool ParserError
        {
            get
            {
                return parserError;
            }
        }

        public string myAnswer
        {
            get
            {
                return Answer;
            }
        }

        public SimpleExpressionParser(string expression)
        {
            if (expression.Length > 0)
            {
                InfixExpression = expression;
                Eval();
            }
            else
            {
                Answer = "Expression is zero length.";
                parserError = true;
            }
        }

        public bool Eval()
        {
            bool breturn = false;

            if (IsValidData())
            {
                Answer = SolveMe();
                breturn = true;
            }
            else
            {
                Answer = "Unsupported data in expression.  Unable to evaluate expression.";
                parserError = true;
            }

            return breturn;
        }

        private string SolveMe()
        {

            toPostFixOrder();
            PostFixEval();

            return Answer;
        }
        /*
            1. read postfix expression token by token
            2.   if the token is an operand, push it 
                 into the stack
            3.   if the token is a binary operator, 
            3.1    pop the two top most operands 
                   from the stack
            3.2    apply the binary operator with the 
                   two operands
            3.3    push the result into the stack
            4. finally, the value of the whole postfix 
               expression remains in the stack
         * */
        private string PostFixEval()
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
                        Operand2 = Pop(Stack S);
                        Evaluate (Operand1,Operand2,Operator x);
                        ??Push answer back to stack s??
                        }

                    x = ReadNextToken();  // Read Token
            }
             * */

            Stack<decimal> operandStack = new Stack<decimal>();
            string readInput = readNextInput();
            string myAnswer = "";
            decimal dAnswer = 0.0000m;
            decimal outDouble = 0.0000m;

            while (readInput.Length > 0)
            {
                if (parserError)
                {
                    break;
                }

                if (IsOperand(readInput))
                {//duplicate TryParse....
                    if (decimal.TryParse(readInput, out outDouble))
                    {
                        operandStack.Push(outDouble);
                    }
                    else
                    {
                        myAnswer = "Error unable to parse double: " + readInput;
                        parserError = true;
                    }
                }
                else if (readInput.Length == 1)
                {
                    char[] tmp = readInput.ToCharArray();

                    if (isOperator(tmp[0]))
                    {
                        if (operandStack.Count >= 2)
                        {
                            decimal operand2 = operandStack.Pop();
                            decimal operand1 = operandStack.Pop();
                                                       
                            dAnswer = Evaluate(operand1, operand2, tmp[0]);

                            operandStack.Push(dAnswer);
                        }
                        else
                        {
                            myAnswer = "Trying to evaluate with less than 2 operand in stack.";
                            parserError = true;
                        }
                    }
                    else
                    {
                        myAnswer = "Error unable to parse operator: " + readInput;
                        parserError = true;
                    }
                }

                readInput = readNextInput();
            }

            if (operandStack.Count == 1 && !parserError)
            {
                Answer = operandStack.Pop().ToString();
            }

            return myAnswer;
        }
        private decimal Evaluate(decimal operand1, decimal operand2, char p)
        {
            decimal myEAnswer = 0.0000m;

            switch (p)
            {
                case '/':
                    if (operand2 != 0)
                    {
                        myEAnswer = operand1 / operand2;
                    }
                    else
                    {
                        Answer = "Error! Divide by zero";
                        parserError = true;
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
                    Answer = "Error! Unsuport operation when trying to evaluate.";
                    parserError = true;
                    break;
            }

            return myEAnswer;
        }

        private string readNextInput()
        {
            bool bFirstInputFound = false;
            string myAnswer = "";
            string newPostFixExpression = "";
            char[] tmpPostFixExpression = PostfixExpression.ToCharArray();

            for (int i = 0; i < tmpPostFixExpression.Length; i++)
            {
                if (!bFirstInputFound)
                {
                    if (tmpPostFixExpression[i] != '|' && !isOperator(tmpPostFixExpression[i]))
                    {
                        myAnswer += tmpPostFixExpression[i];
                    }
                    else if (isOperator(tmpPostFixExpression[i]))
                    {
                        myAnswer = tmpPostFixExpression[i].ToString();
                        bFirstInputFound = true;
                    }
                    else if (tmpPostFixExpression[i] == '|')
                    {
                        bFirstInputFound = true;
                    }
                }
                else
                {
                    newPostFixExpression += tmpPostFixExpression[i];
                }
            }

            PostfixExpression = newPostFixExpression;

            return myAnswer;
        }
        private short PrecedenceNum(char top)
        {
            switch (top)
            {
                case '/':
                case '*':
                    return 4;
                case '+':
                case '-':
                    return 1;
                default:
                    return -1;
            }
        }

        /*
         1. Scan the Infix Expression from left to right.
         2. If the scannned character is an operand, copy it to the Postfix Expression.
         3. If the scanned character is left parentheses, push it onto the stack.
         4. If the scanned character is right parenthesis, the symbol at the top of the stack is popped off the stack 
                and copied to the Postfix Expression. Repeat until the symbol at the top of the stack is a left parenthesis 
                (both parentheses are discarded in this process).
         5. If the scanned character is an operator and has a higher precedence than the symbol at the top of the stack, 
                push it onto the stack.
         6. If the scanned character is an operator and the precedence is lower than or equal to the precedence of the operator 
                at the top of the stack, one element of the stack is popped to the Postfix Expression; repeat this step with the new top 
                element on the stack. Finally, push the scanned character onto the stack.
         7. After all characters are scanned, the stack is popped to the Postfix Expression until the stack is empty
         * */
        private void toPostFixOrder()
        {
            string tmpInput = "";
            Stack<char> operator_stack = new Stack<char>();
            string PostFix = "";

            string tmpInfix = InfixExpression;

            char[] infixExpression = tmpInfix.ToCharArray();

            for (int i = 0; i < infixExpression.Length; i++)
            {
                if (isOperator(infixExpression[i]))
                {
                    //we found an operator implying previous tmpInput as Operand?
                    if (IsOperand(tmpInput))
                    {
                        //insert | to tell us that this seperate each input because we scanning one char
                        //at a time but we can have one input as 25.62
                        PostFix += tmpInput + "|";

                        tmpInput = "";
                    }

                    if (operator_stack.Count == 0)
                    {
                        operator_stack.Push(infixExpression[i]);
                    }
                    else
                    {
                        while (PrecedenceNum(operator_stack.Peek()) >= PrecedenceNum(infixExpression[i]))
                        {
                            PostFix += operator_stack.Pop();
                        }

                        operator_stack.Push(infixExpression[i]);

                        /*
                        if (PrecedenceNum(infixExpression[i]) > PrecedenceNum(operator_stack.Peek()))
                        {
                            operator_stack.Push(infixExpression[i]);
                        }
                        else
                        {
                            while (operator_stack.Count>0 && PrecedenceNum(infixExpression[i]) <= PrecedenceNum(operator_stack.Peek()) && operator_stack.Peek() != '(')
                            {
                                PostFix += operator_stack.Pop();
                            }

                            operator_stack.Push(infixExpression[i]);
                        }
                            * */
                    }

                }
                else if (infixExpression[i] == '(')
                {
                    operator_stack.Push(infixExpression[i]);
                }
                else if (infixExpression[i] == ')')
                {
                    ////////////////////////
                    if (tmpInput.Length > 0)
                    {
                        PostFix += tmpInput + "|";
                        tmpInput = "";
                    }

                    while (operator_stack.Count > 0 && operator_stack.Peek() != '(')
                    {
                        PostFix += operator_stack.Pop();
                    }

                    //pop the left parentheses
                    operator_stack.Pop();
                }
                else
                {
                    tmpInput += infixExpression[i];
                }
            }

            while (operator_stack.Count != 0)
            {
                PostFix += operator_stack.Pop();
            }

            PostfixExpression = PostFix;
            Answer = PostFix;
        }

        private bool IsOperand(string tmpInput)
        {
            bool bReturn = false;
            decimal outDouble = 0.0000m;

            if (decimal.TryParse(tmpInput, out outDouble))
            {
                bReturn = true;
            }


            return bReturn;
        }

        private bool isOperator(char p)
        {
            bool bReturn = false;

            switch (p)
            {
                case '/':
                case '+':
                case '-':
                case '*':
                    bReturn = true;
                    break;
                default:
                    bReturn = false;
                    break;
            }

            return bReturn;
        }

        private bool IsValidData()
        {
            bool bValid = true;
            //string tmpInput = "";
            //double doubleResult = 0.00f;

            //look at the equation each characters to see if it not /,+,-,*,(,), number
            /*
            for (int i = 0; i < InfixExpression.Length; i++)
            {
                switch (InfixExpression[i])
                {
                    case '/':
                    case '+':
                    case '-':
                    case '*':
                    case '(':
                    case ')':
                        if (tmpInput.Length > 0)
                        {
                            //if it not a number then you have unsupported data in expression
                            if (!double.TryParse(tmpInput, out doubleResult))
                            {
                                bValid = false;
                            }
                        }

                        tmpInput = "";
                        break;
                    default:
                        tmpInput += InfixExpression[i];
                        break;
                }
            }*/
            if (Regex.IsMatch(InfixExpression, @"[^()+-/*1234567890.]", RegexOptions.IgnoreCase))
            {
                bValid = false;
            }//count of open ( must match count of close )
            else if( InfixExpression.Count( (a)=>a=='(') != InfixExpression.Count( (a)=> a==')') )
            {
                bValid = false;
            }

            return bValid;
        }

    }
}
