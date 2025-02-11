// Copyright Zero Games. All Rights Reserved.

namespace ZeroGames.Evalite;

public interface IContext
{
	object Call(string name, params object[] parameters);
	object Read(string name);
	
	Type GetFunctionReturnType(string name);
	Type GetPropertyType(string name);
}


