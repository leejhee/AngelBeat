using System;
using System.Collections.Generic;

public class PureFormulaCalculator
{
    private readonly string _formula;

    public PureFormulaCalculator(string formula)
    {
        _formula = formula;
    }

    public double Calculate(params double[] values)
    {
        // {0}, {1}을 실제 값으로 치환
        string expression = _formula;
        for (int i = 0; i < values.Length; i++)
        {
            expression = expression.Replace($"{{{i}}}", values[i].ToString());
        }

        return EvaluateInfix(expression);
    }

    private double EvaluateInfix(string expression)
    {
        var postfix = ConvertToPostfix(expression);
        return EvaluatePostfix(postfix);
    }

    // 중위 → 후위 표기법 변환 (Shunting-yard 알고리즘)
    private List<string> ConvertToPostfix(string expr)
    {
        List<string> output = new List<string>();
        Stack<string> stack = new Stack<string>();
        var tokens = Tokenize(expr);

        Dictionary<string, int> precedence = new Dictionary<string, int>
        {
            { "^", 4 }, { "*", 3 }, { "/", 3 }, { "+", 2 }, { "-", 2 }, { "(", 1 }
        };

        foreach (var token in tokens)
        {
            if (double.TryParse(token, out _))
            {
                output.Add(token);
            }
            else if (token == "(")
            {
                stack.Push(token);
            }
            else if (token == ")")
            {
                while (stack.Peek() != "(")
                {
                    output.Add(stack.Pop());
                }
                stack.Pop();
            }
            else
            {
                while (stack.Count > 0 && precedence[stack.Peek()] >= precedence[token])
                {
                    output.Add(stack.Pop());
                }
                stack.Push(token);
            }
        }

        while (stack.Count > 0)
        {
            output.Add(stack.Pop());
        }

        return output;
    }

    // 후위 표기식 계산
    private double EvaluatePostfix(List<string> postfix)
    {
        Stack<double> stack = new Stack<double>();

        foreach (var token in postfix)
        {
            if (double.TryParse(token, out double num))
            {
                stack.Push(num);
            }
            else
            {
                double b = stack.Pop();
                double a = stack.Pop();
                stack.Push(ApplyOperator(a, b, token));
            }
        }

        return stack.Pop();
    }

    // 연산자 적용
    private double ApplyOperator(double a, double b, string op)
    {
        return op switch
        {
            "+" => a + b,
            "-" => a - b,
            "*" => a * b,
            "/" => a / b,
            "^" => Math.Pow(a, b),
            _ => throw new ArgumentException($"Invalid operator: {op}")
        };
    }

    // 수식 토큰 분리
    private List<string> Tokenize(string expr)
    {
        List<string> tokens = new List<string>();
        string numBuffer = "";

        for (int i = 0; i < expr.Length; i++)
        {
            char c = expr[i];

            if (char.IsDigit(c) || c == '.')
            {
                numBuffer += c;
            }
            else if ("+-*/^()".Contains(c))
            {
                if (numBuffer != "")
                {
                    tokens.Add(numBuffer);
                    numBuffer = "";
                }
                tokens.Add(c.ToString());
            }
        }

        if (numBuffer != "")
        {
            tokens.Add(numBuffer);
        }

        return tokens;
    }
}
