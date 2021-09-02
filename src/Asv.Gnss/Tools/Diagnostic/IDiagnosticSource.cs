using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;

namespace Asv.Gnss
{
    public interface IDiagnosticSource:IDisposable
    {
        string GroupName { get; }
        IDigitDiagnostic<double> Real { get; }
        IDigitDiagnostic<int> Int { get; }
        IStringDiagnostic Str { get; }
        ISpeedDiagnostic Speed { get; }
    }

    public interface ISpeedDiagnostic:IDisposable
    {
        SpeedIndicator this[string name] { get; }
        SpeedIndicator this[string name, string format, TimeSpan? lifeTime = null, TimeSpan? updateTime = null] { get; }
    }

    public class SpeedDiagnostic : ISpeedDiagnostic
    {
        private readonly IDigitDiagnostic<double> _src;
        private readonly ConcurrentDictionary<string, SpeedIndicator> _indicators = new ConcurrentDictionary<string, SpeedIndicator>();

        public SpeedDiagnostic(IDigitDiagnostic<double> src)
        {
            _src = src;
        }

        public void Dispose()
        {
            foreach (var keyValuePair in _indicators)
            {
                keyValuePair.Value.Dispose();
            }
        }

        public SpeedIndicator this[string name] => _indicators.GetOrAdd(name, _=> _src.CreateSpeedIndicator(_));

        public SpeedIndicator
            this[string name, string format, TimeSpan? lifeTime = null, TimeSpan? updateTime = null] =>
            _indicators.GetOrAdd(name, _src.CreateSpeedIndicator(name, format, lifeTime, updateTime));

    }
}