using System;

namespace Dot42.DexLib
{
    public interface IMemberReference : IEquatable<IMemberReference>
    {
        string Name { get; set; }
    }
}