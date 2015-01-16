using System.Threading;

namespace Dot42.JvmClassLib
{
    public abstract class AbstractReference
    {
        private int reachable = 0; // No bool is used since we're using Interlocked.Exchange.

        /// <summary>
        /// Is this item reachable?
        /// </summary>
        public bool IsReachable { get { return (reachable != 0); } }

        /// <summary>
        /// Mark this type reachable.
        /// </summary>
        public void SetReachable(IReachableContext context)
        {
            // Already reachable?
            if (reachable != 0) { return; }

            // Mark it reachable
            if (Interlocked.Exchange(ref reachable, 1) == 0)
            {
                if (context != null)
                {
                    context.NewReachableDetected();

                    // Member was not yet walked, do it now unless its a type that is not in the project
                    if (ShouldWalk(context, this))
                    {
                        context.Walk(this);
                    }
                }
            }
        }

        /// <summary>
        /// Should we walk through the member for all children?
        /// </summary>
        private static bool ShouldWalk(IReachableContext context, AbstractReference member)
        {
            var typeRef = member as TypeReference;
            if (typeRef == null) { return true; }
            return context.Contains(typeRef);
        }

    }
}
