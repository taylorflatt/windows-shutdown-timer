using System;

namespace WindowsShutdownTimer
{
    /// <summary>
    /// Trouble stopping the timer.
    /// </summary>
    [Serializable]
    class StopTimerException : Exception
    {
        public string ErrorCode = "";

        public StopTimerException() { }

        public StopTimerException(string message, string code) : base(message)
        {
            this.ErrorCode = code;
        }

        public StopTimerException(string message, Exception inner, string code) : base(message, inner)
        {
            this.ErrorCode = code;
        }
    }

    /// <summary>
    /// Trouble starting the timer.
    /// </summary>
    [Serializable]
    class StartTimerException : Exception
    {
        public string ErrorCode = "";

        public StartTimerException() { }

        public StartTimerException(string message, string code) : base(message)
        {
            this.ErrorCode = code;
        }

        public StartTimerException(string message, Exception inner, string code) : base(message, inner)
        {
            this.ErrorCode = code;
        }
    }

    /// <summary>
    /// Used to show that the timer has ended. This isn't a true exception. This seemed easier (to read/understand) than traditional logic to me.
    /// </summary>
    [Serializable]
    class TimerEnded : Exception
    {
        public TimerEnded() { }

        public TimerEnded(string message) : base(message) { }
    }
}
