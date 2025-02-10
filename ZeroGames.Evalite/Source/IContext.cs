// Copyright Zero Games. All Rights Reserved.

using System.Linq.Expressions;

namespace ZeroGames.Evalite;

public interface IContext
{
	Expression CallMethod(string name, params Expression[] parameters);
	Expression ReadProperty(string name);
	
	bool HasProperty(string name);
}


