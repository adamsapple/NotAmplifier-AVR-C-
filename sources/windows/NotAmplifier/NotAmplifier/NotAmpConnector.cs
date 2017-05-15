﻿using System;
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
        private static readonly long NOTAMP_WHOAREYOU_INTERVAL = 2500 * 10000;
        private static readonly long NOTAMP_REQUEST_INTERVAL = 50 * 10000;
        

        // ピークメータ表示用タイマ
        private readonly TimeSpan intervalWAY = new TimeSpan(NOTAMP_WHOAREYOU_INTERVAL);
        private readonly TimeSpan intervalPKM = new TimeSpan(NOTAMP_REQUEST_INTERVAL);

        /// <summary></summary>
        private DispatcherTimer StatusTimer = new DispatcherTimer(DispatcherPriority.Normal);
        
        ///<summary></summary>
        public delegate void NotAmpDataRecievedHandler(object sender, NotAmpDataRecievedEventArgs e);

        public event NotAmpDataRecievedHandler OnNotAmpDataRecieved;


        public VolumeMonitor volumeMonitor;               // CoreAudio連携(デバイスの状態変更を監視し通知)
        public VolumeMonitor micMonitor;                  // CoreAudio連携(デバイスの状態変更を監視し通知)

        private int prev_volume  = -10;
        private int prev_mic     = -10;

        public NotAmpConnector()
        {
            serialPort.BaudRate     = 9600;
            serialPort.Handshake    = Handshake.RequestToSend;
            serialPort.DataBits     = 8;                                   //!< データビットをセットする. (データビット = 8ビット)
            serialPort.Parity       = Parity.None;                         //!< パリティビットをセットする. (パリティビット = なし)
            serialPort.StopBits     = StopBits.One;                        //!< ストップビットをセットする. (ストップビット = 1ビット)
            serialPort.Encoding     = Encoding.ASCII;                      //!< 文字コードをセットする
            serialPort.WriteTimeout = 100;                                 //!< Timeout設定 
            //! シリアルデータ受信イベント通知設定
            serialPort.DataReceived += DataReceived;
            

            MessageSize = Marshal.SizeOf(typeof(Message));



            // VolumeMonitor初期化
            volumeMonitor   = new VolumeMonitor(EDataFlow.eRender, ERole.eConsole);
            micMonitor      = new VolumeMonitor(EDataFlow.eCapture, ERole.eConsole);
            volumeMonitor.initDevice();
            micMonitor.initDevice();

            


            StatusTimer.Tick += (o, el) => {
                
                var timer = o as DispatcherTimer;

                if (timer.Interval == intervalWAY)
                {
                    SendNapMessage(new Message(MessageOp.MSG_OP_WAY, 0, 0, 0));
                }
                else if (timer.Interval == intervalPKM)
                {
                    var renderMeter = volumeMonitor.AudioDevice?.AudioMeterInformation;
                    var value = (Int16)Math.Round((renderMeter?.PeakValue ?? 0) * 1023);
                    SendNapMessage(new Message(MessageOp.MSG_OP_PKM, value, 0, 0));
                }
            };
        }

        private void SendNapMessage(Message msg)
        {
            if (serialPort.IsOpen == false)             //!< シリアルポートをオープンしていない場合、処理を行わない.
            {
                return;
            }
            try
            {
                serialPort.Write(msg);
            }
            catch (Exception ex)
            {
                this.Close();
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

                

                switch (MessageOp.OpToType(msg.op))
                {
                    case MessageType.MSG_OP_ID_VOL:
                        if(Math.Abs(msg.val_i_a - prev_volume) < 2)
                        {
                            break;
                        }
                        prev_volume = msg.val_i_a;
                        volumeMonitor.AudioVolume.MasterVolumeLevelScalar = msg.val_i_a / 1023f;
                        break;

                    case MessageType.MSG_OP_ID_MIC:
                        if (Math.Abs(msg.val_i_a - prev_mic) < 2)
                        {
                            break;
                        }
                        prev_mic = msg.val_i_a;
                        micMonitor.AudioVolume.MasterVolumeLevelScalar = msg.val_i_a / 1023f;
                        break;

                    case MessageType.MSG_OP_ID_IAM:
                        StatusTimer.Interval = intervalPKM;
                        break;

                    case MessageType.MSG_OP_ID_MPW:
                        //Debug.WriteLine(msg.val_i_a);
                        //volumeMonitor.AudioVolume.Mute = !(msg.val_i_a == 1);
                        micMonitor.AudioVolume.Mute = !(msg.val_i_a == 1);
                        break;
                }


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
            StatusTimer.Interval = intervalWAY;
            StatusTimer.Start();
        }

        public void Close()
        {
            StatusTimer.Stop();
            if (!IsOpen)
            {
                return;
            }
            serialPort.Close();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
