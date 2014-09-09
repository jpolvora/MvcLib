using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MvcLib.Common
{
    public class DisposableTimer : IDisposable
    {
        private readonly string _msg;
        private readonly Stopwatch _stopwatch;
        private readonly Action<double> _callback;

        public static DisposableTimer StartNew(string msg, Type type, Action<double> callBack = null)
        {
            return new DisposableTimer(type.Name, msg, callBack);
        }

        public static DisposableTimer StartNew(string msg = "", Action<double> callBack = null, [CallerMemberName]string caller = "")
        {
            return new DisposableTimer(caller, msg, callBack);
        }

        private DisposableTimer(string caller, string msg, Action<double> callback)
        {
            _msg = msg;
            _callback = callback;

            Trace.Indent();
            Trace.TraceInformation("[{0}]: Begin Timer: {1}", _msg, caller);
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
            Trace.TraceInformation("[{0}], Execution time: {1}ms", _msg, ms.ToString("##.000"));
            Trace.Unindent();
        }
    }
}