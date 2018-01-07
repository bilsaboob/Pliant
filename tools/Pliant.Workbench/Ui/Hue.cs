using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Pliant.Workbench.Ui
{
    public class Hue
    {
        public Hue(string name, Color color, Color foreground)
        {
            Name = name;
            Color = color;
            Foreground = foreground;
        }

        public string Name { get; }

        public Color Color { get; }

        public Color Foreground { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
