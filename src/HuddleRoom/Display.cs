using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace HuddleRoom
{
    public class Display
    {
        private ComPort _port;

        public Display(ComPort port, ComPort.ComPortSpec comspec)
        {
            _port = port;
            _port.SetComPortSpec(comspec);
            _port.SerialDataReceived += onDataReceived;
        }

        private void onDataReceived(ComPort port, ComPortSerialDataEventArgs args)
        {
        }

        public void PowerOn()
        {
        }

        public void PowerOff()
        {
        }
    }
}