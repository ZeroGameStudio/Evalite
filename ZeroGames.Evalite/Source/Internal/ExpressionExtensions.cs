// Copyright Zero Games. All Rights Reserved.

using System.Linq.Expressions;
using System.Reflection;

namespace ZeroGames.Evalite;

internal static class ExpressionExtensions
{

	public static Expression As(this Expression @this, Type targetType)
	{
		Type sourceType = @this.Type;
		if (sourceType == targetType)
		{
			return @this;
		}

		if (sourceType.IsAssignableTo(targetType) || targetType.IsAssignableTo(typeof(IContext)))
		{
			return Expression.Convert(@this, targetType);
		}

		return Expression.Convert(Expression.Call(_convertChangeType, Expression.Convert(@this, typeof(object)), Expression.Constant(targetType)), targetType);
	}
	
	public static Expression As<T>(this Expression @this) => As(@this, typeof(T));

	public static Expression CallToString(this Expression @this) => @this.Type == typeof(string) ? @this : Expression.Call(@this, _objectToString);

	private static readonly MethodInfo _convertChangeType = typeof(Convert).GetMethod(nameof(Convert.ChangeType), [typeof(object), typeof(Type)])!;
	private static readonly MethodInfo _objectToString = typeof(object).GetMethod(nameof(ToString))!;
	
}


