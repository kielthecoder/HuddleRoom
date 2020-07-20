using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM.AirMedia;

namespace HuddleRoom
{
    public class AudioVideoSwitcher
    {
        public AudioVideoSwitcher()
        {
        }
    }

    public enum AirMediaInputs
    {
        None = 0,
        AirMedia = 1,
        HDMI = 2,
        DM = 3,
        AirBoard = 4
    }

    public class AirMediaSwitcher : AudioVideoSwitcher
    {
        private AmX00 _airmedia;

        public AirMediaSwitcher(AmX00 device) : base()
        {
            _airmedia = device;
        }

        public void Switch(AirMediaInputs input)
        {
            try
            {
                AmX00DisplayControl.eAirMediaX00VideoSource source = AmX00DisplayControl.eAirMediaX00VideoSource.NA;

                switch (input)
                {
                    case AirMediaInputs.None:
                        source = AmX00DisplayControl.eAirMediaX00VideoSource.PinPointUxLandingPage;
                        break;
                    case AirMediaInputs.AirMedia:
                        source = AmX00DisplayControl.eAirMediaX00VideoSource.AirMedia;
                        break;
                    case AirMediaInputs.HDMI:
                        source = AmX00DisplayControl.eAirMediaX00VideoSource.HDMI;
                        break;
                    case AirMediaInputs.DM:
                        source = AmX00DisplayControl.eAirMediaX00VideoSource.DM;
                        break;
                    case AirMediaInputs.AirBoard:
                        source = AmX00DisplayControl.eAirMediaX00VideoSource.AirBoard;
                        break;
                    default:
                        throw new InvalidOperationException("unrecognized AirMedia input value: " + input);
                }

                _airmedia.DisplayControl.VideoOut = source;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Exception in AirMediaSwitcher::Switch: {0}", e.Message);
            }
        }
    }
}