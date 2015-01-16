namespace Dot42.JvmClassLib
{
    public interface IReachableContext
    {
        /// <summary>
        /// Is the given type reference part of the "product" that should be included in the reachables search?
        /// </summary>
        bool Contains(TypeReference typeRef);

        /// <summary>
        /// Walk over the given member to marks its children reachable.
        /// </summary>
        void Walk(AbstractReference member);

        /// <summary>
        /// Called every time a new reachable member is detected.
        /// </summary>
        void NewReachableDetected();
    }
}
