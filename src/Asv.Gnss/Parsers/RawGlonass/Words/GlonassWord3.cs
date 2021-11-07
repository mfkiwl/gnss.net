namespace Asv.Gnss
{
    public class GlonassWord3 : GlonassWordBase
    {
        public override byte WordId => 3;

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);
            var bitIndex = 8U;

            P3 = (byte)GlonassRawHelper.GetBitU(data, bitIndex, 1); bitIndex += 1;
            RelativeFreqBias = GlonassRawHelper.GetBitG(data, bitIndex, 11) * RtcmV3Helper.P2_40; bitIndex += 11 + 1;
            P = (byte)GlonassRawHelper.GetBitU(data, bitIndex, 2); bitIndex += 2;
            Ln = (byte)GlonassRawHelper.GetBitU(data, bitIndex, 1); bitIndex += 1;

            VelocityZ = GlonassRawHelper.GetBitU(data, bitIndex, 24) * GlonassRawHelper.P2_20 * 1E3; bitIndex += 24;
            AccelerationZ = GlonassRawHelper.GetBitU(data, bitIndex, 5) * GlonassRawHelper.P2_30 * 1E3; bitIndex += 5;
            PositionZ = GlonassRawHelper.GetBitU(data, bitIndex, 27) * GlonassRawHelper.P2_11 * 1E3; bitIndex += 27;
        }

        public byte P3 { get; set; }

        /// <summary>
        /// relative freq bias (γn(tb)). Относительное отклонение прогнозируемого значения несущей частоты излучаемого сигнала n-го
        /// спутника от номинального значения на момент времени tb
        /// </summary>
        public double RelativeFreqBias { get; set; }

        public byte P { get; set; }

        public byte Ln { get; set; }

        /// <summary>
        /// satellite position (ecef) (m). Координаты n-го спутника в системе координат ПЗ-90 на момент времени tb
        /// </summary>
        public double PositionZ { get; set; }

        /// <summary>
        /// satellite velocity (ecef) (m/s). составляющие вектора скорости n-го спутника в системе координат ПЗ-90 на момент
        /// времени tb
        /// </summary>
        public double VelocityZ { get; set; }

        /// <summary>
        /// satellite acceleration (ecef) (m/s^2). Составляющие ускорения n-го спутника в системе координат ПЗ-90 на момент времени tb,
        /// обусловленные действием луны и солнца
        /// </summary>
        public double AccelerationZ { get; set; }

        
    }
}