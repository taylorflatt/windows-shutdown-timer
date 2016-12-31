using System;

namespace WindowsShutdownTimer
{
    /// <summary>
    /// Trouble stopping the timer.
    /// </summary>
    [Serializable]
    class StopTimerException : Exception
    {
        public StopTimerException() { }

        public StopTimerException(string message) : base(message) { }

        public StopTimerException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Trouble starting the timer.
    /// </summary>
    [Serializable]
    class StartTimerException : Exception
    {
        public StartTimerException() { }

        public StartTimerException(string message) : base(message) { }

        public StartTimerException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Used to show that the timer has ended. This isn't a true exception. This seemed easier (to read/understand) than traditional logic to me.
    /// </summary>
    [Serializable]
    class TimerEnded : Exception
    {
        public TimerEnded() { }

        public TimerEnded(string message) : base(message) { }

        public TimerEnded(string message, Exception inner) : base(message, inner) { }
    }
}
