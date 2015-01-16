using Dot42;
using Dot42.Manifest;

[assembly: Application("dot42 CompilationModeAll Tests")]
[assembly: Instrumentation(Label = "dot42 CompilationModeAll Tests", FunctionalTest = true)]
[assembly: UsesLibrary("android.test.runner")]