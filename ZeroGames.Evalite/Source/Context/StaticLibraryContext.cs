// Copyright Zero Games. All Rights Reserved.

namespace ZeroGames.Evalite;

public sealed class StaticLibraryContext(Type libraryType) : ReflectiveContextBase(libraryType, null, false);
public sealed class StaticLibraryContext<T>() : ReflectiveContextBase(typeof(T), null, false);


