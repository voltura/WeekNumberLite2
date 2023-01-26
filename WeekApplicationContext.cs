#region Using statements

using System.Runtime.Versioning;
using WeekNumberLite2.Properties;

#endregion

namespace WeekNumberLite2
{
    internal class WeekApplicationContext : ApplicationContext
    {
        #region Internal Taskbar GUI

        internal IGui? Gui;

        #endregion Internal Taskbar GUI

        #region Private variables

        private readonly System.Windows.Forms.Timer? _timer;
        private int _currentWeek;

        #endregion Private variables

        #region Constructor

        [SupportedOSPlatform("windows")]
        internal WeekApplicationContext()
        {
            try
            {
                Application.ApplicationExit += OnApplicationExit;
                _currentWeek = Week.Current();
                Gui = new TaskbarGui(_currentWeek);
                _timer = GetTimer;
            }
            catch (Exception ex)
            {
                _timer?.Stop();
                Message.Show(Resources.UnhandledException, ex);
                Application.Exit();
            }
        }

        #endregion Constructor

        #region Private Timer property

        [SupportedOSPlatform("windows")]
        private System.Windows.Forms.Timer GetTimer
        {
            get
            {
                if (_timer != null)
                {
                    return _timer;
                }

                System.Windows.Forms.Timer timer = new() { Interval = 10000, Enabled = true };
                timer.Tick += OnTimerTick;
                return timer;
            }
        }

        #endregion Private Timer property

        #region Private event handlers

        private void OnApplicationExit(object? sender, EventArgs e)
        {
            Cleanup(false);
        }

        [SupportedOSPlatform("windows")]
        private void OnTimerTick(object? sender, EventArgs e)
        {
            UpdateIcon();
        }

        [SupportedOSPlatform("windows")]
        private void UpdateIcon()
        {
            if (_currentWeek == Week.Current())
            {
                return;
            }

            _timer?.Stop();
            Application.DoEvents();
            try
            {
                _currentWeek = Week.Current();
                Gui?.UpdateIcon(_currentWeek);
            }
            catch (Exception ex)
            {
                Message.Show(Resources.FailedToSetIcon, ex);
                Cleanup();
                throw;
            }
            if (_timer != null)
            {
                _timer.Interval = 10000;
                _timer.Start();
            }
        }

        #endregion Private event handlers

        #region Private methods

        private void Cleanup(bool forceExit = true)
        {
            _timer?.Stop();
            _timer?.Dispose();
            Gui?.Dispose();
            Gui = null;
            if (forceExit)
            {
                Application.Exit();
            }
        }

        #endregion Private methods
    }
}