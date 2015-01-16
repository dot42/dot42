namespace Dot42.DexLib
{
    internal class DexHeader
    {
        internal byte[] Magic { get; set; }
        internal uint CheckSum { get; set; }
        internal byte[] Signature { get; set; }

        internal uint FileSize { get; set; }
        internal uint HeaderSize { get; set; }
        internal uint EndianTag { get; set; }

        internal uint LinkSize { get; set; }
        internal uint LinkOffset { get; set; }

        internal uint MapOffset { get; set; }

        internal uint StringsSize { get; set; }
        internal uint StringsOffset { get; set; }

        internal uint TypeReferencesSize { get; set; }
        internal uint TypeReferencesOffset { get; set; }

        internal uint PrototypesSize { get; set; }
        internal uint PrototypesOffset { get; set; }

        internal uint FieldReferencesSize { get; set; }
        internal uint FieldReferencesOffset { get; set; }

        internal uint MethodReferencesSize { get; set; }
        internal uint MethodReferencesOffset { get; set; }

        internal uint ClassDefinitionsSize { get; set; }
        internal uint ClassDefinitionsOffset { get; set; }

        internal uint DataSize { get; set; }
        internal uint DataOffset { get; set; }
    }
}