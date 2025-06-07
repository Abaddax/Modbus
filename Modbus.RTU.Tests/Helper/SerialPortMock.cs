u﻿sing Abaddax.Utilities.IO;
using Abaddax.Utilities.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Abaddax.Modbus.RTU.Tests.Helper
{
    internal sealed class SerialPortMock : IDisposable
    {
        private static readonly ConcurrentDictionary<SerialPort, SerialPortMock> _mocks = new();

        public const string SERVER_PORT = "SERVER";
        public const string CLIENT_PORT = "CLIENT";

        private readonly SerialPort _serialPort;
        private readonly Queue<byte> _receiveBuffer = new();
        private ListenStream? _stream = null;
        private bool _disposedValue;

        private void RaiseEvent()
        {
            var spType = typeof(SerialPort);
            var evnt = spType.GetField("_dataReceived", BindingFlags.Instance | BindingFlags.NonPublic);
            var evntValue = evnt?.GetValue(_serialPort);
            var eventDelegate = evntValue as SerialDataReceivedEventHandler;
            if (eventDelegate == null)
                Console.WriteLine("SerialDataReceivedEventHandler is null!");

            var evntArgsType = typeof(SerialDataReceivedEventArgs);
            var ctor = evntArgsType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, [typeof(SerialData)]);
            var args = ctor?.Invoke([SerialData.Chars]) as SerialDataReceivedEventArgs;
            if (args == null)
                Console.WriteLine("SerialDataReceivedEventArgs are null!");

            eventDelegate?.Invoke(_serialPort, args);
        }
        private async Task OnMessageReceivedHandler(Exception? readException, ReadOnlyMemory<byte> message, CancellationToken token)
        {
            if (readException != null)
            {
                Console.WriteLine(readException);
                Close();
                return;
            }
            var span = message.Span;
            lock (_receiveBuffer)
            {
                foreach (var b in span)
                {
                    _receiveBuffer.Enqueue(b);
                }
            }
            RaiseEvent();
        }

        private SerialPortMock(SerialPort serialPort)
        {
            _serialPort = serialPort ?? throw new ArgumentNullException(nameof(serialPort));
        }
        public static SerialPortMock GetMock(SerialPort serialPort)
        {
            if (_mocks.TryGetValue(serialPort, out var mock))
                return mock;
            return _mocks.AddOrUpdate(serialPort,
                (key) =>
                {
                    return new SerialPortMock(serialPort);
                },
                (key, oldValue) =>
                {
                    oldValue?.Dispose();
                    return new SerialPortMock(serialPort);
                });
        }

        #region SerialPort
        public new bool IsOpen => _stream?.Listening ?? false;
        public new int BytesToRead => _receiveBuffer.Count;
        public new int BytesToWrite => 0;
        public new void Open()
        {
            const int port = 25000;
            using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                _stream?.Dispose();
                switch (_serialPort.PortName)
                {
                    case SERVER_PORT:
                        {
                            //Open Listener and accept client
                            using var listener = new TcpListener(IPAddress.Loopback, port);
                            listener.Start();
                            while (!listener.Pending())
                            {
                                tokenSource.Token.ThrowIfCancellationRequested();
                                Thread.Sleep(10);
                            }
                            var client = listener.AcceptTcpClientAsync(tokenSource.Token).AsTask().AwaitSync();
                            _stream = new ListenStream(client.GetStream());
                        }
                        break;
                    case CLIENT_PORT:
                        {
                            while (true)
                            {
                                try
                                {
                                    var client = new TcpClient();
                                    client.ConnectAsync("127.0.0.1", port).AwaitSync();
                                    _stream = new ListenStream(client.GetStream());
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    if (tokenSource.Token.IsCancellationRequested)
                                        throw;
                                    Thread.Sleep(10);
                                }
                            }
                        }
                        break;
                    default:
                        throw new Exception($"Unknown PortName {_serialPort.PortName}");
                }

                _stream.StartListening(OnMessageReceivedHandler);
            }
        }
        public new int Read(byte[] buffer, int offset, int count)
        {
            lock (_receiveBuffer)
            {
                int ret = 0;
                for (int i = 0; i < count; i++)
                {
                    if (!_receiveBuffer.TryDequeue(out var b))
                        return ret;
                    buffer[i + offset] = b;
                    ret++;
                }
                return ret;
            }
        }
        public new void Write(byte[] buffer, int offset, int count)
        {
            _stream?.Write(buffer, offset, count);
        }
        public new void DiscardInBuffer()
        {
            lock (_receiveBuffer)
            {
                _receiveBuffer.Clear();
            }
        }
        public new void DiscardOutBuffer()
        {
            return;
        }
        public new void Close()
        {
            _stream?.Close();
        }
        #endregion

        #region IDisposable
        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _stream?.Dispose();
                _disposedValue = true;
                //Remove from list a bit delayed to avoid accidential recreation by patches
                _ = Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    _mocks.TryRemove(_serialPort, out _);
                });
            }
        }
        ~SerialPortMock()
        {
            Dispose(disposing: false);
        }
        public new void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
