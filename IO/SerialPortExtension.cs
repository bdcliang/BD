using System;
using System.Collections.Generic;
using System.Text;

namespace BD.IO
{
    /// <summary>
    /// 
    /// </summary>
    public static class SerialPortExtension
    {
        /// <summary>
        /// Open a single delay
        /// </summary>
        /// <param name="SerialPort"></param>
        /// <param name="id"></param>
        /// <param name="port"></param>
        /// <param name="crc">crc</param>
        public static void OpenDelay(this BD.IO.SerialPortHelper SerialPort, byte id, byte port,bool crc= false)
        {
            if (id <= 0) return;
            if (crc)
            {
                byte[] cmd = new byte[] { 0xAA, 0xBB, id, port, 0x01, 0x74 };
                int crcx = 0;
                foreach (var num in cmd)
                    crcx += num;
                cmd[5] = (byte)(crcx - cmd[5]);
                SerialPort.WriteData(cmd);
            }
            else
            {
                byte[] cmd = new byte[] { 0xAA, 0xBB, id, port, 0x01 };
                SerialPort.WriteData(cmd);
            }
        }
        /// <summary>
        /// Close a signal delay
        /// </summary>
        /// <param name="SerialPort"></param>
        /// <param name="id"></param>
        /// <param name="port"></param>
        /// <param name="crc">crc</param>
        public static void CloseDelay(this BD.IO.SerialPortHelper SerialPort, byte id, byte port,bool crc=false)
        {
            if (id <= 0) return;
            if (crc)
            {
                byte[] cmd = new byte[] { 0xAA, 0xBB, id, port, 0x00, 0x74 };
                int crcx = 0;
                foreach (var num in cmd)
                    crcx += num;
                cmd[5] = (byte)(crcx - cmd[5]);
                SerialPort.WriteData(cmd);
            }
            else
            {
                byte[] cmd = new byte[] { 0xAA, 0xBB, id, port, 0x00 };
                SerialPort.WriteData(cmd);
            }
        }
        /// <summary>
        /// open all delays
        /// </summary>
        /// <param name="SerialPort"></param>
        /// <param name="num">total ports</param>
        /// <param name="crc">crc</param>
        public static void OpenAll(this BD.IO.SerialPortHelper SerialPort,byte id=1, byte num = 16, bool crc = false)
        {
            for (byte i = 1; i <= num; i++)
            {
                if (crc)
                {
                    byte[] cmd = new byte[] { 0xAA, 0xBB, 0x01, 0x01, 0x01, 0x74 };
                    cmd[2] = id;
                    cmd[3] = i;
                    int crcx = 0;
                    foreach (var numx in cmd)
                        crcx += numx;
                    cmd[5] = (byte)(crcx - cmd[5]);
                    SerialPort.WriteData(cmd);
                }
                else
                {
                    byte[] cmd = new byte[] { 0xAA, 0xBB, 0x01, i, 0x01 };
                    cmd[2] = id;
                    SerialPort.WriteData(cmd);                   
                }
            }
        }
        /// <summary>
        /// close all delays
        /// </summary>
        /// <param name="SerialPort"></param>
        /// <param name="num">total ports</param>
        /// <param name="crc">crc</param>
        public static void CloseAll(this BD.IO.SerialPortHelper SerialPort,byte id=1,byte num=16,bool crc=false)
        {
            for (byte i = 1; i <= num; i++)
            {
                if (crc)
                {
                    byte[] cmd = new byte[] { 0xAA, 0xBB, 0x01, 0x01, 0x00, 0x74 };
                    cmd[3] = i;
                    cmd[2] = id;
                    int crcx = 0;
                    foreach (var numx in cmd)
                        crcx += numx;
                    cmd[5] = (byte)(crcx - cmd[5]);
                    SerialPort.WriteData(cmd);
                }
                else
                {
                    byte[] cmd = new byte[] { 0xAA, 0xBB, 0x01, i, 0x00 };
                    cmd[2] = id;
                    SerialPort.WriteData(cmd);
                }
            }
        }
        /// <summary>
        /// query command
        /// </summary>
        /// <param name="SerialPort"></param>
        /// <param name="crc">crc</param>
        public static void Query(this BD.IO.SerialPortHelper SerialPort,byte id=1,bool crc=false)
        {
            byte[] cmdcrc = new byte[] { 0xAA, 0xBB, 0x01, 0xFD, 0x01, 0x74 };
            byte[] cmd= new byte[] { 0xAA, 0xBB, 0x01, 0xFD, 0x01};
            cmd[2] = id;
            cmdcrc[2] = id;
            if (crc)
            {
                int crcx = 0;
                foreach (var numx in cmdcrc)
                    crcx += numx;
                cmdcrc[5] = (byte)(crcx - cmdcrc[5]);
                SerialPort.WriteData(cmdcrc);
            }
            else
                SerialPort.WriteData(cmd);
        }
    }
}
