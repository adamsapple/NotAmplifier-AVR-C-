using System.IO.Ports;

namespace NotAmpTestWindow
{
    internal class HandShakeItem
    {
        public string NAME = "";
        public Handshake HANDSHAKE;

        public override string ToString()
        {
            return NAME;
        }
    }
}