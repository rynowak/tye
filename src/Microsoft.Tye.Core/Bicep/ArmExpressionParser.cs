using System;
using System.Linq;
using System.Text;

namespace Microsoft.Tye.Core
{
    internal static class ArmExpressionParser
    {
        public static ParsedValue Parse(string input)
        {
            // SOOO dirty. We only handle the cases we need for OAM.
            if (input.Length >= 2 && input[0] == '[' && input[^1] == ']')
            {
                 var innards = input[1..^1];

                 var lparen = innards.IndexOf('(');
                 var rparen = innards.IndexOf(')');

                 var name = innards.Substring(0, lparen);
                 var args = innards[lparen..rparen];
                 var values = args.Split(",");

                 var expr = new FunctionExpression(name, values.Select(v => v.Trim().Trim('\'')).ToArray());
                 return new ParsedValue(new[]{ string.Empty, string.Empty, }, new[] { expr, });
            }

            return new ParsedValue(input);
        }
    }

    internal readonly struct ParsedValue
    {
        public readonly string[] Strings;
        public readonly ArmExpression[] Expressions;

        public ParsedValue(string text)
        {
            Strings = new[]{ text, };
            Expressions = Array.Empty<ArmExpression>();
        }

        public ParsedValue(string[] strings, ArmExpression[] expressions)
        {
            Strings = strings;
            Expressions = expressions;
        } 

        public string Eval()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < Strings.Length; i++)
            {
                sb.Append(Strings[i]);
                if (i < Expressions.Length)
                {
                    sb.Append(Expressions[i].Eval());
                }
            }

            return sb.ToString();
        }
    }

    internal abstract class ArmExpression
    {
        public abstract string Eval();
    }

    internal class FunctionExpression : ArmExpression
    { 
        public FunctionExpression(string name, string[] args)
        {
            Name = name;
            Args = args;
        }

        public string Name { get; }

        public string[] Args { get; }

        public override string Eval()
        {
            if (Name == "format")
            {
                return string.Format(Args[0], Args[1..]);
            }

            throw new InvalidOperationException("Unsupported function: " + Name);
        }
    }
}
