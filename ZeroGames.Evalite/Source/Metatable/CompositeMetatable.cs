// Copyright Zero Games. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;

namespace ZeroGames.Evalite;

public sealed class CompositeMetatable : IMetatable
{
	
	public CompositeMetatable(){}

	public CompositeMetatable(params ReadOnlySpan<IMetatable> inners)
	{
		foreach (var inner in inners)
		{
			AddLast(inner);
		}
	}

	public bool TryCall(string name, [NotNullWhen(true)] out object? result, params object[] parameters)
	{
		foreach (var metatable in _metatableLink)
		{
			if (metatable.TryCall(name, out result, parameters))
			{
				return true;
			}
		}

		result = null;
		return false;
	}

	public bool TryRead(string name, [NotNullWhen(true)] out object? result)
	{
		foreach (var metatable in _metatableLink)
		{
			if (metatable.TryRead(name, out result))
			{
				return true;
			}
		}

		result = null;
		return false;
	}

	public Type GetFunctionReturnType(string name) => typeof(object);
	public Type GetPropertyType(string name) => typeof(object);
	
	public void Add(IMetatable metatable) => AddLast(metatable);

	public void AddFirst(IMetatable metatable)
	{
		Remove(metatable);

		LinkedListNode<IMetatable> node = _metatableLink.AddFirst(metatable);
		_nodeLookup[metatable] = node;
	}

	public void AddLast(IMetatable metatable)
	{
		Remove(metatable);
		
		LinkedListNode<IMetatable> node = _metatableLink.AddLast(metatable);
		_nodeLookup[metatable] = node;
	}

	public void AddBefore(IMetatable targetMetatable, IMetatable metatable)
	{
		if (!_nodeLookup.TryGetValue(targetMetatable, out var targetNode))
		{
			throw new KeyNotFoundException("Target metatable not found.");
		}
		
		Remove(metatable);

		LinkedListNode<IMetatable> node = _metatableLink.AddBefore(targetNode, metatable);
		_nodeLookup[metatable] = node;
	}

	public void AddAfter(IMetatable targetMetatable, IMetatable metatable)
	{
		if (!_nodeLookup.TryGetValue(targetMetatable, out var targetNode))
		{
			throw new KeyNotFoundException("Target metatable not found.");
		}
		
		Remove(metatable);
		
		LinkedListNode<IMetatable> node = _metatableLink.AddAfter(targetNode, metatable);
		_nodeLookup[metatable] = node;
	}

	public void Remove(IMetatable metatable)
	{
		if (!_nodeLookup.Remove(metatable, out var node))
		{
			return;
		}

		_metatableLink.Remove(node);
	}

	private readonly LinkedList<IMetatable> _metatableLink = [];
	private readonly Dictionary<IMetatable, LinkedListNode<IMetatable>> _nodeLookup = [];

}


