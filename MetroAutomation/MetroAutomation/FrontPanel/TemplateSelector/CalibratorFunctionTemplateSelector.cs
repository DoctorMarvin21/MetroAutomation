﻿using MetroAutomation.Calibration;
using System.Windows;
using System.Windows.Controls;

namespace MetroAutomation.FrontPanel
{
    public class FunctionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Default { get; set; } = new DataTemplate();

        public DataTemplate GetDCV { get; set; }

        public DataTemplate SetDCV { get; set; }

        public DataTemplate GetACV { get; set; }

        public DataTemplate SetACV { get; set; }

        public DataTemplate GetDCI { get; set; }

        public DataTemplate SetDCI { get; set; }

        public DataTemplate GetACI { get; set; }

        public DataTemplate SetACI { get; set; }

        public DataTemplate GetRES2W { get; set; }

        public DataTemplate SetRES2W { get; set; }

        public DataTemplate GetRES4W { get; set; }

        public DataTemplate SetRES4W { get; set; }

        public DataTemplate GetCAP2W { get; set; }

        public DataTemplate SetCAP2W { get; set; }

        public DataTemplate GetCAP4W { get; set; }

        public DataTemplate SetCAP4W { get; set; }

        public DataTemplate GetIND2W { get; set; }

        public DataTemplate SetIND2W { get; set; }

        public DataTemplate GetIND4W { get; set; }

        public DataTemplate SetIND4W { get; set; }

        public DataTemplate GetADM4W { get; set; }

        public DataTemplate SetADM4W { get; set; }

        public DataTemplate GetDCP { get; set; }

        public DataTemplate SetDCP { get; set; }

        public DataTemplate GetACP { get; set; }

        public DataTemplate SetACP { get; set; }

        public DataTemplate SetDCV_DCV { get; set; }

        public DataTemplate SetACV_ACV { get; set; }

        public DataTemplate GetTEMP { get; set; }

        public DataTemplate SetTEMP { get; set; }

        public DataTemplate GetFREQ { get; set; }

        public DataTemplate SetFREQ { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Function function)
            {
                return GetTemplate(function.Mode) ?? Default;
            }
            else
            {
                return Default;
            }
        }

        private DataTemplate GetTemplate(Mode mode)
        {
            switch (mode)
            {
                case Mode.GetDCV:
                    {
                        return GetDCV;
                    }
                case Mode.SetDCV:
                    {
                        return SetDCV;
                    }
                case Mode.GetACV:
                    {
                        return GetACV;
                    }
                case Mode.SetACV:
                    {
                        return SetACV;
                    }
                case Mode.GetDCI:
                    {
                        return GetDCI;
                    }
                case Mode.SetDCI:
                    {
                        return SetDCI;
                    }
                case Mode.GetACI:
                    {
                        return GetACI;
                    }
                case Mode.SetACI:
                    {
                        return SetACI;
                    }
                case Mode.GetRES2W:
                    {
                        return GetRES2W;
                    }
                case Mode.SetRES2W:
                    {
                        return SetRES2W;
                    }
                case Mode.GetRES4W:
                    {
                        return GetRES4W;
                    }
                case Mode.SetRES4W:
                    {
                        return SetRES4W;
                    }
                case Mode.GetCAP2W:
                    {
                        return GetCAP2W;
                    }
                case Mode.SetCAP2W:
                    {
                        return SetCAP2W;
                    }
                case Mode.GetCAP4W:
                    {
                        return GetCAP4W;
                    }
                case Mode.SetCAP4W:
                    {
                        return SetCAP4W;
                    }
                case Mode.GetIND2W:
                    {
                        return GetIND2W;
                    }
                case Mode.SetIND2W:
                    {
                        return SetIND2W;
                    }
                case Mode.GetIND4W:
                    {
                        return GetIND4W;
                    }
                case Mode.SetIND4W:
                    {
                        return SetIND4W;
                    }
                case Mode.GetADM4W:
                    {
                        return GetADM4W;
                    }
                case Mode.SetADM4W:
                    {
                        return SetADM4W;
                    }
                case Mode.GetDCP:
                    {
                        return GetDCP;
                    }
                case Mode.SetDCP:
                    {
                        return SetDCP;
                    }
                case Mode.GetACP:
                    {
                        return GetACP;
                    }
                case Mode.SetACP:
                    {
                        return SetACP;
                    }
                case Mode.SetDCV_DCV:
                    {
                        return SetDCV_DCV;
                    }
                case Mode.SetACV_ACV:
                    {
                        return SetACV_ACV;
                    }
                case Mode.GetTEMP:
                    {
                        return GetTEMP;
                    }
                case Mode.SetTEMP:
                    {
                        return SetTEMP;
                    }
                case Mode.GetFREQ:
                    {
                        return GetFREQ;
                    }
                case Mode.SetFREQ:
                    {
                        return SetFREQ;
                    }
                default:
                    {
                        return null;
                    }
            }
        }
    }
}
