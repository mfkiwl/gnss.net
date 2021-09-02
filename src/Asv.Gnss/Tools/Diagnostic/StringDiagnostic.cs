using System;
using System.Collections.Concurrent;

namespace Asv.Gnss
{
    public class StringDiagnostic : IStringDiagnostic
    {
        private readonly ConcurrentDictionary<DiagnosticKey, DiagnosticItem> _values;

        public StringDiagnostic(string groupName, ConcurrentDictionary<DiagnosticKey, DiagnosticItem> values)
        {
            _values = values;
            GroupName = groupName;
        }

        public string GroupName { get; }

        public string this[string name]
        {
            get => Get(name)?.StrValue;
            set => _values.AddOrUpdate(new DiagnosticKey(GroupName, name), new DiagnosticItem
            {
                LastUpdate = DateTime.Now,
                FormatString = null,
                StrValue = value,
                ItemType = DiagnosticItemType.String,
            }, (diagnosticKey, s) =>
            {
                s.StrValue = value;
                s.LastUpdate = DateTime.Now;
                return s;
            });
        }

        private DiagnosticItem Get(string name)
        {
            return _values.TryGetValue(new DiagnosticKey(GroupName, name), out var item) ? item : null;
        }

        public string this[string name, TimeSpan lifeTime]
        {
            set => _values.AddOrUpdate(new DiagnosticKey(GroupName, name), new DiagnosticItem
            {
                LastUpdate = DateTime.Now,
                FormatString = null,
                StrValue = value,
                ItemType = DiagnosticItemType.Real,
                LifeTime = lifeTime,
            }, (diagnosticKey, s) =>
            {
                s.StrValue = value;
                s.LastUpdate = DateTime.Now;
                return s;
            });
        }

        public void Dispose()
        {
        }
    }
}