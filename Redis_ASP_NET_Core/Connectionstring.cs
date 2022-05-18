using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redis_ASP_NET_Core
{
    public class Connectionstring
    {
        public string Value { get; set; }
        public Connectionstring(string value)
        {
            Value = value;
        }
    }
}
