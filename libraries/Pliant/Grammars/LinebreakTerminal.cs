using System;
using System.Collections.Generic;

namespace Pliant.Grammars
{
    public class LinebreakTerminal : BaseTerminal
    {
        private static readonly Interval[] _intervals =
        {
            new Interval('\n', '\n')
        };

        public override IReadOnlyList<Interval> GetIntervals()
        {
            return _intervals;
        }

        public override bool IsMatch(char character)
        {
            return character == '\n';
        }
    }
}