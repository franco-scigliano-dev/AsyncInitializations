# Async Initialization System - Refactored

## Overview
This refactored async initialization system provides a clean, simple way to manage component initialization with dependency resolution. Components now only need to implement `IInitializable` - all state management is handled internally by the `InitializationHandler`.

## Key Benefits
✅ **Simple Implementation**: Components just implement `IInitializable.InitAsync()`  
✅ **No State Management**: All initialization states managed internally  
✅ **Dependency Resolution**: Automatic dependency ordering with circular dependency detection  
✅ **Inspector Integration**: Easy setup through Unity inspector  
✅ **Code-based Setup**: Programmatic configuration support  
✅ **Nested Support**: Handlers can be items of other handlers  

## Quick Start

### 1. Component Implementation
```csharp
public class MySystem : MonoBehaviour, IInitializable
{
    public async Task InitAsync()
    {
        // Your initialization logic here
        await SomeAsyncOperation();
        Debug.Log("MySystem initialized!");
    }
}
```

### 2. Inspector Setup
1. Add `InitializationHandler` to a GameObject
2. In the inspector, add your `IInitializable` components to the list
3. Set dependencies for each component as needed
4. Enable "Autorun" if you want automatic initialization on Start()

### 3. Code-based Setup
```csharp
var handler = GetComponent<InitializationHandler>();

// Add items without dependencies
handler.AddItems(systemA, systemB);

// Add items with dependencies
handler.AddItemWithDependencies(systemC, systemA, systemB); // C depends on A and B

// Start initialization
await handler.InitAsync();
```

## Migration from Old System

### Before (Complex)
```csharp
public class OldSystem : MonoBehaviour, IInitWrapper
{
    public InitHandlerData InitData { get; private set; }
    public bool IsEnabled => enabled;
    
    void Awake()
    {
        InitData = new InitHandlerData(this);
        // Complex state management...
    }
    
    public async Task InitAsync()
    {
        InitData.InitState = InitState.INITIALIZING;
        // Your logic
        InitData.InitState = InitState.INITIALIZED;
    }
}
```

### After (Simple)
```csharp
public class NewSystem : MonoBehaviour, IInitializable
{
    public async Task InitAsync()
    {
        // Just your logic - no state management needed!
        await YourInitializationLogic();
    }
}
```

## Features

### Dependency Resolution
The system automatically resolves initialization order based on dependencies:
- Components with no dependencies initialize first
- Components wait for their dependencies to complete
- Circular dependencies are detected and reported

### Error Handling
- Individual component failures don't stop the entire process
- Failed components are logged with detailed error messages
- System continues initializing other components

### Cancellation Support
- Proper cancellation token support
- Graceful shutdown when GameObject is destroyed
- No memory leaks from incomplete operations

## Examples

See the `Examples` folder for:
- `ExampleInitializableComponent.cs` - Basic component implementation
- `InitializationHandlerUsageExample.cs` - Complete usage examples

## Legacy Support

The old `IInitWrapper` interface is marked as obsolete but still exists for backward compatibility. However, we recommend migrating to the new `IInitializable`-only approach for cleaner code.

## Best Practices

1. **Keep InitAsync() Fast**: Avoid blocking operations
2. **Use Dependency Injection**: Consider using DI containers for complex dependencies
3. **Error Handling**: Wrap risky operations in try-catch blocks
4. **Logging**: Add meaningful logs for debugging initialization issues
5. **Testing**: Test initialization order and failure scenarios

## Performance Notes

- Parallel initialization of independent components
- Minimal overhead from state management
- Memory-efficient dependency tracking
- Proper cleanup on destruction
