// Copyright Zero Games. All Rights Reserved.

namespace ZeroGames.Evalite;

public sealed partial class Compiler
{

	public Compiler(ECompilerFeatures availableFeatures = ECompilerFeatures.Common)
	{
		if (availableFeatures.HasFlag(ECompilerFeatures.Member | ECompilerFeatures.PathIdentifier))
		{
			throw new ArgumentOutOfRangeException(nameof(availableFeatures), "Conflict features: Member and PathIdentifier.");
		}

		AvailableFeatures = availableFeatures;
	}

	public Func<TResult> Compile<TResult>(string expression, IMetatable? metatable = null) => 
		(Func<TResult>)InternalCompile(expression, metatable, typeof(TResult));

	public Func<T1, TResult> Compile<T1, TResult>(string expression, string parameterName1, IMetatable? metatable = null) => 
		(Func<T1, TResult>)InternalCompile(expression, metatable, typeof(TResult), new Parameter(typeof(T1), parameterName1));

	public Func<T1, T2, TResult> Compile<T1, T2, TResult>(string expression, string parameterName1, string parameterName2, IMetatable? metatable = null) => 
		(Func<T1, T2, TResult>)InternalCompile(expression, metatable, typeof(TResult), new(typeof(T1), parameterName1), new(typeof(T2), parameterName2));

	public Func<T1, T2, T3, TResult> Compile<T1, T2, T3, TResult>(string expression, string parameterName1, string parameterName2, string parameterName3, IMetatable? metatable = null) => 
		(Func<T1, T2, T3, TResult>)InternalCompile(expression, metatable, typeof(TResult), new(typeof(T1), parameterName1), new(typeof(T2), parameterName2), new(typeof(T3), parameterName3));

	public Func<T1, T2, T3, T4, TResult> Compile<T1, T2, T3, T4, TResult>(string expression, string parameterName1, string parameterName2, string parameterName3, string parameterName4, IMetatable? metatable = null) => 
		(Func<T1, T2, T3, T4, TResult>)InternalCompile(expression, metatable, typeof(TResult), new(typeof(T1), parameterName1), new(typeof(T2), parameterName2), new(typeof(T3), parameterName3), new(typeof(T4), parameterName4));

	public Func<T1, T2, T3, T4, T5, TResult> Compile<T1, T2, T3, T4, T5, TResult>(string expression, string parameterName1, string parameterName2, string parameterName3, string parameterName4, string parameterName5, IMetatable? metatable = null) => 
		(Func<T1, T2, T3, T4, T5, TResult>)InternalCompile(expression, metatable, typeof(TResult), new(typeof(T1), parameterName1), new(typeof(T2), parameterName2), new(typeof(T3), parameterName3), new(typeof(T4), parameterName4), new(typeof(T5), parameterName5));

	public Func<T1, T2, T3, T4, T5, T6, TResult> Compile<T1, T2, T3, T4, T5, T6, TResult>(string expression, string parameterName1, string parameterName2, string parameterName3, string parameterName4, string parameterName5, string parameterName6, IMetatable? metatable = null) => 
		(Func<T1, T2, T3, T4, T5, T6, TResult>)InternalCompile(expression, metatable, typeof(TResult), new(typeof(T1), parameterName1), new(typeof(T2), parameterName2), new(typeof(T3), parameterName3), new(typeof(T4), parameterName4), new(typeof(T5), parameterName5), new(typeof(T6), parameterName6));

	public Func<T1, T2, T3, T4, T5, T6, T7, TResult> Compile<T1, T2, T3, T4, T5, T6, T7, TResult>(string expression, string parameterName1, string parameterName2, string parameterName3, string parameterName4, string parameterName5, string parameterName6, string parameterName7, IMetatable? metatable = null) => 
		(Func<T1, T2, T3, T4, T5, T6, T7, TResult>)InternalCompile(expression, metatable, typeof(TResult), new(typeof(T1), parameterName1), new(typeof(T2), parameterName2), new(typeof(T3), parameterName3), new(typeof(T4), parameterName4), new(typeof(T5), parameterName5), new(typeof(T6), parameterName6), new(typeof(T7), parameterName7));

	public TDelegate CompileDelegate<TDelegate>(string expression, IMetatable? metatable = null) where TDelegate : Delegate
		=> (TDelegate)InternalCompileDelegate<TDelegate>(expression, metatable);
	
	public ECompilerFeatures AvailableFeatures { get; }

}


