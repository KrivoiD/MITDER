using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Multimeters;
using System.Configuration;
using Core;
using MITDER.ViewModelClasses;
using System.Collections.ObjectModel;

namespace MITDER.ViewModel
{
	public class MainWindowViewModel : ViewModelBase
    {
        #region Properties

        Agilent34410 _bottomThermocuple = null;
        Agilent34410 _topThermocuple = null;
        Agilent34410 _resistanceDevice = null;
        MeasurementCore _core = null;

        private double _bottomTemperature;
        public double BottomTemperature {
            get { return _bottomTemperature; }
            set { RaisePropertyChanged("BottomTemperature", ref _bottomTemperature, value); }
        }

        private double _topTemperature;
        public double TopTemperature
        {
            get { return _topTemperature; }
			set { RaisePropertyChanged("TopTemperature", ref _topTemperature, value); }
        }

        private double _resistanceValue;
        public double Resistance
        {
            get { return _resistanceValue; }
			set { RaisePropertyChanged("Resistance", ref _resistanceValue, value); }
        }

        private double _nextPoint;
        public double NextPoint
        {
            get { return _nextPoint; }
			set { RaisePropertyChanged("NextPoint", ref _nextPoint, value); }
        }

		public StepSettings MeasurementSettings { get; set; }

        /// <summary>
        /// Коллекция, содержащая измерянные значения.
        /// </summary>
        public ObservableCollection<MeasuredValues> MeasuredValuesCollection { get; set; }
        /// <summary>
        /// Коллекция, содержащая настройки этапов измерения.
        /// </summary>
        public ObservableCollection<StepSettings> StepSettingsCollection { get; set; }

        #endregion

        public MainWindowViewModel()
        {
            if (   string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["BottomVISA"])
                || string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TopVISA"])
                || string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["ResistanceVISA"]))
                throw new ApplicationException("Не указаны VISA-адреса в app.config.");

            _bottomThermocuple = new Agilent34410(ConfigurationManager.AppSettings["BottomVISA"]);
            _topThermocuple = new Agilent34410(ConfigurationManager.AppSettings["TopVISA"]);
            _resistanceDevice = new Agilent34410(ConfigurationManager.AppSettings["ResistanceVISA"]);

            MeasuredValuesCollection = new ObservableCollection<MeasuredValues>();
            StepSettingsCollection = new ObservableCollection<StepSettings>();

            MeasurementSettings = new StepSettings()
			{
				From = -5.6,
				To = -0.2,
				Step = 0.2,
				PointRange = 0.1
			};

			_core = new MeasurementCore(_topThermocuple, _bottomThermocuple, _resistanceDevice, MeasurementSettings);
            _core.MeasuredVoltage += _core_MeasuredVoltages;
            _core.MeasuredResistance += _core_MeasuredResistance;
            _core.IsResistanceMeasured = true;
        }

        private void _core_MeasuredVoltages(MeasuredValues value)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                BottomTemperature = value.BottomTemperature;
                TopTemperature = value.TopTemperature;
                NextPoint = _core.Next;
            }));
        }

        private void _core_MeasuredResistance(MeasuredValues value)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Resistance = value.Resistance;
                MeasuredValuesCollection.Insert(0, value);
            }));
        }
    
        public override void Dispose()
        {
            if(MeasuredValuesCollection != null)
                MeasuredValuesCollection.Clear();
            MeasuredValuesCollection = null;
            if (_core != null)
            {
                _core.MeasuredVoltage -= _core_MeasuredVoltages;
                _core.MeasuredResistance -= _core_MeasuredResistance;
                _core.Dispose();
            }
        }
    }
}
