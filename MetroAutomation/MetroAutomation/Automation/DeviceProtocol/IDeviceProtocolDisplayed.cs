﻿using System;

namespace MetroAutomation.Automation
{
    public interface IDeviceProtocolDisplayed
    {
        int ID { get; set; }

        string Name { get; set; }

        string AccountInfo { get; set; }

        string ProtocolNumber { get; set; }

        DateTime CalibrationDate { get; set; }

        string DeviceOwner { get; set; }

        string Grsi { get; set; }

        string SerialNumber { get; set; }

        string Type { get; set; }

        WorkStatus WorkStatus { get; set; }
    }
}