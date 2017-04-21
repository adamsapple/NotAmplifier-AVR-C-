using System;
using System.Collections.Generic;
using System.Text;


namespace Audio.CoreAudio
{
    /**
     * <summary>デバイス種別を示すEnum</summary>
     */
    public enum EDataFlow
    {
        /// <summary>再生デバイス</summary>
        eRender,
        /// <summary>録音デバイス</summary>
        eCapture,
        /// <summary>全部</summary>
        eAll,
        EDataFlow_enum_count
    }
}
