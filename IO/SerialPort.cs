using System;
using System.Collections.Generic;
using System.Text;

namespace BD.IO
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SerialPortHelper
    {
        private SerialPortHelper() { }
        private static SerialPortHelper _LLSerialPort = new SerialPortHelper();
        /// <summary>
        /// 
        /// </summary>
        public static SerialPortHelper Default
        {
            get { return _LLSerialPort; }
        }

        private Queue<byte[]> DataQueue = new Queue<byte[]>();

        /// <summary>
        /// Get the Avaiable Port Names
        /// </summary>
        public List<string> SerialPorts
        {
            get { return new List<string>(System.IO.Ports.SerialPort.GetPortNames()); }
        }
        private System.IO.Ports.SerialPort _SerialPort;
        /// <summary>
        /// 
        /// </summary>
        public System.IO.Ports.SerialPort SerialPort
        {
            get
            {
                if (null == _SerialPort)
                {
                    _SerialPort = new System.IO.Ports.SerialPort();
                    _SerialPort.DataReceived += _SerialPort_DataReceived;
                }
                return _SerialPort;
            }
        }
        private System.Timers.Timer _Timer;
        private System.Timers.Timer Timer
        {
            get
            {
                if (null == _Timer)
                {
                    _Timer = new System.Timers.Timer(150);
                    _Timer.Elapsed += _Timer_Elapsed;
                }
                return _Timer;
            }
        }
        /// <summary>
        /// SerialPortHelper Initialization
        /// </summary>
        /// <param name="com"></param>
        /// <param name="BaudRate"></param>
        /// <param name="DataBits"></param>
        /// <param name="StopBIts"></param>
        /// <param name="Parity"></param>
        public bool Load(string com, int BaudRate = 9600, int DataBits = 8,
            System.IO.Ports.StopBits StopBIts = System.IO.Ports.StopBits.One,
            System.IO.Ports.Parity Parity = System.IO.Ports.Parity.None)
        {
            SerialPort.PortName = com;
            SerialPort.BaudRate = BaudRate;
            SerialPort.DataBits = DataBits;
            SerialPort.StopBits = StopBIts;
            SerialPort.Parity = Parity;
            Timer.Start();
            try {
                SerialPort.Open();
                return true;
            } catch {
                _SerialPort.DataReceived -= _SerialPort_DataReceived;
                _Timer.Elapsed -= _Timer_Elapsed;
                SerialPort.Dispose();
                Timer.Dispose();
                return false;  }            
        }
        /// <summary>
        /// Write bytes to COM
        /// </summary>
        /// <param name="data"></param>
        public void WriteData(byte[] data)
        {
            if (data.Length <= 0) return;
            DataQueue.Enqueue(data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void WriteData(string data)
        {
            var tmp = Encoding.UTF8.GetBytes(data);
            WriteData(tmp);
        }
        /// <summary>
        /// Serial Port Receive datas event
        /// </summary>
        public event SerialDataInHandler SerialDataInEvent;

        /// <summary>
        /// Relay Execute Function
        /// </summary>
        /// <param name="time">delay time</param>
        /// <param name="action"></param>
        public void DelayFun(double time, Action<object> action = null)
        {
            System.Timers.Timer timer = new System.Timers.Timer(time);
            timer.AutoReset = false;
            timer.Elapsed += delegate {
                action?.Invoke(1);
                timer.Dispose();
            };
            timer.Start();
        }

        /// <summary>
        /// Unload the com port
        /// </summary>
        public void UnLoad()
        {
            if(SerialPort.IsOpen)
            {                
                _SerialPort.DataReceived -= _SerialPort_DataReceived;
                _Timer.Elapsed -= _Timer_Elapsed;
                SerialPort.Close();
                SerialPort.Dispose();
                Timer.Dispose();
            }
        }
        private void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (DataQueue.Count <= 0) return;
            var cmd = DataQueue.Dequeue();
            SerialPort.Write(cmd, 0, cmd.Length);
        }
        private void _SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            var num = SerialPort.BytesToRead;
            byte[] buffer = new byte[num];
            SerialPort.Read(buffer, 0, num);
            SerialPort.DiscardInBuffer();
            if (buffer.Length > 0) SerialDataInEvent?.Invoke(this,new SerialDataArgs(buffer));            
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class SerialDataArgs:EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public string SerialHexStr;
        /// <summary>
        /// 
        /// </summary>
        public byte[] SerialByte;
        /// <summary>
        /// 
        /// </summary>
        public List<byte> SerialList;
        /// <summary>
        /// 
        /// </summary>
        public string SerialStr;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public SerialDataArgs(byte[] data)
        {
            SerialByte = data;
            SerialList = new List<byte>(data);

            SerialStr = Encoding.UTF8.GetString(data);

            var result = SerialList.ConvertAll<string>((bin) => "0X" + bin.ToString("X"));
            string strf = "";
            result.ForEach(str => { strf += str + "\t"; });
            SerialHexStr = strf;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void SerialDataInHandler(object sender,SerialDataArgs args);
}
