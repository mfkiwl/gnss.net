using System;
using System.Collections.Concurrent;

namespace Asv.Gnss
{
    public class DoubleDiagnostic : IDigitDiagnostic<double>
    {
        private readonly ConcurrentDictionary<DiagnosticKey, DiagnosticItem> _values;

        public DoubleDiagnostic(string groupName, ConcurrentDictionary<DiagnosticKey, DiagnosticItem> values)
        {
            _values = values;
            GroupName = groupName;
        }

        public string GroupName { get; }

        public double this[string name]
        {
            get => Get(name)?.RealValue ?? 0.0;
            set => _values.AddOrUpdate(new DiagnosticKey(GroupName, name), new DiagnosticItem
            {
                LastUpdate = DateTime.Now,
                FormatString = null,
                RealValue = value,
                ItemType = DiagnosticItemType.Real,
            }, (diagnosticKey, s) =>
            {
                s.RealValue = value;
                s.LastUpdate = DateTime.Now;
                return s;
            });
        }

        private DiagnosticItem Get(string name)
        {
            return _values.TryGetValue(new DiagnosticKey(GroupName, name), out var item) ? item : null;
        }

        public double this[string name, string format]
        {
            set => _values.AddOrUpdate(new DiagnosticKey(GroupName, name), new DiagnosticItem
            {
                LastUpdate = DateTime.Now,
                FormatString = format,
                RealValue = value,
                ItemType = DiagnosticItemType.Real,
            }, (diagnosticKey, s) =>
            {
                s.RealValue = value;
                s.LastUpdate = DateTime.Now;
                return s;
            });
        }

        public double this[string name, string format, TimeSpan lifeTime]
        {
            set => _values.AddOrUpdate(new DiagnosticKey(GroupName, name), new DiagnosticItem
            {
                LastUpdate = DateTime.Now,
                FormatString = format,
                RealValue = value,
                ItemType = DiagnosticItemType.Real,
                LifeTime = lifeTime,
            }, (diagnosticKey, s) =>
            {
                s.RealValue = value;
                s.LastUpdate = DateTime.Now;
                return s;
            });
        }

        public double this[string name, TimeSpan lifeTime]
        {
            set => _values.AddOrUpdate(new DiagnosticKey(GroupName, name), new DiagnosticItem
            {
                LastUpdate = DateTime.Now,
                FormatString = null,
                RealValue = value,
                ItemType = DiagnosticItemType.Real,
                LifeTime = lifeTime,
            }, (diagnosticKey, s) =>
            {
                s.RealValue = value;
                s.LastUpdate = DateTime.Now;
                return s;
            });
        }

        public void Dispose()
        {
        }
    }
}