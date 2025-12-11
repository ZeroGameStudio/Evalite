// Copyright Zero Games. All Rights Reserved.

using ZeroGames.Evalite;

Compiler compiler = new();
StaticLibraryMetatable mathMetatable = new(typeof(Math));
StaticLibraryMetatable constantMetatable = new(typeof(Constants));
CompositeMetatable metatable = new(mathMetatable, constantMetatable);

string? cmd;
do
{
	Console.Write("> ");
	cmd = Console.ReadLine();
	
	if (!string.IsNullOrWhiteSpace(cmd))
	{
		try
		{
			var result = compiler.Compile<object>(cmd, metatable)();
			Console.WriteLine(result);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
		}
	}
} while (cmd != "exit");

static class Constants
{
	public static double Pi => Math.PI;
}

class Metatable : ReflectionMetatableBase
{
	
	public static double Sqrt(double x) => Math.Sqrt(x);
	public static double Abs(double x) => Math.Abs(x);
	public static double Pow(double x, double y) => Math.Pow(x, y);
	public static double Exp(double x) => Math.Exp(x);
	public static double Log(double x) => Math.Log(x);
	public static double Sin(double x) => Math.Sin(x);
	public static double Cos(double x) => Math.Cos(x);
	public static double Tan(double x) => Math.Tan(x);
	public static double Asin(double x) => Math.Asin(x);
	public static double Acos(double x) => Math.Acos(x);
	public static double Atan(double x) => Math.Atan(x);
	public static double Atan2(double y, double x) => Math.Atan2(y, x);
	public static double Cosh(double x) => Math.Cosh(x);
	public static double Sinh(double x) => Math.Sinh(x);
	public static double Tanh(double x) => Math.Tanh(x);
	public static double Floor(double x) => Math.Floor(x);
	public static double Ceiling(double x) => Math.Ceiling(x);
	public static double Round(double x) => Math.Round(x);
	
}


