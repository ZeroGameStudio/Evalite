// Copyright Zero Games. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ZeroGames.Evalite;

public abstract class ReflectionContextBase : IContext
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
	
	public bool AllowsPrivateAccess { get; init; }
	public bool IsCaseInsensitive { get; init; }

	protected ReflectionContextBase()
	{
		_targetType = GetType();
		_targetInstance = this;
	}

	protected ReflectionContextBase(Type targetType, object? targetInstance)
	{
		_targetType = targetType;
		_targetInstance = targetInstance;
	}
	
	private readonly struct TypeMetadata
	{
		public TypeMetadata(Type type, BindingFlags bindingFlags)
		{
			bool caseInsensitive = bindingFlags.HasFlag(BindingFlags.IgnoreCase);
			
			MethodCache = type.GetMethods(bindingFlags)
				.Where(m => m.ReturnType != typeof(void))
				.DistinctBy(m => caseInsensitive ? m.Name.ToLower() : m.Name)
				.ToDictionary(m => caseInsensitive ? m.Name.ToLower() : m.Name);
			
			PropertyCache = type.GetProperties(bindingFlags)
				.Where(p => p.CanRead)
				.ToDictionary(p => caseInsensitive ? p.Name.ToLower() : p.Name);
		}
		
		public IReadOnlyDictionary<string, MethodInfo> MethodCache { get; }
		public IReadOnlyDictionary<string, PropertyInfo> PropertyCache { get; }
	}
	
	private bool TryGetMethod(string name, [NotNullWhen(true)] out MethodInfo? method)
	{
		return TypeMetadataCache.MethodCache.TryGetValue(IsCaseInsensitive ? name.ToLower() : name, out method);
	}

	private bool TryGetProperty(string name, [NotNullWhen(true)] out PropertyInfo? property)
	{
		return TypeMetadataCache.PropertyCache.TryGetValue(IsCaseInsensitive ? name.ToLower() : name, out property);
	}

	private TypeMetadata TypeMetadataCache
	{
		get
		{
			if (_typeMetadataCache is null)
			{
				var bindingFlags = BindingFlags.Public | BindingFlags.Static;
				if (_targetInstance is not null)
				{
					bindingFlags |= BindingFlags.Instance;
				}
				if (AllowsPrivateAccess)
				{
					bindingFlags |= BindingFlags.NonPublic;
				}
				if (IsCaseInsensitive)
				{
					bindingFlags |= BindingFlags.IgnoreCase;
				}

				_typeMetadataCache = new(_targetType, bindingFlags);
			}

			return _typeMetadataCache.Value;
		}
	}

	private readonly Type _targetType;
	private readonly object? _targetInstance;
	private TypeMetadata? _typeMetadataCache;

}