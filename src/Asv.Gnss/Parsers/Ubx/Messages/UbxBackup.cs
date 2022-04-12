using System;

namespace Asv.Gnss
{
    public abstract class UbxBackupInFlashBase : UbxMessageBase
    {
        public override byte Class => 0x09;
        public override byte SubClass => 0x14;

        public override int GetMaxByteSize()
        {
            return base.GetMaxByteSize() + 4;
        }

        protected abstract byte Command { get; }

        protected override uint InternalSerialize(byte[] buffer, uint offsetBytes)
        {
            var byteIndex = offsetBytes;

            buffer[byteIndex++] = Command;
            for (var i = 0; i < 3; i++)
            {
                buffer[byteIndex++] = 0;
            }

            return byteIndex - offsetBytes;
        }
    }

    public class UbxCreateBackupInFlash : UbxBackupInFlashBase
    {
        protected override byte Command => 0;
    }

    public class UbxClearBackupInFlash : UbxBackupInFlashBase
    {
        protected override byte Command => 1;
    }

    public class UbxBackupRestoreStatus : UbxMessageBase
    {
        public override byte Class => 0x09;
        public override byte SubClass => 0x14;

        public override int GetMaxByteSize()
        {
            return base.GetMaxByteSize() + 8;
        }

        public BackupCreationEnum? BackupCreation { get; private set; }

        public RestoredFromBackupEnum? RestoredFromBackup { get; private set; }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var byteIndex = (offsetBits + base.Deserialize(buffer, offsetBits)) / 8;

            var command = buffer[byteIndex]; byteIndex += 4;
            switch (command)
            {
                case 2:
                    BackupCreation = (BackupCreationEnum)buffer[byteIndex];
                    break;
                case 3:
                    RestoredFromBackup = (RestoredFromBackupEnum)buffer[byteIndex];
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            byteIndex += 4;
            return byteIndex * 8 - offsetBits;
        }

    }

    public enum BackupCreationEnum
    {
        NotAcknowledged = 0,
        Acknowledged = 1
    }

    public enum RestoredFromBackupEnum
    {
        Unknown = 0,
        FailedRestoring = 1,
        RestoredOk = 2,
        NoBackup = 3
    }
}
