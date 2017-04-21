using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NotAmplifier.Protocol
{
    public static class MessageOp
    {
        //! シリアルから流れてくるメッセージ群
        public const string MSG_OP_NOP = "nop";					//!< no operation
        public const string MSG_OP_OK  = "ok_";					//!< ok
        public const string MSG_OP_ERR = "err";					//!< err
        public const string MSG_OP_WAY = "way";					//!< who are you
        public const string MSG_OP_IAM = "iam";					//!< i am
        public const string MSG_OP_VER = "ver";					//!< version
        public const string MSG_OP_MIC = "mic";					//!< mic
        public const string MSG_OP_MPW = "mpw";					//!< mic power
        public const string MSG_OP_VOL = "vol";					//!< volume
        public const string MSG_OP_PKM = "pkm";					//!< peak meter
        public const string MSG_OP_PIN = "pin";					//!< ping

        static string[] OPERATIONS = new string[] {
             MSG_OP_NOP,
             MSG_OP_OK,
             MSG_OP_ERR,
             MSG_OP_WAY,
             MSG_OP_IAM,
             MSG_OP_VER,
             MSG_OP_MIC,
             MSG_OP_MPW,
             MSG_OP_VOL,
             MSG_OP_PKM,
             MSG_OP_PIN
         };

        public static MessageType OpToType(string op)
        {
            MessageType result = (MessageType)Array.IndexOf(OPERATIONS, op);
            if(result < 0)
            {
                result = MessageType.MSG_OP_ID_NOP;
            }

            return result;
        }
    }

    //! messageの命令ID一覧
    public enum MessageType
    {
        MSG_OP_ID_NOP,                  //!< no operation
        MSG_OP_ID_OK,                   //!< ok
        MSG_OP_ID_ERR,                  //!< error
        MSG_OP_ID_WAY,                  //!< who are you
        MSG_OP_ID_IAM,                  //!< i am
        MSG_OP_ID_VER,                  //!< version
        MSG_OP_ID_MIC,                  //!< mic
        MSG_OP_ID_MPW,                  //!< mic power
        MSG_OP_ID_VOL,                  //!< volume
        MSG_OP_ID_PKM,                  //!< peak meter
        MSG_OP_ID_PIN,                  //!< ping
        NUM_OF_MSG_OP_ID
    }


    ///<summary>KeyboardHookedイベントのデータを提供する。</summary>
    public class NotAmpDataRecievedEventArgs : EventArgs
    {
        public MessageType Type;
        public int ValueA;
        public int ValueB;
        public int ValueC;

        ///<summary>新しいインスタンスを作成する。</summary>
        internal NotAmpDataRecievedEventArgs(Message msg)
        {
            this.Type   = MessageOp.OpToType(msg.op);
            this.ValueA = msg.val_i_a;
            this.ValueB = msg.val_i_b;
            this.ValueC = msg.val_i_c;
        }
    }
}
