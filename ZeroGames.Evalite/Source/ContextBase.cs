// Copyright Zero Games. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ZeroGames.Evalite;

public abstract class ContextBase : IContext
{
	
	public object Call(string name, params object[] parameters)
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
				return method.Invoke(this, parameters)!;
			}
			else if (parameterInfos[^1].ParameterType is { IsArray: true } lastParameterType)
			{
				Type elementType = lastParameterType.GetElementType()!;
				int32 numFixedParameters = parameterInfos.Length - 1;
				Array paramArray = Array.CreateInstance(elementType, parameters.Length - numFixedParameters);
				parameters.Skip(numFixedParameters).Select(p => Convert.ChangeType(p, elementType)).ToArray().CopyTo(paramArray, 0);
				return method.Invoke(this, parameters.Take(numFixedParameters).Append(paramArray).ToArray())!;
			}
			else
			{
				throw new ArgumentOutOfRangeException();
			}
		}

		throw new KeyNotFoundException();
	}

	public object Read(string name)
	{
		if (TryGetProperty(name, out var property))
		{
			return property.GetValue(this)!;
		}

		throw new KeyNotFoundException();
	}

	public Type GetFunctionReturnType(string name)
	{
		if (TryGetMethod(name, out var method))
		{
			return method.ReturnType;
		}

		return typeof(object);
	}

	public Type GetPropertyType(string name)
	{
		if (TryGetProperty(name, out var property))
		{
			return property.PropertyType;
		}

		return typeof(object);
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
		if (_typeMetadataCache.TryGetValue(type, out var metadata))
		{
			return metadata;
		}

		BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
		metadata = new()
		{
			MethodCache = type.GetMethods(bindingFlags).Where(m => m.ReturnType != typeof(void)).ToDictionary(m => m.Name),
			PropertyCache = type.GetProperties(bindingFlags).Where(p => p.CanRead).ToDictionary(p => p.Name),
		};
		_typeMetadataCache[type] = metadata;
		
		return metadata;
	}

	private readonly struct TypeMetadataCache
	{
		public required IReadOnlyDictionary<string, MethodInfo> MethodCache { get; init; }
		public required IReadOnlyDictionary<string, PropertyInfo> PropertyCache { get; init; }
	}
	
	private static readonly Dictionary<Type, TypeMetadataCache> _typeMetadataCache = [];

}