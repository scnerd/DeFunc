using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RegexParser
{
	/*
	 * This just helps with support for variables
	 */
	public class Variable : FuncInst
	{
		internal static Dictionary<string, FuncInst> sVariables = new Dictionary<string, FuncInst>();
		private static Dictionary<FuncInst, object> sValues = new Dictionary<FuncInst, object>();
		private char mc;

		private Variable(string Name) : base(Function.RegVariable, new FuncInst[0])
		{}

		private static void ResetRegVariable()
		{
			Function.RegVariable = new Function(
				v => sValues[sVariables[v[0].ToString()]],
				1,
				int.MaxValue,
				" {0} ",
				'(' + string.Join("|", sVariables.Keys) + ')');
		}
		
		public override object Solve (params object[] Variables)
		{
			return sValues[this];
		}

		public static void SetVariable(string Name, object Value)
		{
			if (!sVariables.Keys.Contains(Name))
			{
				sVariables.Add(Name, new Variable(Name));
				sValues.Add(sVariables[Name], Value);
			}
			else
			{
				sValues[sVariables[Name]] = Value;
			}
			ResetRegVariable();
		}

		public static void ClearVariables()
		{
			sVariables = new Dictionary<string, FuncInst>();
			sValues = new Dictionary<FuncInst, object>();
		}

		public override string ToString()
		{
			return sVariables.First(k => k.Value == this).Key;
		}
	}
}
