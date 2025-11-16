using System;

namespace EShellULoadSetTools.Services
{
    /// <summary>
    /// Provides a lightweight abstraction around the SAFE <c>cSapModel</c> reference.
    /// Unlike the ETABS connection service, SAFE is typically automated via COM
    /// from an external process, therefore this service focuses on safely storing
    /// and exposing the model instance for later use.
    /// </summary>
    public interface ISafeConnectionService
    {
        /// <summary>
        /// Initializes the service with the SAFE model reference provided by the caller.
        /// </summary>
        /// <param name="safeSapModel">SAFE model object (usually SAFEv1.cSapModel).</param>
        void Initialize(object safeSapModel);

        /// <summary>
        /// Indicates whether the SAFE model reference has been initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Safely executes an action against the SAFE model after ensuring it has been initialized.
        /// </summary>
        /// <param name="action">Callback that receives the SAFE model reference.</param>
        void Execute(Action<dynamic> action);

        /// <summary>
        /// Safely executes a function against the SAFE model after ensuring it has been initialized.
        /// </summary>
        /// <typeparam name="TResult">Return type of the callback.</typeparam>
        /// <param name="func">Callback that receives the SAFE model reference.</param>
        TResult Execute<TResult>(Func<dynamic, TResult> func);
    }
}
