using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MetroAutomation.FrontPanel
{
    public class FrontPanelTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Calibrator { get; set; }

        public DataTemplate Fluke8508 { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is FrontPanelViewModel frontPanel)
            {
                switch (frontPanel.Type)
                {
                    case FrontPanelType.Base:
                        {
                            return Calibrator;
                        }
                    case FrontPanelType.Fluke8508:
                        {
                            return Fluke8508;
                        }
                    default:
                        {
                            return null;
                        }

                }
            }
            else
            {
                return null;
            }
        }
    }
}
