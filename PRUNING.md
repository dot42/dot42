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

#### Using `[SerializedParameter]`-Attribute

Specifies that a parameter is used in serialization. Types and objects passed as this parameter will have all their public fields and public and private properties preserved and not pruned.
This preservation propagates recursivly to the types of fields and properties of the passed type.

```

public class JsonConvert
{
	public static T Deserialize<[SerializedParameter] T>(string json) 
	{
		...
	}

	public static object Deserialize([SerializedParameter] Type type, string json) 
	{
		...
	}
	
	public string Serialize([SerializedParameter] object obj)
	{
		...
	}

	public string Serialize<[SerializedParameter] T>(T obj)
	{
		...
	}
}

// MyClass1 will have all fields and properties preserved.
>> var class1 = JsonConvert.Deserialize<MyClass1>(json);
>> var obj    = JsonConvert.Deserialize(typeof(MyClass1), json);

class MyClass2
{
	public MyClass3 class3;
	...
}

// MyClass2 and MyClass3 will have all their fields and properties preserved.
>> var class2 = new MyClass2 { ... }
>> string json = JsonConvert.Serialize(class2);

```
#### Using `[Dot42.Include]` on the assembly level
###### Including types
```
// Note that ApplyToMembers is ignored.
[assembly: Include(Type=typeof(MyClass))]
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
// For INotifyPropertyChanged implementing classes keep all public members to enable method/property databinding
[assembly: Include(Pattern = "Apply to type * when inherits('INotifyPropertyChanged'): Apply to member * when public: Dot42.Include")]
// Keep also private properties for use with code-based databinding.
[assembly: Include(Pattern = "Apply to type * when inherits('INotifyPropertyChanged'): Apply to member set_*: Dot42.Include")]
[assembly: Include(Pattern = "Apply to type * when inherits('INotifyPropertyChanged'): Apply to member get_*: Dot42.Include")]

// Keep all view types in all assemblies, possibly referenced from AXML. Some bytes might be saved
// if only the actual used types are included.
[assembly: Include(Pattern = "Apply to type * when extends('Android.Views.View'): Dot42.IncludeType", IsGlobal = true)]

// Keep MvvmCross reflection looked-up ressouce ids.
[assembly: Include(Pattern = "Apply to type *.R/*Mvx*: Dot42.IncludeType")]
[assembly: Include(Pattern = " Apply to type *.R/*: Apply to member Mvx*: Dot42.Include")]

// Keep MvvmCross ValueConverters
[assembly: Include(Pattern = "Apply to type * when implements('IMvxValueConverter'): Dot42.Include", IsGlobal = true)]

// Keep ICommand, including the event which is used by MvvmCross through reflection.
[assembly: Include(Pattern = "Apply to type System.Windows.Input.ICommand: Dot42.IncludeType", IsGlobal = true)]

// Keep bootstrapper and setup.
[assembly: Include(Pattern = "Apply to type *.Setup: Dot42.IncludeType")]
[assembly: Include(Pattern = "Apply to type * when inherits('IMvxBootstrapAction'): Dot42.IncludeType")]

// make Dot42 preserve property information for framework classes, to enable databinding on them.
[assembly: Include(Type = typeof(IncludeFrameworkProperties))]

// Include my types found through dependency injection.
[assembly: Include(Pattern = "Apply to type *Service: Dot42.Include")]
[assembly: Include(Pattern = "Apply to type *Factory: Dot42.Include")]
[assembly: Include(Pattern = "Apply to type *Manager: Dot42.Include")]
```

