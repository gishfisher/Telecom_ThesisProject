using System.Windows;
using System.Windows.Controls;
using Telecom_ThesisProject.MVVM.ViewModel;

namespace Telecom_ThesisProject.MVVM.View
{
    public partial class AddressTreeView : UserControl
    {
        public AddressTreeView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is AddressTreeViewModel vm)
            {
                vm.SelectedNode = e.NewValue;
            }
        }
    }
}
