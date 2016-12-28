using System;

namespace WindowsShutdownTimer
{
    [Serializable]
    class StopTimerException : Exception
    {
        public StopTimerException() { }

        public StopTimerException(string message) : base(message) { }

        public StopTimerException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    class StartTimerException : Exception
    {
        public StartTimerException() { }

        public StartTimerException(string message) : base(message) { }

        public StartTimerException(string message, Exception inner) : base(message, inner) { }
    }
}
