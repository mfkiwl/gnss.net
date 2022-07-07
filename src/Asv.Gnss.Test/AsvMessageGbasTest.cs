using System;
using DeepEqual.Syntax;
using Xunit;

namespace Asv.Gnss.Test
{
    public class AsvMessageGbasTest
    {
        [Fact]
        public void AsvMessageGbasVdbSendV2()
        {
            var r = new Random();
            var msg = new AsvMessageGbasVdbSendV2
            {
                Tag = null,
                Sequence = (ushort)r.Next(0,ushort.MaxValue),
                TargetId = (byte)r.Next(0,byte.MaxValue),
                SenderId = (byte)r.Next(0, byte.MaxValue),
                Slot = (AsvGbasSlotMsg)r.Next(0,Enum.GetValues(typeof(AsvGbasSlotMsg)).Length - 1),
                GbasMessageId = (byte)r.Next(0, 32),
                ActiveSlots = (AsvGbasSlot)r.Next(0, byte.MaxValue),
                LifeTime = (byte)r.Next(0, byte.MaxValue),
                LastByteOffset = (byte)r.Next(0, 7),
                ReservedFlgas = 0,
                IsLastSlotInFrame = r.Next(0, byte.MaxValue)%2 == 0,
                Data = new byte[r.Next(0, 1000)],

            };
            Assert.Equal(msg.ProtocolId, AsvParser.GnssProtocolId);
            r.NextBytes(msg.Data);
            var data = new byte[AsvParser.MaxMessageSize];
            var size = msg.Serialize(data, 0);

            var result = new AsvMessageGbasVdbSendV2();
            var resultSize = result.Deserialize(data, 0);

            msg.WithDeepEqual(result).Assert();
            Assert.Equal(size, resultSize);

        }
    }
}
