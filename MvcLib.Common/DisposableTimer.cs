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
            _msg = msg;
            _callback = callback;

            Trace.TraceInformation("[Begin Timer]:{0}, Initialized by: {1}", _msg, caller);
            Trace.Indent();
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            Trace.Unindent();
            _stopwatch.Stop();
            var ms = _stopwatch.Elapsed.TotalSeconds;

            if (_callback != null)
            {
                _callback.Invoke(ms);
            }
            else
            {
                Trace.TraceInformation("[End Timer]:{0}, Completed: {1}ms", _msg, ms.ToString("##.000"));
            }
        }
    }
}