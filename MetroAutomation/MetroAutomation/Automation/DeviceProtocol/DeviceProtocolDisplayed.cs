﻿using MetroAutomation.Model;
using System;

namespace MetroAutomation.Automation
{
    public class DeviceProtocolDisplayed : IDataObject, IDeviceProtocolDisplayed
    {
        public Guid ID { get; set; }

        public string Name { get; set; }

        public string AccountInfo { get; set; }

        public string ProtocolNumber { get; set; }

        public string Grsi { get; set; }

        public string Type { get; set; }

        public string SerialNumber { get; set; }

        public string DeviceOwner { get; set; }

        public DateTime CalibrationDate { get; set; }

        public WorkStatus WorkStatus { get; set; }

        public override string ToString()
        {
            return $"{Name} {Type}";
        }
    }
}
