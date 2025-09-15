namespace com.fscigliano.AsyncInitialization
{
    /// <summary>
    /// Creation Date:   8/5/2020 8:19:35 PM
    /// Product Name:    Async Initializations
    /// Developers:      Franco Scigliano
    /// Description:     Simplified initialization states for the refactored system
    /// Changelog:       Removed WAITING_DEPENDENCE - dependencies are now managed internally by InitializationHandler
    /// </summary>
    public enum InitState
    {
        NOT_INITIALIZED,
        INITIALIZING,
        INITIALIZED
    }
}