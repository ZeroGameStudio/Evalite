﻿// Copyright Zero Games. All Rights Reserved.

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
		Boolean,
		String,
		Operator,
		LeftParen,
		RightParen,
		Comma,
		Identifier,
	}

	private readonly record struct Token(ETokenType Type, string Value);

	private readonly struct LexerContext
	{
		public void Eat(int32 count = 1)
		{
			for (int32 i = 0; i < count; ++i)
			{
				Next();
			}
		}
		
		public required Func<char> Peek { get; init; }
		public required Func<int32, char> PeekWithOffset { get; init; }
		public required Func<int32, string> PeekWord { get; init; }
		public required Func<char> Next { get; init; }
		public required Func<Exception> Exception { get; init; }
		public required Action<ECompilerFeatures> RequireFeatures { get; init; }
	}
	
	private string Trim(string expression)
	{
		if (!AvailableFeatures.HasFlag(ECompilerFeatures.String))
		{
			return expression.Replace(" ", string.Empty);
		}
		
		StringBuilder sb = new();
		char quote = '\0';
		for (int32 i = 0; i < expression.Length; ++i)
		{
			char c = expression[i];
			if (c is '"' or '\'')
			{
				if (quote is '\0')
				{
					quote = c;
				}
				else if (c == quote && (expression[i - 1] != '\\' || expression[i - 2] == '\\' /* NOTE: i > 1 is always true here. */))
				{
					quote = '\0';
				}
			}
			
			if (quote is '\0' && c is ' ')
			{
				continue;
			}
			
			sb.Append(c);
		}
		
		if (quote is not '\0')
		{
			throw new ArgumentException($"Unterminated string in expression: {expression}", nameof(expression));
		}
		
		return sb.ToString();
	}

	private IEnumerable<Token> Tokenize(string expression)
	{
		string trimmed = Trim(expression);
		int32 i = 0;
		Func<Exception> exception = () => new ArgumentException($"Expression '{expression}' is illegal.", nameof(expression));
		LexerContext context = new()
		{
			Peek = () => i < trimmed.Length ? trimmed[i] : '\0',
			PeekWithOffset = offset => i + offset < trimmed.Length ? trimmed[i + offset] : '\0',
			PeekWord = maxNumLetters =>
			{
				StringBuilder sb = new();
				char c;
				int32 j = 0;
				int32 endIndex = Math.Min(i + maxNumLetters, trimmed.Length);
				while (i + j < endIndex && char.IsLetter(c = trimmed[i + j]))
				{
					sb.Append(c);
					++j;
				}
				return sb.ToString();
			},
			Next = () => i < trimmed.Length ? trimmed[i++] : '\0',
			Exception = exception,
			RequireFeatures = features =>
			{
				if (!AvailableFeatures.HasFlag(features))
				{
					throw new NotSupportedException($"Feature {features} is unavailable for this compiler instance.");
				}
			},
		};
		
		while (i < trimmed.Length)
		{
			char c = context.Peek();
			if (c is '\0')
			{
				break;
			}
			else if (char.IsDigit(c))
			{
				yield return ReadNumberToken(context);
			}
			else if (char.IsLetter(c) || c is '_')
			{
				string word = context.PeekWord(5);
				if (word is "true" or "false")
				{
					context.RequireFeatures(ECompilerFeatures.Boolean);
					
					context.Eat(word.Length);
					yield return new(ETokenType.Boolean, word);
				}
				else
				{
					yield return ReadIdentifierToken(context);
				}
			}
			else if (c is '"' or '\'')
			{
				context.RequireFeatures(ECompilerFeatures.String);
				
				yield return ReadStringToken(context);
			}
			else if (c is '+' or '-' or '*' or '/' or '^' or '%' or '=' or '!' or '>' or '<' or '&' or '|' or '.')
			{
				yield return ReadOperatorToken(context);
			}
			else if (c is '(')
			{
				context.Eat();
				yield return new(ETokenType.LeftParen, string.Empty);
			}
			else if (c is ')')
			{
				context.Eat();
				yield return new(ETokenType.RightParen, string.Empty);
			}
			else if (c is ',')
			{
				context.Eat();
				yield return new(ETokenType.Comma, string.Empty);
			}
			else
			{
				throw context.Exception();
			}
		}
	}

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
				// String concat operator
				if (context.PeekWithOffset(1) is '.')
				{
					break;
				}
				
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

				if (context.Peek() is '+' or '-')
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

		bool integerAvailable = AvailableFeatures.HasFlag(ECompilerFeatures.Integer);
		ETokenType type = state switch
		{
			STATE_INTEGER when integerAvailable => ETokenType.Integer,
			STATE_INTEGER or STATE_NUMBER or STATE_SCIENCE_NUMBER => ETokenType.Number,
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
	private Token ReadStringToken(in LexerContext context)
	{
		char quote = context.Next();
		StringBuilder sb = new();
		bool escaped = false;
		while (true)
		{
			char c = context.Next();
			if (c is '\0')
			{
				throw context.Exception();
			}
			
			if (escaped)
			{
				char escapedChar = c switch
				{
					'n' => '\n',
					'r' => '\r',
					't' => '\t',
					'\\' => '\\',
					_ when c == quote => c,
					_ => throw context.Exception()
				};
				sb.Append(escapedChar);
				escaped = false;
			}
			else if (c == '\\')
			{
				escaped = true;
			}
			else if (c == quote)
			{
				break;
			}
			else
			{
				sb.Append(c);
			}
		}
		
		return new(ETokenType.String, sb.ToString());
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Token ReadOperatorToken(in LexerContext context)
	{
		char first = context.Next();
		char second = context.Peek();
		
		Token token = first switch
		{
			'=' when second == '=' => new(ETokenType.Operator, "=" + context.Next()),
			'!' when second == '=' => new(ETokenType.Operator, "!" + context.Next()),
			'>' when second == '=' => new(ETokenType.Operator, ">" + context.Next()),
			'<' when second == '=' => new(ETokenType.Operator, "<" + context.Next()),
			'&' when second == '&' => new(ETokenType.Operator, "&" + context.Next()),
			'|' when second == '|' => new(ETokenType.Operator, "|" + context.Next()),
			'.' when second == '.' => new(ETokenType.Operator, "." + context.Next()),
			'+' or '-' or '*' or '/' or '^' or '%' or '>' or '<' or '!' => new(ETokenType.Operator, first.ToString()),
			_ => throw context.Exception()
		};

		string op = token.Value;
		if (op is "&&" or "||" or "!" or "==" or "!=" or ">" or "<" or ">=" or "<=")
		{
			context.RequireFeatures(ECompilerFeatures.Boolean);
		}
		else if (op is "..")
		{
			context.RequireFeatures(ECompilerFeatures.String);
		}

		return token;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Token ReadIdentifierToken(in LexerContext context)
	{
		StringBuilder sb = new();
		sb.Append(context.Next());
		
		char c;
		bool allowSeparator = true;
		while ((c = context.Peek()) != '\0')
		{
			if (char.IsLetterOrDigit(c) || c is '_')
			{
				sb.Append(context.Next());
				allowSeparator = true;
			}
			else if (c is '.')
			{
				context.RequireFeatures(ECompilerFeatures.Member);
				
				if (!allowSeparator)
				{
					throw context.Exception();
				}
				
				sb.Append(context.Next());
				allowSeparator = false;
			}
			else
			{
				break;
			}
		}
		
		if (!allowSeparator)
		{
			throw context.Exception();
		}
		
		return new(ETokenType.Identifier, sb.ToString());
	}
	
}


