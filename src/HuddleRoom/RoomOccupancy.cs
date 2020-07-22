using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.GeneralIO;

namespace HuddleRoom
{
    public class RoomOccupancy
    {
        public delegate void OccupancyChangeHandler();

        private GlsOccupancySensorBase _sensor;
        private CTimer _vacancyTimer;
        private long _timeout;

        public OccupancyChangeHandler RoomOccupied;
        public OccupancyChangeHandler RoomVacant;

        public RoomOccupancy(GlsOccupancySensorBase sensor, int seconds)
        {
            _sensor = sensor;
            _sensor.GlsOccupancySensorChange += OnOccupancySensorChange;

            _vacancyTimer = new CTimer(OnRoomVacantTimeout, Timeout.Infinite);

            _timeout = seconds * 1000;
        }

        private void OnOccupancySensorChange(GlsOccupancySensorBase device, GlsOccupancySensorChangeEventArgs args)
        {
            switch (args.EventId)
            {
                case GlsOccupancySensorBase.RoomOccupiedFeedbackEventId:
                    _vacancyTimer.Stop();
                    if (RoomOccupied != null)
                        RoomOccupied();
                    break;
                case GlsOccupancySensorBase.RoomVacantFeedbackEventId:
                    _vacancyTimer.Reset(_timeout);
                    break;
            }
        }

        private void OnRoomVacantTimeout(Object o)
        {
            if (RoomVacant != null)
                RoomVacant();
        }
    }
}