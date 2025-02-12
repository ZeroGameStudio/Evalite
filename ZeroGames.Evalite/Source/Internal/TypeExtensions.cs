// Copyright Zero Games. All Rights Reserved.

namespace ZeroGames.Evalite;

internal static class TypeExtensions
{
	
	public static bool IsInteger(this Type @this) => Type.GetTypeCode(@this)
		is TypeCode.Byte
		or TypeCode.UInt16
		or TypeCode.UInt32
		or TypeCode.UInt64
		or TypeCode.SByte
		or TypeCode.Int16
		or TypeCode.Int32
		or TypeCode.Int64;
	
}


