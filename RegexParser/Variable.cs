using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RegexParser
{
    /*
     * This just helps with support for variables
     */
    public class Variable
    {
        private static List<Variable> sVars = new List<Variable>();
        private char mc;

        private Variable(char c)
        { mc = c; sVars.Add(this); }

        public static implicit operator Variable(char c)
        { return new Variable(c); }

        public static explicit operator char(Variable v)
        { return v.mc; }

        internal static Variable Find(char c)
        {
            return sVars[sVars.Cast<char>().ToList<char>().IndexOf(c)];
        }

        public static Variable[] GetVars
        { get { return sVars.ToArray(); } }

        public override string ToString()
        {
            return mc.ToString();
        }
    }
}
