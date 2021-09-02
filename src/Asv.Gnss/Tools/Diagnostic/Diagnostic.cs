using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Asv.Gnss
{
    public class Diagnostic : IDiagnostic,IDisposable
    {

        private readonly ConcurrentDictionary<DiagnosticKey, DiagnosticItem> _values = new ConcurrentDictionary<DiagnosticKey, DiagnosticItem>();

        public KeyValuePair<DiagnosticKey, DiagnosticItem>[] GetItems()
        {
            return _values.ToArray();
        }

        public void ClearItems()
        {
            _values.Clear();
        }


        public IDiagnosticSource this[string group] => new DiagnosticSource(group, _values);


        public void Dispose()
        {
            
        }
    }
}