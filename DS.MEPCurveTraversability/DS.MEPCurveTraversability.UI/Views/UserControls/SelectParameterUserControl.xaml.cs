using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DS.MEPCurveTraversability.UI.View.UserControls
{
    /// <summary>
    /// Interaction logic for SelectParameterUserControl.xaml
    /// </summary>
    public partial class SelectParameterUserControl : UserControl
    {
        public SelectParameterUserControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DescriptionProperty =
          DependencyProperty.Register(
              nameof(Description),
              typeof(string),
              typeof(SelectParameterUserControl),
              new PropertyMetadata(nameof(Description)));

        public static readonly DependencyProperty ValuesProperty =
            DependencyProperty.Register(
                nameof(Values),
                typeof(List<int>),
                typeof(SelectParameterUserControl),
                new PropertyMetadata(new List<int>() { 90}));

        public static readonly DependencyProperty ValueProperty =
             DependencyProperty.Register(
                 nameof(Value),
                 typeof(int),
                 typeof(SelectParameterUserControl),
                 new PropertyMetadata(90));

        public string Description
        {
            get { return (string)this.GetValue(DescriptionProperty); }
            set { this.SetValue(DescriptionProperty, value); }
        }      

        public List<int> Values
        {
            get { return (List<int>)this.GetValue(ValuesProperty); }
            set { this.SetValue(ValuesProperty, value); }
        }

        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }
    }
}
