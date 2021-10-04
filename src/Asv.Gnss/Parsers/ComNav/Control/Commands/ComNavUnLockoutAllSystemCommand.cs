namespace Asv.Gnss.Control
{
    /// Reinstates all previously locked out satellites
    /// This command allows all satellites or systems which have been previously locked out
    /// (LOCKOUT command on page 242 or LOCKOUTSYSTEM command on page 243) to be reinstated in the solution computation.
    /// </summary>
    public class ComNavUnLockoutAllSystemCommand : ComNavAsciiCommandBase
    {
        public ComNavSatelliteSystemEnum SatelliteSystem { get; }

        protected override string SerializeToAsciiString()
        {
            return "UNLOCKOUTALL";

        }
    }
}