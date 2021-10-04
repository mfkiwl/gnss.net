using System;
using System.Threading;
using System.Threading.Tasks;

namespace Asv.Gnss.Control
{
    public interface IComNavControl:IDisposable
    {
        Task Send(ComNavAsciiCommandBase command, CancellationToken cancel);
    }

    

    public static class ComNavControlHelper
    {

        public static Task SaveConfig(this IComNavControl src, CancellationToken cancel = default)
        {
            return src.Send(new ComNavSaveConfigCommand(), cancel);
        }

        public static Task UnlockoutAllSystem(this IComNavControl src, CancellationToken cancel = default)
        {
            return src.Send(new ComNavUnLockoutAllSystemCommand(), cancel);
        }

        public static Task SetLockoutSystem(this IComNavControl src, ComNavSatelliteSystemEnum system, CancellationToken cancel = default)
        {
            return src.Send(new ComNavSetLockoutSystemCommand
            {
                SatelliteSystem = system
            }, cancel);
        }
        public static Task SendDgpsStationId(this IComNavControl src, DgpsTxIdEnum type, byte id, CancellationToken cancel = default)
        {
            return src.Send(new ComNavDgpsTxIdCommand
            {
                Type = type,
                Id = id,
            }, cancel);
        }


        public static Task LogCommand(this IComNavControl src, ComNavMessageEnum message, string portName = default, ComNavFormat? format = default,
            ComNavTriggerEnum? trigger = default,
            uint? period = default,
            CancellationToken cancel = default)
        {
            return src.Send(new ComNavAsciiLogCommand
            {
                Type = message,
                Format = format,
                PortName = portName,
                Trigger = trigger,
                Period = period,
                
            }, cancel);
        }
        /// <summary>
        /// Configures the receiver to fix the height at the last calculated value if the number of
        /// satellites available is insufficient for a 3-D solution. This provides a 2-D solution.
        /// Height calculation resumes when the number of satellites available allows a 3-D solution
        /// </summary>
        public static Task FixAuto(this IComNavControl src, CancellationToken cancel = default)
        {
            return src.Send(new ComNavFixCommand
            {
                FixType = ComNavFixType.Auto,
            }, cancel);
        }
        /// <summary>
        /// Unfix. Clears any previous FIX commands
        /// </summary>
        /// <param name="src"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public static Task SendFixNone(this IComNavControl src, CancellationToken cancel = default)
        {
            return src.Send(new ComNavFixCommand
            {
                FixType = ComNavFixType.None,
            }, cancel);
        }
        /// <summary>
        /// Configures the receiver in 2-D mode with its height constrained to a given value. This
        /// command is used mainly in marine applications where height in relation to mean sea
        /// level may be considered to be approximately constant. The height entered using this
        /// command is referenced to the mean sea level, see the BESTPOS log on page 497
        /// (is in metres). The receiver is capable of receiving and applying differential
        /// corrections from a base station while fix height is in effect. The fix height command
        /// overrides any previous FIX HEIGHT or FIX POSITION command.
        /// Note: This command only affects pseudorange corrections and solutions.
        /// </summary>
        public static Task SendFixHeight(this IComNavControl src, double altitude, CancellationToken cancel = default)
        {
            return src.Send(new ComNavFixCommand
            {
                FixType = ComNavFixType.Height,
                Alt = altitude,
            }, cancel);
        }
        /// <summary>
        /// Configures the receiver with its position fixed. This command is used when it is
        /// necessary to generate differential corrections.
        /// For both pseudorange and differential corrections, this command must be properly
        /// initialized before the receiver can operate as a GNSS base station. Once initialized,
        /// the receiver computes differential corrections for each satellite being tracked. The
        /// computed differential corrections can then be output to rover stations using the
        /// RTCMV3 differential corrections data log format.S
        /// The values entered into the fix position command should reflect the precise position
        /// of the base station antenna phase center. Any errors in the fix position coordinates
        /// directly bias the corrections calculated by the base receiver
        /// 
        /// The receiver performs all internal computations based on WGS84 and the DATUM
        /// command (see page 131) is defaulted as such. The datum in which you choose to
        /// operate (by changing the DATUM command (see page 131)) is internally converted
        /// to and from WGS84. Therefore, all differential corrections are based on WGS84,
        /// regardless of your operating datum.
        ///
        /// The FIX POSITION command overrides any previous FIX HEIGHT or FIX
        /// POSITION command settings.
        /// </summary>
        public static Task FixPosition(this IComNavControl src,double latitude, double longitude, double altitude, CancellationToken cancel = default)
        {
            return src.Send(new ComNavFixCommand
            {
                FixType = ComNavFixType.Position,
                Lat = latitude,
                Lon = longitude,
                Alt = altitude,
            }, cancel);
        }
    }
}