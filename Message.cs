#region Using statements

using System.Runtime.Versioning;
using WeekNumberLite2.Properties;

#endregion Using statements

namespace WeekNumberLite2
{
    internal static class Message
    {
        #region Internal readonly strings

        internal static readonly string[] SWEDISH_DAY_OF_WEEK_PREFIX = { "Söndagen ", "Måndagen ", "Tisdagen ", "Onsdagen ", "Torsdagen ", "Fredagen ", "Lördagen " };
        internal static readonly string CAPTION = $"{Resources.ProductName} {Resources.Version} {Application.ProductVersion}";

        #endregion Internal readonly strings

        #region Show Information or Error dialog methods

        [SupportedOSPlatform("windows")]
        internal static void Show(string text, Exception? ex = null)
        {
            string message = ex is null ? text : $"{text}\r\n{ex}";
            Forms.MessageForm.DisplayMessage(message, ex is not null);
        }

        #endregion Show Information or Error dialog methods
    }
}
