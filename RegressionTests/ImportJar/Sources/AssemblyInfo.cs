using Dot42;
using Dot42.Manifest;

[assembly: Application("dot42 ImportJar Tests")]
[assembly: Instrumentation(Label = "dot42 ImportJar Tests", FunctionalTest = true)]
[assembly: UsesLibrary("android.test.runner")]