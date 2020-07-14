using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.DM.AirMedia;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using Crestron.SimplSharpPro.GeneralIO;

namespace HuddleRoom
{
    public class ControlSystem : CrestronControlSystem
    {
        private Am300 _dmRx;
        private DmTx201C _dmTx;
        private GlsOdtCCn _occSensor;

        public bool HasAirMedia { get; private set; }
        public bool HasLaptop { get; private set; }
        public bool HasOccSensor { get; private set; }

        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 40;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in ControlSystem constructor: {0}", e.Message);
            }
        }

        public override void InitializeSystem()
        {
            HasAirMedia = false;
            HasLaptop = false;
            HasOccSensor = false;

            try
            {
                if (this.SupportsEthernet)
                {
                    _dmRx = new Am300(0x15, this);
                    if (_dmRx.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                        ErrorLog.Error("Unable to register {0} on IP ID {1}!", _dmRx.Name, _dmRx.ID);

                    _dmTx = new DmTx201C(0x14, this);
                    if (_dmTx.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                        ErrorLog.Error("Unable to register {0} on IP ID {1}!", _dmTx.Name, _dmTx.ID);
                }

                if (this.SupportsCresnet)
                {
                    _occSensor = new GlsOdtCCn(0x97, this);
                    if (_occSensor.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                        ErrorLog.Error("Unable to register {0} on Cresnet ID {1}!", _occSensor.Name, _occSensor.ID);
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }
    }
}