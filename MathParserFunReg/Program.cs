using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RegexParser;
using System.Diagnostics;

namespace MathParserFunReg
{
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

        static void Main(string[] args)
        {
            /******************************
             * CALCULATOR CODE
             ******************************/
             
            Stopwatch parse = new Stopwatch(), solve = new Stopwatch();
            Function[] Funcs = new Function[] { 
            new Function(d => (double)d[0]+(double)d[1],2,1,"{0}+{1}",@"(.+)[+](.+)"),
            new Function(d => (double)d[0]-(double)d[1],2,1,"{0}-{1}",@"(.+)[-](.+)"),
            new Function(d => (double)d[0]*(double)d[1],2,2,"{0}*{1}",@"(.+)[*](.+)"),
            new Function(d => (double)d[0]/(double)d[1],2,2,"{0}/{1}",@"(.+)[/](.+)"),
            new Function(d => (double)d[0],1,5,"({0})",@"\((.+)\)"),
            new Function(d => Math.Sqrt((double)d[0]),1,5,"sqrt({0})",@"sqrt\((.+)\)"),
            new Function(d => Math.Sin((double)d[0]),1,5,"sin({0})",@"sin\((.+)\)"),
            new Function(d => Math.Cos((double)d[0]),1,5,"cos({0})",@"cos\((.+)\)"),
            new Function(d => Fac(d[0]), 1, 6, "{0}!", @"(.+)!"),
            new Function(d => Fac(d[0])/(Fac(d[2])*(Fac((double)d[0]-(double)d[2]))) * Math.Pow((double)d[1], (double)d[2]) * Math.Pow((1d - (double)d[1]), (double)d[0] - (double)d[2]), 3, 5, "binompdf({0},{1},{2})", @"binompdf\((.+),(.+),(.+)\)")
            };
            char[] Alphabet = Enumerable.Range(97, 26).Select<int,char>(i => (char)i).ToArray();
            Console.WriteLine("Currently supported functions:");
            foreach (Function f in Funcs)
                Console.WriteLine(f.GetPrintFormat, 'a','b','c');
            Console.WriteLine();
            Console.WriteLine("Please input a math function of some sort");
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
                    double ans = (double)Result.Solve();
                    solve.Stop();
                    Console.WriteLine(ans.ToString());
                    Console.WriteLine("Parse time: {0} ({1} ms)\t\t\tSolve time: {2} ({3} ms)", parse.ElapsedTicks, parse.ElapsedMilliseconds, solve.ElapsedTicks, solve.ElapsedMilliseconds);
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
