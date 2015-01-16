using Dot42;
using Dot42.Manifest;

[assembly: Application("dot42 NUnit Tests")]
[assembly: Instrumentation(Label = "dot42 NUnit Tests", FunctionalTest = true)]
[assembly: UsesLibrary("android.test.runner")]