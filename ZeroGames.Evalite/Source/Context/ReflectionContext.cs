// Copyright Zero Games. All Rights Reserved.

namespace ZeroGames.Evalite;

public sealed class ReflectionContext(Type type, object? instance = null) : ReflectionContextBase(type, instance);
public sealed class ReflectionContext<T>(T? instance = default) : ReflectionContextBase(typeof(T), instance);


