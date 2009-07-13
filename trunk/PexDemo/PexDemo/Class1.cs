using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace PexDemo
{
    public class Class1
    {
        public string PascalCase(string value)
        {
            // <pex>
            if (value == (string)null)
                throw new ArgumentNullException("value");
            // </pex>
            Contract.Requires(value != null);

            // capitalise characters following space
            string output = string.Empty;

            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] == ' ')
                {
                    if (i < value.Length-1)
                        output += value[++i].ToString().ToUpper();
                }
                else
                {
                    output += value[i];
                }
            }
            return output;
        }
    }
}
