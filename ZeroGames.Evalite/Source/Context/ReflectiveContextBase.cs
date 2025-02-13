// Copyright Zero Games. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ZeroGames.Evalite;

public abstract class ReflectiveContextBase : IContext
{
	
	public bool TryCall(string name, [NotNullWhen(true)] out object? result, params object[] parameters)
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
				result = method.Invoke(_targetInstance, parameters.Select(p => Convert.ChangeType(p, parameterInfos[i++].ParameterType)).ToArray())!;
			}
			else if (parameterInfos[^1].ParameterType is { IsArray: true } lastParameterType)
			{
				Type elementType = lastParameterType.GetElementType()!;
				int32 numFixedParameters = parameterInfos.Length - 1;
				Array paramArray = Array.CreateInstance(elementType, parameters.Length - numFixedParameters);
				parameters.Skip(numFixedParameters).Select(p => Convert.ChangeType(p, elementType)).ToArray().CopyTo(paramArray, 0);
				int32 i = 0;
				result = method.Invoke(_targetInstance, parameters.Take(numFixedParameters).Select(p => Convert.ChangeType(p, parameterInfos[i++].ParameterType)).Append(paramArray).ToArray())!;
			}
			else
			{
				throw new ArgumentOutOfRangeException();
			}
			
			return true;
		}

		result = null;
		return false;
	}

	public bool TryRead(string name, [NotNullWhen(true)] out object? result)
	{
		if (TryGetProperty(name, out var property))
		{
			result = property.GetValue(_targetInstance)!;
			return true;
		}

		result = null;
		return false;
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

	protected ReflectiveContextBase(bool allowPrivateAccess)
	{
		Type targetType = GetType();
		object targetInstance = this;
		
		_targetInstance = targetInstance;
		
		var bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
		if (allowPrivateAccess)
		{
			bindingFlags |= BindingFlags.NonPublic;
		}
		
		_typeMetadata = new(targetType, bindingFlags);
	}

	protected ReflectiveContextBase(Type targetType, object? targetInstance, bool allowPrivateAccess)
	{
		_targetInstance = targetInstance;
		
		var bindingFlags = BindingFlags.Public | BindingFlags.Static;
		if (targetInstance is not null)
		{
			bindingFlags |= BindingFlags.Instance;
		}
		if (allowPrivateAccess)
		{
			bindingFlags |= BindingFlags.NonPublic;
		}

		_typeMetadata = new(targetType, bindingFlags);
	}
	
	private bool TryGetMethod(string name, [NotNullWhen(true)] out MethodInfo? method)
	{
		return _typeMetadata.MethodCache.TryGetValue(name, out method);
	}

	private bool TryGetProperty(string name, [NotNullWhen(true)] out PropertyInfo? property)
	{
		return _typeMetadata.PropertyCache.TryGetValue(name, out property);
	}

	private readonly struct TypeMetadata
	{
		public TypeMetadata(Type type, BindingFlags bindingFlags)
		{
			MethodCache = type.GetMethods(bindingFlags).Where(m => m.ReturnType != typeof(void)).DistinctBy(m => m.Name).ToDictionary(m => m.Name);
			PropertyCache = type.GetProperties(bindingFlags).Where(p => p.CanRead).ToDictionary(p => p.Name);
		}
		
		public IReadOnlyDictionary<string, MethodInfo> MethodCache { get; }
		public IReadOnlyDictionary<string, PropertyInfo> PropertyCache { get; }
	}
	
	private readonly object? _targetInstance;
	private readonly TypeMetadata _typeMetadata;

}