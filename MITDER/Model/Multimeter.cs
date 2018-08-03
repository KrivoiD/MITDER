using Agilent.Agilent34410.Interop;
using Ivi.Dmm.Interop;
using Ivi.Driver.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace MITDER.Model
{
    class Multimeter<T> where T : IAgilent34410
    {
        private BackgroundWorker worker;
        private T driver;
        private int measurementPeriod;

        /// <summary>
        /// Время в миллисекундах перед повторным измерением
        /// </summary>
        public int MeasurementPeriod {
            get => measurementPeriod;
            set {
                if (value < 0) throw new ArgumentOutOfRangeException();
                measurementPeriod = value;
            }
        }

        public string ResourceName { get; private set; }

        public event Action<double> VoltageMeasured;

        public Multimeter(string resourceName)
        {
            ResourceName = resourceName;
        }

        public void Start()
        {
            InitializeDevice();
            InitializeBackgroundWorker();
        }

        public void Stop()
        {
            if(worker != null && worker.IsBusy)
                worker.CancelAsync();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            worker.DoWork -= Worker_DoWork;
            worker.RunWorkerCompleted -= Worker_RunWorkerCompleted;
            worker.Dispose();
            worker = null;
            driver?.Close();
            driver = default(T);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
#if DEBUG
            Random rand = new Random((int)DateTime.Now.Ticks);
#endif
            double value = 0;
            while (worker.CancellationPending != true)
            {
#if DEBUG
                value = rand.NextDouble() * rand.Next(30);
#else
                if (!driver.Initialized) InitializeDevice();
                value = driver.Voltage.DCVoltage.Measure(0.01, Agilent34410ResolutionEnum.Agilent34410ResolutionDefault);
#endif
                VoltageMeasured?.BeginInvoke(value, null, @object: null);
                Thread.Sleep(MeasurementPeriod);
            }
        }

        private void InitializeBackgroundWorker()
        {
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void InitializeDevice()
        {
            string standartInitOptions = "Cache=true, InterchangeCheck=false, QueryInstrStatus=true, RangeCheck=true, RecordCoercions=false, Simulate=false";
            driver = Activator.CreateInstance<T>();
            driver.Initialize(ResourceName, false, false, standartInitOptions);
            driver.Trigger.TriggerSource = Agilent34410TriggerSourceEnum.Agilent34410TriggerSourceInternal;
            driver.Trigger.TriggerCount = 5;
            driver.Trigger.SampleCount = 1;
        }
    }
}
