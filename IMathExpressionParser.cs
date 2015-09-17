using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleMathExpression
{
    public interface IMathExpressionParser
    {
        bool SolveExpression(string expression, out double answer,out string error);
    }
}
