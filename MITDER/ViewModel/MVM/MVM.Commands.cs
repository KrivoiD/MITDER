using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace MITDER.ViewModel
{
    public partial class MainViewModel : ViewModelBase
    {
        private void InitializeCommands()
        {
            ExitCommand = new RelayCommand(ExitMethod);
        }

        #region RelayCommands
        public RelayCommand ExitCommand { get; set; }
        #endregion

        #region Methods for Commands
        private void ExitMethod()
        {
            App.Current.MainWindow.Close();
        }
        #endregion
    }
}