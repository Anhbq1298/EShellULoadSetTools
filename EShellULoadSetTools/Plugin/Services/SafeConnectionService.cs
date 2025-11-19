using System;

namespace EShellULoadSetTools.Services
{
    /// <summary>
    /// Default SAFE implementation that stores the SAFE <c>cSapModel</c> reference and
    /// provides helper methods to invoke SAFE API calls in a thread-safe manner.
    /// The SAFE COM interface is usually consumed dynamically, therefore the
    /// service only stores the reference as <see cref="object"/>/dynamic and does
    /// not depend on the actual SAFE interop assembly at compile time.
    /// </summary>
    public sealed class SafeConnectionService : ISafeConnectionService
    {
        private readonly object _syncRoot = new();
        private object? _safeModel;

        /// <inheritdoc />
        public bool IsInitialized
        {
            get
            {
                lock (_syncRoot)
                {
                    return _safeModel is not null;
                }
            }
        }

        /// <inheritdoc />
        public void Initialize(object safeSapModel)
        {
            if (safeSapModel == null) throw new ArgumentNullException(nameof(safeSapModel));

            lock (_syncRoot)
            {
                _safeModel = safeSapModel;
            }
        }

        /// <inheritdoc />
        public void Execute(Action<dynamic> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            dynamic model = GetSafeModelOrThrow();

            try
            {
                action(model);
            }
            catch (Exception ex)
            {
                throw BuildSafeInvocationException(ex);
            }
        }

        /// <inheritdoc />
        public TResult Execute<TResult>(Func<dynamic, TResult> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            dynamic model = GetSafeModelOrThrow();

            try
            {
                return func(model);
            }
            catch (Exception ex)
            {
                throw BuildSafeInvocationException(ex);
            }
        }

        private dynamic GetSafeModelOrThrow()
        {
            lock (_syncRoot)
            {
                return _safeModel ?? throw new InvalidOperationException(
                    "SAFE connection has not been initialized yet. Call Initialize before invoking SAFE operations.");
            }
        }

        private static InvalidOperationException BuildSafeInvocationException(Exception innerException)
        {
            return new InvalidOperationException(
                "SAFE API call failed. See inner exception for details.",
                innerException);
        }
    }
}
