using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegexParser
{
	public class Constant : FuncInst
	{
		private string mVal;
		public static Func<string, string> sIsActuallyConstant = (s) => null;
		
		public Constant(object value)
			: base(Function.RegConstant, null)
		{
			mVal = value.ToString();
		}

		public override object Solve(params object[] Variables)
		{
			string tmp;
			if((tmp = sIsActuallyConstant(mVal)) == null)
				return mVal;
			else
				return tmp;
		}

		public override string ToString()
		{
			return mVal.ToString();
		}
	}
}