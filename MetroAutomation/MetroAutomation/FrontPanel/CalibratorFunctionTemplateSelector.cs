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

        public DataTemplate SetACV { get; set; }

        public DataTemplate SetDCI { get; set; }

        public DataTemplate SetACI { get; set; }

        public DataTemplate SetRES2W { get; set; }

        public DataTemplate SetRES4W { get; set; }

        public DataTemplate GetRES4W { get; set; }

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
                    case Mode.SetACV:
                        {
                            return SetACV ?? Default;
                        }
                    case Mode.SetDCI:
                        {
                            return SetDCI ?? Default;
                        }
                    case Mode.SetACI:
                        {
                            return SetACI ?? Default;
                        }
                    case Mode.SetRES2W:
                        {
                            return SetRES2W ?? Default;
                        }
                    case Mode.SetRES4W:
                        {
                            return SetRES4W ?? Default;
                        }
                    case Mode.GetRES4W:
                        {
                            return GetRES4W ?? Default;
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
