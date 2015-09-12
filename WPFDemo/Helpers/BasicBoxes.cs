using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFDemo
{
    public static class BasicBoxes
    {
        // Use boxes for default values in dependency property init
        // to minimize memory footprint.
        public static readonly double doubleBox = 0D;
        public static readonly bool trueBox = true;
        public static readonly bool falseBox = false;
    }
}
