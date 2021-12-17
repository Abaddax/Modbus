using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ModbusTCP
{
    internal enum ModbusFunctionCode
    {
        ReadCoils = 0x01,
        ReadDiscreteInputs = 0x02,
        ReadHoldingRegisters = 0x03,
        ReadInputRegisters = 0x04,
        WriteSingleCoil = 0x05,
        WriteSingleRegister = 0x06,
        WriteMultipleCoils = 0x0F,
        WriteMultipleRegisters = 0x10,
        //ReadDeviceIdentification = 0x2B,
        //ReadFileRecord = 0x14
    }
    internal struct ModbusTCPFrame
    {
        public ushort TransactionID;
        public byte UnitID;
        public ModbusFunctionCode Function;
        public byte[] Data;
    }

    public class ModbusTCPMaster : IDisposable
    {
        private bool disposedValue = false;

        private TcpClient client = null;
        private NetworkStream stream = null;

        private readonly object sendLock = new();
        private readonly object readLock = new();

        private readonly string hostname;
        private readonly int port;

        private readonly ConcurrentDictionary<ushort, ModbusTCPFrame?> transactions = new();

        #region Public Static Converters
        /// <summary>
        /// 1 byte -> 8 bool
        /// </summary>
        public static bool[] ReadAsBool(byte[] data)
        {
            if (data == null)
                return null;

            List<bool> bools = new();
            int i = 0;
            while (i < data.Length)
            {
                bools.Add((data[i] & 0b00000001) >> 0 == 1);
                bools.Add((data[i] & 0b00000010) >> 1 == 1);
                bools.Add((data[i] & 0b00000100) >> 2 == 1);
                bools.Add((data[i] & 0b00001000) >> 3 == 1);
                bools.Add((data[i] & 0b00010000) >> 4 == 1);
                bools.Add((data[i] & 0b00100000) >> 5 == 1);
                bools.Add((data[i] & 0b01000000) >> 6 == 1);
                bools.Add((data[i] & 0b10000000) >> 7 == 1);
                i += 1;
            }
            return bools.ToArray();
        }
        /// <summary>
        /// 2 byte -> 1 short
        /// </summary>
        public static short[] ReadAsShort(byte[] data)
        {
            if (data == null)
                return null;

            List<short> shorts = new();
            int i = 0;
            while (i < data.Length - data.Length % 2)
            {
                short value = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, i));
                i += 2;
                shorts.Add(value);
            }
            return shorts.ToArray();
        }
        /// <summary>
        /// 4 byte -> 1 float
        /// </summary>
        public static float[] ReadAsFloat(byte[] data)
        {
            if (data == null)
                return null;

            List<float> floats = new();
            var i = 0;
            while (i < data.Length - data.Length % 4)
            {
                int value = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, i));
                float f = BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
                i += 4;
                floats.Add(f);
            }
            return floats.ToArray();
        }
        #endregion

        #region Private Functions
        private ushort NewTransactionID()
        {
            Random rand = new Random();
            ushort transactionID;
            do
            {
                transactionID = (ushort)rand.Next(0, 65535);
            }
            while (!transactions.TryAdd(transactionID, null));
            return transactionID;
        }

        #region Send Functions
        private int SendModbusTCPFrame(in ModbusTCPFrame Message)
        {
            if (stream == null)
                return -2;
            if (Message.Data == null)
                return -1;
            int ModbusRequestLength = Message.Data.Length + 2; //UnitID+FunctionCode + Data
            byte[] buffer = new byte[6 + ModbusRequestLength];

            //ModbusTCP-Header
            //Transaction ID
            buffer[0] = (byte)((Message.TransactionID >> 8) & 0xFF);
            buffer[1] = (byte)((Message.TransactionID) & 0xFF);
            //Protocol Identifier
            buffer[2] = 0;
            buffer[3] = 0;
            //Length
            buffer[4] = (byte)((ModbusRequestLength >> 8) & 0xFF);
            buffer[5] = (byte)((ModbusRequestLength) & 0xFF);
            //Unit Identifier
            buffer[6] = Message.UnitID;

            //Modbus-Frame
            //Function Code
            buffer[7] = (byte)Message.Function;

            if (Message.Data != null)
                Buffer.BlockCopy(Message.Data, 0, buffer, 8, Message.Data.Length);

            try
            {
                lock (sendLock)
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (System.IO.IOException) { return -2; }
            catch (ObjectDisposedException) { return -2; }

            return 0;
        }
        private int SendRequest(in byte[] Data, in ModbusFunctionCode Function, in byte Unit, out ushort TransactionID)
        {
            TransactionID = 0;
            ModbusTCPFrame frame = new ModbusTCPFrame { Data = Data, Function = Function, UnitID = Unit, TransactionID = NewTransactionID() };
            int ret = SendModbusTCPFrame(frame);
            if (ret < 0)
            {
                ModbusTCPFrame? help;
                transactions.TryRemove(frame.TransactionID, out help);

                //Connection Error
                if (ret == -2)
                    CloseOnError();
                return -1;
            }
            TransactionID = frame.TransactionID;
            return 0;
        }
        #endregion

        #region Receive Functions
        private int ReadModbusTCPFrame(out ModbusTCPFrame Message)
        {
            byte[] header = new byte[8];
            Message = new ModbusTCPFrame() { TransactionID = 0, UnitID = 0, Function = (ModbusFunctionCode)0, Data = null };

            if (stream == null)
                return -2;

            int read = 0;
            lock (readLock)
            {
                try
                {
                    while (read < header.Length)
                    {
                        int ret = stream.Read(header, read, header.Length - read);
                        if (ret > 0)
                        {
                            read += ret;
                        }
                    }
                }
                catch (System.IO.IOException) { return -2; }
                catch (ObjectDisposedException) { return -2; }

                if (header[2] != 0 || header[3] != 0)
                    return -1;

                Message.TransactionID = (ushort)((header[0] << 8) + header[1]);
                int DataLength = (header[4] << 8) + header[5] - 2;

                Message.UnitID = header[6];
                Message.Function = (ModbusFunctionCode)header[7];

                Message.Data = new byte[DataLength];
                //Reading Data
                read = 0;
                try
                {
                    while (read < DataLength)
                    {
                        int ret = stream.Read(Message.Data, read, DataLength - read);
                        if (ret > 0)
                        {
                            read += ret;
                        }
                    }
                }
                catch (System.IO.IOException) { return -2; }
                catch (ObjectDisposedException) { return -2; }
            }
            return 0;
        }
        private int ReadResponse(ModbusFunctionCode Function, ushort TransactionID, byte unit, out byte[] Data)
        {
            Data = null;
            ModbusTCPFrame frame;
            if (!transactions.ContainsKey(TransactionID))
                return -1;

            while (transactions[TransactionID] == null)
            {
                int ret = ReadModbusTCPFrame(out frame);
                if (ret < 0)
                {
                    if (ret == -2)
                        CloseOnError();
                    return -1;
                }
                if (transactions.ContainsKey(frame.TransactionID) && transactions[frame.TransactionID] == null)
                {
                    transactions[frame.TransactionID] = frame;
                }
            }

            frame = transactions[TransactionID].Value;

            if (frame.Function != Function || frame.UnitID != unit)
                return -1;

            Data = frame.Data;

            ModbusTCPFrame? help;
            transactions.TryRemove(TransactionID, out help);

            return 0;
        }
        #endregion
        private void CloseOnError()
        {
            Close();
        }
        #endregion

        public ModbusTCPMaster(string Hostname, int Port = 502)
        {
            hostname = Hostname;
            port = Port;
        }

        public int Connect()
        {
            if (disposedValue)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (Connected)
                return -1;

            client = new TcpClient();
            try
            {
                client.Connect(hostname, port);
                stream = client.GetStream();
            }
            catch (SocketException)
            {
                client?.Close();
                client?.Dispose();
                client = null;
                stream = null;
                Console.WriteLine("Couldnt connected to Modbus-Slave");
                return -1;
            }
            Console.WriteLine("Connected to Modbus-Slave");
            return 0;
        }
        public void Close()
        {
            if (disposedValue)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (client != null)
            {
                stream?.Close();
                stream?.Dispose();
                client?.Close();
                stream = null;
                client = null;
                transactions?.Clear();
                OnClosed?.Invoke(this, null);
            }
        }

        public bool Connected { get { return client == null ? false : client.Connected; } }

        public event EventHandler OnClosed;

        #region R/W Functions
        /// <summary>
        /// Read Coil Status (1BIT*count) (Functioncode 0x01) 
        /// </summary>
        /// <returns>
        /// <para>array: OK</para>
        /// <para>null: ERROR</para>
        /// </returns>
        public byte[] ReadCoils(int offset, int count, byte unit = 0x01)
        {
            if (disposedValue)
                throw new ObjectDisposedException(this.GetType().FullName);

            int byteCount = count;
            byte[] response;
            byte[] request = new byte[4];
            //Starting Address Hi 
            request[0] = (byte)((offset >> 8) & 0xFF);
            //Starting Address Lo 
            request[1] = (byte)((offset) & 0xFF);
            //No.of Points Hi
            request[2] = (byte)((byteCount >> 8) & 0xFF);
            //No.of Points Lo
            request[3] = (byte)((byteCount) & 0xFF);

            ushort transactionID;

            int ret = SendRequest(request, ModbusFunctionCode.ReadCoils, unit, out transactionID);
            if (ret < 0)
                return null;

            ret = ReadResponse(ModbusFunctionCode.ReadCoils, transactionID, unit, out response);
            if (ret < 0)
                return null;

            if (response[0] + 1 != response.Length)
                return null;

            byte[] values = new byte[response.Length - 1];
            Buffer.BlockCopy(response, 1, values, 0, values.Length);
            return values;
        }
        /// <summary>
        /// Read Input Status (1BIT*count) (Functioncode 0x02) 
        /// </summary>
        /// <returns>
        /// <para>array: OK</para>
        /// <para>null: ERROR</para>
        /// </returns>
        public byte[] ReadInputs(int offset, int count, byte unit = 0x01)
        {
            if (disposedValue)
                throw new ObjectDisposedException(this.GetType().FullName);

            int byteCount = count;
            byte[] response;
            byte[] request = new byte[4];
            //Starting Address Hi 
            request[0] = (byte)((offset >> 8) & 0xFF);
            //Starting Address Lo 
            request[1] = (byte)((offset) & 0xFF);
            //No.of Points Hi
            request[2] = (byte)((byteCount >> 8) & 0xFF);
            //No.of Points Lo
            request[3] = (byte)((byteCount) & 0xFF);

            ushort transactionID;

            int ret = SendRequest(request, ModbusFunctionCode.ReadDiscreteInputs, unit, out transactionID);
            if (ret < 0)
                return null;

            ret = ReadResponse(ModbusFunctionCode.ReadDiscreteInputs, transactionID, unit, out response);
            if (ret < 0)
                return null;

            if (response[0] + 1 != response.Length)
                return null;

            byte[] values = new byte[response.Length - 1];
            Buffer.BlockCopy(response, 1, values, 0, values.Length);
            return values;
        }
        /// <summary>
        /// Read Holding Registers (2BYTE*count) (Functioncode 0x03) 
        /// </summary>
        /// <returns>
        /// <para>array: OK</para>
        /// <para>null: ERROR</para>
        /// </returns>
        public byte[] ReadHoldingRegisters(int offset, int count, byte unit = 0x01)
        {
            if (disposedValue)
                throw new ObjectDisposedException(this.GetType().FullName);

            int byteCount = count;
            byte[] response;
            byte[] request = new byte[4];
            //Starting Address Hi 
            request[0] = (byte)((offset >> 8) & 0xFF);
            //Starting Address Lo 
            request[1] = (byte)((offset) & 0xFF);
            //No.of Points Hi
            request[2] = (byte)((byteCount >> 8) & 0xFF);
            //No.of Points Lo
            request[3] = (byte)((byteCount) & 0xFF);

            ushort transactionID;

            int ret = SendRequest(request, ModbusFunctionCode.ReadHoldingRegisters, unit, out transactionID);
            if (ret < 0)
                return null;

            ret = ReadResponse(ModbusFunctionCode.ReadHoldingRegisters, transactionID, unit, out response);
            if (ret < 0)
                return null;

            if (response[0] + 1 != response.Length)
                return null;

            byte[] values = new byte[response.Length - 1];
            Buffer.BlockCopy(response, 1, values, 0, values.Length);
            return values;
        }
        /// <summary>
        /// Read Input Registers (2BYTE*count) (Functioncode 0x04) 
        /// </summary>
        /// <returns>
        /// <para>array: OK</para>
        /// <para>null: ERROR</para>
        /// </returns>
        public byte[] ReadInputRegisters(int offset, int count, byte unit = 0x01)
        {
            if (disposedValue)
                throw new ObjectDisposedException(this.GetType().FullName);

            int byteCount = count;
            byte[] response;
            byte[] request = new byte[4];
            //Starting Address Hi 
            request[0] = (byte)((offset >> 8) & 0xFF);
            //Starting Address Lo 
            request[1] = (byte)((offset) & 0xFF);
            //No.of Points Hi
            request[2] = (byte)((byteCount >> 8) & 0xFF);
            //No.of Points Lo
            request[3] = (byte)((byteCount) & 0xFF);

            ushort transactionID;

            int ret = SendRequest(request, ModbusFunctionCode.ReadInputRegisters, unit, out transactionID);
            if (ret < 0)
                return null;

            ret = ReadResponse(ModbusFunctionCode.ReadInputRegisters, transactionID, unit, out response);
            if (ret < 0)
                return null;

            if (response[0] + 1 != response.Length)
                return null;

            byte[] values = new byte[response.Length - 1];
            Buffer.BlockCopy(response, 1, values, 0, values.Length);
            return values;
        }

        /// <summary>
        /// Force Single Coil (Functioncode 0x05) 
        /// </summary>
        /// <returns>
        /// <para>0: OK</para>
        /// <para>-1: ERROR</para>
        /// </returns>
        public int WriteCoil(int offset, bool value, byte unit = 0x01)
        {
            if (disposedValue)
                throw new ObjectDisposedException(this.GetType().FullName);

            ushort focevalue = (ushort)(value ? 0xFF00 : 0x0000);
            byte[] response;
            byte[] request = new byte[4];
            //Coil Address Hi 
            request[0] = (byte)((offset >> 8) & 0xFF);
            //Coil Address Lo 
            request[1] = (byte)((offset) & 0xFF);
            //Force Data Hi            
            request[2] = (byte)((focevalue >> 8) & 0xFF);
            //Force Data Lo
            request[3] = (byte)((focevalue) & 0xFF);

            ushort transactionID;

            int ret = SendRequest(request, ModbusFunctionCode.WriteSingleCoil, unit, out transactionID);
            if (ret < 0)
                return -1;

            ret = ReadResponse(ModbusFunctionCode.WriteSingleCoil, transactionID, unit, out response);
            if (ret < 0)
                return -1;

            if (!request.SequenceEqual(response))
                return -1;

            return 0;
        }
        /// <summary>
        /// Preset Single Register (Functioncode 0x06) 
        /// </summary>
        /// <returns>
        /// <para>0: OK</para>
        /// <para>-1: ERROR</para>
        /// </returns>
        public int WriteRegister(int offset, short value, byte unit = 0x01)
        {
            if (disposedValue)
                throw new ObjectDisposedException(this.GetType().FullName);

            byte[] response;
            byte[] request = new byte[4];
            //Register Address Hi 
            request[0] = (byte)((offset >> 8) & 0xFF);
            //Register Address Lo 
            request[1] = (byte)((offset) & 0xFF);
            //Preset Data Hi            
            request[2] = (byte)((value >> 8) & 0xFF);
            //Preset Data Lo
            request[3] = (byte)((value) & 0xFF);

            ushort transactionID;

            int ret = SendRequest(request, ModbusFunctionCode.WriteSingleRegister, unit, out transactionID);
            if (ret < 0)
                return -1;

            ret = ReadResponse(ModbusFunctionCode.WriteSingleRegister, transactionID, unit, out response);
            if (ret < 0)
                return -1;

            if (!request.SequenceEqual(response))
                return -1;

            return 0;
        }
        /// <summary>
        /// Force Multiple Coils (Functioncode 0x0F) 
        /// </summary>
        /// <returns>
        /// <para>0: OK</para>
        /// <para>-1: ERROR</para>
        /// </returns>
        public int WriteCoils(int offset, bool[] values, byte unit = 0x01)
        {
            if (disposedValue)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (values == null)
                return -1;

            byte byteCount = (byte)(values.Length / 8 + 1);

            byte[] response;
            byte[] request = new byte[5 + byteCount];
            //Coil Address Hi 
            request[0] = (byte)((offset >> 8) & 0xFF);
            //Coil Address Lo 
            request[1] = (byte)((offset) & 0xFF);
            //Quantity of Coils Hi            
            request[2] = (byte)((values.Length >> 8) & 0xFF);
            //Quantity of Coils Lo
            request[3] = (byte)((values.Length) & 0xFF);
            //Byte Count
            request[4] = byteCount;
            for (int i = 0; i < byteCount; i++)
            {
                request[i + 5] = 0;
                request[i + 5] |= (byte)(((i < values.Length) ? (values[i * 8 + 0] ? 1 : 0) : 0) << 0);
                request[i + 5] |= (byte)(((i < values.Length) ? (values[i * 8 + 1] ? 1 : 0) : 0) << 0);
                request[i + 5] |= (byte)(((i < values.Length) ? (values[i * 8 + 2] ? 1 : 0) : 0) << 0);
                request[i + 5] |= (byte)(((i < values.Length) ? (values[i * 8 + 3] ? 1 : 0) : 0) << 0);
                request[i + 5] |= (byte)(((i < values.Length) ? (values[i * 8 + 4] ? 1 : 0) : 0) << 0);
                request[i + 5] |= (byte)(((i < values.Length) ? (values[i * 8 + 5] ? 1 : 0) : 0) << 0);
                request[i + 5] |= (byte)(((i < values.Length) ? (values[i * 8 + 6] ? 1 : 0) : 0) << 0);
                request[i + 5] |= (byte)(((i < values.Length) ? (values[i * 8 + 7] ? 1 : 0) : 0) << 0);
            }

            ushort transactionID;

            int ret = SendRequest(request, ModbusFunctionCode.WriteMultipleCoils, unit, out transactionID);
            if (ret < 0)
                return -1;

            ret = ReadResponse(ModbusFunctionCode.WriteMultipleCoils, transactionID, unit, out response);
            if (ret < 0)
                return -1;

            if (request[0] != response[0] || request[1] != response[1] || request[2] != response[2] || request[3] != response[3])
                return -1;

            return 0;
        }
        /// <summary>
        /// Preset Multiple Registers (Functioncode 0x10) 
        /// </summary>
        /// <returns>
        /// <para>0: OK</para>
        /// <para>-1: ERROR</para>
        /// </returns>
        public int WriteRegisters(int offset, short[] values, byte unit = 0x01)
        {
            if (disposedValue)
                throw new ObjectDisposedException(this.GetType().FullName);

            if (values == null)
                return -1;

            byte byteCount = (byte)(values.Length * 2);

            byte[] response;
            byte[] request = new byte[5 + byteCount];
            //Starting Address Hi 
            request[0] = (byte)((offset >> 8) & 0xFF);
            //Starting Address Lo 
            request[1] = (byte)((offset) & 0xFF);
            //No. of Registers Hi           
            request[2] = (byte)((values.Length >> 8) & 0xFF);
            //No. of Registers Lo
            request[3] = (byte)((values.Length) & 0xFF);
            //Byte Count
            request[4] = byteCount;

            for (int i = 0; i < byteCount; i += 2)
            {
                //Data Hi
                request[i + 5] = (byte)((values[i] >> 8) & 0xFF);
                //Data Lo
                request[i + 6] = (byte)((values[i + 1]) & 0xFF);
            }

            ushort transactionID;

            int ret = SendRequest(request, ModbusFunctionCode.WriteMultipleRegisters, unit, out transactionID);
            if (ret < 0)
                return -1;

            ret = ReadResponse(ModbusFunctionCode.WriteMultipleRegisters, transactionID, unit, out response);
            if (ret < 0)
                return -1;

            if (request[0] != response[0] || request[1] != response[1] || request[2] != response[2] || request[3] != response[3])
                return -1;

            return 0;
        }
        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: Verwalteten Zustand (verwaltete Objekte) bereinigen
                    transactions.Clear();

                }
                // TODO: Nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer überschreiben
                // TODO: Große Felder auf NULL setzen
                client.Close();
                stream.Dispose();

                disposedValue = true;
            }
        }
        ~ModbusTCPMaster()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(false);
        }
        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
