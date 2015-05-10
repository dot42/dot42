

### Version 1.2.1505.1

The changes focus on improving compatibility with existing .NET code. Both Json.NET and MvvmCross will run with this release with only minor modifications.
Other improvements include performance and the Visual Studio Debugger.  

**Changes since the first Github Dot42 release (1.1.0.81/82)**



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

- Fixed quite a few of the dreaded dalvik verification errors
	- in System.Convert
	- when implementing an interface using an inherited method
	- when using `volatile` on primitive types. 
	- when a base class has protected methods/fields and a subclass of a derived class tries to access these fields/methods 
	- in several other places


- Fixed using arrays in `foreach` calls and upon method returns (with `IList`, `IList<T>`, `IEnumerable`, `IEnumerable<T>`, `ICollection` and `ICollection<T>` as actual return value)

- `typeof(IEnumerable).AssignableFrom([array type])` and related calls should work.

- Fixed casts leading to expressions evaluated more than once.

- Fixed nested try/catch/rethrow leading to compiler errors.  

- Fixed compilation errors due to types with same namespace and name being in more than one assembly. This happens regularly to compiler generated classes without namespace, but could also happen due to private classes, especially attributes, being duplicated in assemblies.
  The fix at the moment is to always prepend the scope, i.e. the assembly name, to the namespace. *Note: There should be a better solution.*

- Swapped out a broken `decimal` implementation by a wrapper around `java.math.BigDecimal`. The new implementation is largely untested, but  it does not provoke a dalvik verification error when being used. It would be great if someone who actually uses `decimal` could thoroughly test the wrapper and report (or fix) problems.


- Added optimizations for immutable structs, preventing them being unnecessary cloned or instantiated. See [BUGS_AND_LIMITATIONS.md](BUGS_AND_LIMITATIONS.md).

- Some code generation optimizations:
	- dead code elimination
	- collapse empty try blocks
	- collapse goto-chains (how is this called?)
	- eliminate empty switch statements	

- Significant compiler performance optimizations:
	- Reduced compile time by more than 50% by rewriting several hotspots (measured with CLR only project)
	- Making use of more than one processor during some of the compilation steps results in another 10% or so reduction, on a dual core system.  
	- Introduced a compiler cache for incremental builds, leading to up to 50% reduction in compile time. With the other optimizations, depending on the number of assemblies changed, this can lead to a total reduction of compile time of over 75%.
	The compiler cache is currently only enabled for .NET assemblies, but it should be simple to support imported .jars as well.

- Modified several algorithms to make the compiler output stable and independent of the used .NET runtime. This is valuable when working on the compiler itself to quickly check for regressions with e.g. WinMerge. These changes include: 
	- FrameworkBuilder/JarImporter generating classes in alphabetical order.
	- IL-Transformations processing classes in alphabetical order, so that renames are processed in a stable way.
	- Making the generation of hashcodes independent of the x64/x32 runtime.

- Allow the user to define types in `System` namespace, instead of failing with a nondescript error message.   
  
###### Changes to the Framework / API

- Improved `System.Collection.Generics.Dictionary<TKey,TValue>`
	- Full support for the `Dictionary<TKey,TVal>(IComparer<TKey> comparer)` constructor.
	- Many compatibility fixes, so that it looks like a true .NET dictionary.
- Made the collection classes inheritance match .NETs, fixed some signatures.
- Added `Queue<T>`
- Added `LinkedList<>`, and `Tuple<>` from mono
- Improved performance for List copying / initialization / AddRange /InsertRange
- Added `ISet<T>.IntersectWith`
- Fixed `Enumerable.GroupBy` returning groups in arbitrary order.
- Fixed `EqualityComparer<T>` and `Comparer<T>` implementations.
- Added `StringReader`
- Improved `CultureInfo` and related classes. This should allow formatting and parsing with locales. When no locale is specified the current culture is used, not the invariant culture. This matches the way the BCL operates. 
- Improved `DateTime` compatibility, including parsing and ToString() with custom formats.
- Added `SemaphoreSlim` and `AutoResetEvent`
- Fixed `Task.Run` and added `Task.WhenAll`, `Task.WhenAny` and  `Task.FromResult`
- Implemented `System.Threading.Interlocked` based on an automatic implementation of an AtomicXxxFieldUpdater. For limitations when used with static fields see  [BUGS_AND_LIMITATIONS.md](BUGS_AND_LIMITATIONS.md).
- Added `System.Threading.ThreadPool`
- Fixed the implementation of `ThreadPoolScheduler` leading to possible deadlocks due to too few threads being available.
- Added a simple `Lazy<T>` implementation.
- Added an `[IncludeType]`-Attribute which works like `[Include(ApplyToMembers=true)]`, but is simpler to inject when using automated attribute generators.
- Added an `[SerializationMethod]`-Attribute (is there a better name?). When applied to a method, all types passed into the method are treated as if they where annotated with `[IncludeType]`. This applies to generic method parameters as well. The automatic `[IncludeType]` propagates recursively to the types of fields and properties of the passed type. 
	Using the `[SerializationMethod]`-Attribute allows for serialization of anonymous types, and can lead to fewer bugs due to over-pruning of the compiler.
- Implemented `Delegate.Method` and `Delegate.Target` to allow certain patterns of weak event binding to be used. 
- Added `Android.App.Application.Context` and  `Android.App.Application.SynchronizationContext` and automatically initialize them during application startup. This mimics Xamarin.Android's behavior. 
- Quite a few smaller changes, fixes and enhancements

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


  
###### ApkSpy
- Don't keep a lock on opened .apks
- Allow loading of .dex files
- Optionally use an external back end to decompile .dex classes, e.g. baksmali; this proved useful when debugging dalvik verify errors
- Allow to export the whole disassembly using an external tool.
- Swapped out the text display control to a syntax highlighting version (preliminiary).
- Optionally embed source code and/or source code positions in the disassembly.	
- For jump instructions, show she jump distance next to the target offset
- For constants larger than +/- 8, show the hexadecimal value as well.
- Improved the formatting.

###### Regressions

Unfortunately, there are also a couple of known regressions, and probably unknowns as well. The known are:

- There is a `StringFormatTest` in `CasesGitHub\Case6`. It breaks when formatting floats (and doubles?) on the least significant digits. Unfortunately, I was not aware of the test when improving the formatting, since I developed solely with `FrameworkTests` and  `CompilerTests`, for the lack of a `NAnt` build. It should be checked if the test fails because it is flawed, or because the formatting code is erroneous.
- The `Cases\cs666-Support4Demos` projects fails to compile its resources for reasons beyond me.
- WpfProxy test project failes to compile because a function in `NAnt.BuildTools.Tasks` is not yet implemented.
- Not all samples are fully adapted yet to support the new Framework API with its extended property generation. This should be a mostly a matter of going through the code and replacing method calls with properties, maybe even using a "replace in file" feature.
- I disabled compilation for VS2010 / VS2012 plugins, mainly because the XmlEditor components seemed uncompilable without VS2010 and VS2012 installed. Also, I fought for a long time to get the plugin to compile and debug in VS2013 from VS2013, and messed with the project files quite a bit. This should be rectified by someone with the proper knowledge.
   


	