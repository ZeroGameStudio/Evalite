// Copyright Zero Games. All Rights Reserved.

using System.Diagnostics.CodeAnalysis;

namespace ZeroGames.Evalite;

public sealed class CompositeContext : IContext
{
	
	public CompositeContext(){}

	public CompositeContext(params ReadOnlySpan<IContext> innerContexts)
	{
		foreach (var innerContext in innerContexts)
		{
			AddLast(innerContext);
		}
	}

	public bool TryCall(string name, [NotNullWhen(true)] out object? result, params object[] parameters)
	{
		foreach (var context in _contextLink)
		{
			if (context.TryCall(name, out result, parameters))
			{
				return true;
			}
		}

		result = null;
		return false;
	}

	public bool TryRead(string name, [NotNullWhen(true)] out object? result)
	{
		foreach (var context in _contextLink)
		{
			if (context.TryRead(name, out result))
			{
				return true;
			}
		}

		result = null;
		return false;
	}

	public Type GetFunctionReturnType(string name) => typeof(object);
	public Type GetPropertyType(string name) => typeof(object);
	
	public void Add(IContext context) => AddLast(context);

	public void AddFirst(IContext context)
	{
		Remove(context);

		LinkedListNode<IContext> node = _contextLink.AddFirst(context);
		_nodeLookup[context] = node;
	}

	public void AddLast(IContext context)
	{
		Remove(context);
		
		LinkedListNode<IContext> node = _contextLink.AddLast(context);
		_nodeLookup[context] = node;
	}

	public void AddBefore(IContext targetContext, IContext context)
	{
		if (!_nodeLookup.TryGetValue(targetContext, out var targetNode))
		{
			throw new KeyNotFoundException($"Target context is not in this context.");
		}
		
		Remove(context);

		LinkedListNode<IContext> node = _contextLink.AddBefore(targetNode, context);
		_nodeLookup[context] = node;
	}

	public void AddAfter(IContext targetContext, IContext context)
	{
		if (!_nodeLookup.TryGetValue(targetContext, out var targetNode))
		{
			throw new KeyNotFoundException($"Target context is not in this context.");
		}
		
		Remove(context);
		
		LinkedListNode<IContext> node = _contextLink.AddAfter(targetNode, context);
		_nodeLookup[context] = node;
	}

	public void Remove(IContext context)
	{
		if (!_nodeLookup.Remove(context, out var node))
		{
			return;
		}

		_contextLink.Remove(node);
	}

	private readonly LinkedList<IContext> _contextLink = [];
	private readonly Dictionary<IContext, LinkedListNode<IContext>> _nodeLookup = [];

}


