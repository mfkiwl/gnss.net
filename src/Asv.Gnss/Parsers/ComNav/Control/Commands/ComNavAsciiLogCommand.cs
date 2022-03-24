using Asv.Tools;
using System;
using System.Text;

namespace Asv.Gnss.Control
{
    public enum ComNavFormat
    {
        Binary,
        Ascii
    }

    public class ComNavAsciiLogCommand: ComNavAsciiCommandBase
    {
        public ComNavMessageEnum Type { get; set; }
        public ComNavTriggerEnum? Trigger { get; set; }
        public uint? Period { get; set; }
        public uint? OffsetTime { get; set; }
        public ComNavFormat? Format { get; set; }
        public string PortName { get; set; }

        protected override string SerializeToAsciiString()
        {
            var sb = new StringBuilder();
            sb.Append("LOG ");
            if (PortName.IsNullOrWhiteSpace() == false)
            {
                sb.Append(PortName);
                sb.Append(" ");
            }
            sb.Append(Type.GetMessageName());
            switch (Format)
            {
                case ComNavFormat.Binary:
                    sb.Append("B");
                    break;
                case ComNavFormat.Ascii:
                    sb.Append("A");
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (Trigger.HasValue)
            {
                sb.Append(" ");
                sb.Append(Trigger.Value.GetTriggerName());
            }
            if (Period.HasValue)
            {
                sb.Append(" ");
                sb.Append(Period.Value);
            }
            if (OffsetTime.HasValue)
            {
                sb.Append(" ");
                sb.Append(OffsetTime.Value);
            }
            return sb.ToString();
        }
    }
}