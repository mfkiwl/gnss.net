using System;

namespace Asv.Gnss
{
    public class DiagnosticKey : IEquatable<DiagnosticKey>
    {
        public DiagnosticKey(string module, string param)
        {
            Group = module ?? throw new ArgumentNullException(nameof(module));
            Param = param ?? throw new ArgumentNullException(nameof(param));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DiagnosticKey)obj);
        }

        public bool Equals(DiagnosticKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Group, other.Group) && string.Equals(Param, other.Param);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Group?.GetHashCode() ?? 0) * 397) ^ (Param?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(DiagnosticKey left, DiagnosticKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DiagnosticKey left, DiagnosticKey right)
        {
            return !Equals(left, right);
        }

        public string Group { get; }
        public string Param { get; }
        public override string ToString()
        {
            return $"{Group}.{Param}";
        }
    }
}