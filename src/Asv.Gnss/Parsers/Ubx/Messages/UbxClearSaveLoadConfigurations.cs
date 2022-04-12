namespace Asv.Gnss
{
    public class UbxClearConfigurations : UbxClearSaveLoadConfigurations
    {
        protected override ConfAction Action => ConfAction.Clear;
    }

    public class UbxSaveConfigurations : UbxClearSaveLoadConfigurations
    {
        protected override ConfAction Action => ConfAction.Save;
    }

    public class UbxLoadConfigurations : UbxClearSaveLoadConfigurations
    {
        protected override ConfAction Action => ConfAction.Load;
    }

    public abstract class UbxClearSaveLoadConfigurations : UbxMessageBase
    {
        public override byte Class => 0x06;
        public override byte SubClass => 0x09;

        public override int GetMaxByteSize()
        {
            return base.GetMaxByteSize() + 13;
        }

        protected enum ConfAction
        {
            Clear,
            Save,
            Load
        }

        protected abstract ConfAction Action { get; }

        protected override uint InternalSerialize(byte[] buffer, uint offset)
        {
            var byteIndex = offset;

            if (Action == ConfAction.Clear || Action == ConfAction.Save)
            {
                buffer[byteIndex++] = 0xF8;
                buffer[byteIndex++] = 0xF8;
            }
            else
            {
                buffer[byteIndex++] = 0x00;
                buffer[byteIndex++] = 0x00;
            }
            buffer[byteIndex++] = 0x00;
            buffer[byteIndex++] = 0x00;

            if (Action == ConfAction.Save)
            {
                buffer[byteIndex++] = 0xF8;
                buffer[byteIndex++] = 0xF8;
            }
            else
            {
                buffer[byteIndex++] = 0x00;
                buffer[byteIndex++] = 0x00;
            }
            buffer[byteIndex++] = 0x00;
            buffer[byteIndex++] = 0x00;

            if (Action == ConfAction.Load)
            {
                buffer[byteIndex++] = 0xF8;
                buffer[byteIndex++] = 0xF8;
            }
            else
            {
                buffer[byteIndex++] = 0x00;
                buffer[byteIndex++] = 0x00;
            }
            buffer[byteIndex++] = 0x00;
            buffer[byteIndex++] = 0x00;

            buffer[byteIndex++] = 0x01; // Battery backed RAM

            return byteIndex - offset;
        }
    }
}