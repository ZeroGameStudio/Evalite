// Copyright Zero Games. All Rights Reserved.

namespace ZeroGames.Evalite;

public sealed class ReflectiveContext(Type type, object? instance = null, bool allowPrivateAccess = false) : ReflectiveContextBase(type, instance, allowPrivateAccess);
public sealed class ReflectiveContext<T>(T? instance = default, bool allowPrivateAccess = false) : ReflectiveContextBase(typeof(T), instance, allowPrivateAccess);


