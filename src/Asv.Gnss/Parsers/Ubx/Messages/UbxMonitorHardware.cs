using System;

namespace Asv.Gnss
{
    public class UbxMonitorHardware : UbxMessageBase
    {
        public int PinSel { get; set; }
        public int PinBank { get; set; }
        public int PinDir { get; set; }
        public int PinVal { get; set; }
        public ushort Noise { get; set; }
        public ushort AgcCnt { get; set; }
        public AntennaSupervisorStateMachineStatus AStatus { get; set; }
        public AntennaPowerStatus APower { get; set; }
        public bool RtcCalib { get; set; }
        public bool SafeBoot { get; set; }
        public bool XTalAbsent { get; set; }
        public int UsedMask { get; set; }
        public byte[] VP { get; set; }
        public byte JamInd { get; set; }
        public int PinIrq { get; set; }
        public int PullH { get; set; }
        public int PullL { get; set; }

        public JammingStateEnum JammingState { get; set; }
        public double CwJammingIndicator { get; set; }
        public double AgcMonitor { get; set; }

        public override byte Class => 0x0A;
        public override byte SubClass => 0x09;

        public enum AntennaSupervisorStateMachineStatus
        {
            Init = 0,
            DontKnow = 1,
            Ok = 2,
            Short = 3,
            Open = 4
        }

        public enum AntennaPowerStatus
        {
            Off = 0,
            On = 1,
            DontKnow = 2
        }

        public enum JammingStateEnum
        {
            Unknown = 0,
            Ok = 1,
            Warning = 2,
            Critical = 3
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var byteIndex = (offsetBits + base.Deserialize(buffer, offsetBits)) / 8;

            PinSel = BitConverter.ToInt32(buffer, (int)byteIndex); byteIndex += 4;
            PinBank = BitConverter.ToInt32(buffer, (int)byteIndex); byteIndex += 4;
            PinDir = BitConverter.ToInt32(buffer, (int)byteIndex); byteIndex += 4;
            PinVal = BitConverter.ToInt32(buffer, (int)byteIndex); byteIndex += 4;
            Noise = BitConverter.ToUInt16(buffer, (int)byteIndex); byteIndex += 2;
            AgcCnt = BitConverter.ToUInt16(buffer, (int)byteIndex); byteIndex += 2;
            AStatus = (AntennaSupervisorStateMachineStatus)buffer[byteIndex++];
            APower = (AntennaPowerStatus)buffer[byteIndex++];
            RtcCalib = (buffer[byteIndex] & 0x1) != 0;
            SafeBoot = (buffer[byteIndex] & 0x2) != 0;
            JammingState = (JammingStateEnum)((buffer[byteIndex] & 0x0C) >> 2);
            XTalAbsent = (buffer[byteIndex] & 0x10) != 0; byteIndex += 2;
            UsedMask = BitConverter.ToInt32(buffer, (int)byteIndex); byteIndex += 4;
            VP = new byte[17];
            for (var i = byteIndex; i < 17 + byteIndex; i++)
            {
                VP[i] = buffer[i];
            }
            byteIndex += 17;
            JamInd = buffer[byteIndex]; byteIndex += 3;
            PinIrq = BitConverter.ToInt32(buffer, (int)byteIndex); byteIndex += 4;
            PullH = BitConverter.ToInt32(buffer, (int)byteIndex); byteIndex += 4;
            PullL = BitConverter.ToInt32(buffer, (int)byteIndex); byteIndex += 4;

            AgcMonitor = AgcCnt / 8191.0;
            CwJammingIndicator = JamInd / 255.0;
            
            return byteIndex * 8 - offsetBits;
        }
    }
}