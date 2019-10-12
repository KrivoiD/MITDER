using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Multimeters;
using System.Configuration;
using Core;

namespace MITDER.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Properties

        Agilent34410 _bottomThermocuple = null;
        Agilent34410 _topThermocuple = null;
        Agilent34410 _resistanceDevice = null;
        MeasurementCore _core = null;

        private double _bottomTemperature;
        public double BottomTemperature {
            get { return _bottomTemperature; }
            set {
                _bottomTemperature = value;
                OnPropertyChanged("BottomTemperature");
            }
        }

        private double _topTemperature;
        public double TopTemperature
        {
            get { return _topTemperature; }
            set
            {
                _topTemperature = value;
                OnPropertyChanged("TopTemperature");
            }
        }

        private double _resistanceValue;
        public double Resistance
        {
            get { return _resistanceValue; }
            set
            {
                _resistanceValue = value;
                OnPropertyChanged("Resistance");
            }
        }

        private double _nextPoint;
        public double NextPoint
        {
            get { return _nextPoint; }
            set
            {
                if (_nextPoint == value)
                    return;
                _nextPoint = value;
                OnPropertyChanged("NextPoint");
            }
        }

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
            _core = new MeasurementCore(_topThermocuple, _bottomThermocuple, _resistanceDevice);
            _core.MeasuredVoltage += _core_MeasuredVoltages;
            _core.MeasuredResistance += _core_MeasuredResistance;
            _core.From = -5.6;
            _core.To = -0.2;
            _core.Step = 0.2;
            _core.PointRange = 0.1;
            _core.IsResistanceMeasured = true;
        }

        private void _core_MeasuredVoltages(double value)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() => {
                BottomTemperature = _core.BottomTemperature;
                TopTemperature = _core.TopTemperature;
                NextPoint = _core.Next;
            }));
        }

        private void _core_MeasuredResistance(double value)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Resistance = _core.Resistance;
            }));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    
        public void  Dispose()
        {
            if (_core != null)
            {
                _core.MeasuredVoltage -= _core_MeasuredVoltages;
                _core.MeasuredResistance -= _core_MeasuredResistance;
                _core.Dispose();
            }
        }
    }
}
