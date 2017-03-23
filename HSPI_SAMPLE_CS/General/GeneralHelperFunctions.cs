using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSPI_SIID.General
{
    class GeneralHelperFunctions
    {

        public static string[] DaysOfWeek = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        public static double Evaluate(String input) //http://stackoverflow.com/questions/333737/evaluating-string-342-yield-int-18?noredirect=1&lq=1  Modified for numbers of more than one character
        {
            String expr = "(" + input + ")";
            Stack<String> ops = new Stack<String>();
            Stack<Double> vals = new Stack<Double>();
            int SSLength = 1;
      
           
            for (int i = 0; i < expr.Length; i=i+0)
            {
                SSLength = 1;
          
                if (char.IsDigit(expr[i]) || expr[i].Equals('.'))
                {
                    int j = i+1;
                    while (j < expr.Length && (char.IsDigit(expr[j]) || expr[j].Equals('.'))){
                        SSLength++;
                        j++;
                    }

                }

                String s = expr.Substring(i, SSLength);
                i = i + SSLength;

                if (s.Equals("(")) { }
                else if (s.Equals("+")|| s.Equals("-")|| s.Equals("*")||s.Equals("/"))
                 ops.Push(s);


                else if (s.Equals(")"))
                {
                    int count = ops.Count;
                    while (count > 0)
                    {
                        String op = ops.Pop();
                        double v = vals.Pop();
                        if (op.Equals("+")) v = vals.Pop() + v;
                        else if (op.Equals("-")) v = vals.Pop() - v;
                        else if (op.Equals("*")) v = vals.Pop() * v;
                        else if (op.Equals("/")) v = vals.Pop() / v;
                        //else if (op.Equals("sqrt")) v = Math.Sqrt(v);
                        vals.Push(v);

                        count--;
                    }
                }
                else vals.Push(Double.Parse(s));
            }
            return vals.Pop();
        }


    }
}
