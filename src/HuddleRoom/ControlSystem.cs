using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.DM.AirMedia;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using Crestron.SimplSharpPro.GeneralIO;

namespace HuddleRoom
{
    public class ControlSystem : CrestronControlSystem
    {
        private Am300 _dmRx;
        private DmTx201C _dmTx;
        private GlsOdtCCn _occSensor;
        private CTimer _vacancyTimer;
        private Display _display;
        private AudioVideoSwitcher _switcher;

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
            ComPort.ComPortSpec displayComSpec = new ComPort.ComPortSpec {
                BaudRate = ComPort.eComBaudRates.ComspecBaudRate9600,
                DataBits = ComPort.eComDataBits.ComspecDataBits8,
                Parity = ComPort.eComParityType.ComspecParityNone,
                StopBits = ComPort.eComStopBits.ComspecStopBits1,
                SoftwareHandshake = ComPort.eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone,
                HardwareHandShake = ComPort.eComHardwareHandshakeType.ComspecHardwareHandshakeNone
            };

            try
            {
                _vacancyTimer = new CTimer(OnRoomVacantTimeout, Timeout.Infinite);

                if (this.SupportsEthernet)
                {
                    _dmRx = new Am300(0x15, this);
                    _display = new Display(_dmRx.ComPorts[1], displayComSpec);
                    _switcher = new AirMediaSwitcher(_dmRx);
                    if (_dmRx.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                        ErrorLog.Error("Unable to register {0} on IP ID {1}!", _dmRx.Name, _dmRx.ID);

                    _dmTx = new DmTx201C(0x14, this);
                    _dmTx.HdmiInput.InputStreamChange += OnLaptopHDMI;
                    _dmTx.VgaInput.InputStreamChange += OnLaptopVGA;
                    if (_dmTx.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                        ErrorLog.Error("Unable to register {0} on IP ID {1}!", _dmTx.Name, _dmTx.ID);
                }

                if (this.SupportsCresnet)
                {
                    _occSensor = new GlsOdtCCn(0x97, this);
                    _occSensor.GlsOccupancySensorChange += OnOccupancySensorChange;
                    if (_occSensor.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                        ErrorLog.Error("Unable to register {0} on Cresnet ID {1}!", _occSensor.Name, _occSensor.ID);
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        private void OnOccupancySensorChange(GlsOccupancySensorBase device, GlsOccupancySensorChangeEventArgs args)
        {
            switch (args.EventId)
            {
                case GlsOccupancySensorBase.RoomOccupiedFeedbackEventId:
                    _vacancyTimer.Stop();
                    TurnSystemOn();
                    break;
                case GlsOccupancySensorBase.RoomVacantFeedbackEventId:
                    _vacancyTimer.Reset(15 * 60 * 60 * 1000); // 15 minutes (in ms)
                    break;
            }
        }

        private void OnRoomVacantTimeout(Object o)
        {
            TurnSystemOff();
        }

        private void OnLaptopHDMI(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
        {
            var hdmiStream = inputStream as EndpointHdmiInput;
            var switcher = _switcher as AirMediaSwitcher;

            switch (args.EventId)
            {
                case EndpointInputStreamEventIds.SyncDetectedFeedbackEventId:
                    if (hdmiStream.SyncDetectedFeedback.BoolValue)
                        switcher.Switch(AirMediaInputs.DM);
                    else
                        switcher.Switch(AirMediaInputs.AirMedia);

                    break;
            }
        }

        private void OnLaptopVGA(EndpointInputStream inputStream, EndpointInputStreamEventArgs args)
        {
            var vgaStream = inputStream as EndpointVgaInput;
            var switcher = _switcher as AirMediaSwitcher;

            switch (args.EventId)
            {
                case EndpointInputStreamEventIds.SyncDetectedFeedbackEventId:
                    if (vgaStream.SyncDetectedFeedback.BoolValue)
                        switcher.Switch(AirMediaInputs.DM);
                    else
                        switcher.Switch(AirMediaInputs.AirMedia);

                    break;
            }
        }

        public void TurnSystemOn()
        {
            _display.PowerOn();
        }

        public void TurnSystemOff()
        {
            _display.PowerOff();
        }

        public void ShowAirMedia()
        {
            try
            {
                _dmRx.DisplayControl.VideoOut = AmX00DisplayControl.eAirMediaX00VideoSource.AirMedia;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Exception in ShowAirMedia: {0}", e.Message);
            }
        }

        public void ShowLaptop()
        {
            try
            {
                _dmRx.DisplayControl.VideoOut = AmX00DisplayControl.eAirMediaX00VideoSource.DM;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Exception in ShowLaptop: {0}", e.Message);
            }
        }
    }
}