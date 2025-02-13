// Copyright Zero Games. All Rights Reserved.

using System.Runtime.CompilerServices;

namespace ZeroGames.Evalite;

public partial class Compiler
{

	private readonly record struct Operator(int32 Precedence, bool IsRightAssociative)
	{
		public static bool operator >(Operator lhs, Operator rhs) => rhs.IsRightAssociative && lhs.Precedence > rhs.Precedence || !rhs.IsRightAssociative && lhs.Precedence >= rhs.Precedence;
		public static bool operator <(Operator lhs, Operator rhs) => throw new NotSupportedException();
	}

	private readonly record struct CallstackFrame(string FunctionName, int32 NumParameters = 0)
	{
		public Node Node => new(ENodeType.Function, $"{FunctionName}:{NumParameters}");
	}

	private enum ENodeType : uint8
	{
		None,
		Integer,
		Number,
		Boolean,
		String,
		Operator,
		Function,
		Variable,
	}

	private readonly record struct Node(ENodeType Type, string Value);

	private struct ParserContext()
	{
		public void FinalizeScope()
		{
			while (OperatorStack.TryPeek(out var token) && token.Type != ETokenType.LeftParen)
			{
				Output.Add(new(ENodeType.Operator, OperatorStack.Pop().Value));
			}
		}
		
		public Exception Exception() => new InvalidOperationException($"Unknown token type '{CurrentToken.Type}'");
		
		public Token LastToken { get; set; }
		public Token CurrentToken { get; set; }
		public List<Node> Output { get; } = [];
		public Stack<Token> OperatorStack { get; } = [];
		public Stack<CallstackFrame> Callstack { get; } = [];
		public bool ExpectsOperand { get; set; } = true;
	}

	private IEnumerable<Node> Parse(IEnumerable<Token> tokens)
	{
		ParserContext context = new();
		foreach (var token in tokens)
		{
			context.CurrentToken = token;
			
			switch (token.Type)
			{
				case ETokenType.Integer:
				case ETokenType.Number:
				case ETokenType.Boolean:
				case ETokenType.String:
				{
					ParseLiteral(ref context);
					break;
				}
				case ETokenType.Operator:
				{
					ParseOperator(ref context);
					break;
				}
				case ETokenType.LeftParen:
				{
					ParseLeftParen(ref context);
					break;
				}
				case ETokenType.RightParen:
				{
					ParseRightParen(ref context);
					break;
				}
				case ETokenType.Comma:
				{
					ParseComma(ref context);
					break;
				}
				case ETokenType.Identifier:
				{
					ParseIdentifier(ref context);
					break;
				}
				default:
				{
					throw context.Exception();
				}
			}

			context.LastToken = token;
		}

		context.FinalizeScope();
		if (context.OperatorStack.Count != 0)
		{
			throw new InvalidOperationException("Mismatched left paren.");
		}
		
		// We don't check grammar and semantics because Expression.Lambda.Compile() does.
		return context.Output;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ParseLiteral(ref ParserContext context)
	{
		ENodeType type = context.CurrentToken.Type switch
		{
			ETokenType.Integer => ENodeType.Integer,
			ETokenType.Number => ENodeType.Number,
			ETokenType.Boolean => ENodeType.Boolean,
			ETokenType.String => ENodeType.String,
			_ => throw context.Exception(),
		};
		context.Output.Add(new(type, context.CurrentToken.Value));
		context.ExpectsOperand = false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ParseOperator(ref ParserContext context)
	{
		string value = context.CurrentToken.Value;
		if (value is "+" or "-" or "!" && context.ExpectsOperand)
		{
			// Unary operators
			context.OperatorStack.Push(context.CurrentToken with { Value = value == "!" ? "!" : 'u' + value });
		}
		else
		{
			while (context.OperatorStack.TryPeek(out var op) && op.Type == ETokenType.Operator && _operatorMap[op.Value] > _operatorMap[context.CurrentToken.Value])
			{
				context.Output.Add(new(ENodeType.Operator, context.OperatorStack.Pop().Value));
			}
			context.OperatorStack.Push(context.CurrentToken);
			context.ExpectsOperand = true;
		}
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ParseLeftParen(ref ParserContext context)
	{
		if (!context.ExpectsOperand)
		{
			if (context.LastToken.Type != ETokenType.Identifier)
			{
				throw context.Exception();
			}
			
			// Function call
			context.Output.RemoveAt(context.Output.Count - 1);
			context.Callstack.Push(new(context.LastToken.Value));
		}
		
		context.OperatorStack.Push(context.CurrentToken);
		context.ExpectsOperand = true;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ParseRightParen(ref ParserContext context)
	{
		context.FinalizeScope();
		
		if (context.OperatorStack.TryPop(out _))
		{
			if (context.Callstack.TryPop(out var frame))
			{
				if (context.LastToken.Type != ETokenType.LeftParen)
				{
					frame = frame with { NumParameters = frame.NumParameters + 1 };
				}
				
				context.Output.Add(frame.Node);
			}
		}
		
		context.ExpectsOperand = false;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ParseComma(ref ParserContext context)
	{
		context.FinalizeScope();
		
		CallstackFrame frame = context.Callstack.Pop();
		context.Callstack.Push(frame with { NumParameters = frame.NumParameters + 1 });
		context.ExpectsOperand = true;
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ParseIdentifier(ref ParserContext context)
	{
		context.Output.Add(new(ENodeType.Variable, context.CurrentToken.Value));
		context.ExpectsOperand = false;
	}

	private static readonly Dictionary<string, Operator> _operatorMap = new()
	{
		["||"] = new(1, false),
		
		["&&"] = new(2, false),
		
		["=="] = new(3, false),
		["!="] = new(3, false),
		
		[">"] = new(4, false),
		["<"] = new(4, false),
		[">="] = new(4, false),
		["<="] = new(4, false),
		
		["+"] = new(5, false),
		["-"] = new(5, false),
		[".."] = new(5, false),
		
		["*"] = new(6, false),
		["/"] = new(6, false),
		["%"] = new(6, false),
		
		["^"] = new(7, true),
		
		["u+"] = new(8, false),
		["u-"] = new(8, false),
		["!"] = new(8, false),
	};

}


