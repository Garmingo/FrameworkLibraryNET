using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkLibraryClient
{
    public class Interval
    {
        private readonly int _onceEvery;
        private int _currentPosition;
        private readonly Action _action;

        public Interval(int executeEvery, Action action)
        {
            _onceEvery = executeEvery;
            _action = action;
            _currentPosition = 0;
        }

        public void Run()
        {
            if (_currentPosition == _onceEvery)
            {
                _action.Invoke();
                _currentPosition = 0;
            }
            else
            {
                _currentPosition++;
            }
        }
    }
}
