using System;
using Asv.Tools;

namespace Asv.Gnss
{
    public class AsvMessageGbasVdbSendV2 : AsvMessageBase
    {
        public override ushort MessageId => 0x0102;

        /// <summary>
        /// Номер слота текущего сообщения (A - H)
        /// </summary>
        public AsvGbasSlotMsg Slot { get; set; }
        /// <summary>
        /// Тип сообщения GBAS в пакете
        /// </summary>
        public byte GbasMessageId { get; set; }
        /// <summary>
        /// Активные слоты GBAS в данный момент
        /// </summary>
        public AsvGbasSlot ActiveSlots { get; set; }
        /// <summary>
        /// Время жизни сообщения в 500 мс ( 1 frame) отрезках.  
        /// </summary>
        public byte LifeTime { get; set; }

        
        /// <summary>
        /// Размер значащих битов в последнем байте сообщения (0 - 7)
        /// </summary>
        public byte LastByteOffset { get; set; }
        /// <summary>
        /// Зарезервированы для дальнейшего использования.
        /// </summary>
        public byte ReservedFlgas { get; set; }
        /// <summary>
        /// Признака окончания передачи всего фрейма.
        /// По нему устройство понимает, что весь фрейм передан и можно начинать передавать новый фрейм в эфир.
        /// </summary>
        public bool IsLastSlotInFrame { get; set; }
        /// <summary>
        /// Данные для отправки по VDB
        /// </summary>
        public byte[] Data { get; set; }

        protected override int InternalSerialize(byte[] buffer, int offsetInBytes)
        {
            var offsetInBits = (uint)offsetInBytes * 8;
            BitHelper.SetBitU(buffer, offsetInBits, 3, (uint)Slot); offsetInBits += 3;
            BitHelper.SetBitU(buffer, offsetInBits, 5, GbasMessageId); offsetInBits += 5;
            BitHelper.SetBitU(buffer, offsetInBits, 8, (byte)ActiveSlots); offsetInBits += 8;
            BitHelper.SetBitU(buffer, offsetInBits, 8, LifeTime); offsetInBits += 8;
            BitHelper.SetBitU(buffer, offsetInBits, 3, LastByteOffset); offsetInBits += 3;
            BitHelper.SetBitU(buffer, offsetInBits, 1, IsLastSlotInFrame ? 1:0); offsetInBits += 1;
            BitHelper.SetBitU(buffer, offsetInBits, 4, ReservedFlgas); offsetInBits += 4;
            var dataOffset = offsetInBits / 8;
            Array.Copy(Data,0,buffer, dataOffset, Data.Length);
            return Data.Length + 4;
        }

        

        protected override int InternalDeserialize(byte[] buffer, int offsetInBytes, int length)
        {
            var offsetInBits = (uint)offsetInBytes * 8;
            Slot = (AsvGbasSlotMsg)BitHelper.GetBitU(buffer, offsetInBits, 3); offsetInBits += 3;
            GbasMessageId = (byte)BitHelper.GetBitU(buffer, offsetInBits, 5); offsetInBits += 5;
            ActiveSlots = (AsvGbasSlot)BitHelper.GetBitU(buffer, offsetInBits, 8); offsetInBits += 8;
            LifeTime = (byte)BitHelper.GetBitU(buffer, offsetInBits, 8); offsetInBits += 8;
            LastByteOffset = (byte)BitHelper.GetBitU(buffer, offsetInBits, 3); offsetInBits += 3;
            IsLastSlotInFrame = BitHelper.GetBitU(buffer, offsetInBits, 1) != 0; offsetInBits += 1;
            ReservedFlgas = (byte)BitHelper.GetBitU(buffer, offsetInBits, 4); offsetInBits += 4;
            var dataOffset = offsetInBits / 8;
            Data = new byte[length - 4];
            Array.Copy(buffer, dataOffset, Data, 0, Data.Length);
            return length;
        }
    }


    public enum AsvGbasSlotMsg : byte
    {
        SlotA = 0,
        SlotB = 1,
        SlotC = 2,
        SlotD = 3,
        SlotE = 4,
        SlotF = 5,
        SlotG = 6,
        SlotH = 7,
    }
}