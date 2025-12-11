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
	/// Uses int64 arithmetic operations if all operands are of integer type.
	/// </summary>
	Integer = 1 << 0,
	
	/// <summary>
	/// Uses decimal instead of double arithmetic operators.
	/// </summary>
	Decimal = 1 << 1,
	
	/// <summary>
	/// Enables 'true' and 'false' literal, logic and relation operators (&amp;&amp;, ||, !, ==, !=, &gt;, &lt;, &gt;=, &lt;=).
	/// </summary>
	Boolean = 1 << 2,
	
	/// <summary>
	/// Enables string literal and concat operator (..).
	/// </summary>
	String = 1 << 3,
	
	/// <summary>
	/// Enables member access operator (.).
	/// </summary>
	/// <remarks>
	/// Return value of member has no static type, which can affect operator overload policy.
	/// </remarks>
	Member = 1 << 4,
	
	/// <summary>
	/// Enables path identifier (a.b.c), can't use together with Member.
	/// </summary>
	PathIdentifier = 1 << 5,
	
	All = uint64.MaxValue & ~PathIdentifier,
	
	Common = All & ~Decimal,
	CommonPathIdentifier = Common & ~Member | PathIdentifier,
}


