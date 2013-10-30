using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegexParser
{
    public class FuncInst
    {
        protected Function mType;
        protected FuncInst[] mParams;

        public FuncInst(Function Type, FuncInst[] Params)
        {
            mType = Type;
            mParams = Params;
        }

        public virtual object Solve(params object[] Variables)
        {
            object[] ps = new object[mType.InputCount];
            for (int i = 0; i < mType.InputCount; i++)
            {
                ps[i] = mParams[i].Solve(Variables);
            }
            return this.mType.Solve(ps);
        }

        public override string ToString()
        {
            return String.Format(mType.GetPrintFormat, mParams.Select(f => f == null ? "NULL" : f.ToString()).ToArray());
        }
    }

}
