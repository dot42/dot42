### 2016-01-22 

Fix verification errors when using the ART runtime.

### 2015-06-29 commit id f27a0fb67a5b3d472a6133d3d2c284884dbf14a9

The changes focus both on improving compatibility with existing .NET code, and improving target performance.


###### Compiler
- reimplemented the `try/catch/finally` generation code to correctly support nested finally statements, also implicit ones in `lock` or `using ` statements.
- fixes a bug in the code optimizer sometimes producing erroneous `long` or `double` operations.
- reworked `System.Delegate/MulticastDelegate` to adhere to BCL specification; this fixes problems with event unsubscription and also eliminates race conditions.

+ Compile method bodies from .jar files using `dx` from Android SDK Tools. This fixes some issues seen in the support libary.
+ fix compiler error when a .class files uses annotations on parameters.
+ updated the android support library to version 22.2.0.
+ fixes some issues in the binding generator when import jar files.
+ stop the VS plugin to automatically uncheck usage of the support libary.
+ fix DexWriter sometimes writing invalid dex when static field values are only used on some fields.
+ fixes spurious compiler errors when implementing java interface from .NET

- Allow to specify various data attributes on Android intent filters when using attributes.
- Allow android resource configurations to be also specified using directories, the normal Android way.
- improve Android resource handling: try to detect resource type from directory name, accept `anydpi` as configuration qualifier.
 
+ Implemented various optimization steps during the code generation phase. Depending on the used code constructs, leading to reduced code size and improved runtime performance. 
+ reworked parts of the generics emulation for reduced pressure on the garbage collector, smaller code size and better runtime performance.
+ significantly reduced implicit boxing of primitive data types in various situations.
+ significantly improved performance of various enum binary / flags operations.

###### Compiler / Framework

- removed race condition in `System.Threading.Task` continuations 
- removed a race condition when emulating the semantics of `Interlocked.CompareExchange` with `Atomic*FieldUpdater.CompareAndSet`.
- fixed `Task.WhenAny` trying to set the result multiple times
- make `WeakReference` a true WeakReference, not a SoftReference, as this more closely resembles .NET semantics. Also added `WeakReference<T>`
- fix `DateTime.DaysInMonth` returning days for the wrong month
- implemented `Enumerable.Distinct(IComparer)`
- improved `DateTime` formatting compatibility.

- clear internal caches in low memory situations
- clear internal locale specific caches on Android configuration change. A new default locale is therefore automatically applied.

+ significantly improved reflection performance.
+ significantly increased performance of `Activator.CreateInstance(Type, params object[])`
 
- improved Number formatting performance, under some situations significantly.
- improved `DateTime` parsing/formatting performance
- improved `EqualityComparer<T>.Default` performance
- slightly improved performance of various string functions.
- improved performance when using `Interlocked.CompareExchange` in typical usage scenarios.
- reworked the assembly emulation layer for faster start up time and significant less memory usage.
- slightly improved performance when using mutable structs.  

+ suppress various bogus warning messages

###### Debugger
- improved VS debugger performance when break on all exceptions is selected.

###### Development Tools  
- allow the ILSpy-plugin to peek into the Dot42 framework assembly
- allow the ILSpy-plugin to stop at a specified step during the RL 
transformation phase
 
###### Known regressions

- The new optimizations applied during the code generation phase lead to an increased compile time of about 10%. 

### Version 1.2.1505.1

The changes focus on improving compatibility with existing .NET code. Both Json.NET and MvvmCross will run with this release with only minor modifications.
Other improvements include performance and the Visual Studio Debugger.  

**Changes since the first Github Dot42 release (1.1.0.81)**



###### Build system
- Fixed the `NAnt` build: Added `NAnt.BuildTools.Tasks` as a replacement for the missing buildtools in the original Dot42 repo on github. This should enable  compiling Dot42 with `NAnt`.
  This replacement contains the most important task to compile Dot42; some verification tasks are noops, and some not strictly necessary tasks are missing. It would be best if the original buildtools could be made available.   
- Added [INSTALL.md](INSTALL.md) containing instructions on how to compile Dot42.

###### Changes to the compiler 

- Fixed several generics related issues:
	- Fixed generic static methods not working (and therefore many generic delegates).
	- Fixed mixing generic parameters and delegates in the same function leading to a dalvik verification error.
	- Fixed dalvik verification errors when using constraints on generic types.
	- Fixed dalvik verification errors due to primitive boxing/unboxing not handled correctly in all cases.
	- Fixed dalvik verification errors when implementing a generic override using an Enum.
	- Fixed generic methods in interfaces crashing the compiler.
	- Fixed several overload related issues, where different .NET methods would be mapped to the same dex-method, leading to a compiler error, inadvertently overrides or dalvik verification errors. 
	- Fixed some cases where using structs/enums with generics would beak ValueType semantics, possibly leading to `null`-ValueTypes and `NullPointerException`s.
	- Reworked type information and reflection on generics. Most things you can do in CLR should work with Dot42 as well. Typical operations work as expected:   
	```
	class GenericClass<T>
    {
        private T val;

        public GenericClass(T val)
        {
			// should work
			Assert.AssertTrue(((object)val) is T);

			// should work
            Assert.AssertEquals(typeof(int), typeof(T));

			// should work 
            Assert.AssertTrue(val is int);

			// should work 
            Assert.AssertTrue(typeof(int).IsAssignableFrom(typeof(T)));

			// should work 
			var fieldType = typeof(this).GetField("val", BindingFlagsXXX).FieldType;
            Assert.AssertTrue(fieldType.IsAssignableFrom(typeof(T));

			// should work 
			Assert.AssertEquals(fieldType, typeof(T));

			// should work if T is not a generic type itself, 
			// or a primitive; see BUGS_AND_LIMITATIONS.md 
            Assert.AssertTrue(val.GetType() == typeof(T));

			// This will be true if T is an int. Should be false.
			// see BUGS_AND_LIMITATIONS.md.
			Assert.AssertTrue(val.GetType() == typeof(int?));

			// This will be true for any List<TElement>
			// see BUGS_AND_LIMITATIONS.md.
			Assert.AssertTrue(val.GetType() == typeof(List<>));

			// should work in all cases
			Assert.AssertTrue(val.GetTypeReflectionSafe() == typeof(T));

            this.val = val;
        }
    }

	// this should work.
	var x = (GenericClass<int>)Activator.CreateInstance(
							 	typeof(GenericClass<>), typeof(int));

	// should work
	List<string> = jsonNetSerializer.Deserialize<List<string>>("...");
	``` 

- Fixed and enhanced support for `Nullable<T>`
	* Full support for structs
	* Full support `ToString()` and `GetValueOrDefault`
	* Full support for reflection, `is`, and `IsAssignableFrom`
	* Fixed passing `Nullable<T>` by reference. 
	* There should only be obscure cases left where differences to CLR's implementation get visible. These should all work as expected:
```
	foreach (PropertyInfo pi in dt.GetType().GetProperties())
	{
		// this will work (json-net style)
		//
	    if(pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition == typeof(Nullable<>))
	    {
	       // display nullable info
	    }
	
		// this should work
	    if(pi.PropertyType == typeof(DateTime?))
	    {
	       // display nullable info
	    }
	
		// Will work (if of use to anyone)
		Assert.IsNull(Activator.CreateInstance(pi.PropertyType));
		Assert.IsNull(Activator.CreateInstance(typeof(int?)));  
	
	}
```

- Reworked the Reflection-API to be compatible to BCL.
	- .NET code will hardly see any difference when compared to running on CLR. (tested with json-net; for generics see above).
	- Preserve the order of fields, useful for serialization (note: this could be made optional)  
	- Provide `PropertyInfo` for framework types. This is at the moment an opt-in feature, activated by using `[assembly: IncludeType(typeof(IncludeFrameworkProperties))]`
	- Emulated assemblies, to allow dependency injection frameworks to find types based on assemblies. 
	- Improved support for Attributes
		* Allow null values
		* allow array arguments
		* allow enums
		* allow attribute-class inheritance and abstract base classes
		* fixed using constructors and properties/fields at the same time
		* fixed setters and being pruned, leading to dalvik verification errors / properties being not available.

- several Enum related fixes
	- `ToString(string)` working as in CLR 
	- Allow using `System.Enum` itself as a parameter/field/return value.
	- better compatibility for enums with the same value. 

- Fixed quite a few of the dreaded dalvik verification errors
	- in `System.Convert`
	- when implementing an interface using an inherited method
	- when using `volatile` on primitive types. 
	- when a base class has protected methods/fields and a subclass of a derived class tries to access these fields/methods
	- enums would trigger verify errors in release mode on binary operations when used by reference 
	- in several other places


- Fixed using arrays in `foreach` calls.
- Fixed using arrays as return value or storing in a field, when the actual field type / return value is `IList`, `IList<T>`, `IEnumerable`, `IEnumerable<T>`, `ICollection` or `ICollection<T>`.
- `typeof(IEnumerable).AssignableFrom([array type])` and related calls should work.

- Fixed casts leading to expressions evaluated more than once.

- Fixed null coalescing operator `??` always evaluating the second expresion. 

- Fixed nested try/catch/rethrow leading to compiler errors.  

- Fixed compilation errors due to types with same namespace and name being in more than one assembly. This happens regularly to compiler generated classes without namespace, but could also happen due to private classes, especially attributes, being duplicated in assemblies.
  The fix at the moment is to always prepend the scope, i.e. the assembly name, to the namespace. *Note: There should be a better solution.*

- Fixed `float`/`double` comparison producing wrong result when comparing `>` or  `>=` and one of the operands is `NaN`.

- Swapped out a broken `decimal` implementation by a wrapper around `java.math.BigDecimal`. The new implementation is largely untested, but  it does not provoke a dalvik verification error when being used. It would be great if someone who actually uses `decimal` could thoroughly test the wrapper and report (or fix) potential problems.

- Added optimizations for immutable structs, preventing them being unnecessary cloned or instantiated. See [BUGS_AND_LIMITATIONS.md](BUGS_AND_LIMITATIONS.md).

- Code generation optimizations:
	- significantly reduced the number of generated branch/comparison instructions, leading to both reduced code size and improved runtime performance.
	- dead code elimination
	- collapse empty try blocks
	- collapse goto-chains (how is this called?)
	- eliminate empty switch statements

- Significant compiler performance optimizations:
	- Reduced compile time by more than 50% by rewriting several hotspots (measured with CLR only project).
	- Making use of more than one processor during some of the compilation steps results in another 10% or so reduction, on a dual core system.  
	- Introduced a compiler cache for incremental builds, leading to up to 50% reduction in compile time. With the other optimizations, depending on the number of assemblies changed, this can lead to a total reduction of compile time of over 75%.

- Modified several algorithms to make the compiler output stable and independent of the used .NET runtime. This is valuable when working on the compiler itself to quickly check for regressions with e.g. WinMerge. These changes include: 
	- FrameworkBuilder/JarImporter generating classes in alphabetical order.
	- IL-Transformations processing classes in alphabetical order, so that renames are processed in a stable way.
	- Making the generation of hashcodes independent of the x64/x32 runtime.

- Allow the user to define types in `System` namespace, instead of failing with a nondescript error message.   
  
- Fixed resource id generator generating wrong indices for styleables
- Fixed `.jar`-imported enums not being parsable by `Enum<T>.valueOf`
- Fixed `.jar` import possibly leading to dalvik verify error due to reachable walker not looking at interfaces of interfaces.
- Fixed `MultiNewarr` instruction in `.jar` import leading to compiler crash.
- (hopefully) fixed a problem with `finally` statements in `.jar` imports leading to compiler errors. 
- Fixed accessing `.jar`-imported fields from .NET code leading to  compiler errors.

- For embedded resources under an "Asset" folder, try to reconstruct an Android-compatible asset path. 
  

###### Changes to the Framework / API

- Improved `System.Collection.Generics.Dictionary<TKey,TValue>`
	- Full support for the `Dictionary<TKey,TVal>(IComparer<TKey> comparer)` constructor.
	- Many compatibility fixes, so that it looks like a true .NET dictionary.
- Made the collection classes inheritance match .NETs, fixed some signatures.
- Added `Queue<T>`
- Added `LinkedList<>`, and `Tuple<>` from mono
- Improved performance for List copying / initialization / `AddRange` / `InsertRange`
- Added `ISet<T>.IntersectWith`
- Added `List<T>.Sort` and `List<T>.RemoveRange` 
- Fixed `Enumerable.GroupBy` returning groups in arbitrary order.
- Fixed `EqualityComparer<T>` and `Comparer<T>` implementations.
- Fixed `IReadOnlyCollection<T>`
- Fixed `string.Split()` with a count parameter not returning final part.
- Added `StringReader`
- Fixed various `ArrayList` methods to support `IList` and arrays.
- Fixed `Array.Contains()` to perform a linear search instead of a binary one. 
- Improved `CultureInfo` and related classes. This should allow formatting and parsing with locales. When no locale is specified the current culture is used, not the invariant culture. This matches the way the BCL operates. 
- Improved `DateTime` compatibility, including parsing and ToString() with custom formats.
- implemented `IFormattable` on `TimeSpan`, including custom formats.
- Added a basic `DataTimeOffset` implementation. Parsing and formatting are not implemented yet.
- Added `SemaphoreSlim` and `AutoResetEvent`
- Fixed `Task.Run`.
- Fixed `Task.Delay` with CancellationToken.   
- Added `Task.WhenAll`, `Task.WhenAny`, `Task.FromResult`, and `Task.ContinueWith(Action)`.
- Implemented `System.Threading.Interlocked` based on an automatic implementation of an AtomicXxxFieldUpdater. For limitations when used with static fields see  [BUGS_AND_LIMITATIONS.md](BUGS_AND_LIMITATIONS.md).
- Added `System.Threading.ThreadPool`.
- Fixed the implementation of `ThreadPoolScheduler` leading to possible deadlocks due to too few threads being available.
- Added a simple `Lazy<T>` implementation and `WeakReference<T>`.
- Added an `[IncludeType]`-Attribute which works like `[Include(ApplyToMembers=true)]`, but is simpler to inject when using automated attribute generators.
- Added `[assembly: Include(Pattern="...")]`, to allow inclusion of types and members based on matched patterns. For details and examples, see [PRUNING.md](PRUNING.md).
- Added an `[SerializedParameter]`-Attribute, that can be applied to normal and generic method parameters. Types and objects passed as this parameter will have all their public fields and public and private properties preserved and not pruned. For details and examples, see [PRUNING.md](PRUNING.md).  
	Using the `[SerializedParameter]`-Attribute allows serialization of anonymous types, and can lead to fewer bugs due to over-pruning of the compiler.
- Fixed `Delegate.Remove` throwing NullPointerException on event unsubscription if there where no subscribers.
- Implemented `Delegate.Method` and `Delegate.Target` to allow certain patterns of weak event binding to be used. 
- Added `Android.App.Application.Context` and  `Android.App.Application.SynchronizationContext` and automatically initialize them during application startup. This mimics Xamarin.Android's behavior.
- Changes to `Dot42.Manifest.ActivityAttribute`: made `VisibleInLauncher` false by default, added `MainLauncher` property. 
- Added `Context.StartActivity(Type activityType)`, code-compatible with Xamarin.Android.
- Fixed several `System.Math` related issues, including wrong calculations and huge performance increases for `Math.Round`.
- implemented `Math.Log(double, double)` for arbitrary bases.
- Fixed several `float`/`double` formatting issues.
- Significantly improved number formatting performance.
- Fixed `string.Format` throwing an exception when `obj.ToString()` returns `null`. 
- Quite a few smaller changes, fixes and enhancements.

###### *Breaking changes*

- **Java/Android framework**
  I felt it necessary to introduce breaking changes regarding properties and namespaces.
	- When using the Android/Java framework, namespaces are now compatible with Xamarin.Android. Renames include:
		- `Java.Os` => `Java.OS`, `Java.Io` => `Java.IO`, `Android.Os` => `Android.OS`
		- `Android.View` => `Android.Views` and similar renames; these renames do not only improve source code compatibility with Xamarin.Android, but also allow you to use e.g. the `View` class without prepending it's namespace.
		
	- Property generation           
		- Properties are generated for interface members as well.
		- Readonly properties are generated for `CanXxx` and `HasXxx` named methods.
		- **When a property is generated, the original method will no longer be generated**, as this lead to IntelliSense clutter,  confusion about whether to use the property or the setter/getter, and problems when overriding such a method/property.  
		- Removed the `Type` property on `System.Object`, to reduce warnings and confusion with classes that define a property  `Type`. Also reduces clutter in IntelliSense.
		- Do not automatically prepend an `Is` to boolean properties, if there wasn't one to begin with. The source code contains comments on this decision. 
	
	- For methods containing `sbyte` (in java: `byte`) parameters or return values, Dot42 generally creates two overloads, one with `byte` parameters, and one with `sbyte`. The default overload is now `byte`, which is the more common semantic. This only makes a difference when overriding such a method.
	  
	- When generating the `R` ressource-constants class, don't append an additional 's' to the subtypes.
	- Many of the changes will improve source code compatibility with Xamarin.Android as well.
	- Existing code will have to be adapted to the changes. Multiple "replace in files" can do most of the work.
     

- `DateTime.Parse` / `DateTime.TryParse` / `DateTime.ToString`: these methods, when used with a custom format string, will accept .NET compatible format strings; the previous implementation would accept Java-compatible format strings.
- When no locale is specified in various `ToString` or `Parse`/`TryParse` invocations, the current locale will be used, not the invariant locale. This matches the way the .NET operates. 

###### VisualStudio Debugger

- Implemented an experimental "set next instruction" feature, allowing to set the next instruction to the beginning of the current block during debugging. See [BUGS_AND_LIMITATIONS.md](BUGS_AND_LIMITATIONS.md).
- Added support for the disassembly window, including setting breakpoints, single-stepping through dalvik instructions, and viewing and modifying bare register values.
- Allow to modify local primitive variables.
- Allow setting breakpoints inside of a delegate
- Fixed the exception handling. The exception configuration dialog in VS should now work as expected, and exceptions can be selected/deselected while debugging.
- Show the exception message next to the exception type when breaking on an exception; also show information of whether it's a first chance exception.
- (Hopefully) fixed occasions where exceptions reported by the VM for an already dead thread would lead to freezing of the debuggee.
- Fixed some cases where the user would have to press "step over"/"step into" multiple times to advance to the next statement.
- Fixed display of `float`/`double` rounding - and not showing - the least significant digits.

###### ApkSpy
- Don't keep a lock on opened .apks
- Allow loading of .dex files
- Optionally use an external back end to decompile .dex classes, e.g. baksmali; this proved useful when debugging dalvik verify errors
- Search for class names / method names.
- Allow to export the whole disassembly using an external tool.
- Swapped out the text display control to a syntax highlighting version (preliminiary).
- Optionally embed source code and/or source code positions in the disassembly.
- For jump instructions, show she jump distance next to the target offset
- For constants larger than +/- 8, show the hexadecimal value as well.
- Show opcode help in tooltip, show variable names for registers in tooltip.
- Highlight jump targets and exception handlers when pointing at them.
- Highlight register usage when pointing at a register.
- Improved the formatting.

###### ILSpyPlugin
- Fixed the ILSpyPlugin to work with current code base
- Allow Ast conversion to be stopped at an arbitrary step
- optionally line-break Ast-expression, making long ones readable.
- added a compilation cache for quick language switching
- unified 'Dex Output' and ApkSpy formatting
- don't fail early on method body compilation errors, but allow to show all compilation errors when selecting an assembly.
- 'Dex Input' language: allow decompilation of whole types, to allow decompilation of generated methods as well. 

###### Regressions

Unfortunately, there are also a couple of known regressions, and probably unknowns as well. The known are:

- There is a `StringFormatTest` in `CasesGitHub\Case6`. It breaks when formatting floats (and doubles?) on the least significant digits. Unfortunately, I was not aware of the test when improving the formatting, since I developed solely with `FrameworkTests` and  `CompilerTests`, for the lack of a `NAnt` build. It should be checked if the test fails because it is flawed, or because the formatting code is erroneous.
- The `Cases\cs666-Support4Demos` projects fails to compile its resources for reasons beyond me.
- WpfProxy test project failes to compile because a function in `NAnt.BuildTools.Tasks` is not yet implemented.
- Not all samples are fully adapted yet to support the new Framework API with its extended property generation. This should be a mostly a matter of going through the code and replacing method calls with properties, maybe even using a "replace in file" feature.
- I disabled compilation for VS2010 / VS2012 plugins, mainly because the XmlEditor components seemed uncompilable without VS2010 and VS2012 installed. Also, I fought for a long time to get the plugin to compile and debug in VS2013 from VS2013, and messed with the project files quite a bit. This should be rectified by someone with the proper knowledge.
   


	