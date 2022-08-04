﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Asv.Tools;
using NLog;

namespace Asv.Gnss
{
    public class GnssConnection : DisposableOnceWithCancel, IGnssConnection
    {
        private readonly IGnssParser[] _parsers;
        private readonly object _sync = new object();
        private readonly Subject<GnssParserException> _onErrorSubject = new Subject<GnssParserException>();
        private readonly Subject<GnssMessageBase> _onMessageSubject = new Subject<GnssMessageBase>();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDiagnosticSource _diag;
        private readonly RateIndicator _rxInd;

        public GnssConnection(string connectionString, IDiagnosticSource diagSource, params IGnssParser[] parsers)
        {
            DataStream = ConnectionStringConvert(connectionString);
            _parsers = parsers;
            foreach (var parser in parsers)
            {
                parser.OnError.Subscribe(_onErrorSubject, DisposeCancel);
                parser.OnMessage.Subscribe(_onMessageSubject, DisposeCancel);
            }
            DataStream.SelectMany(_ => _).Subscribe(OnByteRecv, DisposeCancel);
            Disposable.Add(_onErrorSubject);
            Disposable.Add(_onMessageSubject);

            Logger.Info($"GNSS connection string: {connectionString}");

            _diag = diagSource;
            Disposable.Add(_diag);

            // diagnostic
            _diag.Str["conn"] = connectionString;
            _diag.Rate["rx", "# ##0 b/s"].Increment(0);
            foreach (var parser in parsers)
            {
                _diag.Rate[parser.ProtocolId, "# ##0 pkt/s"].Increment(0);
            }
        }

        public GnssConnection(string connectionString, IDiagnostic diag, params IGnssParser[] parsers) 
        {
            DataStream = ConnectionStringConvert(connectionString);
            _parsers = parsers;
            foreach (var parser in parsers)
            {
                parser.OnError.Subscribe(_onErrorSubject, DisposeCancel);
                parser.OnMessage.Subscribe(_onMessageSubject, DisposeCancel);
            }
            DataStream.SelectMany(_ => _).Subscribe(OnByteRecv, DisposeCancel);

            
            Logger.Info($"GNSS connection string: {connectionString}");

            _diag = diag[DataStream.Name];
            // diagnostic
            _diag.Str["conn"] = connectionString;
            _diag.Rate["rx", "# ##0 b/s"].Increment(0);
            foreach (var parser in parsers)
            {
                _diag.Rate[parser.ProtocolId, "# ##0 pkt/s"].Increment(0);
            }
            
        }

        private static IDataStream ConnectionStringConvert(string connString)
        {
            var p = PortFactory.Create(connString);
            p.Enable();
            return p;
        }

       

        private void OnByteRecv(byte data)
        {
            lock (_sync)
            {
                _diag.Rate["rx"].Increment(1);
                try
                {
                    var packetFound = false;
                    for (var index = 0; index < _parsers.Length; index++)
                    {
                        var parser = _parsers[index];
                        if (parser.Read(data))
                        {
                            _diag.Rate[parser.ProtocolId].Increment(1);
                            packetFound = true;
                            break;
                        }
                    }

                    if (packetFound)
                    {
                        foreach (var parser in _parsers)
                        {
                            parser.Reset();
                        }
                    }
                }
                catch (Exception e)
                {
                    _diag.Int["parser err"]++;
                    Debug.Assert(false);
                }
                
            }
        }

        public IDataStream DataStream { get; }

        public IEnumerable<IGnssParser> Parsers => _parsers;
        public IObservable<GnssParserException> OnError => _onErrorSubject;
        public IObservable<GnssMessageBase> OnMessage => _onMessageSubject;
        public Task Send(GnssMessageBase msg, CancellationToken cancel)
        {
            var buffer = new byte[msg.GetMaxByteSize()];
            var sizeInBits = msg.Serialize(buffer, 0);
            var additionalByte = 0;
            if (sizeInBits % 8 != 0)
            {
                additionalByte = 1;
                Debug.Fail("Not full byte!");
            }
            return DataStream.Send(buffer, (int) (sizeInBits/8) + additionalByte, cancel);
        }

        protected override void InternalDisposeOnce()
        {
            (DataStream as IDisposable)?.Dispose();
            base.InternalDisposeOnce();
        }
    }
}