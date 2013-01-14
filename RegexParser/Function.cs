using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RegexParser
{
    public class Function
    {
        public static readonly Function RegConstantDouble = new Function((d) => d[0], 1, int.MaxValue, "{0}", @"(?:\d+(?:\.\d*)?)|(?:\.\d+)");
        public static readonly Function RegConstantString = new Function((s) => s[0], 1, int.MaxValue, "{0}", "[\\\"\\']([^'\"]*)[\\\"\\']");
        internal static Function RegVariable;

        Func<object[], object> mRunner;
        int mInputCount;
        string mPrintFormat;
        Regex mSearchPattern;
        int mOrder; //For order of operations

        //private static readonly Regex RefitPrintFormat = new Regex(@"{\d+}");
        //public Function(Func<double[], double> Executable, int InputCount, int OrderOfOperation, string PrintFormat)
        //    : this(Executable, InputCount, OrderOfOperation, PrintFormat, RefitPrintFormat.Replace(PrintFormat,"(.+)"))
        //{ }

        public Function(Func<object[], object> Executable, int InputCount, int OrderOfOperation, string PrintFormat, string SearchString)
        {
            mRunner = Executable;
            mInputCount = InputCount;
            mOrder = OrderOfOperation;
            mPrintFormat = PrintFormat;
            mSearchPattern = new Regex(SearchString);
        }

        public static explicit operator Func<object[], object>(Function f)
        { return f.mRunner; }

        public object Solve(object[] Inputs)
        { return mRunner(Inputs); }

        public int InputCount
        { get { return mInputCount; } }

        ///// <summary>
        ///// Each object MUST be either a Function or a double
        ///// </summary>
        ///// <param name="Params"></param>
        ///// <returns></returns>
        //public string ToString(params object[] Params)
        //{
        //    return string.Format(mPrintFormat, Params);
        //}

        internal Function Merge(int Param, Function f)
        {
            if (Param > mInputCount) throw new InvalidOperationException();
            //Note that, for now, we don't allow construction of compound search patterns
            return new Function(delegate(object[] d)
                {
                    object[] EndInputs = new object[d.Length - f.InputCount + 1];
                    Array.Copy(d, EndInputs, Param);
                    EndInputs[Param] = ((Func<object[], object>)f)(d);// (d.Skip(Param).Take(f.InputCount).ToArray());
                    //
                    //This line is probably messed up somewhere
                    //
                    if (Param + 1 < mInputCount) Array.ConstrainedCopy(d, Param + f.InputCount, EndInputs, Param + 1, mInputCount - Param - f.InputCount);
                    return mRunner(EndInputs);
                },
                mInputCount-1,this.mOrder, mPrintFormat.Replace("{" + Param + "}", "(" + f.mPrintFormat + ")"),"");
        }

        public override string ToString()
        {
            return base.ToString() + " \"" + this.mPrintFormat + "\"";
        }

        public string GetPrintFormat
        { get { return mPrintFormat; } }

        public static bool
            FLAGIgnoreWhitespace = true,
            FLAGIgnoreCase = true,
            FLAGAssumeStrayString = false;

        public static FuncInst Parse(string Input, params Function[] Funcs)
        {
            //
            //Remove all whitespace from the input
            //
            int offset = 0;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i + offset < Input.Length; i++)
            {
                if (Input[i+offset] == '\"' || Input[i+offset] == '\'')
                {
                    while (Input[(++i)+offset] != '\"' && Input[i+offset] != '\'') builder.Append(Input[i+offset]);
                    continue;
                }
                if(FLAGIgnoreWhitespace) while (char.IsWhiteSpace(Input[i + offset]))
                    offset++;
                builder.Append(FLAGIgnoreCase ? char.ToLower(Input[i + offset]) : Input[i + offset]);
            }
            Input = builder.ToString();

            //
            //TODO: Initialize Variables regex
            //
            RegVariable = new Function(null, 0,int.MaxValue,"{0}",string.Join("|",Variable.GetVars.Select(v => v.ToString()).ToArray()));

            //
            //Group Functions by their order of operation (descending order)
            //
            Funcs = Funcs.OrderBy(f => f.mOrder).ToArray();
            List<Regex> PatternsByOrder = new List<Regex>();
            List<List<Function>> SolversByOrder = new List<List<Function>>();
            int OopIndex = 0;
            for (int i = 0; i < Funcs.Length; OopIndex++)
            {
                SolversByOrder.Add(new List<Function>());
                StringBuilder SearchStr = new StringBuilder();
                do
                {
                    SolversByOrder[OopIndex].Add(Funcs[i]);
                    SearchStr.Append("(" + SolversByOrder[OopIndex].Last().mSearchPattern.ToString() + ")|");
                } while (++i < Funcs.Length && Funcs[i].mOrder == SolversByOrder[OopIndex][0].mOrder);
                SearchStr.Remove(SearchStr.Length - 1, 1);
                PatternsByOrder.Add(new Regex(SearchStr.ToString()));
            }

            //
            //Now recursively parse the whole string
            //
            bool found = true;
            return ParseHelper(Input, PatternsByOrder.ToArray(), SolversByOrder, ref found);
        }

        private static FuncInst ParseHelper(string Input, Regex[] SearchPatterns, List<List<Function>> Funcs, ref bool found, bool isRoot = true)
        {
            Match m;
            for (int i = 0; i < SearchPatterns.Length; i++)
                if (SearchPatterns[i].Match(Input).Value == Input)
                    for (int j = 0; j < Funcs[i].Count; j++)
                        if ((m = Funcs[i][j].mSearchPattern.Match(Input)).Value == Input)
                        {
                            FuncInst[] Params = new FuncInst[Funcs[i][j].mInputCount];
                            for (int k = 1; found && k < m.Groups.Count; k++)
                                Params[k-1] = ParseHelper(m.Groups[k].Value, SearchPatterns, Funcs, ref found, false);
                            if (!found) { found = true; continue; }
                            return new FuncInst(Funcs[i][j], Params);
                        }
            //If you got here, then no provided function matches the given input
            double tmpD;
            if (double.TryParse(Input, out tmpD))
            {
                //int tmpI;
                //if (int.TryParse(Input, out tmpI))
                //    return new FuncInstConst(tmpI);
                //else
                return new FuncInstConst(tmpD); 
            }
            if ((m = RegConstantString.mSearchPattern.Match(Input)).Value == Input)
                return new FuncInstConst(m.Groups[1].Value);
            if ((m = RegVariable.mSearchPattern.Match(Input)).Value == Input)
                return new FuncInstVar(Variable.Find(m.Value[0]));
            if (isRoot || !FLAGAssumeStrayString)
            {
                found = false;
                return null;
            }
            else
                return new FuncInstConst(Input); //Just return as string
        }
    }
}
