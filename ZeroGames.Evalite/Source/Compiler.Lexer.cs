// Copyright Zero Games. All Rights Reserved.

using System.Runtime.CompilerServices;
using System.Text;

namespace ZeroGames.Evalite;

public partial class Compiler
{

	private enum ETokenType : uint8
	{
		None,
		Integer,
		Number,
		Operator,
		LeftParen,
		RightParen,
		Comma,
		Identifier,
	}

	private readonly record struct Token(ETokenType Type, string Value);

	private readonly struct LexerContext
	{
		public required Func<char> Peek { get; init; }
		public required Func<char> Next { get; init; }
		public required Func<char, char> Expect { get; init; }
		public required Func<Exception> Exception { get; init; }
	}

	private IEnumerable<Token> Tokenize(string expression)
	{
		string trimmed = expression.Replace(" ", "");
		int32 i = 0;

		LexerContext context = new()
		{
			Peek = () => i < trimmed.Length ? trimmed[i] : '\0',
			Next = () => i < trimmed.Length ? trimmed[i++] : '\0',
			Expect = expected => trimmed[i++] == expected ? expected : throw new ArgumentException($"Expression '{expression}' is illegal.", nameof(expression)),
			Exception = () => new ArgumentException($"Expression '{expression}' is illegal.", nameof(expression)),
		};

		while (i < trimmed.Length)
		{
			char c = context.Peek();
			if (c is '\0')
			{
				break;
			}
			else if (c is '(')
			{
				context.Next();
				yield return new(ETokenType.LeftParen, string.Empty);
			}
			else if (c is ')')
			{
				context.Next();
				yield return new(ETokenType.RightParen, string.Empty);
			}
			else if (c is ',')
			{
				context.Next();
				yield return new(ETokenType.Comma, string.Empty);
			}
			else if (c is '+' or '-' or '*' or '/' or '^' or '%')
			{
				yield return ReadOperatorToken(context);
			}
			else if (char.IsDigit(c))
			{
				yield return ReadNumberToken(context);
			}
			else if (char.IsLetter(c) || c is '_')
			{
				yield return ReadIdentifierToken(context);
			}
			else
			{
				throw context.Exception();
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Token ReadOperatorToken(in LexerContext context) => new(ETokenType.Operator, context.Next().ToString());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Token ReadNumberToken(in LexerContext context)
	{
		const int32 STATE_INTEGER = 1;
		const int32 STATE_POINT = 2;
		const int32 STATE_NUMBER = 3;
		const int32 STATE_EXPONENT = 4;
		const int32 STATE_SCIENCE_NUMBER = 5;
		
		StringBuilder sb = new();
		sb.Append(context.Next());
		int32 state = STATE_INTEGER;
		char c;
		while ((c = context.Peek()) != '\0')
		{
			if (c is '.')
			{
				if (state != STATE_INTEGER)
				{
					throw context.Exception();
				}
				
				sb.Append(context.Next());
				state = STATE_POINT;
			}
			else if (c is 'e' or 'E')
			{
				if (state != STATE_INTEGER && state != STATE_NUMBER)
				{
					throw context.Exception();
				}
				
				sb.Append(context.Next());
				state = STATE_EXPONENT;

				if ((c = context.Peek()) is '+' or '-')
				{
					sb.Append(context.Next());
				}
			}
			else if (char.IsDigit(c))
			{
				if (state == STATE_POINT)
				{
					state = STATE_NUMBER;
				}
				else if (state == STATE_EXPONENT)
				{
					state = STATE_SCIENCE_NUMBER;
				}
				
				sb.Append(context.Next());
			}
			else
			{
				break;
			}
		}

		ETokenType type = state switch
		{
			STATE_INTEGER => ETokenType.Integer,
			STATE_NUMBER or STATE_SCIENCE_NUMBER => ETokenType.Number,
			_ => throw context.Exception()
		};

		string value = sb.ToString();
		if (type == ETokenType.Integer && !int64.TryParse(value, out _) || type == ETokenType.Number && !double.TryParse(value, out _))
		{
			throw context.Exception();
		}

		return new(type, sb.ToString());
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Token ReadIdentifierToken(in LexerContext context)
	{
		StringBuilder sb = new();
		sb.Append(context.Next());
		char c;
		while ((c = context.Peek()) != '\0')
		{
			if (!char.IsLetterOrDigit(c) && c is not '_')
			{
				break;
			}

			sb.Append(context.Next());
		}
		
		return new(ETokenType.Identifier, sb.ToString());
	}
	
}


