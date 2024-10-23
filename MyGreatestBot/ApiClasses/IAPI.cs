﻿using System;

namespace MyGreatestBot.ApiClasses
{
    /// <summary>
    /// Interface for APIs initialization.
    /// </summary>
    public interface IAPI : IDisposable
    {
        /// <summary>
        /// API flag.
        /// </summary>
        public abstract ApiIntents ApiType { get; }

        public ApiStatus Status
        {
            get => OldStatus;
            protected set
            {
                if (OldStatus != value)
                {
                    OldStatus = value;
                    DiscordWrapper.CurrentDomainLogHandler.Send(GetApiStatusString(), LogLevel.Debug);
                }
            }
        }

        protected ApiStatus OldStatus { get; set; }

        /// <summary>
        /// Is API essential for bot running.
        /// </summary>
        public virtual bool IsEssential => false;

        /// <summary>
        /// Performs log in.
        /// </summary>
        public sealed void PerformAuth()
        {
            switch (Status)
            {
                case ApiStatus.NotInitialized:
                case ApiStatus.Failed:
                    break;

                default:
                    return;
            }

            try
            {
                PerformAuthInternal();
                Status = ApiStatus.Success;
            }
            catch
            {
                Status = ApiStatus.Failed;
                throw;
            }
        }

        protected abstract void PerformAuthInternal();

        /// <summary>
        /// Performs log out.
        /// </summary>
        public sealed void Logout()
        {
            switch (Status)
            {
                case ApiStatus.NotInitialized:
                case ApiStatus.DeinitSkip:
                    return;

                case ApiStatus.InitSkip:
                    Status = ApiStatus.NotInitialized;
                    return;

                default:
                    Status = ApiStatus.Deinit;
                    break;
            }

            try
            {
                LogoutInternal();
                Status = ApiStatus.NotInitialized;
            }
            catch
            {
                Status = ApiStatus.NotInitialized;
                throw;
            }
        }

        protected abstract void LogoutInternal();

        public sealed void SetStatus(ApiStatus status)
        {
            Status = status;
        }

        public sealed string GetApiStatusString()
        {
            return $"{ApiType} {Status}.";
        }

        void IDisposable.Dispose()
        {
            Logout();
            GC.SuppressFinalize(this);
        }
    }
}
