using ZeroGames.Evalite;
using static System.Console;
using static TestHelper;
using System;

int32 suc = 0;
int32 fail = 0;

// 一元运算符测试
Assert("+1", 1, ref suc, ref fail);            // 整数
Assert("-1", -1, ref suc, ref fail);           // 整数
Assert("-1.5", -1.5, ref suc, ref fail);       // 小数保持小数
Assert("--1", 1, ref suc, ref fail);           // 整数
Assert("-+1", -1, ref suc, ref fail);          // 整数

// 基本算术运算测试
Assert("1 + 2", 3, ref suc, ref fail);         // 整数
Assert("3 - 4", -1, ref suc, ref fail);        // 整数
Assert("2 * 3", 6, ref suc, ref fail);         // 整数
Assert("6 / 2", 3, ref suc, ref fail);         // 整数除法
Assert("7 % 3", 1, ref suc, ref fail);         // 整数

// 幂运算测试
Assert("2 ^ 3", 8.0, ref suc, ref fail);       // 幂运算总是返回double
Assert("2 ^ 2 ^ 3", 256.0, ref suc, ref fail); // 测试右结合性: 2^(2^3) = 2^8 = 256
Assert("3 ^ -2", 0.1111111111111111, ref suc, ref fail);  // 1/9 ≈ 0.111111...

// 复杂组合测试
Assert("1 + 2 * 3", 7, ref suc, ref fail);         // 整数
Assert("(1 + 2) * 3", 9, ref suc, ref fail);       // 整数
Assert("-2 ^ 2", 4.0, ref suc, ref fail);          // 幂运算返回double
Assert("-(2 ^ 2)", -4.0, ref suc, ref fail);       // 幂运算返回double
Assert("2 * -3 + 4", -2, ref suc, ref fail);       // 整数
Assert("5 % 3 * 2", 4, ref suc, ref fail);         // 整数
Assert("2 ^ 3 % 5", 3.0, ref suc, ref fail);       // 幂运算返回double，所以取模结果也是double

// 科学计数法测试
Assert("1.6e-2", 0.016, ref suc, ref fail);        // 科学计数法总是返回double
Assert("-2.5e2", -250.0, ref suc, ref fail);       // 科学计数法总是返回double
Assert("1.2e+3", 1200.0, ref suc, ref fail);       // 科学计数法总是返回double
Assert("2e3", 2000.0, ref suc, ref fail);          // 科学计数法总是返回double
Assert("1.5e-3 + 2e2", 200.0015, ref suc, ref fail);
Assert("-1.2e2 * 3e-1", -36.0, ref suc, ref fail);
Assert("2.5e2 ^ 2", 62500.0, ref suc, ref fail);
Assert("(1e1 + 2.5e0) * 3", 37.5, ref suc, ref fail);

// 整数运算测试
Assert("1 + 2", 3, ref suc, ref fail);         // 整数加法
Assert("5 - 3", 2, ref suc, ref fail);         // 整数减法
Assert("4 * 3", 12, ref suc, ref fail);        // 整数乘法
Assert("7 / 2", 3, ref suc, ref fail);         // 整数除法，向下取整
Assert("8 / 3", 2, ref suc, ref fail);         // 整数除法，向下取整
Assert("-7 / 2", -3, ref suc, ref fail);       // 负数整数除法，向下取整
Assert("7 % 3", 1, ref suc, ref fail);         // 整数取模
Assert("-5", -5, ref suc, ref fail);           // 整数一元负号
Assert("2 ^ 3", 8.0, ref suc, ref fail);       // 幂运算总是返回double

// 混合运算测试
Assert("2 * 3.0", 6.0, ref suc, ref fail);     // 有小数参与则返回double
Assert("5 + 2.5", 7.5, ref suc, ref fail);     // 有小数参与则返回double
Assert("3 * -2", -6, ref suc, ref fail);       // 整数
Assert("10.0 / 4", 2.5, ref suc, ref fail);    // 浮点除法
Assert("10 / 4.0", 2.5, ref suc, ref fail);    // 浮点除法
Assert("2 ^ 2", 4.0, ref suc, ref fail);       // 幂运算总是返回double

// 添加更多除法测试
Assert("15 / 2", 7, ref suc, ref fail);        // 整数除法
Assert("15.0 / 2", 7.5, ref suc, ref fail);    // 浮点除法
Assert("15 / 2.0", 7.5, ref suc, ref fail);    // 浮点除法
Assert("-15 / 2", -7, ref suc, ref fail);      // 负数整数除法
Assert("-15 / -2", 7, ref suc, ref fail);      // 负数整数除法

// 变量测试
AssertWithParam<int, int>("x + 1", "x", 5, 6, ref suc, ref fail);           // 整数参数和结果
AssertWithParam<int, int>("x / 2", "x", 5, 2, ref suc, ref fail);      // 整数参数，整数结果
AssertWithParam<double, double>("x * 2", "x", 2.5, 5.0, ref suc, ref fail); // 浮点参数和结果
AssertWithParam<int, int>("x * 2 + 1", "x", 3, 7, ref suc, ref fail);       // 复杂表达式
AssertWithParam<int, double>("x ^ 2", "x", 3, 9.0, ref suc, ref fail);      // 幂运算
AssertWithParam<double, double>("-x", "x", 2.5, -2.5, ref suc, ref fail);   // 一元运算符
AssertWithParam<int, int>("(x + 1) * 2", "x", 3, 8, ref suc, ref fail);     // 带括号的表达式

// 参数名验证测试
AssertWithParam<int, int>("_x + 1", "_x", 5, 6, ref suc, ref fail);          // 下划线开头的参数名
AssertWithParam<int, int>("abc123 + 1", "abc123", 5, 6, ref suc, ref fail); // 字母数字混合的参数名
AssertWithParam<int, int>("_abc_123 + 1", "_abc_123", 5, 6, ref suc, ref fail); // 复杂的合法参数名

// 取模运算测试-
Assert("7 % 3", 1, ref suc, ref fail);         // 整数取模
Assert("7.0 % 3", 1.0, ref suc, ref fail);     // 浮点取模
Assert("7 % 3.0", 1.0, ref suc, ref fail);     // 浮点取模
Assert("7.5 % 2", 1.5, ref suc, ref fail);     // 浮点取模，保留小数部分
Assert("-7 % 3", -1, ref suc, ref fail);       // 负数整数取模
Assert("-7.0 % 3", -1.0, ref suc, ref fail);   // 负数浮点取模
Assert("7 % -3", 1, ref suc, ref fail);        // 除数为负整数
Assert("7.0 % -3", 1.0, ref suc, ref fail);    // 除数为负浮点数
Assert("3.14 % 2", 1.14, ref suc, ref fail);   // 浮点取模
Assert("2 ^ 3 % 5", 3.0, ref suc, ref fail);   // 幂运算返回double，所以取模结果也是double
Assert("2.5 ^ 2 % 3", 0.25, ref suc, ref fail); // 浮点数幂运算后取模

// Context测试
WriteLine("\nContext Tests:");

TestContext context = new TestContext();
Compiler compiler = new Compiler();

// 测试访问各种属性
Assert("PublicValue", 42, ref suc, ref fail, context);       // 公开实例属性
Assert("PrivateValue", 3.14, ref suc, ref fail, context);    // 私有实例属性
Assert("InternalValue", 100, ref suc, ref fail, context);    // 内部实例属性
Assert("StaticValue", 2.718, ref suc, ref fail, context);    // 静态属性
Assert("BaseValueProp", 10.0, ref suc, ref fail, context);   // 继承的属性

// 测试属性与参数组合使用
AssertWithParam<int32, double>("x + PublicValue", "x", 8, 50.0, ref suc, ref fail, context);  // 参数和context属性相加
AssertWithParam<double, double>("x * PrivateValue", "x", 2.0, 6.28, ref suc, ref fail, context);  // 参数和context属性相乘

// 测试多个context属性组合
Assert("PublicValue + PrivateValue", 45.14, ref suc, ref fail, context);  // 两个context属性相加
Assert("StaticValue * InternalValue", 271.8, ref suc, ref fail, context); // 静态属性和实例属性相乘

// 测试复杂表达式
Assert("PublicValue * 2 + PrivateValue ^ 2", 93.8596, ref suc, ref fail, context);  // 混合运算
Assert("(StaticValue + BaseValueProp) * InternalValue", 1271.8, ref suc, ref fail, context);  // 包含继承属性的运算

// 函数调用测试
Assert("Sqrt(4)", 2.0, ref suc, ref fail, context); // 简单函数调用
Assert("Sqrt(Sqrt(16))", 2.0, ref suc, ref fail, context); // 复合函数调用
Assert("Pow(Sqrt(4), 2)", 4.0, ref suc, ref fail, context); // 多参数函数调用
Assert("Pi()", Math.PI, ref suc, ref fail, context);

ForegroundColor = fail > 0 ? ConsoleColor.Red : ConsoleColor.Green;
WriteLine($"Total: {suc + fail} Suc: {suc} Fail: {fail}");
ResetColor();

// Context测试类
internal class BaseContext : ContextBase
{
	protected double BaseValue { get; } = 10.0;
}

internal class TestContext : BaseContext
{
	public int32 PublicValue { get; } = 42;
	private double PrivateValue { get; } = 3.14;
	internal int32 InternalValue { get; } = 100;
	public static double StaticValue { get; } = 2.718;
	public double BaseValueProp => BaseValue;  // 访问基类属性
	
	private static double Sqrt(double x) => Math.Sqrt(x);
	private static double Pow(double x, double p) => Math.Pow(x, p);
	private static double Pi() => Math.PI;
}

internal static class TestHelper
{
	public static void Assert(string expression, object expected, ref int32 suc, ref int32 fail, IContext? context = null)
	{
		try
		{
			object actual;
			if (expected is int32)
			{
				actual = new Compiler().Compile<int32>(expression, context)();
			}
			else
			{
				actual = new Compiler().Compile<double>(expression, context)();
			}
			
			if (Equals(actual, expected))
			{
				ForegroundColor = ConsoleColor.Green;
				WriteLine($"Assertion pass: {expression} = {actual}");
				++suc;
			}
			else
			{
				ForegroundColor = ConsoleColor.Red;
				WriteLine($"Assertion failed: {expression} != {actual} (expect {expected})");
				++fail;
			}
		}
		catch (Exception ex)
		{
			ForegroundColor = ConsoleColor.Red;
			WriteLine($"Assertion failed: {expression} is illegal{Environment.NewLine}{ex}");
			++fail;
		}
		finally
		{
			ResetColor();
		}
	}

	public static void AssertWithParam<T1, TResult>(string expression, string paramName, T1 paramValue, TResult expected, ref int32 suc, ref int32 fail, IContext? context = null)
	{
		try
		{
			Compiler compiler = new Compiler();
			Func<T1, TResult> func = compiler.Compile<T1, TResult>(expression, paramName, context);
			TResult actual = func(paramValue);
			
			if (Equals(actual, expected))
			{
				ForegroundColor = ConsoleColor.Green;
				WriteLine($"Assertion pass: {expression} with {paramName}={paramValue} = {actual}");
				++suc;
			}
			else
			{
				ForegroundColor = ConsoleColor.Red;
				WriteLine($"Assertion failed: {expression} with {paramName}={paramValue} != {actual} (expect {expected})");
				++fail;
			}
		}
		catch (Exception ex)
		{
			ForegroundColor = ConsoleColor.Red;
			WriteLine($"Assertion failed: {expression} with {paramName}={paramValue} is illegal{Environment.NewLine}{ex}");
			++fail;
		}
		finally
		{
			ResetColor();
		}
	}

	private new static bool Equals(object? objA, object? objB)
	{
		if (objA is double doubleA && objB is double doubleB)
		{
			const double EPSILON = 1e-7; // 设置一个合适的精度阈值
			return Math.Abs(doubleA - doubleB) < EPSILON;
		}

		return object.Equals(objA, objB);
	}
}


