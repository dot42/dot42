### Target Performance
- There are probably myriads of possibilities to improve target performance, to optimize this and that. Profiling should find out if and where the largest bottlenecks are. 

- One optimization that should be easy to implement is to inline simple getters and setter: http://developer.android.com/training/articles/perf-tips.html#GettersSetters

- There is a performance test on stackoverflow comparing performance between Xamarin.Android and Dot42. It is based on regex evaluation. I don't think the test says much about overall performance of both platforms - I believe Dot42 outperforms Xamarin.Android in many cases -, but anyways it should be easy to score much higher on it. Regex expression should not be retranslated/recompiled on every use, but instead be cached in a LRU cache, just as the BCL does it.
  [http://stackoverflow.com/questions/17134522/does-anyone-have-benchmarks-code-results-comparing-performance-of-android-ap](http://stackoverflow.com/questions/17134522/does-anyone-have-benchmarks-code-results-comparing-performance-of-android-ap)

- When calling GetHashCode(), ToString() and Equals() on primitive types, they get boxed. These common calls could be redirected by the compiler to avoid the boxing. GetHashCode() and Equals() could even be inlined.

- There seems to be an issue in the framework builder leading to Dot42 not always choosing the better `invoke-virtual` opcode and instead using `invoke-interface`. I have seen this for `ThreadPoolExecutor.execute()`. Not sure if this would have any performance implications whatsoever though.


### Improving the generics implementation
The following comments refer mainly to the implementation of the `GenericInstanceConverter`.

- A simple optimization could be done for the common case where there is a single generic argument. Here we don't need to create an array at all, but could simply pass the type around.
  In fact, even for more than one generic argument, performance both memory and runtime wise would be improved for at least up to four parameters (http://programmers.stackexchange.com/questions/162546/why-the-overhead-when-allocating-objects-arrays-in-java); that is, unless we implement static fields with all possible generic arguments (see below) 

- It should be possible to optimize the common case when all generic parameters to pass are the same as the ones stored in my `$g` field, and the order is the same as well. In this case a new array doesn't need to be created, instead the `$g`-field can be just passes as-is.

- One minor modification to the current system that should cut down on code size and improve performance would be to generate at compile time static fields - maybe in a specialized marker class - for all encountered parameter combinations to `new GClass<T1,T2>` or calls to generic methods.  
  This should work when the parameters are known and constant at compile time. We then wouldn't need to create new arrays on these occasions, but a simple field access would suffice.

- A larger change would be to have a generic base class, similar to the current implementation (which would have to have it's sealed modifier if any removed), and derive all generic instances from it. The derived classes would know its instance arguments, preferably from a static field. All constructors are cloned, passing in the required instance arguments. The existing reflection code in `System.Type` would be updated to make these changes invisible to a caller. `Type.MakeGenericType()` style invocation would work as they do now, if no pre-generated type exists.
  - A benefit would be that GetType() would return a unique type for every generic instance type, without any performance overhead.
  - On the call sites, the creation of arrays is not longer necessary leading to performance improvements.
  - Memory consumption is reduced, since we don't create arrays again and again and again.
  - The drawback would be that the class hierarchy is altered, though this could be made invisible to any caller using .NET reflection.
  - Before implementing, tests should show how many of those types need to be actually created, and if this might lead to combinatorial explosion (i don't think so, though)

- (todo: think about specialized implementations for primitive types)

### Nullable&lt;T&gt;

Some nitty-gritty details on how Nullables work: 
>http://blogs.msdn.com/b/haibo_luo/archive/2005/08/23/455241.aspx. 
>http://stackoverflow.com/questions/4759330/why-is-gettype-returning-datetime-type-for-nullabledatetime

- (todo: think about improving the performance of (byte, short, int)-nullables by storing them in a field larger than required and using the extra space as flag if a value is present)

#### Debugger

###### Set next instruction
Based on my experiences with the current implementation - which is already quite useful, but was designed as a proof of concept - I believe it should not be too hard to enable settings the next instruction to an arbitrary location in the method, not only to the beginning. There are comments in the source code on my ideas.

###### "Edit and continue"
- If someone was adventurous to implement this, the existing infrastructure for "set next instruction" could be leveraged. 
- First, the changed class would be compiled to a `.dex`
- This `.dex` is injected via code in the Framework API into the the running process, and loaded via a class loader. It is intended to replace the previous class.
- The debugger sets breakpoints at the first instruction if every method of the replaced class. When one of the breakpoints is hit, the debugger uses some "set next instruction" like mechanism, to redirect the call to the new class. This redirection mechanism generates  instances of the new class on the fly if it hasn't done so yet. Either the debugger keeps a mapping between old and new instances, or every class gets an extra field containing a possible redirection.  
- Finally, the halted method is resumed through "set next instruction" to go to the beginning of the method. 

### Thoughts on renaming methods

When converting CLR code to Java code, method name clashes can occur for several reasons:
- the CLR/C# method is `new` / `IsHideBySig`, therefore has the same name as a name in a base class, but does not override it.
- The method has one of the unsigned primitive types (uint, ushort, ulong, 'sbyte') as one of its parameters - or if it is an implicit conversion operator - as a return value. Such a method will clash if the corresponding signed method is also implemented.
- The method uses generic parameters
	- There is a specific override for the generic parameter. These will clash since Dot42 will implement generic parameters as objects.
``` 		  
	class ClashingGenericOverloads<T>
	{
	    // both methods will name-clash in java
		public void Do(T x) { }
        public void Do(object x) { }
	}
```
	- The method uses generic instances. These will clash since java does Type-erasure.
``` 
	class ClashingTypeErasure
	{
	    // both methods will name-clash in java
		public void Do(IEnumerable<int> x) { }
        public void Do(IEnumerable<string> y) { }
	} 
```

Note that name-clashing can also appear with methods in base classes, resulting not in compiler errors, but in inadvertently overriding the base, leading to unexpected runtime behaviour. Or, if the base method was final, leading to a dalvik verification error.

To mitigate, Dot42 will rename possible clashing methods. Since all this happens automatically, the user will never see any of it, except for decompilation or debugging stack traces. `new`- Methods will get a `$<number>` postfix; methods possibly clashing due to generics, type erasure or unsigned data types will get a additional `$$<erased information>` postfix. E.g. in the above examples, the method names will be `Do`, `Do$$T`, `Do$$Int32` and `Do$$String`. In case the latter method still produces clashes (e.g. there could be types with same name, but different namespaces), an additional `$<number>` will be appended.

**Constructors can not be renamed**. Therefore, clashing constructors will lead to a compile time error. One possible fix in Dot42 compiler would be to introduce special marker parameters types, e.g. `Dot42.Internal.Uint32Marker`, add them to conflicting constructors, and update all call sites to pass in an extra null or empty instance. 
 
### Thoughts optimizing compiler performance
*(Note: the figures are out of date, but the general ideas still apply)*.
Compiling my pet project, NinjaTasks, including a reference to Newtonsoft.Json, MVVMCross and a few custom libraries takes with the newest optimizations still about 35 seconds without the compiler cache. There are ~1300 types with methods and ~10.000 methods in the mapfile (a few more generated-but-not-mapped might be in the dex). These values are far from the dreaded ~65000 dex method limit, which apparently a few projects have already reached.
Obviously compiler performance has to be improved. Profiling showed for this CLR-only project the most time consuming steps are:

- ~3.5% for loading the assemblies
- ~11% for finding reachable types
- ~7% for "IL-To-Java" conversion
- ~12% creation of classes and members 
- ~33% is spend in "TranslateToRL", i.e.IL-to-Ast-to-RL conversion
- ~20% is spend in "CompileToDex" / CompileToTarget
- ~12% takes saving of the .dex

###### Introducing the method body compiler cache
It should be rewarding to reduce the 50% it takes to convert IL to Ast to RL to Dex. To this end I designed a method body compiler cache. If an assembly hasn't changed between builds, then the assembly it depends on can not have changed. The assemblies classes can not have changed, and neither their method bodies. We can therefore just take the compiled bodies of the last build and inject them into the new build.
The difficulty is that the Dot42 compiler might have renamed members/classes differently depending on other classes/methods/fields pulled in or removed during this build. We have to make sure that the compiler cache both finds the correct body, and is able to resolve any class/method/field references in the cached body to the current build.
To accomplish this resolving I introduced the notion of a ScopeId. A ScopeId uniquely identifies an element in its scope, and is constant across builds if the underlying source has not changed. ScopeIds are visible on the level of the XTypeDefinition / XMethodDefinition / XFieldDefinition.
For IL-Types, the ScopeId is `Module.Name + ":" + type.MetadataToken`. For Java-Types, the ScopdId is the name of the class. For IL Methods, the ScopeId is the index into their types `.Method` list (the MetadataToken seems to be not necessarily unique). Java-Methods don't get a ScopeId, it is equivalent to their dex-signature. Fields don't get a ScopeId, it is equivalent to their name.
Synthetic elements have to devise their own notion of a ScopeId with the described characteristics. Nullable Marker Classes take the ScopeId of their based-on class and add ":Nullable". Enum$Info classes take the ScopeId of their enum and add ":Info". Generated methods can either specify their ScopeId explicitly, e.g. `$Clone`, or can leave it empty, in which case they are not resolvable.
It might be possible to always use the full type name for types and name/signature for methods as ScopeId, thus eliminating the need for it altogether. Using an explicit ScopeIds has benefits though. We can use the "name" property in the MapFile for it's original intention: showing debug information. It does not have to be unique, as it is done for the `...$Info` enum members. 

Now to correctly resolve references, we first look up the type. From the MapFile generated during the previous compilation, we find the type by it's dexname. We get the type's Scope and ScopeId, and concatenate them by ':'.  if ScopeId id null, we use the name of the type. We then search the X-Types for the correct type. Then we get fields by their name, and methods by their ScopeId.  

Some cases need special handling:
  - `AstCompilerVisitor.Expression` triggers delegate method creation when reaching an `AstCode.Delegate`. That is, new methods are created during the code generation phase. Besides being problematic if we ever try to make the code multithreading capable, these generations also need to be triggered during fixing cached method bodies.
  - The `__generated` class collects methods and static fields of java-extended classes. These fields are renamed so that they do not clash with other static fields. At the moment, there is no information in the map file as to what is the originating type or field name. References to fields in the `__generated` class are therefore unresolvable. 
  - Method references might be unresolvable. This can happen for synthetic methods that get injected during IL-conversion, e.g. explicit interface implementation stubs.
  - If there are unresolvable references in a cached method body, we don't use it. Instead we resort to normal compilation.

###### Improving the compiler cache
- For assemblies that do not change very often, e.g. the Dot42 framework assembly, it would be worthwhile to compile them fully, and always serve the cache from them. This would speed up non-incremental builds as well. In fact, this could be applied to user libraries as well, placing a fully compiled .dex and mapfile next to the assembly. 
- The performance of the cache itself might be improved by not loading the method bodies from a .dex file, but storing them in e.g. a sqlite database in a quick-to-serialize format.     

###### Further thoughts
- The reachable detection works pretty good for small projects. For larger projects it seems to become a bottleneck. Maybe it can be improved by building an explicit dependency graph of the assemblies, and using graph algorithms to detect reachable types. The graph could be persisted, and reused for unmodified assemblies. 
- Multithreading should speed up compilation time for non-incremental builds. I already made the assembly-loading and the reachable walking multithreading capable.The code generation phase could also be made multithreading capable.
	- A quick "Parallel.ForEach" didn't work. At least some of the code is not multithreading capable.
	- To aid during conversion of the code it would be helpful if the XModel supported some kind of "Freeze" pattern, disallowing changes after the fixup phase. The same holds for the Dex-Model, though here a stepwise freeze might be more appropriate. E.g, can't add classes, can't add members, can't modify method signatures or rename members, and finally can't modify method bodies.
	- Unfortunately, delegate instance types are created when compiling the method bodies. This might be the sole reason for the compilation phase not being multithreading capable. This also might prevent usage of the freeze pattern. Delegate instance creation doesn't happen very often, so maybe a large lock could do the trick.
- Once the compilation phase has been sufficiently optimized, further speedups should be possible by doing more aggressive caching when using the compiler as a service in VS. This caching can include:
	- The mapfile and the generated class structure for the compiler cache
	- Loaded assemblies (if the IL conversion code is altered to not modify them any more)
	- When using graphs for reachable detection the reachable graph for each assembly.
- With all above optimizations applied, saving of the .dex would become the most time consuming step. Futher investigation should look for bottlenecks there.

- precompiling the dot42 compiler with `ngen.exe` does not necessarily lead to reduced compile time. 

- I believe it should in theory be possible to reduce the compile time to a few seconds even for large projects and non-incremental builds. This would require larger changes on how the compiler is internally implemented. They key would be to drop the visitor pattern in a few places, and replace it with proper lookup tables. These would need to be dynamically updated to always reflect the current code structure. For example during Ast conversion, a converter should be able to simple lookup all "call" expressions. Currently each converter goes through all expressions to find what is of interest to him.
    
### Further thoughts

- The Dot42 compiler might be significantly simplified by using Rosylin to convert CLR constructs that are incompatible with java and/or dex to compatible constructs.
  During a transition phase, maybe the current conversion functionality could be reused by converting Rosylins Ast to dot42s Ast, processing, and converting back.
- The ideas employed in Dot42 might be used to create a C# to Java converter, based on Rosylin.
- Could LLVM be introduced into the compile process?

- CreateDelegate might be partitially implemented using http://docs.oracle.com/javase/7/docs/api/java/lang/invoke/MethodHandle.html
 http://stackoverflow.com/questions/19557829/faster-alternatives-to-javas-reflection