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

        public virtual object Solve()
        {
            object[] ps = new object[mType.InputCount];
            for (int i = 0; i < mType.InputCount; i++)
            {
                ps[i] = mParams[i].Solve();
            }
            return this.mType.Solve(ps);
        }

        public override string ToString()
        {
            return String.Format(mType.GetPrintFormat, mParams.Select(f => f == null ? "NULL" : f.ToString()).ToArray());
        }
    }

    //****************************************************************************************************

    public class FuncInstConst : FuncInst
    {
        private object mVal;
        public FuncInstConst(object value)
            : base(Function.RegConstantDouble, null)
        {
            mVal = value;
        }

        public override object Solve()
        {
            return mVal;
        }

        public override string ToString()
        {
            return mVal.ToString();
        }
    }

    //****************************************************************************************************

    public class FuncInstVar : FuncInst
    {
        private Variable mVal;
        public FuncInstVar(Variable value)
            : base(Function.RegConstantDouble, null)
        {
            mVal = value;
        }

        public override object Solve()
        {
            //
            //TODO: NEED TO SUPPORT VARIABLE
            //
            return 1d;
        }

        public override string ToString()
        {
            return ((char)mVal).ToString();
        }
    }
}
