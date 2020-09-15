﻿using MetroAutomation.ViewModel;

namespace MetroAutomation.Calibration
{
    public enum Mode
    {
        [ExtendedDescription("DCV", "Напряжение постоянного тока", "Измерение напряжения постоянного тока")]
        GetDCV,
        [ExtendedDescription("DCV", "Напряжение постоянного тока", "Установка напряжения постоянного тока")]
        SetDCV,
        [ExtendedDescription("ACV", "Напряжение переменного тока", "Измерение напряжения переменного тока")]
        GetACV,
        [ExtendedDescription("ACV", "Напряжение переменного тока", "Установка напряжения переменного тока")]
        SetACV,
        [ExtendedDescription("DCI", "Сила постоянного тока", "Измерение силы постоянного тока")]
        GetDCI,
        [ExtendedDescription("DCI", "Сила постоянного тока", "Установка силы постоянного тока")]
        SetDCI,
        [ExtendedDescription("ACI", "Сила переменного тока", "Измерение силы переменного тока")]
        GetACI,
        [ExtendedDescription("ACI", "Сила переменного тока", "Установка силы переменного тока")]
        SetACI,
        [ExtendedDescription("RES2W", "Сопротивление по двухпроводной схеме", "Измерение сопротивления по двухпроводной схеме")]
        GetRES2W,
        [ExtendedDescription("RES2W", "Сопротивление по двухпроводной схеме", "Установка сопротивления по двухпроводной схеме")]
        SetRES2W,
        [ExtendedDescription("RES4W", "Сопротивление по четырехпроводной схеме", "Измерение сопротивления по четырехпроводной схеме")]
        GetRES4W,
        [ExtendedDescription("RES4W", "Сопротивление по четырехпроводной схеме", "Установка сопротивления по четырехпроводной схеме")]
        SetRES4W,
        [ExtendedDescription("CAP2W", "Емкость по двухпроводной схеме", "Измерение емкости по двухпроводной схеме")]
        GetCAP2W,
        [ExtendedDescription("CAP2W", "Емкость по двухпроводной схеме", "Установка емкости по двухпроводной схеме")]
        SetCAP2W,
        [ExtendedDescription("CAP4W", "Емкость по четырехпроводной схеме", "Измерение емкости по четырехпроводной схеме")]
        GetCAP4W,
        [ExtendedDescription("CAP4W", "Емкость по четырехпроводной схеме", "Установка емкости по четырехпроводной схеме")]
        SetCAP4W,
        [ExtendedDescription("DCP", "Мощность постоянного тока", "Измерение мощности постоянного тока")]
        GetDCP,
        [ExtendedDescription("DCP", "Мощность постоянного тока", "Установка мощности постоянного тока")]
        SetDCP,
        [ExtendedDescription("ACP", "Мощность переменного тока", "Измерение мощности переменного тока")]
        GetACP,
        [ExtendedDescription("ACP", "Мощность переменного тока", "Установка мощности переменного тока")]
        SetACP
    }
}
