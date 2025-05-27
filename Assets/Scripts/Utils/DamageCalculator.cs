using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class PureFormulaCalculator
{
    private readonly string _formula;

    public PureFormulaCalculator(string formula)
    {
        _formula = formula;
    }

    public float Calculate(params float[] values)
    {
        // {0}, {1}을 실제 값으로 치환
        string expression = _formula;
        for (int i = 0; i < values.Length; i++)
        {
            expression = expression.Replace($"{{{i}}}", values[i].ToString());
        }

        return EvaluateInfix(expression);
    }

    private float EvaluateInfix(string expression)
    {
        var postfix = ConvertToPostfix(expression);
        return EvaluatePostfix(postfix);
    }

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
    private float EvaluatePostfix(List<string> postfix)
    {
        Stack<float> stack = new();

        foreach (var token in postfix)
        {
            if (float.TryParse(token, out float num))
            {
                stack.Push(num);
            }
            else
            {
                float b = stack.Pop();
                float a = stack.Pop();
                stack.Push(ApplyOperator(a, b, token));
            }
        }

        return stack.Pop();
    }

    // 연산자 적용
    private float ApplyOperator(float a, float b, string op)
    {
        return op switch
        {
            "+" => a + b,
            "-" => a - b,
            "*" => a * b,
            "/" => a / b,
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
            else if ("+-*/()".Contains(c))
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

public static class DamageCalculator
{
    public static float Evaluate(string formula, List<float> inputStats, List<float> inputKeywords)
    {
        if(string.IsNullOrWhiteSpace(formula)) return 0f;
        
        formula = Regex.Replace(formula, @"\{(\d+)\}", 
            m => inputStats[int.Parse(m.Groups[1].Value)].ToString(CultureInfo.InvariantCulture));
        formula = Regex.Replace(formula, @"\[(\d+)\]", 
            m => inputStats[int.Parse(m.Groups[1].Value)].ToString(CultureInfo.InvariantCulture));
        
        List<string> expr = ExpressionConvert(formula);
        return EvaluateFinal(expr);
    }

    private static List<string> ExpressionConvert(string expr)
    {
        Stack<string> opStack = new();
        List<string> output = new();
        List<string> tokens = Tokenize(expr);

        Dictionary<string, int> precedence = new() { ["+"] = 1, ["-"] = 1, ["*"] = 2, ["/"] = 2 };
        foreach (var token in tokens)
        {
            if(float.TryParse(token, out _))
                output.Add(token);
            else if (token == "(")
                opStack.Push(token);
            else if (token == ")")
            {
                while (opStack.Peek() != "(")
                {
                    output.Add(opStack.Pop());
                }
                opStack.Pop();
            }
            else if (precedence.ContainsKey(token))
            {
                while(opStack.Count > 0 && precedence.TryGetValue(opStack.Peek(), out int p) && p >= precedence[token])
                    output.Add(opStack.Pop());
                opStack.Push(token);
            }
        }
        while(opStack.Count > 0)
            output.Add(opStack.Pop());
        return output;
    }

    private static List<string> Tokenize(string expr)
    {
        List<string> tokens = new();
        Regex regex = new Regex(@"(\d+\.\d+|\d+|[()+\-*/])");
        foreach(Match m in regex.Matches(expr.Replace(" ", "")))
            tokens.Add(m.Value);
        return tokens;
    }

    private static float EvaluateFinal(List<string> expr)
    {
        Stack<float> stack = new();
        foreach (var token in expr)
        {
            if (float.TryParse(token, out float num))
            {
                stack.Push(num);
            }
            else
            {
                float second = stack.Pop();
                float first = stack.Pop();
                stack.Push(token switch
                {
                    "+" => first + second,
                    "-" => first - second,
                    "*" => first * second,
                    "/" => first / second,
                    _ => throw new ArgumentException($"Invalid operator: {token}")
                });
            }
        }
        return stack.Pop();
    }
}