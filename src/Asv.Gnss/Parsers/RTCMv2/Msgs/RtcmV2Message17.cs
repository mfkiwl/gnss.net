using System;

namespace Asv.Gnss
{
    public class RtcmV2Message17 : RtcmV2MessageBase
    {
        private int adjgpsweek(int week)
        {
            var now = DateTime.UtcNow;
            var w = 0;
            double sec = 0;
            RtcmV3Helper.GetFromTime(RtcmV3Helper.Utc2Gps(now), ref w, ref sec);
            if (w < 1560) w = 1560; /* use 2009/12/1 if time is earlier than 2009/12/1 */
            return week + (w - week + 1) / 1024 * 1024;
        }

        public const int RtcmMessageId = 17;

        public override ushort MessageId => RtcmMessageId;

        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            var week = RtcmV3Helper.GetBitU(buffer, bitIndex, 10); bitIndex += 10;
            Idot = RtcmV3Helper.GetBits(buffer, bitIndex, 14) * RtcmV3Helper.P2_43 * RtcmV3Helper.SC2RAD; bitIndex += 14;
            Iode = RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 8;
            var toc = RtcmV3Helper.GetBitU(buffer, bitIndex, 16) * 16.0; bitIndex += 16;
            AF1 = RtcmV3Helper.GetBits(buffer, bitIndex, 16) * RtcmV3Helper.P2_43; bitIndex += 16;
            AF2 = RtcmV3Helper.GetBits(buffer, bitIndex, 8) * RtcmV3Helper.P2_55; bitIndex += 8;
            Crs = RtcmV3Helper.GetBits(buffer, bitIndex, 16) * RtcmV3Helper.P2_5; bitIndex += 16;
            DeltaN = RtcmV3Helper.GetBits(buffer, bitIndex, 16) * RtcmV3Helper.P2_43 * RtcmV3Helper.SC2RAD; bitIndex += 16;
            Cuc = (short)RtcmV3Helper.GetBits(buffer, bitIndex, 16) * RtcmV3Helper.P2_29; bitIndex += 16;
            E = RtcmV3Helper.GetBitU(buffer, bitIndex, 32) * RtcmV3Helper.P2_33; bitIndex += 32;
            Cus = (short)RtcmV3Helper.GetBits(buffer, bitIndex, 16); bitIndex += 16;
            var sqrtA = RtcmV3Helper.GetBitU(buffer, bitIndex, 32) * RtcmV3Helper.P2_19; bitIndex += 32;
            Toes = (ushort)RtcmV3Helper.GetBitU(buffer, bitIndex, 16); bitIndex += 16;
            Omega0 = RtcmV3Helper.GetBits(buffer, bitIndex, 32) * RtcmV3Helper.P2_31 * RtcmV3Helper.SC2RAD; bitIndex += 32;
            Cic = RtcmV3Helper.GetBits(buffer, bitIndex, 16) * RtcmV3Helper.P2_29; bitIndex += 16;
            I0 = RtcmV3Helper.GetBits(buffer, bitIndex, 32) * RtcmV3Helper.P2_31 * RtcmV3Helper.SC2RAD; bitIndex += 32;
            Cis = RtcmV3Helper.GetBits(buffer, bitIndex, 16) * RtcmV3Helper.P2_29; bitIndex += 16;
            Omega = RtcmV3Helper.GetBits(buffer, bitIndex, 32) * RtcmV3Helper.P2_31 * RtcmV3Helper.SC2RAD; bitIndex += 32;
            Crc = RtcmV3Helper.GetBits(buffer, bitIndex, 16) * RtcmV3Helper.P2_5; bitIndex += 16;
            OmegaDot = RtcmV3Helper.GetBits(buffer, bitIndex, 24) * RtcmV3Helper.P2_43 * RtcmV3Helper.SC2RAD; bitIndex += 24;
            M0 = RtcmV3Helper.GetBits(buffer, bitIndex, 32) * RtcmV3Helper.P2_31 * RtcmV3Helper.SC2RAD; bitIndex += 32;
            Iodc = (ushort)RtcmV3Helper.GetBitU(buffer, bitIndex, 10); bitIndex += 10;
            AF0 = RtcmV3Helper.GetBits(buffer, bitIndex, 22) * RtcmV3Helper.P2_31; bitIndex += 22;
            var prn = RtcmV3Helper.GetBitU(buffer, bitIndex, 5); bitIndex += 5 + 3;
            Tgd = RtcmV3Helper.GetBits(buffer, bitIndex, 8) * RtcmV3Helper.P2_31; bitIndex += 8;
            CodeOnL2 = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 2); bitIndex += 2;
            SVAccuracy = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 4); bitIndex += 4;
            SVHealth = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 6); bitIndex += 6;
            L2PDataFlag = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 1); bitIndex += 1;

            if (prn == 0) prn = 32;
            Satellite = RtcmV3Helper.satno(NavigationSystemEnum.SYS_GPS, (int)prn);
            
            WeekNumber = adjgpsweek((int)week);
            Toe = RtcmV3Helper.GetFromGps(WeekNumber, Toes);
            Toc = RtcmV3Helper.GetFromGps(WeekNumber, toc);
            A = sqrtA * sqrtA;

            return bitIndex - offsetBits;
        }

        public int Satellite { get; set; }

        public int WeekNumber { get; set; }

        /// <summary>
        /// Скорость изменения угла наклонения
        /// </summary>
        public double Idot { get; set; }

        /// <summary>
        /// Идентификатор набора данных (эфемериды)
        /// </summary>
        public uint Iode { get; set; }

        public DateTime Toc { get; set; }

        public double AF1 { get; set; }

        public double AF2 { get; set; }

        /// <summary>
        /// Амплитуда синусной гармонической поправки к радиусу орбиты
        /// </summary>
        public double Crs { get; set; }

        /// <summary>
        /// Отличие среднего движения от расчетного значения.
        /// </summary>
        public double DeltaN { get; set; }

        /// <summary>
        /// Амплитуда косинусной гармонической поправки к аргументу широты
        /// </summary>
        public double Cuc { get; set; }

        /// <summary>
        /// Эксцентриситет
        /// </summary>
        public double E { get; set; }

        /// <summary>
        /// Амплитуда синусной гармонической поправки к аргументу широты
        /// </summary>
        public short Cus { get; set; }

        /// <summary>
        /// Большая полуось
        /// </summary>
        public double A { get; set; }

        /// <summary>
        /// Опорное время привязки (эфемериды)
        /// </summary>
        public ushort Toes { get; set; }

        /// <summary>
        /// Опорное время привязки (эфемериды)
        /// </summary>
        public DateTime Toe { get; set; }

        /// <summary>
        /// Долгота восходящего узла орбитальной плоскости на недельную эпоху
        /// </summary>
        public double Omega0 { get; set; }

        /// <summary>
        /// Амплитуда косинусной гармонической поправки к углу наклонения
        /// </summary>
        public double Cic { get; set; }

        /// <summary>
        /// Угол наклонения на время привязки
        /// </summary>
        public double I0 { get; set; }

        /// <summary>
        /// Амплитуда синусной гармонической поправки к углу наклонения
        /// </summary>
        public double Cis { get; set; }

        /// <summary>
        /// Аргумент перигея
        /// </summary>
        public double Omega { get; set; }

        /// <summary>
        /// Амплитуда косинусной гармонической поправки к радиусу орбиты
        /// </summary>
        public double Crc { get; set; }

        /// <summary>
        /// Скорость изменения прямого восхождения
        /// </summary>
        public double OmegaDot { get; set; }

        /// <summary>
        /// Средняя аномалия на время привязки.
        /// </summary>
        public double M0 { get; set; }

        /// <summary>
        /// Идентификатор набора параметров времени
        /// </summary>
        public ushort Iodc { get; set; }

        /// <summary>
        /// Временные параметры альманаха
        /// </summary>
        public double AF0 { get; set; }

        /// <summary>
        /// Оценка групповой дифференциальной задержки
        /// </summary>
        public double Tgd { get; set; }

        public byte CodeOnL2 { get; set; }

        public byte SVAccuracy { get; set; }

        public byte SVHealth { get; set; }

        public byte L2PDataFlag { get; set; }
        
    }
}