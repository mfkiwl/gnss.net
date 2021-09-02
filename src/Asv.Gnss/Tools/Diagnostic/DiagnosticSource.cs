using System.Collections.Concurrent;

namespace Asv.Gnss
{
    public class DiagnosticSource : IDiagnosticSource
    {
        public DiagnosticSource(string group, ConcurrentDictionary<DiagnosticKey, DiagnosticItem> values)
        {
            GroupName = group;
            Real = new DoubleDiagnostic(group,values);
            Int = new IntegerDiagnostic(group, values);
            Str = new StringDiagnostic(group,values);
            Speed = new SpeedDiagnostic(Real);
        }

        public string GroupName { get; }
        public IDigitDiagnostic<double> Real { get; }
        public IDigitDiagnostic<int> Int { get; }
        public IStringDiagnostic Str { get; }
        public ISpeedDiagnostic Speed { get; }

        public void Dispose()
        {
            Real.Dispose();
            Int.Dispose();
            Str.Dispose();
            Speed.Dispose();
        }
    }
}