using System.Threading.Tasks;

namespace com.fscigliano.AsyncInitialization
{
    /// <summary>
    /// Creation Date:   26/02/2020 10:54:39
    /// Product Name:    Async Initializations
    /// Developers:      Franco Scigliano
    /// Description:
    /// Changelog:       
    /// </summary>
    public interface IInitializable
    {
        Task InitAsync();
    }
}