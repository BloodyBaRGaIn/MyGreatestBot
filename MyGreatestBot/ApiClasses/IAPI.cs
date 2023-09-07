namespace MyGreatestBot.ApiClasses
{
    /// <summary>
    /// Interface for APIs initialization
    /// </summary>
    public interface IAPI
    {
        /// <summary>
        /// API flag
        /// </summary>
        ApiIntents ApiType { get; }

        /// <summary>
        /// Performs log in
        /// </summary>
        virtual void PerformAuth()
        {

        }

        /// <summary>
        /// Performs log out
        /// </summary>
        virtual void Logout()
        {

        }
    }
}
