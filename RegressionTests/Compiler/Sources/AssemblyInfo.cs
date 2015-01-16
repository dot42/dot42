using Dot42;
using Dot42.Manifest;

[assembly: Application("dot42 Compiler Tests")]
[assembly: Instrumentation(Label = "dot42 Compiler Tests", FunctionalTest = true)]
[assembly: UsesLibrary("android.test.runner")]