using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegexParser
{
	/*
	 * This just helps with support for variables
	 */
	public class Variable : FuncInst
	{
		internal readonly static List<Variable> sVars = new List<Variable>();
		private char mc;
		
		public Variable(char c) : this(c, false)
		{
			if(!sVars.Select(v => (char)v).Contains(c))
				throw new InvalidOperationException(
					"To create a variable, first register the variable using the \"CreateNewVariable\" function");
		}
		
		private Variable(char c, bool isNew = true) : base(
			Function.RegVariable,
			new FuncInst[0])
		{
			mc = c;
			if(isNew)
			{
				sVars.Add(this);
				Function.RegVariable = new Function(
					v => 0,
					0, int.MaxValue, "{0}",
					"(" + String.Join ("|",sVars.Select(v => (char)v)) + ")");
			}
		}
		
		public override object Solve (params object[] Variables)
		{
			return Variables[sVars.Select(v => (char)v).ToList().IndexOf(this.mc)];
		}

		public static void CreateNewVariable(char c)
		{
			new Variable(char.ToLower(c), true);
		}

		public static void RemoveVariable(char c)
		{
			sVars.RemoveAll(v => (char)v == c);
		}

		public static implicit operator Variable(char c)
		{ return sVars.First(v => (char)v == c); }

		public static explicit operator char(Variable v)
		{ return v.mc; }

		internal static Variable Find(char c)
		{
			return sVars[sVars.Cast<char>().ToList<char>().IndexOf(c)];
		}

		public static Variable[] GetVars
		{ get { return sVars.ToArray(); } }

		public static int GetVarIndex(char c)
		{ return sVars.FindIndex(v => (char)v == c); }

		public override string ToString()
		{
			return mc.ToString();
		}
	}
}
