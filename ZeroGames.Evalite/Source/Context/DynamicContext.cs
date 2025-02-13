// Copyright Zero Games. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;

namespace ZeroGames.Evalite;

public sealed class DynamicContext : IContext
{
	
	public bool TryCall(string name, [NotNullWhen(true)] out object? result, params object[] parameters)
	{
		throw new NotImplementedException();
	}

	public bool TryRead(string name, [NotNullWhen(true)] out object? result)
	{
		throw new NotImplementedException();
	}

	public Type GetFunctionReturnType(string name)
	{
		throw new NotImplementedException();
	}

	public Type GetPropertyType(string name)
	{
		throw new NotImplementedException();
	}
	
	
	
}


