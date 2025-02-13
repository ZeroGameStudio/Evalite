// Copyright Zero Games. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;

namespace ZeroGames.Evalite;

public interface IContext
{
	bool TryCall(string name, [NotNullWhen(true)] out object? result, params object[] parameters);
	bool TryRead(string name, [NotNullWhen(true)] out object? result);
	
	Type GetFunctionReturnType(string name);
	Type GetPropertyType(string name);
	
	object Call(string name, params object[] parameters)
	{
		if (!TryCall(name, out var result, parameters))
		{
			throw new ArgumentException($"Function '{name}' not found.", nameof(name));
		}

		return result;
	}

	object Read(string name)
	{
		if (!TryRead(name, out var result))
		{
			throw new ArgumentException($"Property '{name}' not found.", nameof(name));
		}
		
		return result;
	}
}


