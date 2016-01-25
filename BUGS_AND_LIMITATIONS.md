### Limitations and Know Bugs in Dot42

- `System.Linq.Expressions` is not (yet?) supported. I believe one should try to implement it based on Mono's implementation which in turn is based on Microsofts.
- Dot42 will redirect usages of `System.Threading.Interlocked` to Javas `AtomicXxxFieldUpdater` implementation. This works seamless for instance fields. The `AtomicXxxFieldUpdater` does not support static fields, so Dot42 reverts to `lock`ing on the the fields containing type. To prevent this locking, use the `AtomicXxx` classes directly, e.g. `AtomicInteger`.  
  Note that, when available, `System.Threading.Interlocked` is used by the C# compiler when generating support code for events. Therefore, when using static events, locking will occur. Dot42 will emit a warning in this case. At the moment, there is no known workaround.
- to get Json.NET to run I swapped out a totally broken `decimal` by a `java.math.BigDecimal` based one. This hasn't been thoroughly tested yet.
- When getting fields/methods of a type in Android through reflection, the original order of items is not preserved. This is a limitation of the dex file format. (Note that the BCL documentation also states the the return order of members is undefined. A lot of code seems to rely on the order, so the current behavior is unlikely to change in the future). For fields and properties Dot42 employs special code to preserve the original order, as these may be used in serialization. The order of methods is not preserved.
- since Java doesn't have (unsigned-)`byte`, `ushort`, `uint` or `ulong`, Dot42 maps these values to their appropriate signed counterparts during compilation. All unsigned calculation are properly done. Conflicting method overloads are renamed, e.g. if you have `Foo(int)` and `Foo(uint)` the latter will internally renamed to `Foo$I`. This renaming is impossible for constructors, since they have to be named `<init>` in java.
  *Conflicting constructor-overloads are therefore not possible.*  The workaround is to remove one of the offending constructors, and replace the functionality with static initializers. You can have a look at how this is accomplished in the `System.Decimal` implementation. Note that it is perfectly fine to have explicit or implicit conversion operators with conflicting overloads, as well as static factory methods.
- Reflecting on parameters that are references will not give the desired results, due to the conversion Dot42 applies to them to make them work with java (this shouldn't be a big problem).
- Dot42 emulates properties, so they can be properly reflected upon. Dot42 does not generate reflection information for properties with arguments (a.k.a. indexers). Custom indexers can therefore not be used in reflection based serializations. For indexers for the common interfaces like IList, this should not pose a problem.
  You probably have never implemented a custom indexer, and if you did, you most certainly did not rely on it for serialization, so there is probably no real problem.
- Many operations on multidimensional arrays should work. Nevertheless some indexing usages will crash the Dot42 compiler complaining about a missing `<ArrayType>[][].Address` method.
-  `protected ValueType NumberFromText(string numberText, bool decimalPeriodSeen)`, as seen in MvvmCross, will not work if you intent to return doubles, ints or other primitives. Only structs inherit from `ValueType` in Dot42, primitives and enums do not. The workaround is to use `object` as return or parameter type instead of `ValueType`.
- Importing jar references should work, but I could not get Intellisense/Resharper to work out of the box. When importing a jar reference (or when the underlying jar has changed) Dot42 creates a `.cs` -File with CLR compatible types, that can be accessed as normal during development. All references to them will magically be replaced to the actual implementation when compiling to `.dex`. This generated file is put in the `obj` intermediate directory, and not part of the project. Therefore Intellisense does not work.
  What worked for me (with Resharper) is to have a dedicated import project that does nothing else but import jar-references. I don't even worry about unused references, since dot42 is smart enough to not compile them into the final `.apk`. Then I don't reference the import project itself, but the assembly it generates. This make IntelliSense to work as expected.
  I believe it would be better if Dot42 would put the generated `.cs` files into a "Generated" folder of the project (or right next to the `.jar`s in VS), and compile it from there. The user should be able to adjust the imported file in case the Dot42 `.jar` import did not work seamless.
- Importing multiple jar references will fail or omit methods when some of the references depend on each other. The workaround is to either import the dependencies into multiple Dot42 library projects, and reference these projects, or to combine the jars into a single jar.
- Java doesn't support Ephemerons. These are e.g. used by `ConditionalWeakTable` to attach properties to objects in a GC friendly way. No workaround available.
- ART is over-eagerly checking if `lock`s are properly released in  functions. This can pose a problem in rare cases, most notably complex async methods. These might provoke a dex2oat verification error with a message like "expected empty monitor stack". The workaround is to extract the locked region into it's own method, or, alternativly, use other synchronization primitives like `SemaphoreSlim` or something. This problem will manifest itself more with the VS 2015 Roslyn compiler.
See also https://docs.oracle.com/javase/specs/jvms/se7/html/jvms-2.html#jvms-2.11.10, https://code.google.com/p/android/issues/detail?id=80823, http://stackoverflow.com/questions/4201713/synchronization-vs-lock .
In the long run, this might be fixed in the Dot42 compiler, by using something else than dex build in monitors (... in all places ...)
- **Bug: ** Arithmetic operations that are not constant with enums are not only slow, but some of them also provoke dalvik verification errors. When `enum E { A=-1, B=0, C=1 }` then  `var a = E.A; var b = (E)(-(int)a);` will fail. Workaround is to write e.g. `int a = (int)E.A; ...` and then do the aritmetic.
- Dot42 seems not to support activities / android services, etc. in other than the main assembly. Everything that needs to go into the manifest needs to be in the main assembly.
- Implementing interfaces using framework methods will trigger an "abstract method not implemented" exception at runtime. The workaround is to add an explicit interface implementation redirecting to the framework implementation.
   ```
	public interface IMvxLayoutInflater
	{
	    LayoutInflater LayoutInflater { get; }
	}
	
	public class MvxActivity : Activity, IMvxLayoutInflater
	{
	    ...
	#if DOT42 
		// This is required in Dot42 at the moment, as the
	    // actual getter method names differ.
	    LayoutInflater IMvxLayoutInflater.LayoutInflater { get { return base.LayoutInflater; } }
	#endif
	   ...
	}
   ``` 

- When a `.pdb` doesn't match it's `.dll`, e.g. because the `.dll` has been updated but not the `.pdb`, the compiler might fail with a non descriptive error message. Delete or update the `.pdb` in this case. 

### Structs

In CLR/C# structs are usually employed when performance matters. `CancellationToken` might be the prime example, a lightweight wrapper around a `CancellationTokenSource` that is extensively employed when working with  `Task`s.
Java/Dex does not natively support struct semantics. Dot42 goes at great length to emulate the behavior of structs. Whenever a struct is used as a parameter, in a return value, assigned to a field, etc. the struct is cloned: a new object is created, and the values are copied. Structs can not be null: When initializing struct arrays, all elements are initialized to a new value, i.e. a whole bunch of new objects are created.
All this emulation comes at a performance cost. **Therefore, if performance matters, avoid structs in Dot42.**

For immutable structs like `DateTime`, `TimeSpan` or the afore-mentioned `CancellationToken`, Dot42 has special optimizations. Immutable structs are structs where all fields are `readonly` and, if the field type is itself a struct, the field type is an immutable struct. Immutable structs can in many regards be handled like classes. Most important they don't have to be cloned and copied around, since they won't change. Immutable structs therefore don't have the performance penalty as described above.
*Be aware that this optimization breaks if reflection is used to alter a readonly field of an immutable struct*, something you shouldn't be doing anyways.

There are some limitations where the emulation is not yet fully implemented:

- **Limitation: ** Equals & GetHashCode are not automatically implemented for structs. Therefore, structs will not work as a key in a Dictionary or HashTable or similar things, if you don't explicitly override these methods (which you should do for performance reasons anyway).
  (note: Dot42 could do this automatically either at the compiler level, or implement it in `ValueType`, based on reflection, in the Framework; the latter approach is probably more compatible to BCL).
- **Bug:** When structs are passed as arguments as a generic instance parameter, at the moment they are not cloned as they should be. This will make no difference with immutable structs, but breaks struct semantics with mutable structs.
- **Bug:** Creating arrays of generic structs will fail at runtime with a dalvik verification error. Arrays of generic classes will work just fine.
- **Bug:** When passing a generic struct by reference as generic parameter, and the calling function assigns default(T) to the reference before returning, a NullPointerException might occur.   

### Generics  

###### Generics and Reflection
Dot42 handles generic type definitions slightly different depend on context. When reflecting on class members, the full generic instance reflection information is preserved; a special proxy type will look just like the real instance type. The same applies when using e.g. `typeof(List<int>)`. `Type.IsAssignabeFrom`, `is` and similar expressions should work as expected in almost all cases.
When using `list.GetType()` though, the underlying java type is returned, which is equivalent to `typeof(List<>)`. While this call could easily be made to return a proxy type with full CLR compatibility, Dot42 refrains from doing so for performance reasons. `obj.GetType()` is used in virtually all `Equals` implementations, which in turn are use heavily by `HashSet`, `Dictionary` and even `List` implementations.
If you need to fully reflect on an instance, e.g. because you are writing a serialization API, you need to call `obj.GetTypeReflectionSafe()`.
Note that besides the CLR compatible reflection API the underlying Java reflection API is also available, in many cases with a `"Java"`-prefix to the methods.

###### **Important:** Static fields in generic classes
The way static field in generic classes are handled differs between CLR and Dot42. In CLR, every generic instance of a generic type gets its own static field, i.e. if `List<T>` had a static field `X`, `List<int>::X` and `List<long>::X` would be two different fields. In Dot42 these fields will actually be the same field. One workaround is to access a `ConcurrentDictionary<Type, [original field type]>` when the behavior of the CLR is to be emulated. You can find an example of this pattern in the `EqualityComparer<T>` implementation.
For similar reasons, the static class constructor has never access to the generic parameter, it will always see `typeof(object)` when accessing `typeof(T)`. Normal constructors are not affected by this limitation.

### Debugger

While Dot42s Visual Studio debugger is quite advanced, it does not yet have all the features you might be accustomed to from Visual Studio. 

- The debugger can display local variables, including `this`, and fields of objects. It will not display properties or evaluate methods or expressions in general.
- Sometimes local variables will not show up in the 'locals' watch window.
  [This - at least sometimes - seems to be related to them being assigned to the `r0` Dalvik register. It appears to be a problem/feature of Dalvik itself, which tries to work around limitations of Eclipse by remapping registers. A fix might be to not use the `r0` register during debug builds.]

- Code breakpoints are supported. Currently, you can not set breakpoints on data.
- When using partial classes in different files, the debugger will only step into methods in one of the files. You can set breakpoints in all of them though.
- The debugger has issues with multithreading code and will occasionally deadlock code not running on the main thread. This situation is usually accompanied by a dalvik warning like his: `06-28 22:54:19.575	30238	30305	D	dalvikvm	threadid=11: still suspended after undo (sc=1 dc=1)`.
- There is no "edit and continue". 
	- This would be quite difficult - though not impossible - to implement, as the Dalvik virtual machine has no support whatsoever for this.
- Dot42 has an experimental implementation for the "set next instruction" feature in Visual Studio. 
	- This allows you to set the next instruction to the beginning of the current block, i.e. beginning of method, or beginning of try/catch/finally block.
	- It must be enabled in Android project settings.
	- This feature will only be activated for debug-build assemblies.
	- This is an experimental feature, i.e. will not always work as expected.
	- In general, you will only be able to set the next instruction when you are at the beginning of a statement, i.e. when you have completed a step or breakpoint has been hit. When returning from a method via "step out" you might have to do an additional step to be able to set the next instruction.   
	- `foreach` statements generate an implicit `try/finally` block. Since you can not - at the moment - set the instruction out of the current block, you will not be able to restart the `foreach` from within. The workaround is to set the next instruction from after the `foreach` instruction.
- For reasons unknown, the disassembly window might not be able to show source code next to the disassembly. If you wish to see the disassembly and have problems, you should disable the "source code" checkbox. This should enable tracking the current position and setting breakpoints in the disassembly.
  ApkSpy will embed the source code without problems.
- When using the disassembly window, values of registers can be watched, e.g. by entering `r2` in the watch window. You can specify the desired type, e.g. `(double)r12` or `(string)p1`. Note that using `(string)` or `(object)` as cast types might crash the virtual machine if the specified register does not actually hold a `string` or `object` type. This seems to be a bug of the Dalvik VM. It is recommended to always remove non-primitive cast expression from the watch window as soon as possible, so that they will not be evaluated on the next breakpoint/exception.

### Differences to Xamarin.Android
- Xamarin.Android names the Android-`R` class `Ressources`, while Dot42 sticks to `R`. I am not sure there is a good reason to rename the `R` class. On the contrary, I remember myself searching for `R` when first working with Xamarin.Android for quite some time until I figured it out.
- The Android way to specifying resources for different configurations is using folders, e.g. `Layout\MainView.axml` and `Layout-Land\MainView.axml`, casing does not matter.
  In Dot42 you can alternatively specify the configuration appended to the filename, e.g. `Layout\MainView.axml` and `Layout\MainView-Land.axml`. When a configuration is specified in both folder and filename, the folder configuration takes preference, and the file configuration is ignored.
- You can have more than one root resources folder,e.g. `Resources`, `Resources_MyLib`, etc. The name actually does not matter, the resources are found by their build type.
- (TODO: there are more differences...)

