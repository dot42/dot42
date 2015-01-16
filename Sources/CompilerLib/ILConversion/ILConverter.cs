namespace Dot42.CompilerLib.ILConversion
{
    public interface ILConverter
    {
        /// <summary>
        /// Convert the given reachable state.
        /// </summary>
        void Convert(Reachable.ReachableContext context);
    }
}
