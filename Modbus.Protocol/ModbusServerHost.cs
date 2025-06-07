using Abaddax.Modbus.Protocol.Contracts;
using Abaddax.Utilities.Threading.Tasks;

namespace Abaddax.Modbus.Protocol
{
    public abstract class ModbusServerHost<TModbusProtocol> : IDisposable where TModbusProtocol : IModbusProtocol
    {
        private readonly List<ModbusServer<TModbusProtocol>> _servers = new();
        private CancellationTokenSource? _tokenSource;
        private bool disposedValue;

        protected CancellationToken CancellationToken => _tokenSource?.Token ?? new CancellationToken(true);
        public int MaxServerConnections { get; init; } = -1;
        public IEnumerable<ModbusServer<TModbusProtocol>> Connections
        {
            get
            {
                RemoveDisconnected();
                return _servers.ToArray();
            }
        }

        protected void AddConnection(ModbusServer<TModbusProtocol> server, CancellationToken token)
        {
            lock (_servers)
            {
                RemoveDisconnected();

                //Just in case
                if (token.IsCancellationRequested ||
                    (MaxServerConnections >= 0 && _servers.Count >= MaxServerConnections))
                {
                    server.Dispose();
                    return;
                }
                server.StartAsync(token).AwaitSync();
                _servers.Add(server);
            }
        }
        private void RemoveDisconnected()
        {
            List<ModbusServer<TModbusProtocol>> toRemove = new();
            lock (_servers)
            {
                foreach (var server in _servers)
                {
                    if (server.Running)
                        continue;
                    server.Dispose();
                    toRemove.Add(server);
                }
                foreach (var remove in toRemove)
                {
                    _servers.Remove(remove);
                }
            }
        }

        public virtual async Task StartAsync(CancellationToken token = default)
        {
            if (_tokenSource != null && !_tokenSource.IsCancellationRequested)
                throw new InvalidOperationException("Already started");

            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();
        }
        public virtual async Task StopAsync(CancellationToken token = default)
        {
            if (_tokenSource == null)
                return;
            _tokenSource.Cancel();
            lock (_servers)
            {
                foreach (var server in _servers)
                {
                    server.Dispose();
                }
                _servers.Clear();
            }
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                StopAsync().AwaitSync();
                _tokenSource?.Dispose();
                disposedValue = true;
            }
        }
        ~ModbusServerHost()
        {
            Dispose(disposing: false);
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
