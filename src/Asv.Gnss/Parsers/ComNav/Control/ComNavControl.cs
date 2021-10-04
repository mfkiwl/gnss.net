using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Asv.Gnss.Control
{
    public class ComNavControl : IComNavControl
    {
        private readonly IDataStream _strm;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IDisposable _outputSubscribe;

        public ComNavControl(IDataStream strm)
        {
            _strm = strm;
            _outputSubscribe = _strm.Subscribe(_=>OnAnswerByte(_));
        }

        private void OnAnswerByte(byte[] bytes)
        {
            _logger.Trace(Encoding.ASCII.GetString(bytes));
             
        }

        public Task Send(ComNavAsciiCommandBase command,CancellationToken cancel)
        {
            _logger.Info($"{_strm} => {command}");
           return SendCommand(command, cancel);
        }

        private async Task SendCommand(ComNavAsciiCommandBase command, CancellationToken cancel)
        {
            var buffer = new byte[command.GetMaxByteSize()];
            var writedBits = command.Serialize(buffer, 0);
            await _strm.Send(buffer, (int)(writedBits / 8), cancel);
            await Task.Delay(3000, cancel); // TODO: remove this and check the answer "OK!"
        }

        public void Dispose()
        {
            _outputSubscribe.Dispose();
        }

        
    }
}