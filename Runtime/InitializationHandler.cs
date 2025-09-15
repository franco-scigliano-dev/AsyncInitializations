using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace com.fscigliano.AsyncInitialization
{
    /// <summary>
    /// Refactored Initialization Handler that works directly with IInitializable components
    /// and manages all state internally without requiring wrapper interfaces.
    /// </summary>
    public sealed class InitializationHandler : MonoBehaviour, IInitializable
    {
        [System.Serializable]
        public class InitializationItem
        {
            public Component component;
            public List<Component> dependencies = new List<Component>();
            public bool isEnabled = true;
        }

        [SerializeField] private List<InitializationItem> _initializationItems = new List<InitializationItem>();
        [SerializeField] private bool autorun = false;
        [SerializeField] private UnityEvent onComplete;

        // Internal state management
        private readonly Dictionary<IInitializable, InitState> _itemStates = new Dictionary<IInitializable, InitState>();
        private readonly Dictionary<IInitializable, List<IInitializable>> _dependencies = new Dictionary<IInitializable, List<IInitializable>>();
        private readonly HashSet<IInitializable> _initializedItems = new HashSet<IInitializable>();
        private readonly List<Task> _runningTasks = new List<Task>();
        private CancellationTokenSource _cancellationToken;

        public async Task InitAsync()
        {
            _cancellationToken = new CancellationTokenSource();
            _itemStates.Clear();
            _dependencies.Clear();
            _initializedItems.Clear();
            _runningTasks.Clear();

            var initializables = BuildInitializableGraph();
            if (initializables.Count == 0) 
            {
                onComplete?.Invoke();
                return;
            }

            await ExecuteInitialization(initializables);
            onComplete?.Invoke();
        }

        private List<IInitializable> BuildInitializableGraph()
        {
            var items = new List<IInitializable>();

            foreach (var item in _initializationItems)
            {
                if (!item.isEnabled || item.component == null) continue;

                if (item.component is IInitializable initializable)
                {
                    items.Add(initializable);
                    _itemStates[initializable] = InitState.NOT_INITIALIZED;

                    // Map dependencies
                    var deps = new List<IInitializable>();
                    foreach (var dep in item.dependencies)
                    {
                        if (dep is IInitializable depInit)
                            deps.Add(depInit);
                    }
                    _dependencies[initializable] = deps;
                }
            }

            return items;
        }

        private async Task ExecuteInitialization(List<IInitializable> items)
        {
            while (_initializedItems.Count < items.Count && !_cancellationToken.IsCancellationRequested)
            {
                var readyToInit = items.Where(CanInitialize).ToList();
                
                if (readyToInit.Count == 0)
                {
                    if (_runningTasks.Count == 0)
                    {
                        Debug.LogError("[InitializationHandler] Circular dependency detected or no items can be initialized!");
                        break;
                    }
                    await Task.WhenAny(_runningTasks);
                    _runningTasks.RemoveAll(t => t.IsCompleted);
                    continue;
                }

                foreach (var item in readyToInit)
                {
                    _itemStates[item] = InitState.INITIALIZING;
                    _runningTasks.Add(InitializeItem(item));
                }

                await Task.WhenAny(_runningTasks);
                _runningTasks.RemoveAll(t => t.IsCompleted);
            }

            await Task.WhenAll(_runningTasks);
        }

        private bool CanInitialize(IInitializable item)
        {
            if (_itemStates[item] != InitState.NOT_INITIALIZED) return false;
            
            var deps = _dependencies[item];
            return deps.All(dep => _initializedItems.Contains(dep));
        }

        private async Task InitializeItem(IInitializable item)
        {
            try
            {
                await item.InitAsync();
                _itemStates[item] = InitState.INITIALIZED;
                _initializedItems.Add(item);
                
                Debug.Log($"[InitializationHandler] Initialized: {item}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[InitializationHandler] Failed to initialize {item}: {ex.Message}");
                _itemStates[item] = InitState.NOT_INITIALIZED;
            }
        }

        void Start()
        {
            if (autorun)
                _ = InitAsync();
        }

        void OnDestroy()
        {
            _cancellationToken?.Cancel();
        }

        // Support for code-based setup
        public void AddItems(params IInitializable[] items)
        {
            foreach (var item in items)
            {
                if (item is Component comp)
                {
                    _initializationItems.Add(new InitializationItem { component = comp });
                }
            }
        }

        public void AddItemWithDependencies(IInitializable item, params IInitializable[] dependencies)
        {
            if (item is Component comp)
            {
                var initItem = new InitializationItem { component = comp };
                initItem.dependencies.AddRange(dependencies.OfType<Component>());
                _initializationItems.Add(initItem);
            }
        }
        
        public InitState GetState(IInitializable item)
        {
            if (_itemStates.TryGetValue(item, out var state))
                return state;
            return InitState.NOT_INITIALIZED;
        }

        // Legacy support
        [System.Obsolete("Use AddItems or AddItemWithDependencies instead")]
        public void SetUp(List<IInitializable> dependences)
        {
            AddItems(dependences.ToArray());
        }
    }
}