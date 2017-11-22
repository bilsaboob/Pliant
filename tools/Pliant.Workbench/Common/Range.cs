using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace Pliant.Workbench.Common
{
    public class Position
    {
        public Position(int line, int col)
        {
            Line = line;
            Col = col;
        }

        public int Line { get; set; }
        public int Col { get; set; }
    }
}
