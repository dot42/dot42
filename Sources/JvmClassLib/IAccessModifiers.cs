namespace Dot42.JvmClassLib
{
    public interface IAccessModifiers
    {
        /// <summary>
        /// Visible to class, package, subclass, world
        /// </summary>
        bool IsPublic { get; }

        /// <summary>
        /// Visible to class, package, subclass
        /// </summary>
        bool IsProtected { get; }

        /// <summary>
        /// Visible to class, package
        /// </summary>
        bool IsPackagePrivate { get; }

        /// <summary>
        /// Visible to class
        /// </summary>
        bool IsPrivate { get; }
    }
}
