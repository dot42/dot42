## Pruning

Dot42 employs and extensive system of pruning to keep the final `.apk` size to a minimum. All types and members of types that are not referenced from code will not be included in the output.
This works great, except when it comes to reflection and/or reflection based serialization. Types / members used in reflection can not automatically be detected by Dot42. Manual interventions is neccessary. There are several ways to hint to Dot42 which types or members are not to be pruned.

#### Applying `[Dot42.Include]`-Attribute to types or members 
This attribute  can be applied to types or members. When applied to a type, the type will be included. When applied to a member, the member will be included if the type is referenced or otherwise explicitly included. If `[Include(ApplyToMembers=true)]` is specified, all members will be automatically included as well. `[IncludeType]` is a shorthand for this. 
```
[Include]
class WillAlwaysBeIncluded1
{
	public int MightNotBeIncluded { get; set; }
}

class MightBeIncluded
{
	// Note how inclusion of the property is 
	// independent of inclusion of getters and setters
	[Include]
	public int WillBeIncludedIfTypeIsIncluded { [Include] get; [Include] set; }

	[Include]
	private int willBeIncludedIfTypeIsIncluded;
}

[Include(ApplyToMembers=True)]
class WillAlwaysBeIncluded2
{
	public int WillBeIncluded { get; set; }
}

[IncludeType]
class WillAlwaysBeIncluded3
{
	public int WillBeIncluded { get; set; }
}
```

#### Using `[SerializationMethod]`-Attribute
When applied to a method, all parameters passed passed into this method, including generic arguments, will have all their fields and properties preserved and not pruned. This applies recursively to the types of fields and properties.  

```

public class JsonConvert
{
	[SerializationMethod]
	public static T Deserialize<T>(string json) 
	{
		...
	}
	
	[SerializationMethod]
	public string Serialize(object obj)
	{
		...
	}
}

// MyClass1 will have all fields and properties preserved.
>> var class1 = JsonConvert.Deserialize<MyClass1>(json);

class MyClass2
{
	public MyClass3 class3;
	...
}

// MyClass2 and MyClass3 will have all their 
// fields and properties preserved.
>> var class2 = new MyClass2 { ... }
>> string json = JsonConvert.Serialize(class2);

```
#### Using `[Dot42.Include]` on the assembly level
###### Including types
```
[assembly: Include(Type=typeof(MyClass))]
[assembly: Include(Type=typeof(MyClass), ApplyToMembers=true)]
``` 
###### Inheritance condition
```
// This will include all types in the current assembly
// that implement INotifyPropertyChanged.
// Note that ApplyToMembers is ignored.
[assembly:Include(InstanceOfCondition=typeof(INotifyPropertyChanged))]

// If IsGlobal is true, this rule is applied to all assemblies.
[assembly:Include(InstanceOfCondition=typeof(INotifyPropertyChanged), IsGlobal=true)]
```
###### Pattern matching 

This allows you to specify types or members that should be included based on patterns. The syntax is for now loosely based on Eazyfuscators syntax. Use `IsGlobal=true` to apply a rule to all assemblies.
```
// DataBinding needs to see all public INotifyPropertyChanged members; also include private setters.
// Note that this does not automatically include the type.
[assembly: Include(Pattern = "Apply to type * when inherits('INotifyPropertyChanged'): Apply to member * when public: Dot42.Include")]
[assembly: Include(Pattern = "Apply to type * when inherits('INotifyPropertyChanged'): Apply to member set_*: Dot42.Include")]

// To keep all types that implement INotifyPropertyChanged, the following
// line would be needed as well.
// [assembly: Include(Pattern = "Apply to type * when inherits('INotifyPropertyChanged'): Dot42.Include")]

// Keep MvvmCross reflection looked-up ressouces.
[assembly: Include(Pattern = "Apply to type *.R/*Mvx*: Dot42.IncludeType")]
[assembly: Include(Pattern =" Apply to type *.R/*: Apply to member Mvx*: Dot42.Include")]

// keep ValueConverters, possibly referenced from .axml.
[assembly: Include(Pattern = "Apply to type * when implements('IMvxValueConverter'): Dot42.Include", IsGlobal = true)]

// Keep all view types, bound by MvvmCross through reflection
[assembly: Include(Pattern = "Apply to type *View: Dot42.Include")]
[assembly: Include(Pattern = "Apply to type *Dlg: Dot42.Include")]
[assembly: Include(Pattern = "Apply to type *Fragment: Dot42.Include")]

// alternativly, keep all ViewTypes in all assemblies. 
[assembly: Include(Pattern = "Apply to type * when extends('Android.Views.View'): Dot42.Include", IsGlobal = true)]

// Keep setup and bootstrapper.
[assembly: Include(Pattern = "Apply to type *.Setup: Dot42.IncludeType")]
[assembly: Include(Pattern = "Apply to type * when inherits('IMvxBootstrapAction'): Dot42.IncludeType")]

// Include types found through DI
[assembly: Include(Pattern = "Apply to type *Service: Dot42.Include")]
[assembly: Include(Pattern = "Apply to type *Factory: Dot42.Include")]
[assembly: Include(Pattern = "Apply to type *Manager: Dot42.Include")]


```

