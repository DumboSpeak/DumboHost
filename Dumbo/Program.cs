using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dumbo
{
    class Program
    {
        static void Main(string[] args)
        {
            var recognizer = new Recognizer();
            recognizer.ListenIO();
        }
    }
}
