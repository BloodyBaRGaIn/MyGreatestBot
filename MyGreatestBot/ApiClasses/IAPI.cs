using System;

namespace MyGreatestBot.ApiClasses
{
    /// <summary>
    /// Interface for APIs initialization
    /// </summary>
    public interface IAPI : IDisposable
    {
        /// <summary>
        /// API flag
        /// </summary>
        ApiIntents ApiType { get; }

        /// <summary>
        /// Is API essential for bot running
        /// </summary>
        virtual bool IsEssential => false;

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

        void IDisposable.Dispose()
        {
            Logout();
            GC.SuppressFinalize(this);
        }
    }
}
