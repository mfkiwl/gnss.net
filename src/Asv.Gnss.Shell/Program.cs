using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Asv.Tools;

namespace Asv.Gnss.Shell
{
    class Program
    {
        static void Main(string[] args)
        {
            // Diagnostic is a class for simple debug
            var diag = new Diagnostic();

            var connection = new GnssConnection("tcp://10.10.6.209:64100",
                // "tcp://127.0.0.1:9002",
                // for UDP - udp://127.0.0.1:1234?rhost=127.0.0.1&rport=1235
                // for COM - serial:/dev/ttyACM0?br=115200 or serial:COM4?br=115200
                diag,
                new RtcmV3Parser(diag).RegisterDefaultFrames(),
                new Nmea0183Parser(diag).RegisterDefaultFrames(),
                new ComNavBinaryParser(diag).RegisterDefaultFrames(),
                new RtcmV2Parser(diag).RegisterDefaultFrames(),
                new SbfBinaryParser(diag)
            );



            connection.OnMessage
                .Where(_ => (_ as RtcmV3MSM4) != null)
                .Cast<RtcmV3MSM4>()
                .Subscribe(_ => { /* do something with RTCM MSM4 */ });

            connection.OnMessage
                .Where(_ => _.ProtocolId == RtcmV3Parser.GnssProtocolId)
                .Cast<RtcmV3MessageBase>()
                .Where(_ => _.MessageId == RtcmV3Message1006.RtcmMessageId)
                .Cast<RtcmV3Message1006>()
                .Subscribe(_=> { /* do something with RTCM 1006 */ });

            while (true)
            {
                Console.Clear();
                diag.Print(Console.WriteLine);
                Thread.Sleep(3000);
            }
        }
    }
}
