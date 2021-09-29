namespace Asv.Gnss.Control
{
    /// <summary>
    /// This command saves the user’s present configuration, including the current log settings (type, whether output testing data, etc.), FIX settings, baud rate, and so on, refer to Table 14.
    /// </summary>
    public class ComNavSaveConfigCommand : ComNavAsciiCommandBase
    {
        protected override string SerializeToAsciiString()
        {
            return "SAVECONFIG";
        }
    }
}