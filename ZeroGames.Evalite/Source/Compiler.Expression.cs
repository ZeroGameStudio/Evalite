// Copyright Zero Games. All Rights Reserved.

using System.Linq.Expressions;
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
		public required IContext? Context { get; init; }
		public required Dictionary<string, Expression> ParameterMap { get; init; }
	}
	
	private static Expression ToDouble(Expression expr) => expr.Type == typeof(double) ? expr : Expression.Convert(expr, typeof(double));
	
	private object InternalCompile(string expression, IContext? context, Type returnType, params Parameter[] parameters)
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

		IEnumerable<Node> rpn = ConvertToRpn(Tokenize(expression));
		Expression body = BuildExpressionTree(rpn, returnType, parameters, context, out var parameterExpressions);
		Type delegateType = Expression.GetFuncType(parameters.Select(p => p.Type).Append(returnType).ToArray());
		return Expression.Lambda(delegateType, body, parameterExpressions).Compile();
	}
	
	private Expression BuildExpressionTree(IEnumerable<Node> rpn, Type type, Parameter[] parameters, IContext? context, out ParameterExpression[] parameterExpressions)
	{
		parameterExpressions = parameters.Select(p => Expression.Parameter(p.Type, p.Name)).ToArray();
		Dictionary<string, Expression> parameterMap = parameterExpressions.ToDictionary(p => p.Name!, p => (Expression)p);

		ExpressionContext expressionContext = new()
		{
			Context = context,
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

		Expression expr = expressionContext.Stack.Pop();
		return expr.Type == type ? expr : Expression.Convert(expr, type);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessIntegerNode(in ExpressionContext context)
		=> context.Stack.Push(Expression.Constant(int64.Parse(context.CurrentNode.Value)));
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessNumberNode(in ExpressionContext context)
		=> context.Stack.Push(Expression.Constant(double.Parse(context.CurrentNode.Value)));
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessOperatorNode(in ExpressionContext context)
	{
		string op = context.CurrentNode.Value;
		if (op is "u+" or "u-")
		{
			Expression operand = context.Stack.Pop();
			Expression operation = op == "u-" ? Expression.Negate(operand) : operand;
			context.Stack.Push(operation);
		}
		else
		{
			Expression right = context.Stack.Pop();
			Expression left = context.Stack.Pop();
			bool integer = left.Type == typeof(int64) && right.Type == typeof(int64);
			Expression operation = op switch
			{
				"+" => integer ? Expression.Add(left, right) : Expression.Add(ToDouble(left), ToDouble(right)),
				"-" => integer ? Expression.Subtract(left, right) : Expression.Subtract(ToDouble(left), ToDouble(right)),
				"*" => integer ? Expression.Multiply(left, right) : Expression.Multiply(ToDouble(left), ToDouble(right)),
				"/" => integer ? Expression.Divide(left, right) : Expression.Divide(ToDouble(left), ToDouble(right)),
				"%" => integer ? Expression.Modulo(left, right) : Expression.Modulo(ToDouble(left), ToDouble(right)),
				"^" => Expression.Power(ToDouble(left), ToDouble(right)),
				_ => throw new NotSupportedException($"Operator {op} is not supported.")
			};

			context.Stack.Push(operation);
		}
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessFunctionNode(in ExpressionContext context)
	{
		string[] pair = context.CurrentNode.Value.Split(':');
		string functionName = pair[0];
		int32 numParameters = int32.Parse(pair[1]);
		Expression[] functionParameterExpressions = new Expression[numParameters];
		for (int32 i = 0; i < numParameters; ++i)
		{
			functionParameterExpressions[i] = context.Stack.Pop();
		}
					
		if (context.Context is null)
		{
			throw new ArgumentException("Function call requires a context", nameof(context));
		}
				
		context.Stack.Push(context.Context.CallMethod(functionName, functionParameterExpressions));
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ProcessVariableNode(in ExpressionContext context)
	{
		string variableName = context.CurrentNode.Value;
		if (context.ParameterMap.TryGetValue(variableName, out var parameterExpression))
		{
			context.Stack.Push(parameterExpression);
		}
		else if (context.Context is not null)
		{
			context.Stack.Push(context.Context.ReadProperty(variableName));
		}
		else
		{
			throw new InvalidOperationException($"Unknown variable '{variableName}'.");
		}
	}
	
	[GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$")]
	private static partial Regex _identifierRegex { get; }
	
}


