using System;

namespace Checkers.Observer
{
    public interface ISerializable
    {
        public event Action TurnPerformed;

        public event Action<BaseClickComponent> ChipDestroyed;

        public event Action<ColorType> GameEnded;

        public event Action StepFinished;
    }
}
