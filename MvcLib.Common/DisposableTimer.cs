using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MvcLib.Common
{
    public class DisposableTimer : IDisposable
    {
        private readonly string _caller;
        private readonly string _msg;

        private readonly Stopwatch _stopwatch;
        private readonly Action<double> _callback;

        public static DisposableTimer StartNew(Type type, string msg = "", Action<double> callBack = null)
        {
            return new DisposableTimer(type.Name, msg, callBack);
        }

        public static DisposableTimer StartNew(string msg = "", Action<double> callBack = null, [CallerMemberName]string caller = "")
        {
            return new DisposableTimer(caller, msg, callBack);
        }

        private DisposableTimer(string caller, string msg, Action<double> callback)
        {
            _caller = caller;
            _msg = msg;
            _callback = callback;

            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            var ms = _stopwatch.Elapsed.TotalSeconds;

            if (_callback != null)
            {
                _callback.Invoke(ms);
            }
            else
            {
                Trace.TraceInformation("[DisposableTimer]:{0}, Caller: {1}, Elapsed: {2:##.000} ms", _msg, _caller, ms);
            }
        }
    }
}