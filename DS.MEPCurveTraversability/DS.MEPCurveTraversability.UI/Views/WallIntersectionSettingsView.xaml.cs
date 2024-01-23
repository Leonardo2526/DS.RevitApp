using DS.MEPCurveTraversability.Presenters;
using MahApps.Metro.Controls;

namespace DS.MEPCurveTraversability.UI
{
    public partial class WallIntersectionSettingsView : MetroWindow
    {
        public WallIntersectionSettingsView(WallCheckerViewModel wallCheckerViewModel)
        {
            InitializeComponent();
            this.DataContext = wallCheckerViewModel;
            Show();
        }
    }
}
