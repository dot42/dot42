namespace Dot42.ImportJarLib.Model
{
    public interface INetMemberVisibility
    {
        /// <summary>
        /// Gets the name of the member.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Is this member public 
        /// </summary>
        bool IsPublic { get; set; }

        /// <summary>
        /// Is this member public and nested (only types)
        /// </summary>
        bool IsNestedPublic { get; set; }

        /// <summary>
        /// Is this member private 
        /// </summary>
        bool IsPrivate { get; set; }

        /// <summary>
        /// Is this member protected
        /// </summary>
        bool IsFamily { get; set; }

        /// <summary>
        /// Is this member internal
        /// </summary>
        bool IsAssembly { get; set; }

        /// <summary>
        /// Is this member internal or protected
        /// </summary>
        bool IsFamilyOrAssembly { get; set; }

        /// <summary>
        /// Is this member internal and protected
        /// </summary>
        bool IsFamilyAndAssembly { get; set; }

        /// <summary>
        /// Are this member and the given type definition in the same scope?
        /// </summary>
        bool HasSameScope(NetTypeDefinition type);

        /// <summary>
        /// Are this member and the given other member in the same scope?
        /// </summary>
        bool HasSameScope(INetMemberVisibility other);
    }
}
