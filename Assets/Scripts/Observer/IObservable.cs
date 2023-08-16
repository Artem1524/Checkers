using System;
using System.Collections;
using System.Threading.Tasks;

namespace Checkers.Observer
{
    public interface IObservable
    {
        public Task Serialize(string input);

        public event Action<Coordinate> NextStepReady;
    }
}
