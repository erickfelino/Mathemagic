using System;
using System.Collections.Generic;
using UnityEngine;

public static class MathExpressionValidator
{
    private enum EvalType
    {
        Number,
        Operator,
        OpenParen,
        CloseParen
    }

    private struct EvalToken
    {
        public EvalType Type;
        public double NumberValue;
        public MathOperatorKind OperatorValue;
    }

    public static bool TryEvaluate(List<MathTokenView> tokens, out double result, out string error)
    {
        result = 0;
        error = "";

        if (tokens == null || tokens.Count == 0)
        {
            error = "A equação está vazia.";
            return false;
        }

        List<EvalToken> evalTokens = new List<EvalToken>();

        foreach (var token in tokens)
        {
            if (token == null)
            {
                error = "Há espaços vazios na equação.";
                return false;
            }

            if (token.Kind == MathTokenKind.Number)
            {
                evalTokens.Add(new EvalToken
                {
                    Type = EvalType.Number,
                    NumberValue = token.NumberValue
                });
            }
            else if (token.Kind == MathTokenKind.Operator)
            {
                evalTokens.Add(new EvalToken
                {
                    Type = EvalType.Operator,
                    OperatorValue = token.OperatorValue
                });
            }
        }

        if (!IsValidSequence(evalTokens, out error))
            return false;

        List<EvalToken> outputQueue = new List<EvalToken>();
        Stack<EvalToken> opStack = new Stack<EvalToken>();

        foreach (var token in evalTokens)
        {
            switch (token.Type)
            {
                case EvalType.Number:
                    outputQueue.Add(token);
                    break;

                case EvalType.Operator:
                    while (opStack.Count > 0 && opStack.Peek().Type == EvalType.Operator &&
                           GetPrecedence(opStack.Peek().OperatorValue) >= GetPrecedence(token.OperatorValue))
                    {
                        outputQueue.Add(opStack.Pop());
                    }
                    opStack.Push(token);
                    break;
            }
        }

        while (opStack.Count > 0)
            outputQueue.Add(opStack.Pop());

        Stack<double> valueStack = new Stack<double>();

        foreach (var token in outputQueue)
        {
            if (token.Type == EvalType.Number)
            {
                valueStack.Push(token.NumberValue);
            }
            else if (token.Type == EvalType.Operator)
            {
                if (valueStack.Count < 2)
                {
                    error = "Expressão inválida.";
                    return false;
                }

                double b = valueStack.Pop();
                double a = valueStack.Pop();

                switch (token.OperatorValue)
                {
                    case MathOperatorKind.Add:
                        valueStack.Push(a + b);
                        break;

                    case MathOperatorKind.Subtract:
                        valueStack.Push(a - b);
                        break;

                    case MathOperatorKind.Multiply:
                        valueStack.Push(a * b);
                        break;

                    case MathOperatorKind.Divide:
                        if (Math.Abs(b) < 0.000001)
                        {
                            error = "Divisão por zero.";
                            return false;
                        }
                        valueStack.Push(a / b);
                        break;
                }
            }
        }

        if (valueStack.Count != 1)
        {
            error = "Expressão inválida.";
            return false;
        }

        result = valueStack.Pop();
        return true;
    }

    private static bool IsValidSequence(List<EvalToken> tokens, out string error)
    {
        error = "";

        if (tokens.Count == 0)
        {
            error = "A equação está vazia.";
            return false;
        }

        if (tokens[0].Type != EvalType.Number)
        {
            error = "A conta precisa começar com um número.";
            return false;
        }

        if (tokens[tokens.Count - 1].Type != EvalType.Number)
        {
            error = "A conta precisa terminar com um número.";
            return false;
        }

        for (int i = 0; i < tokens.Count; i++)
        {
            bool shouldBeNumber = i % 2 == 0;

            if (shouldBeNumber && tokens[i].Type != EvalType.Number)
            {
                error = "Números e operadores precisam alternar.";
                return false;
            }

            if (!shouldBeNumber && tokens[i].Type != EvalType.Operator)
            {
                error = "Números e operadores precisam alternar.";
                return false;
            }
        }

        return true;
    }

    private static int GetPrecedence(MathOperatorKind op)
    {
        return op switch
        {
            MathOperatorKind.Multiply => 2,
            MathOperatorKind.Divide => 2,
            MathOperatorKind.Add => 1,
            MathOperatorKind.Subtract => 1,
            _ => 0
        };
    }
}