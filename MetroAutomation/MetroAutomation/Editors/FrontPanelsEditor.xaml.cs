using MetroAutomation.FrontPanel;
using System.ComponentModel;

namespace MetroAutomation.Editors
{
    /// <summary>
    /// Interaction logic for FrontPanelsEditor.xaml
    /// </summary>
    public partial class FrontPanelsEditor : BaseEditorDialog
    {
        public FrontPanelsEditor(FrontPanels frontPanels)
            : base(frontPanels)
        {
            frontPanels.OnBeginEdit();
            FrontPanels = frontPanels;

            InitializeComponent();
        }

        public FrontPanels FrontPanels { get; }

        protected override void OnClosing(CancelEventArgs e)
        {
            FrontPanels.OnEndEdit();
            base.OnClosing(e);
        }
    }
}
