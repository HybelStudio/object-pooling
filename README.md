# Object Pooling
 A small package to handle easy object pooling in Unity using Scriptable Objects. Also contains a flexible API for scripted Object Pooling with generics.

## Adding the package to your Unity project

Open the package manager in unity by navigating to "Window/Package Manager". Then click the plus icon in the top-left, add package from git URL, and paste the following link into the text box, and press Add.
```
https://github.com/HybelStudio/object-pooling.git
```

## Usage

Here's a step by step process to use the Asset based object pooling workflow which this package provides.

### Setup

Create a new empty `GameObject` in your hierarchy and call it `ObjectPooler`. Add the `ObjectPooler` component to that object.

### Creating an Object Pool Asset

Open the create menu and navigate to the `/Objects` path and click `Object Pool`. This creates a new `Object Pool` in your project.

#### Properties

- **Prefab:** The prefab you wish to spawn. Note that this prefab MUST have a component which implements `IPoolableObject` on the root. If you don't want to implement it yourself, you can add the `PoolableObjectLifeTime` component to it and it will be returned to the pool after a given amount of time.
- **Overflow Mode:** This determines the behaviour when you have reached the max limit for the object pool.
    - **Steal From Active:** This behaviour respawns the oldest, currently active object. (Active objects are the ones currently in use).
    - **Hard Limit:** This behvaiour disallows the spawning of new objects when the limit is reached.
    - **Allow Overflow:** This behaviour allows the pool to exceed the max limit, but the extra object will be destroyed as soon as possible instead of being disabled. This is recommended for the best performance, given you have provided an apropriate initial max limit.
    - **Increase Size:** This behaviour allows the pools max limit to grow as the max limit is reached.
- **Overflow Increment:** If the `Increase Size` option is chosen for `Overflow Mode` this determines by how much the pool grows when reaching the limit.
- **Amount to Start With:** This determines how many objects will be instantiated and pooled during initialization.
- **Instantiation Type:** This determines how to objects are instatiated during initialization.
    - **Bulk:** Using this option means all the objects are instantiated at the same time during initialization.
    - **Batches Per Frame:** Using this option means the objects will be instantiated over multiple frames.
- **Batch Amount:** If the `Batches Per Frame` option is chosen for `Instantiation Type` this detrmines how many objects are instantiated each frame. If you have many object pools with this option, its better to choose smaller numbers since all the pools are handled in parallel and will accumulate their batches on each frame.

### Add the Object Pool Asset to the Object Pooler

When you have created your `ObjectPoolAsset` you need to add it to the `pools` list on the `ObjectPooler` component you made in the "Setup" stage.

### Using it in your Code

On a component you want to spawn objects from add a field with the `ObjectPoolAsset` type:

```csharp
[SerializeField] private ObjectPoolAsset objectPool;
```

Then, where you want to spawn a new object add this code:

```csharp
GameObject obj = objectPool.Get();
```

You can provide a position and rotation to the `Get()` method:

```csharp
Vector3 position = Vector3.zero;
Quaternion rotation = Quaternion.identity;
GameObject obj = objectPool.Get(position, rotation);
```

It is recommended to use the `TryGet()` method instead which will return true only if an object will be spawned (which won't happen when using the `Hard Limit` option for overflow.


```csharp
Vector3 position = Vector3.zero;
Quaternion rotation = Quaternion.identity;
if (objectPool.TryGet(position, rotation, out GameObject obj))
{
    // Object was spawned
}
else
{
    // Object was not spawned
}
```


#### Implement the IPoolableObject interface

This interface requires you to implement two members:
- The Property: `ObjectPool`
- The Method: `HandlePoolReturn()`

`ObjectPool` can be implemented as an auto-property. The value is set by the `ObjectPooler` upon instantiation.
`HandlePoolReturn()` can be implemented however you want to, but you should call `objectPool.Release()` somewhere here to *despawn* the object.

The `PoolableObject` abstract class contains a default implementation for `ObjectPool` which disallows assigning it more than once.

## Contributing

**Bug Reports & Feature Requests**

Please use the [issue tracker](https://github.com/HybelStudio/object-pooling/issues) to report bugs or file features.

Pull requests are welcome. To begin developing, do this under the assets folder of a Unity project.

```sh
git clone https://github.com/HybelStudio/object-pooling.git
```

Then open your Unity project and start developing :D

## License

[CC0](https://creativecommons.org/publicdomain/zero/1.0/)
