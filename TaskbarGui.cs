#region Using statements

using System.Runtime.Versioning;
using WeekNumberLite2.Properties;

#endregion

namespace WeekNumberLite2
{
    internal class TaskbarGui : IDisposable, IGui
    {
        #region Private variables

        private NotifyIcon? _notifyIcon;
        private readonly WeekNumberLite2ContextMenu _contextMenu;
        private int _latestWeek;

        #endregion Private variables

        #region Constructor

        [SupportedOSPlatform("windows")]
        internal TaskbarGui(int week)
        {
            _latestWeek = week;
            _contextMenu = new WeekNumberLite2ContextMenu();
            _notifyIcon = GetNotifyIcon(_contextMenu.ContextMenu);
            UpdateIcon(week, ref _notifyIcon);
        }

        #endregion Constructor

        #region Public UpdateIcon method

        /// <summary>
        /// Updates icon on GUI with given week number
        /// </summary>
        /// <param name="weekNumber">The week number to display on icon</param>
        [SupportedOSPlatform("windows")]
        public void UpdateIcon(int weekNumber) => UpdateIcon(weekNumber, ref _notifyIcon);

        #endregion Public UpdateIcon method

        #region Private UpdateIcon method

        [SupportedOSPlatform("windows")]
        private void UpdateIcon(int weekNumber, ref NotifyIcon? notifyIcon)
        {
            try
            {
                string weekDayPrefix = string.Empty;
                string longDateString = DateTime.Now.ToLongDateString();
                const string SWEDISH_LONG_DATE_PREFIX_STRING = "den ";
                if (Thread.CurrentThread.CurrentUICulture.Name == Resources.Swedish || longDateString.StartsWith(SWEDISH_LONG_DATE_PREFIX_STRING))
                    weekDayPrefix = Message.SWEDISH_DAY_OF_WEEK_PREFIX[(int)DateTime.Now.DayOfWeek];
                if (notifyIcon != null)
                {
                    notifyIcon.Text = $"{Resources.Week} {weekNumber}\r\n{weekDayPrefix}{DateTime.Now.ToLongDateString()}";
                    Icon prevIcon = notifyIcon.Icon;
                    notifyIcon.Icon = WeekIcon.GetIcon(weekNumber);
                    WeekIcon.CleanupIcon(ref prevIcon);
                }
            }
            finally
            {
                if (_latestWeek != weekNumber) _latestWeek = weekNumber;
            }
        }

        #endregion Private UpdateIcon method

        #region Private helper property to create NotifyIcon

        private static NotifyIcon GetNotifyIcon(ContextMenuStrip? contextMenu) => new() { Visible = true, ContextMenuStrip = contextMenu };

        #endregion Private helper property to create NotifyIcon

        #region IDisposable methods

        /// <summary>
        /// Disposes the GUI resources
        /// </summary>
        [SupportedOSPlatform("windows")]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SupportedOSPlatform("windows")]
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            CleanupNotifyIcon();
            _contextMenu.Dispose();
        }

        [SupportedOSPlatform("windows")]
        private void CleanupNotifyIcon()
        {
            if (_notifyIcon is null) return;
            _notifyIcon.Visible = false;
            if (_notifyIcon.Icon != null)
            {
                NativeMethods.DestroyIcon(_notifyIcon.Icon.Handle);
                _notifyIcon.Icon?.Dispose();
            }
            _notifyIcon.ContextMenuStrip?.Items.Clear();
            _notifyIcon.ContextMenuStrip?.Dispose();
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        #endregion IDisposable methods
    }
}