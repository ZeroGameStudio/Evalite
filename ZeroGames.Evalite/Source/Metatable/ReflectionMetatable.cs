// Copyright Zero Games. All Rights Reserved.

namespace ZeroGames.Evalite;

public sealed class ReflectionMetatable(Type type, object? instance = null) : ReflectionMetatableBase(type, instance);
public sealed class ReflectionMetatable<T>(T? instance = default) : ReflectionMetatableBase(typeof(T), instance);


