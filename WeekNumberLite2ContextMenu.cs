#region Using statements

using WeekNumberLite2.Properties;

#endregion

namespace WeekNumberLite2
{
    internal class WeekNumberLite2ContextMenu : IDisposable
    {
        #region Internal context menu

        internal ContextMenuStrip? ContextMenu { get; private set; }

        #endregion Internal context menu

        #region Internal contructor

        internal WeekNumberLite2ContextMenu()
        {
            CreateContextMenu();
        }

        #endregion Internal constructor

        #region Private event handling

        private static void ExitMenuClick(object? o, EventArgs e)
        {
            Application.Exit();
        }

        private void AboutClick(object? o, EventArgs e)
        {
            ToolStripMenuItem? mi = o is null ? null : (ToolStripMenuItem)o;
            try
            {
                if (mi != null)
                {
                    mi.Enabled = false;
                }

                Forms.MessageForm.LogAndDisplayLinkMessage(Resources.About, Resources.APPLICATION_URL);
            }
            finally
            {
                EnableMenuItem(mi);
            }
        }

        #endregion Private event handling

        #region Private method for context menu creation

        internal void CreateContextMenu()
        {
            ContextMenu = new ContextMenuStrip();
            ToolStripMenuItem AboutMenu = new(Resources.AboutMenu);
            ToolStripMenuItem ExitMenu = new(Resources.ExitMenu);
            AboutMenu.Click += AboutClick;
            ExitMenu.Click += ExitMenuClick;
            _ = ContextMenu.Items.Add(AboutMenu);
            _ = ContextMenu.Items.Add(ExitMenu);
        }

        #endregion Private method for context menu creation

        #region Private helper methods for menu items

        private static void EnableMenuItem(ToolStripMenuItem? mi)
        {
            if (mi != null)
            {
                mi.Enabled = true;
            }
        }

        #endregion Private helper methods for menu items

        #region IDisposable methods

        /// <summary>
        /// Disposes the context menu
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            CleanupContextMenu();
        }

        private void CleanupContextMenu()
        {
            ContextMenu?.Dispose();
            ContextMenu = null;
        }

        #endregion IDisposable methods
    }
}