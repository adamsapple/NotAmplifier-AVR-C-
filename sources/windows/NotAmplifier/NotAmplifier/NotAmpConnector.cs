using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using NotAmplifier.Protocol;
using System.Runtime.InteropServices;
using System.Windows.Threading;

using VolumeWatcher.Audio;
using Audio.CoreAudio;

namespace NotAmplifier
{
    

    public class NotAmpConnector: IDisposable
    {
        public int       BaudRate  { get { return serialPort.BaudRate;  } }
        public Handshake Handshake { get { return serialPort.Handshake; } }
        public bool      IsOpen    { get { return serialPort.IsOpen;    } }
        public string    PortName  { get { return serialPort.PortName;  }  set { serialPort.PortName = value; } }

        private SerialPort serialPort = new SerialPort();
        private static int MessageSize;


        /// <summary>
        /// メータ更新間隔(単位:100ナノ秒)。= 1/10000ミリ秒。
        /// 
        /// </summary>
        private readonly long NOTAMP_REQUEST_INTERVAL = 50 * 10000;
        /// <summary></summary>
        private DispatcherTimer StatusTimer = new DispatcherTimer(DispatcherPriority.Normal);
        
        ///<summary></summary>
        public delegate void NotAmpDataRecievedHandler(object sender, NotAmpDataRecievedEventArgs e);

        public event NotAmpDataRecievedHandler OnNotAmpDataRecieved;


        public VolumeMonitor volumeMonitor;               // CoreAudio連携(デバイスの状態変更を監視し通知)



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



            // VolumeMonitor初期化
            volumeMonitor = new VolumeMonitor(EDataFlow.eRender, ERole.eConsole);
            volumeMonitor.initDevice();

            // ピークメータ表示用タイマ
            StatusTimer.Interval = new TimeSpan(NOTAMP_REQUEST_INTERVAL);
            StatusTimer.Tick += (o, el) => {
                var renderMeter = volumeMonitor.AudioDevice?.AudioMeterInformation;
                var value = (Int16)Math.Round((renderMeter?.PeakValue ?? 0) * 1023);
                SendNapMessage(new Message(MessageOp.MSG_OP_PKM, value, 0, 0));
            };
        }

        private void SendNapMessage(Message msg)
        {
            if (serialPort.IsOpen == false)        //!< シリアルポートをオープンしていない場合、処理を行わない.
            {
                return;
            }

            try
            {
                serialPort.Write(msg);
            }
            catch (Exception ex)
            {
            }
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
            StatusTimer.Start();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }
            SendNapMessage(new Message(MessageOp.MSG_OP_PKM, 0, 0, 0));
            StatusTimer.Stop();
            serialPort.Close();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
