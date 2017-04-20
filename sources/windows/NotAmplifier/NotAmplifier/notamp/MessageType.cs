using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NotAmplifier.Protocol
{
    public enum MessageType : byte
    {
        
    }

    ///<summary>KeyboardHookedイベントのデータを提供する。</summary>
    public class NotAmpDataRecievedEventArgs : EventArgs
    {
        //public Message Data;
        public MessageType Type;
        public int ValueA;
        public int ValueB;
        public int ValueC;

        ///<summary>新しいインスタンスを作成する。</summary>
        internal NotAmpDataRecievedEventArgs(Message msg)
        {
            //this.Type = ;
            this.ValueA = msg.val_i_a;
            this.ValueB = msg.val_i_b;
            this.ValueC = msg.val_i_c;
        }
    }
}
