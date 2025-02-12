// Copyright Zero Games. All Rights Reserved.

namespace ZeroGames.Evalite;

/// <summary>
/// Flags indicate which compiler features are available.
/// </summary>
[Flags]
public enum ECompilerFeatures : uint64
{
	Minimal = 0,
	
	/// <summary>
	/// Use int64 operations if all operands are of integer type.
	/// </summary>
	Integer = 1 << 0,
	
	/// <summary>
	/// Use decimal instead of double.
	/// </summary>
	Decimal = 1 << 1,
	
	/// <summary>
	/// Enable 'true' and 'false' literal, logic and relation operators (&amp;&amp;, ||, !, ==, !=, &gt;, &lt;, &gt;=, &lt;=).
	/// </summary>
	Boolean = 1 << 2,
	
	/// <summary>
	/// Enable string literal and concat operator (..).
	/// </summary>
	String = 1 << 3,
	
	/// <summary>
	/// Enable nested context and member access operator (.).
	/// </summary>
	/// <remarks>
	/// Return value of nested context has no static type, which can affect operator overload policy.
	/// </remarks>
	Member = 1 << 4,
	
	All = uint64.MaxValue,
	
	Common = All & ~Decimal,
}