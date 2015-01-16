// Guids.cs
// MUST match guids.h

using System;

namespace Dot42.VStudio
{
    /// <summary>
    /// All guids used in this project.
    /// </summary>
    static class GuidList
    {
        public static class Strings
        {
#if ANDROID
            private const string prefix = "337B7DB7-2D1E-448D-BEBF-";
                                           
#elif BB
            private const string prefix = "337B7DB7-E1D2-D844-BEBF-";
#else
#error Define target               
#endif

            // Guid of the package
            internal const string guidDot42Package = prefix + "1BFB9A61C08F";
            internal const string guidDot42ProjectFlavorPackage = prefix + "17E887A46E37";
            internal const string guidDot42OutputPane = prefix + "105A6BDF2342";

            // UI
            internal const string guidDot42ProjectCmdSet = prefix + "517E25185656";
            internal const string guidDot42LogCatToolWindow = prefix + "5240BA0DFEE9";

            // Guid of the Android Property Page
            internal const string guidDot42AndroidPropertyPage = prefix + "A1753B9A7535";

            // Debugger guids
            internal const string guidDot42DebuggerId = prefix + "E950A06312EF";
            internal const string guidDot42DebugEngineClsid = prefix + "76C5E1A7EE0F";
            internal const string guidDot42PortSupplierId = prefix + "9DE2D54956AB";
            internal const string guidDot42PortSupplierClsid = prefix + "04B0D9710136";
            internal const string guidDot42ProgramProviderClsid = prefix + "C0E1C579B036";

            // Editor guids
            internal const string guidXmlEditorFactory = prefix + "87797CC1479F";
        }

        public static class Guids
        {
            // UI
            internal static readonly Guid guidDot42ProjectCmdSet = new Guid(Strings.guidDot42ProjectCmdSet);

            // Debugger
            internal static readonly Guid guidDot42DebuggerId = new Guid(Strings.guidDot42DebuggerId);

            // Editor
            internal static readonly Guid guidXmlEditorFactory = new Guid(Strings.guidXmlEditorFactory);
        }
    };
}