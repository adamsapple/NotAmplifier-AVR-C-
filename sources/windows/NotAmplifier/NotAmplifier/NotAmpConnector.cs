using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using NotAmplifier.Protocol;
using System.Runtime.InteropServices;

namespace NotAmplifier
{
    

    public class NotAmpConnector
    {
        public int       BaudRate  { get { return serialPort.BaudRate;  } }
        public Handshake Handshake { get { return serialPort.Handshake; } }
        public bool      IsOpen    { get { return serialPort.IsOpen;    } }
        public string    PortName  { get { return serialPort.PortName;  }  set { serialPort.PortName = value; } }

        private SerialPort serialPort = new SerialPort();
        private static int MessageSize;

        ///<summary></summary>
        public delegate void NotAmpDataRecievedHandler(object sender, NotAmpDataRecievedEventArgs e);

        public event NotAmpDataRecievedHandler OnNotAmpDataRecieved;

        public NotAmpConnector()
        {
            serialPort.BaudRate  = 9600;
            serialPort.Handshake = Handshake.RequestToSend;
            serialPort.DataBits  = 8;                                   //!< データビットをセットする. (データビット = 8ビット)
            serialPort.Parity    = Parity.None;                         //!< パリティビットをセットする. (パリティビット = なし)
            serialPort.StopBits  = StopBits.One;                        //!< ストップビットをセットする. (ストップビット = 1ビット)
            serialPort.Encoding  = Encoding.ASCII;                      //!< 文字コードをセットする
                                                                        //! シリアルデータ受信イベント通知設定
            serialPort.DataReceived += DataReceived;

            MessageSize = Marshal.SizeOf(typeof(Message));
        }

        private void DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (!serialPort.IsOpen)                    //!< シリアルポートをオープンしていない場合、処理を行わない.
            {
                return;
            }

            if (serialPort.BytesToRead < MessageSize)
            {
                return;
            }

            try
            {
                byte[] data = new byte[MessageSize];
                serialPort.Read(data, 0, MessageSize);

                var msg = Message.ToStruct(data);

                var ea = new NotAmpDataRecievedEventArgs(msg);
                OnNotAmpDataRecieved?.Invoke(this, ea);

            }
            catch (Exception ex)
            {
            }
        }

        public void Open()
        {
            if (IsOpen)
            {
                return;
            }

            serialPort.Open();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            serialPort.Close();
        }
    }
}
