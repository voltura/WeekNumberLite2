namespace WeekNumberLite2
{
    /// <summary>
    /// GUI Interface
    /// </summary>
    public interface IGui
    {
        /// <summary>
        /// Updates icon on GUI with given week number
        /// </summary>
        /// <param name="weekNumber">The week number to display on icon</param>
        void UpdateIcon(int weekNumber);

        /// <summary>
        /// Disposes GUI
        /// </summary>
        void Dispose();
    }
}