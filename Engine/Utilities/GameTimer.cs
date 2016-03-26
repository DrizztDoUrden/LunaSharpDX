using System.Runtime.InteropServices;

namespace Engine.Utilities
{
    public sealed class GameTimer
    {
        [DllImport("kernel32.dll")]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("kernel32.dll")]
        public static extern bool QueryPerformanceFrequency(out long frequency);

        public GameTimer()
        {
            long freq;
            QueryPerformanceFrequency(out freq);
            _secondsPerCount = 1.0 / freq;
        }

        public float GameTime
        {
            get
            {
                if (_stopped)
                    return (float)((_stopTime - _pausedTime - _baseTime) * _secondsPerCount);
                return (float)((_currTime - _pausedTime) * _secondsPerCount);
            }
        }

        public float DeltaTime => (float)_deltaTime;

        public void Reset()
        {
            QueryPerformanceCounter(out _currTime);
            _prevTime = _currTime;
            _baseTime = _currTime;
            _stopTime = 0;
            _stopped = false;
        }

        public void Start()
        {
            if (!_stopped) return;

            QueryPerformanceCounter(out _prevTime);
            _pausedTime += _stopTime - _prevTime;
            _stopTime = 0;
            _stopped = false;
        }

        public void Stop()
        {
            if (_stopped) return;

            QueryPerformanceCounter(out _stopTime);
            _stopped = true;
        }

        public void Tick()
        {
            if (_stopped)
            {
                _deltaTime = 0;
                return;
            }

            QueryPerformanceCounter(out _currTime);
            _deltaTime = (_currTime - _prevTime) * _secondsPerCount;

            _prevTime = _currTime;
            if (_deltaTime < 0)
                _deltaTime = 0;
        }

        private readonly double _secondsPerCount;
        private double _deltaTime;
        private long _baseTime;
        private long _pausedTime;
        private long _stopTime;
        private long _prevTime;
        private long _currTime;
        private bool _stopped;
    }
}
