using System.Threading.Tasks;

namespace com.fscigliano.AsyncInitialization
{
    /// <summary>
    /// Modification Date:  18/09/2025
    /// Product Name:       Async Initializations
    /// Developers:         Franco Scigliano
    /// Description:        Initializes the object asynchronously.
    ///                     Implementations should handle any necessary setup that may take time,
    ///                     such as loading resources, establishing connections, or performing calculations.
    /// </summary>
    public interface IInitializable
    {
        Task InitAsync();
    }
}