### Generic Advanced C# & Unity Patterns

This document provides a deep dive into high-performance and architectural patterns used in modern C# and Unity development.

---

### 1. High-Performance Reflection via `Delegate.CreateDelegate`

**Problem**: Using `MethodInfo.Invoke` in performance-critical code (like Update loops or high-frequency events) is extremely slow because it requires array allocation for arguments and boxing of value types.
**Solution**: Convert the `MethodInfo` into a strongly-typed `Delegate` once and cache it.
**Reasons**: Cached delegates execute at near-native speed, avoiding the overhead of reflection lookups, argument array allocations, and boxing.

**Mini-Tutorial**:
1. **Identify the Target**: Get the `MethodInfo` of the method you want to call.
   ```csharp
   var method = typeof(MyClass).GetMethod("MyMethod");
   ```
2. **Define the Delegate Type**: Choose an `Action` or `Func` that matches the signature.
   ```csharp
   // For: public void MyMethod(float value)
   Type delegateType = typeof(Action<float>);
   ```
3. **Create the Delegate**: Bind the method to an instance (or null for static).
   ```csharp
   var fastCall = (Action<float>)Delegate.CreateDelegate(delegateType, instance, method);
   ```
4. **Cache and Invoke**: Store the delegate and call it directly.
   ```csharp
   fastCall(10f); // Fast, no allocations
   ```

---

### 2. Recursive Generic Constraints (Strategy Pattern)

**Problem**: Designing a system that handles multiple types (e.g., Stats like Health, Speed) often leads to using `object` or common interfaces that require casting and runtime type checks, breaking type safety and performance.
**Solution**: Use recursive generic constraints to link a processor to its specific strategy type at compile time.
**Reasons**: Ensures that a `SpeedModifier` can only be applied to a `SpeedStat`, eliminates runtime casting, and allows the compiler to optimize struct-based strategies.

**Mini-Tutorial**:
1. **Define the Interface**: Create a generic interface for the strategy.
   ```csharp
   public interface IStrategy<T> where T : struct {
       T Apply(T input);
   }
   ```
2. **Apply Constraints**: Create the processor with linked generic parameters.
   ```csharp
   public class Processor<T, TStrat>
       where T : struct
       where TStrat : struct, IStrategy<T> {
       public void Execute(ref T value, TStrat strat) {
           value = strat.Apply(value);
       }
   }
   ```
3. **Implement Strategy**: Create a concrete struct strategy.
   ```csharp
   public struct MultiplyStrategy : IStrategy<float> {
       public float Apply(float input) => input * 2f;
   }
   ```

---

### 3. Efficient Generic Comparisons with `EqualityComparer<T>.Default`

**Problem**: Using `a == b` inside a generic method `<T>` won't compile unless constrained to a class, and using `a.Equals(b)` causes boxing if `T` is a value type (struct).
**Solution**: Use `EqualityComparer<T>.Default.Equals(a, b)`.
**Reasons**: It uses the most efficient comparison available for the type (preferring `IEquatable<T>`) and completely avoids boxing for structs.

**Mini-Tutorial**:
1. **Access the Comparer**: Use the static `Default` property for your type.
   ```csharp
   var comparer = EqualityComparer<T>.Default;
   ```
2. **Perform Comparison**: Call `.Equals()` on the comparer.
   ```csharp
   public void SetValue<T>(T newValue) {
       if (EqualityComparer<T>.Default.Equals(_value, newValue)) return;
       _value = newValue;
   }
   ```

---

### 4. Expression Trees for Logic Hashing

**Problem**: If you allow users to pass `Func<T, T>` for logic, you cannot easily identify or remove a specific logic block later because every lambda/delegate instance can be unique.
**Solution**: Use `Expression<Func<T, T>>` to capture the "intent" of the code as data, then hash its string representation.
**Reasons**: Allows you to compare the *structure* of the logic (e.g., "x => x + 1") rather than just the reference of the delegate.

**Mini-Tutorial**:
1. **Capture Expression**: Change the method signature to accept an `Expression`.
   ```csharp
   public void AddLogic(Expression<Func<float, float>> expr) { /* ... */ }
   ```
2. **Generate Hash**: Convert the expression to a string and hash it.
   ```csharp
   string code = expr.ToString();
   ulong id = Hashing.Fnv1a64(code);
   ```
3. **Compile and Store**: Compile the expression back into a callable function.
   ```csharp
   Func<float, float> compiled = expr.Compile();
   _logicStore.Add(id, compiled);
   ```

---

### 5. Lazy Initialization for Memory Efficiency

**Problem**: Initializing `new List<T>()` or `new Dictionary<K, V>()` in a constructor for an object that has thousands of instances wastes memory if those collections are often empty.
**Solution**: Keep the collection reference `null` until the first item is added.
**Reasons**: Saves ~32-64 bytes per instance. In large systems, this can save megabytes of heap space.

**Mini-Tutorial**:
1. **Declare as Null**: Do not initialize in the constructor.
   ```csharp
   private List<int> _items;
   ```
2. **Initialize on Demand**: Create the list only when `Add` is called.
   ```csharp
   public void Add(int item) {
       _items ??= new List<int>();
       _items.Add(item);
   }
   ```
3. **Safe Access**: Use null-conditional operators or checks when reading.
   ```csharp
   public void Process() {
       if (_items == null) return;
       foreach(var item in _items) { /* ... */ }
   }
   ```

---

### 6. The "Router" / Type-Keyed Lookup Pattern

**Problem**: A central class becomes bloated and tightly coupled if it needs a direct reference to every subsystem.
**Solution**: Store subsystems in a dictionary keyed by their `Type`.
**Reasons**: New subsystems can be added without modifying the core class, promoting a clean, modular architecture.

**Mini-Tutorial**:
1. **Setup Registry**: Create a dictionary in your base class.
   ```csharp
   private Dictionary<Type, object> _registry = new();
   ```
2. **Register Subsystem**: Add a method to store instances.
   ```csharp
   public void Register<T>(T system) => _registry[typeof(T)] = system;
   ```
3. **Retrieve via Type**: Use generics to get the system back.
   ```csharp
   public T Get<T>() => (T)_registry[typeof(T)];
   ```

---

### 7. Extension Methods as an API Layer

**Problem**: Deeply nested generic systems are syntactically "ugly" and hard to use.
**Solution**: Create extension methods that wrap the complex generic calls.
**Reasons**: Improves code readability and provides a "Fluent API" that feels like a native part of the objects.

**Mini-Tutorial**:
1. **Create Static Class**: Define the extension container.
   ```csharp
   public static class StatExtensions {
       public static void Modify(this IStatUser user, float val) { /* ... */ }
   }
   ```
2. **Use Native-Style**: Call the method directly on the object.
   ```csharp
   player.Modify(10f);
   ```

---

### 8. Decorator Pattern for Cross-Cutting Concerns

**Problem**: Adding logic like "Timers" directly into a Stat system makes the core logic messy.
**Solution**: Wrap the base logic in a "Decorator" class that implements the same interface but adds extra behavior.
**Reasons**: Follows the Open/Closed Principle—add new behaviors without changing the existing class.

**Mini-Tutorial**:
1. **Base Interface**: Ensure you have a common interface.
   ```csharp
   public interface ILogic { float Apply(float val); }
   ```
2. **Create Decorator**: Wrap the original logic and add features.
   ```csharp
   public class TimedDecorator : ILogic {
       private ILogic _inner;
       public TimedDecorator(ILogic inner) => _inner = inner;
       public float Apply(float val) {
           if (TimerExpired) return val;
           return _inner.Apply(val);
       }
   }
   ```

---

### 9. Ultra-Fast Hashing via FNV-1a

**Problem**: `string.GetHashCode()` is non-deterministic and unsuitable for persistent IDs or networking.
**Solution**: Implement the FNV-1a algorithm for deterministic 64-bit hashing.
**Reasons**: Extremely fast, low collision rate, and produces consistent results across game launches.

**Mini-Tutorial**:
1. **Define Constants**: Use the FNV offset and prime.
   ```csharp
   const ulong offset = 1469598103934665603UL;
   const ulong prime  = 1099511628211UL;
   ```
2. **Iterate and XOR**: Process each character.
   ```csharp
   ulong hash = offset;
   for (int i = 0; i < text.Length; i++) {
       hash ^= text[i];
       hash *= prime;
   }
   ```

---

### 10. Expression-Based "Concrete-to-Interface" Binding

**Problem**: `Delegate.CreateDelegate` cannot bind a method taking a concrete class to a delegate taking an interface directly.
**Solution**: Use Expression Trees to build a "bridge" method that performs the cast.
**Reasons**: Allows for flexible decoupling while maintaining high performance.

**Mini-Tutorial**:
1. **Create Parameter**: Define the interface parameter.
   ```csharp
   var arg = Expression.Parameter(typeof(IInterface), "arg");
   ```
2. **Cast to Concrete**: Add a conversion step.
   ```csharp
   var cast = Expression.Convert(arg, typeof(ConcreteClass));
   ```
3. **Call and Compile**: Build the call and compile.
   ```csharp
   var call = Expression.Call(instance, methodInfo, cast);
   var lambda = Expression.Lambda<Action<IInterface>>(call, arg).Compile();
   ```

---

### 11. Explicit Generic Redirection

**Problem**: Multiple generic parameters `<T, TMod>` can be hard for the compiler to infer, forcing redundant code.
**Solution**: Provide specialized extension methods for common types.
**Reasons**: Simplifies the API by hiding secondary generic parameters.

**Mini-Tutorial**:
1. **Original Method**: A complex multi-generic method.
   ```csharp
   public void DoWork<T, TMod>(T val) { ... }
   ```
2. **Specialized Redirection**: Create a "float" specific version.
   ```csharp
   public static void DoWork<TMod>(this IEntity e, float val) where TMod : IMod<float> {
       e.DoWork<float, TMod>(val);
   }
   ```

---

### 12. Double Bridge Casting (JIT Boxing Elision)

**Problem**: When handling heterogeneous structs in a generic method, you often store them as `object`. Casting back to the concrete type typically causes boxing/unboxing overhead, which is unacceptable in high-frequency loops.
**Solution**: Use a "Double Bridge Cast" `(T)(object)(ConcreteType)val`.
**Reasons**: If `T` is known at runtime to be `ConcreteType`, the JIT compiler performs "Elision"—it sees the intermediate `object` cast and removes the boxing instructions entirely, resulting in a direct assignment with zero heap allocation.

**Mini-Tutorial**:
1. **The Generic Context**: Set up a method where `T` is a value type.
   ```csharp
   public T Resolve<T>(object list, T val) where T : struct
   ```
2. **The Runtime Type Check**: Check if the runtime type matches your specific optimization target.
   ```csharp
   if (typeof(T) == typeof(float))
   ```
3. **The Double Bridge Cast**: Perform the cast through `object`.
   ```csharp
   // The JIT sees (T)(object)(float) and removes the boxing
   return (T)(object)((List<MyMod>)list).Apply((float)(object)val);
   ```
4. **The Safe Fallback**: Always provide a default path for other types.
   ```csharp
   return val;
   ```

---
