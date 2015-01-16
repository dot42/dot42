using System.Reflection;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Dot42.TemplateBuilder")]

[assembly: ObfuscateAssembly(true)]

[assembly: Obfuscation(Feature = "inject /a:Mono.Options.dll /internal:true")]

[assembly: Obfuscation(Feature = "inject /a:Dot42.Utility.dll /internal:true")]

[assembly: Obfuscation(Feature="protect-serialization", ApplyToMembers=true)]
