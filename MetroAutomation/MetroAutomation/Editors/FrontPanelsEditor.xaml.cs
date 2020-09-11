using MetroAutomation.FrontPanel;

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
            FrontPanels = frontPanels;

            InitializeComponent();
        }

        public FrontPanels FrontPanels { get; }
    }
}
