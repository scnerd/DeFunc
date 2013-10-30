using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RegexParser;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathParserFunReg
{
	/*
	 * http://stackoverflow.com/questions/3188882/compile-and-run-dynamic-code-without-generating-exe
	 * 
	 * The above link will allow you to allow the user to, in-console, dynamically write and compile
	 * code, to allow new Functions at run-time (all with or without creating dll's for new functions).
	 * 
	 * Note that this idea is geared towards a combo of this with TextMenu, to allow custom, C#'ish
	 * coding that's interpretted and run totally in command line. This deserves its own program, b/c
	 * it's a very cool idea.
	 */
	class Program
	{
		private static double Fac(object d) { return (double)Fac((long)(double)d); }
		private static long Fac(long d)
		{
			if (d > 1)
				return d * Fac(d - 1);
			else
				return 1;
		}
		private static double D(object o)
		{ return double.Parse(o.ToString()); }
		private static Vector Vec(params double[] d)
		{ return new DenseVector(d); }
		private static double GetMult(AngleMode From)
		{
			return 
				From == AngleMode.deg ? Math.PI / 180d :
				From == AngleMode.rad ? 1 : 
				1;
		}

		private enum AngleMode
		{
			deg,
			rad
		}

		private static AngleMode Mode = AngleMode.rad;

		static void Main(string[] args)
		{
			/******************************
			 * CALCULATOR CODE
			 ******************************/
			
			double ANGLE_MULT = GetMult(Mode);

			Stopwatch parse = new Stopwatch(), solve = new Stopwatch();
			Func<object[], int, int, Vector> Str2V = (o, l, r) =>
				new DenseVector(o.Where((x, i) => l <= i && i <= r).Select(d => D(d)).ToArray());
			Func<object[], int, Vector> Str2D = (o, i) => Str2V(o, i, i);
			//Func<object[], Vector> Str2A = (o) => Str2V(o, 0, o.Length - 1);
			Func<double, Vector> DefineConstant = (d) => new DenseVector(new double[]{d});
			Function[] Funcs = new Function[] {
				//OPERATORS
			new Function(d => Str2V(d,0,1),
				2,2,@"[{0},{1}]",@"\[(.+),(.+)\]")
			,new Function(d => Vec(Str2D(d,0)[0] * Math.Cos(Str2D(d,1)[0]), Str2D(d,0)[0] * Math.Sin(Str2D(d,1)[0])),
				2,2,@"[{0},{1}]",@"\[(.+),<(.+)\]")
			,new Function(d => Str2D(d,0).Add(Str2D(d,1)),
				2,1,"{0}+{1}",@"(.+)[+](.+)")
			,new Function(d => Str2D(d,0).Subtract(Str2D(d,1)),
				2,1,"{0}-{1}",@"(.+)[-](.+)")
			,new Function(d => Str2D(d,0).PointwiseMultiply(Str2D(d,1)),
				2,2,"{0}*{1}",@"(.+)[*](.+)")
			,new Function(d => Str2D(d,0).PointwiseDivide(Str2D(d,1)),
				2,2,"{0}/{1}",@"(.+)[/](.+)")
			,new Function(d => Str2D(d,0),
				1,5,"({0})",@"\((.+)\)")
				//FUNCTIONS
			,new Function(d => Vec(d.Select(i => Math.Sin(D(i) * ANGLE_MULT)).ToArray()),
				1,4,"SIN({0})",@"sin\((.+)\)")
			,new Function(d => Vec(d.Select(i => Math.Cos(D(i) * ANGLE_MULT)).ToArray()),
				1,4,"COS({0})",@"cos\((.+)\)")
			,new Function(d => Vec(d.Select(i => Math.Tan(D(i) * ANGLE_MULT)).ToArray()),
				1,4,"TAN({0})",@"tan\((.+)\)")
			,new Function(d => Vec(d.Select(i => Math.Asin(D(i) * ANGLE_MULT)).ToArray()),
				1,4,"ASIN({0})",@"asin\((.+)\)")
			,new Function(d => Vec(d.Select(i => Math.Acos(D(i) * ANGLE_MULT)).ToArray()),
				1,4,"ACOS({0})",@"acos\((.+)\)")
			,new Function(d => Vec(d.Select(i => Math.Atan(D(i) * ANGLE_MULT)).ToArray()),
				1,4,"ATAN({0})",@"atan\((.+)\)")
			,new Function(d => Vec(d.Select(i => Math.Sqrt(D(i) * ANGLE_MULT)).ToArray()),
				1,4,"SQRT({0})",@"sqrt\((.+)\)")
			,new Function(d => { Variable.SetVariable(d[0].ToString(), d[1]); return null;},
				2, 4, "{{{0}}} set to {1}", @"set\((\w+)=(.+)\)")
			,new Function(d => {Variable.ClearVariables(); return null;},
				0, 4, "Variables Cleared", "clear")
				//CONSTANTS
			,new Function(d => DefineConstant(Math.PI),0,5,"PI","pi")
				//SETTINSG
			,new Function(d => 
			{
				try
				{
					Mode = (AngleMode)Enum.Parse(typeof(AngleMode), d[0].ToString(), true);
				}
				catch (ArgumentException)
				{
					return d[0].ToString() + " is an invalid mode. Use one of the following: " + string.Join(",", Enum.GetNames(typeof(AngleMode)));
				}
				ANGLE_MULT = GetMult(Mode); 
				return "Mode Set";
			},
				1,5,"MODE={0}",@"mode=(\w+)")
			};
			
			//char[] Alphabet = Enumerable.Range(97, 26).Select<int,char>(i => (char)i).ToArray();
			Console.WriteLine("Currently supported functions:");
			foreach (Function f in Funcs)
				Console.WriteLine(f.GetPrintFormat, 'a','b','c');
			Console.WriteLine();
			Console.WriteLine("Please input a math function of some sort");

			Function.FLAGAssumeStrayString = true;
			Function.RegConstant = new Function(v => double.Parse(v[0].ToString()), 0, int.MaxValue, "{0}", @"((?:\d+\.?\d*?)|(?:\d*\.?\d+))");
			double junkDoub;
			Constant.sIsActuallyConstant = (s) => double.TryParse(s, out junkDoub) ? null : s;
			
			do
			{
				string input = Console.ReadLine();
				parse.Restart();
				FuncInst Result = Function.Parse(input, Funcs);
				parse.Stop();
				Console.WriteLine("Here's what I think you were saying:");
				Console.WriteLine(Result == null ? "NULL" : Result.ToString());
				if (Result != null)
				{
					Console.WriteLine("Which equals:");
					solve.Restart();
					var ans = Result.Solve();
					solve.Stop();
					Console.WriteLine((ans ?? "No response").ToString());
					Console.WriteLine("Parse time: {0} ({1} ms)\t\t\tSolve time: {2} ({3} ms)",
						parse.ElapsedTicks, parse.ElapsedMilliseconds, solve.ElapsedTicks, solve.ElapsedMilliseconds);
				}
				Console.WriteLine("Try another one");
			} while (true);
			 

			//Function[] Funcs = new Function[]{
			//    new Function(o => null, 1, 2, "{0};", "(.+);"),
			//    new Function(o => {Console.WriteLine(o[0].ToString());return null;}, 1, 1, "write({0})", @"write\((.+)\)"),
			//    new Function(o => (string)o[0] + (string)o[1], 2, 1, "cat({0},{1})", @"cat\((.+),(.+)\)")
			//};
			//FuncInst Result = Function.Parse(Console.ReadLine(), Funcs);
			//Console.WriteLine(Result.Solve());
			//Console.ReadLine();
		}
	}
}
