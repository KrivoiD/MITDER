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
        public double TopTemperature {
            get { return _topTemperature; }
            set { RaisePropertyChanged("TopTemperature", ref _topTemperature, value); }
        }

        private double _resistanceValue;
        public double Resistance {
            get { return _resistanceValue; }
            set { RaisePropertyChanged("Resistance", ref _resistanceValue, value); }
        }

        private double _nextPoint;
        public double NextPoint {
            get { return _nextPoint; }
            set { RaisePropertyChanged("NextPoint", ref _nextPoint, value); }
        }

        /// <summary>
        /// Коллекция, содержащая измерянные значения.
        /// </summary>
        public ObservableCollection<MeasuredValues> MeasuredValuesCollection { get; set; }
        /// <summary>
        /// Коллекция, содержащая настройки этапов измерения.
        /// </summary>
        public ObservableCollection<StepSettings> StepSettings {
            get { return _core.MeasurementSteps; }
        }

        #endregion

        #region Commands

        #region Start command
        /// <summary>
        /// Комманда запуска измерения сопротивления
        /// </summary>
        public RelayCommand Start {
            get { return new RelayCommand(StartMeasurements, CanStartMeasurements); }
        }

        /// <summary>
        /// Запускает измерение сопротивления
        /// </summary>
        private void StartMeasurements(object obj)
        {
            _core.IsMeasurementStarted = true;
            _core.IsResistanceMeasured = true;
        }

        /// <summary>
        /// Возвращает возможность выполнения запуска измерения сопротивления.
        /// </summary>
        private bool CanStartMeasurements(object obj)
        {
            return _bottomThermocuple.IsInitialized && _resistanceDevice.IsInitialized;
        }
        #endregion //Start command

        #region Stop command
        public RelayCommand Stop {
            get { return new RelayCommand(StopMeasurements); }
        }

        /// <summary>
        /// Запускает измерение сопротивления
        /// </summary>
        private void StopMeasurements(object obj)
        {
            _core.IsResistanceMeasured = false;
            _core.IsMeasurementStarted = false;
        }
        #endregion //Stop command

        #endregion

        public MainWindowViewModel()
        {
            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["BottomVISA"])
                || string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TopVISA"])
                || string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["ResistanceVISA"]))
                throw new ApplicationException("Не указаны VISA-адреса в app.config.");

            _bottomThermocuple = new Agilent34410(ConfigurationManager.AppSettings["BottomVISA"]);
            _topThermocuple = new Agilent34410(ConfigurationManager.AppSettings["TopVISA"]);
            _resistanceDevice = new Agilent34410(ConfigurationManager.AppSettings["ResistanceVISA"]);

            MeasuredValuesCollection = new ObservableCollection<MeasuredValues>();

            _core = new MeasurementCore(_topThermocuple, _bottomThermocuple, _resistanceDevice);

            _core.MeasurementSteps.Add(new StepSettings()
            {
                From = -5.6,
                To = -0.2,
                Step = 0.2,
                PointRange = 0.1,
                Type = StepType.Heating
            });

            _core.MeasuredVoltage += _core_MeasuredVoltages;
            _core.MeasuredResistance += _core_MeasuredResistance;
        }

        private void _core_MeasuredVoltages(MeasuredValues value)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
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
            if (_core != null)
            {
                _core.IsMeasurementStarted = false;
                _core.IsResistanceMeasured = false;
                _core.MeasuredVoltage -= _core_MeasuredVoltages;
                _core.MeasuredResistance -= _core_MeasuredResistance;
                _core.Dispose();
            }
            if (MeasuredValuesCollection != null)
            {
                MeasuredValuesCollection.Clear();
                MeasuredValuesCollection = null;
            }
        }
    }
}
