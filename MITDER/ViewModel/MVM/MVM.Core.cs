using GalaSoft.MvvmLight;
using System;
using System.Linq;
using Ivi.Visa.Interop;
using MITDER.Model;

namespace MITDER.ViewModel
{
    public partial class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            InitializeCommands();

            ResourceManager mgr = new ResourceManager();
            string[] resourseNames = mgr.FindRsrc("USB?*");
            multimeter = new Multimeter<Agilent.Agilent34410.Interop.Agilent34410Class>(resourseNames[0]);
            multimeter.VoltageMeasured += Multimeter_VoltageMeasured;
            multimeter.MeasurementPeriod = 1000;
            multimeter.Start();
        }

        private void Multimeter_VoltageMeasured(double obj)
        {
            CurrentValue = obj;
        }

        #region Fields
        private double currentValue;
        private Multimeter<Agilent.Agilent34410.Interop.Agilent34410Class> multimeter;
        #endregion

        #region Properties
        public double CurrentValue {
            get => currentValue;
            set => Set(nameof(CurrentValue), ref currentValue, value);
        }
        #endregion

        public override void Cleanup()
        {
            multimeter.Stop();
            base.Cleanup();
        }
    }
}