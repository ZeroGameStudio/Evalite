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
AssertWithParam<int32, int32>("x + PublicValue", "x", 8, 50, ref suc, ref fail, context);  // 参数和context属性相加
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
Assert("Sum(1,2,3,4)", 10.0, ref suc, ref fail, context);

// 布尔字面量测试
Assert("true", true, ref suc, ref fail);
Assert("false", false, ref suc, ref fail);

// 比较运算符测试
Assert("1 == 1", true, ref suc, ref fail);
Assert("1 != 2", true, ref suc, ref fail);
Assert("2 > 1", true, ref suc, ref fail);
Assert("1 < 2", true, ref suc, ref fail);
Assert("2 >= 2", true, ref suc, ref fail);
Assert("1 <= 1", true, ref suc, ref fail);

// 逻辑运算符测试
Assert("true && true", true, ref suc, ref fail);
Assert("true || false", true, ref suc, ref fail);
Assert("!false", true, ref suc, ref fail);

// 复杂表达式测试
Assert("1 < 2 && 3 > 2", true, ref suc, ref fail);
Assert("!(1 > 2) && 3 != 2", true, ref suc, ref fail);
Assert("(1 <= 2 && 2 <= 3) || false", true, ref suc, ref fail);

// 混合运算测试
Assert("1 + 2 > 2 && 3 * 2 <= 6", true, ref suc, ref fail);

Assert("IfElse(1 > 2, 5.0, IfElse(2 >= 2, true, 123e-2))", (object)true, ref suc, ref fail, context);
Assert("Select(1+1, true, 1.5, 2e2, -7)", (object)200.0, ref suc, ref fail, context);

// 测试成员嵌套
Assert("InnerContext.Int * 100.5", 90 * 100.5, ref suc, ref fail, context);
AssertWithParam<int32, double>("InnerContext.Exp(PrivateValue-p)", "p", 1, Math.Exp(3.14-1), ref suc, ref fail, context);

// String literal tests
WriteLine("\nString Literal Tests:");

// Basic string literals
Assert("\"Hello, world!\"", "Hello, world!", ref suc, ref fail);
Assert("'Hello, world!'", "Hello, world!", ref suc, ref fail);

// Empty strings
Assert("\"\"", "", ref suc, ref fail);
Assert("''", "", ref suc, ref fail);

// Escape sequences
Assert("\"\\n\"", "\n", ref suc, ref fail);          // Newline
Assert("\"\\r\"", "\r", ref suc, ref fail);          // Carriage return
Assert("\"\\t\"", "\t", ref suc, ref fail);          // Tab
Assert("\"\\\\\"", "\\", ref suc, ref fail);         // Backslash

// Quote escaping
Assert("\"He said \\\"Hello\\\"\"", "He said \"Hello\"", ref suc, ref fail);     // Escape double quotes in double-quoted string
Assert("'He said \\'Hello\\''", "He said 'Hello'", ref suc, ref fail);           // Escape single quotes in single-quoted string
Assert("\"He said 'Hello'\"", "He said 'Hello'", ref suc, ref fail);             // Unescaped single quotes in double-quoted string
Assert("'He said \"Hello\"'", "He said \"Hello\"", ref suc, ref fail);           // Unescaped double quotes in single-quoted string

// Mixed with other expressions
Assert("\"The sum is: \" .. (1 + 2)", "The sum is: 3", ref suc, ref fail);
Assert("'Value: ' .. true", "Value: True", ref suc, ref fail);

// String in function calls
// Assert("Format('Result: {0}', 42)", "Result: 42", ref suc, ref fail, context);
// Assert("Format(\"x = {0}, y = {1}\", 1, 2)", "x = 1, y = 2", ref suc, ref fail, context);

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
	private static double Sum(params double[] values) => values.Sum();
	private static object IfElse(bool condition, object trueValue, object falseValue) => condition ? trueValue : falseValue;
	private static object Select(int64 index, object[] values) => values[index];

	public InnerContext InnerContext { get; } = new();
}

internal class InnerContext : ContextBase
{
	public int32 Int { get; } = 90;
	public static double Exp(double x) => Math.Exp(x);
}

internal static class TestHelper
{
	public static void Assert<TResult>(string expression, TResult expected, ref int32 suc, ref int32 fail, IContext? context = null)
	{
		try
		{
			TResult actual = new Compiler().Compile<TResult>(expression, context)();
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
		// catch (Exception ex)
		// {
		// 	ForegroundColor = ConsoleColor.Red;
		// 	WriteLine($"Assertion failed: {expression} is illegal{Environment.NewLine}{ex}");
		// 	++fail;
		// }
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
		// catch (Exception ex)
		// {
		// 	ForegroundColor = ConsoleColor.Red;
		// 	WriteLine($"Assertion failed: {expression} with {paramName}={paramValue} is illegal{Environment.NewLine}{ex}");
		// 	++fail;
		// }
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


