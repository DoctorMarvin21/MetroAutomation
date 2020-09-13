using MetroAutomation.Calibration;
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

        public DataTemplate GetDCP { get; set; }

        public DataTemplate SetDCP { get; set; }

        public DataTemplate GetACP { get; set; }

        public DataTemplate SetACP { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Function function)
            {
                switch (function.Mode)
                {
                    case Mode.GetDCV:
                        {
                            return GetDCV ?? Default;
                        }
                    case Mode.SetDCV:
                        {
                            return SetDCV ?? Default;
                        }
                    case Mode.GetACV:
                        {
                            return GetACV ?? Default;
                        }
                    case Mode.SetACV:
                        {
                            return SetACV ?? Default;
                        }
                    case Mode.GetDCI:
                        {
                            return GetDCI ?? Default;
                        }
                    case Mode.SetDCI:
                        {
                            return SetDCI ?? Default;
                        }
                    case Mode.GetACI:
                        {
                            return GetACI ?? Default;
                        }
                    case Mode.SetACI:
                        {
                            return SetACI ?? Default;
                        }
                    case Mode.GetRES2W:
                        {
                            return GetRES2W ?? Default;
                        }
                    case Mode.SetRES2W:
                        {
                            return SetRES2W ?? Default;
                        }
                    case Mode.GetRES4W:
                        {
                            return GetRES4W ?? Default;
                        }
                    case Mode.SetRES4W:
                        {
                            return SetRES4W ?? Default;
                        }
                    case Mode.GetCAP2W:
                        {
                            return GetCAP2W ?? Default;
                        }
                    case Mode.SetCAP2W:
                        {
                            return SetCAP2W ?? Default;
                        }
                    case Mode.GetCAP4W:
                        {
                            return GetCAP4W ?? Default;
                        }
                    case Mode.SetCAP4W:
                        {
                            return SetCAP4W ?? Default;
                        }
                    case Mode.GetDCP:
                        {
                            return GetDCP ?? Default;
                        }
                    case Mode.SetDCP:
                        {
                            return SetDCP ?? Default;
                        }
                    case Mode.GetACP:
                        {
                            return GetACP ?? Default;
                        }
                    case Mode.SetACP:
                        {
                            return SetACP ?? Default;
                        }
                    default:
                        {
                            return Default;
                        }
                }
            }
            else
            {
                return Default;
            }
        }
    }
}
