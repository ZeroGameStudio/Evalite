// Copyright Zero Games. All Rights Reserved.

using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ZeroGames.Evalite;

public partial class Compiler
{
	
	private readonly record struct Parameter(Type Type, string Name);

	private struct ExpressionContext()
	{
		public Node CurrentNode { get; set; }
		public Stack<Expression> Stack { get; } = [];
		public required IMetatable? Metatable { get; init; }
		public required Dictionary<string, Expression> ParameterMap { get; init; }
	}
	
	private Delegate InternalCompile(string expression, IMetatable? metatable, Type returnType, params Parameter[] parameters)
	{
		string invalidParameterNames = string.Join(", ", parameters.Where(p => !_identifierRegex.IsMatch(p.Name)));
		if (!string.IsNullOrWhiteSpace(invalidParameterNames))
		{
			throw new ArgumentException($"Invalid parameter names: {invalidParameterNames}", nameof(parameters));
		}

		string duplicatedParameterNames = string.Join(", ", parameters.GroupBy(p => p.Name).Where(g => g.Count() > 1).Select(g => g.Key));
		if (!string.IsNullOrWhiteSpace(duplicatedParameterNames))
		{
			throw new ArgumentException($"Duplicated parameter names: {duplicatedParameterNames}", nameof(parameters));
		}

		IEnumerable<Node> rpn = Parse(Tokenize(expression));
		Expression body = BuildExpressionTree(rpn, returnType, parameters, metatable, out var parameterExpressions);
		Type delegateType = Expression.GetFuncType(parameters.Select(p => p.Type).Append(returnType).ToArray());
		return Expression.Lambda(delegateType, body, parameterExpressions).Compile();
	}

	private Delegate InternalCompileDelegate<TDelegate>(string expression, IMetatable? context) where TDelegate : Delegate
	{
		Type delegateType = typeof(TDelegate);
		MethodInfo signature = delegateType.GetMethod(nameof(Action.Invoke))!;
		Type returnType = signature.ReturnType;
		if (returnType == typeof(void))
		{
			throw new ArgumentException($"Invalid delegate type: {typeof(TDelegate).Name}");
		}

		Parameter[] parameters = signature.GetParameters().Select(p => new Parameter(p.ParameterType, p.Name ?? throw new ArgumentException("Invalid parameter name."))).ToArray();
		
		IEnumerable<Node> rpn = Parse(Tokenize(expression));
		Expression body = BuildExpressionTree(rpn, returnType, parameters, context, out var parameterExpressions);
		return Expression.Lambda(delegateType, body, parameterExpressions).Compile();
	}
	
	private Expression BuildExpressionTree(IEnumerable<Node> rpn, Type type, Parameter[] parameters, IMetatable? context, out ParameterExpression[] parameterExpressions)
	{
		parameterExpressions = parameters.Select(p => Expression.Parameter(p.Type, p.Name)).ToArray();
		Dictionary<string, Expression> parameterMap = parameterExpressions.ToDictionary(p => p.Name!, p => (Expression)p);

		ExpressionContext expressionContext = new()
		{
			Metatable = context,
			ParameterMap = parameterMap,
		};
		
		foreach (var node in rpn)
		{
			expressionContext.CurrentNode = node;
			switch (node.Type)
			{
				case ENodeType.Integer:
				{
					ProcessIntegerNode(expressionContext);
					break;
				}
				case ENodeType.Number:
				{
					ProcessNumberNode(expressionContext);
					break;
				}
				case ENodeType.Boolean:
				{
					ProcessBooleanNode(expressionContext);
					break;
				}
				case ENodeType.String:
				{
					ProcessStringNode(expressionContext);
					break;
				}
				case ENodeType.Operator:
				{
					ProcessOperatorNode(expressionContext);
					break;
				}
				case ENodeType.Function:
				{
					ProcessFunctionNode(expressionContext);
					break;
				}
				case ENodeType.Variable:
				{
					ProcessVariableNode(expressionContext);
					break;
				}
				default:
				{
					throw new InvalidOperationException("Failed to build expression tree.");
				}
			}
		}

		if (expressionContext.Stack.Count != 1)
		{
			throw new InvalidOperationException("Failed to build expression tree.");
		}

		return expressionContext.Stack.Pop().As(type);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessIntegerNode(in ExpressionContext context)
		=> context.Stack.Push(Expression.Constant(int64.Parse(context.CurrentNode.Value)));
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessNumberNode(in ExpressionContext context)
		=> context.Stack.Push(Expression.Constant(double.Parse(context.CurrentNode.Value)));
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessBooleanNode(in ExpressionContext context)
		=> context.Stack.Push(Expression.Constant(bool.Parse(context.CurrentNode.Value)));
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessStringNode(in ExpressionContext context)
		=> context.Stack.Push(Expression.Constant(context.CurrentNode.Value));
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessOperatorNode(in ExpressionContext context)
	{
		string op = context.CurrentNode.Value;
		if (op is "u+" or "u-" or "!")
		{
			Expression operand = context.Stack.Pop();
			Expression operation = op switch
			{
				"u-" => Expression.Negate(operand),
				"!" => Expression.Not(operand),
				_ => operand
			};
			context.Stack.Push(operation);
		}
		else
		{
			Expression right = context.Stack.Pop();
			Expression left = context.Stack.Pop();
			bool useIntegerOperators = left.Type.IsInteger() && right.Type.IsInteger() && AvailableFeatures.HasFlag(ECompilerFeatures.Integer);
			bool useDecimalOperators = AvailableFeatures.HasFlag(ECompilerFeatures.Decimal);
			Expression operation = op switch
			{
				// Arithmetic operators
				"+" when useIntegerOperators => Expression.Add(left.As<int64>(), right.As<int64>()),
				"-" when useIntegerOperators => Expression.Subtract(left.As<int64>(), right.As<int64>()),
				"*" when useIntegerOperators => Expression.Multiply(left.As<int64>(), right.As<int64>()),
				"/" when useIntegerOperators => Expression.Divide(left.As<int64>(), right.As<int64>()),
				"%" when useIntegerOperators => Expression.Modulo(left.As<int64>(), right.As<int64>()),
				
				"+" when useDecimalOperators => Expression.Add(left.As<decimal>(), right.As<decimal>()),
				"-" when useDecimalOperators => Expression.Subtract(left.As<decimal>(), right.As<decimal>()),
				"*" when useDecimalOperators => Expression.Multiply(left.As<decimal>(), right.As<decimal>()),
				"/" when useDecimalOperators => Expression.Divide(left.As<decimal>(), right.As<decimal>()),
				"%" when useDecimalOperators => Expression.Modulo(left.As<decimal>(), right.As<decimal>()),
				
				"+" => Expression.Add(left.As<double>(), right.As<double>()),
				"-" => Expression.Subtract(left.As<double>(), right.As<double>()),
				"*" => Expression.Multiply(left.As<double>(), right.As<double>()),
				"/" => Expression.Divide(left.As<double>(), right.As<double>()),
				"%" => Expression.Modulo(left.As<double>(), right.As<double>()),
				
				"^" => Expression.Power(left.As<double>(), right.As<double>()),
				
				// Relation operators
				"==" => Expression.Equal(left, right),
				"!=" => Expression.NotEqual(left, right),
				">" => Expression.GreaterThan(left, right),
				"<" => Expression.LessThan(left, right),
				">=" => Expression.GreaterThanOrEqual(left, right),
				"<=" => Expression.LessThanOrEqual(left, right),
				
				// Boolean operators
				"&&" => Expression.AndAlso(left, right),
				"||" => Expression.OrElse(left, right),
				
				// String operators
				// Expression.Add doesn't work...
				".." => Expression.Call(_stringConcat, left.CallToString(), right.CallToString()),
				
				_ => throw new NotSupportedException($"Operator {op} is not supported.")
			};
			
			context.Stack.Push(operation);
		}
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessFunctionNode(in ExpressionContext context)
	{
		if (context.Metatable is null)
		{
			throw new ArgumentException("Function call requires a context", nameof(context));
		}

		string[] pair = context.CurrentNode.Value.Split(':');
		int32 numParameters = int32.Parse(pair[1]);
		Expression[] parameterExpressions = new Expression[numParameters];
		// Fill parameters from right to left.
		for (int32 i = numParameters - 1; i >= 0; --i)
		{
			parameterExpressions[i] = context.Stack.Pop().As<object>();
		}

		string[] pathNodes = pair[0].Split('.');
		string functionName = pathNodes[^1];
		Expression expression = pathNodes.Length > 1 ? ProcessPath(pathNodes.SkipLast(1), context).As<IMetatable>() : Expression.Constant(context.Metatable);
		expression = Expression.Call(expression, _contextCall, Expression.Constant(functionName), Expression.NewArrayInit(typeof(object), parameterExpressions));
		if (pathNodes.Length == 1)
		{
			expression = expression.As(context.Metatable.GetFunctionReturnType(functionName));
		}
		
		context.Stack.Push(expression);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessVariableNode(in ExpressionContext context)
	{
		Expression expression = ProcessPath(AvailableFeatures.HasFlag(ECompilerFeatures.PathIdentifier) ? [context.CurrentNode.Value] : context.CurrentNode.Value.Split('.'), context);
		context.Stack.Push(expression);
	}

	private Expression ProcessPath(IEnumerable<string> pathNodes, in ExpressionContext context)
	{
		Expression result = null!;

		bool first = true;
		foreach (var node in pathNodes)
		{
			if (first)
			{
				first = false;
				
				if (context.ParameterMap.TryGetValue(node, out var parameterExpression))
				{
					result = parameterExpression;
				}
				else if (context.Metatable is not null)
				{
					result = Expression.Constant(context.Metatable);
					result = Expression.Call(result, _contextRead, Expression.Constant(node)).As(context.Metatable.GetPropertyType(node));
				}
				else
				{
					throw new InvalidOperationException($"Unknown identifier '{node}'.");
				}
			}
			else
			{
				result = Expression.Call(result.As<IMetatable>(), _contextRead, Expression.Constant(node));
			}
		}

		return result;
	}
	
	[GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$")]
	private static partial Regex _identifierRegex { get; }

	private static readonly MethodInfo _contextCall = typeof(IMetatable).GetMethod(nameof(IMetatable.Call))!;
	private static readonly MethodInfo _contextRead = typeof(IMetatable).GetMethod(nameof(IMetatable.Read))!;
	private static readonly MethodInfo _stringConcat = typeof(string).GetMethod(nameof(string.Concat), [ typeof(string), typeof(string) ])!;
	
}


