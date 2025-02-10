// Copyright Zero Games. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace ZeroGames.Evalite;

public abstract class ContextBase : IContext
{
	
	public Expression CallMethod(string name, params Expression[] parameters)
	{
		if (TryGetMethod(name, out var method))
		{
			ParameterInfo[] parameterInfos = method.GetParameters();
			if (parameterInfos.Length > parameters.Length)
			{
				throw new ArgumentOutOfRangeException();
			}

			if (parameterInfos.Length == parameters.Length)
			{
				int32 i = 0;
				return Expression.Call(!method.IsStatic ? Expression.Constant(this) : null, method, parameters
					.Select(p =>
					{
						Type parameterType = parameterInfos[i++].ParameterType;
						return p.Type == parameterType ? p : Expression.Convert(p, parameterType);
					})
					.ToArray());
			}
			else if (parameterInfos[^1].ParameterType is { IsArray: true } lastParameterType)
			{
				Type elementType = lastParameterType.GetElementType()!;
				int32 numFixedParameters = parameterInfos.Length - 1;
			
				Expression arrayExpression = Expression.NewArrayInit(elementType, parameters[numFixedParameters..]
					.Select(p => p.Type == elementType ? p : Expression.Convert(p, elementType)));

				int32 i = 0;
				return Expression.Call(!method.IsStatic ? Expression.Constant(this) : null, method, parameters
					.Take(numFixedParameters)
					.Select(p =>
					{
						Type parameterType = parameterInfos[i++].ParameterType;
						return p.Type == parameterType ? p : Expression.Convert(p, parameterType);
					})
					.Append(arrayExpression)
					.ToArray());
			}
			else
			{
				throw new ArgumentOutOfRangeException();
			}
		}

		throw new KeyNotFoundException();
	}

	public Expression ReadProperty(string name)
	{
		if (TryGetProperty(name, out var property))
		{
			return Expression.Property(!property.GetMethod!.IsStatic ? Expression.Constant(this) : null, property);
		}

		throw new KeyNotFoundException();
	}

	public bool HasProperty(string name)
	{
		return TryGetProperty(name, out _);
	}

	private bool TryGetMethod(string name, [NotNullWhen(true)] out MethodInfo? method)
	{
		return GetOrCacheTypeMetadata(GetType()).MethodCache.TryGetValue(name, out method);
	}

	private bool TryGetProperty(string name, [NotNullWhen(true)] out PropertyInfo? property)
	{
		return GetOrCacheTypeMetadata(GetType()).PropertyCache.TryGetValue(name, out property);
	}

	private TypeMetadataCache GetOrCacheTypeMetadata(Type type)
	{
		if (_typeMetadataCache.TryGetValue(type, out var existing))
		{
			return existing;
		}

		BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
		return new()
		{
			MethodCache = type.GetMethods(bindingFlags).ToDictionary(m => m.Name),
			PropertyCache = type.GetProperties(bindingFlags).Where(p => p.CanRead).ToDictionary(p => p.Name),
		};
	}

	private readonly struct TypeMetadataCache
	{
		public required IReadOnlyDictionary<string, MethodInfo> MethodCache { get; init; }
		public required IReadOnlyDictionary<string, PropertyInfo> PropertyCache { get; init; }
	}
	
	private static readonly Dictionary<Type, TypeMetadataCache> _typeMetadataCache = [];

}