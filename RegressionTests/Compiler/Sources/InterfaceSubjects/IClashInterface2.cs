namespace Dot42.Tests.Compiler.Sources.InterfaceSubjects
{
    public interface IClashInterface2
    {
        /// <summary>
        /// This will clash with <see cref="IExtSimpleInterface.FooInt"/>
        /// </summary>
        int FooInt();
    }
}
