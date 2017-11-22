using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pliant.Workbench.Common
{
    public class EventBase<T>
    {
        private static List<Action<T>> _eventListeners = new List<Action<T>>();

        public static void On(Action<T> action)
        {
            _eventListeners.Add(action);
        }

        public void Invoke()
        {
            var thisTyped = (T)(object)this;
            foreach (var listener in _eventListeners)
            {
                listener(thisTyped);
            }
        }
    }
}
