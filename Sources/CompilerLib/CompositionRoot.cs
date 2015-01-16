using System.ComponentModel.Composition.Hosting;

namespace Dot42.CompilerLib
{
    public static class CompositionRoot
    {
        private static CompositionContainer compositionContainer;

        /// <summary>
        /// Get the MEF based composition container.
        /// </summary>
        internal static CompositionContainer CompositionContainer
        {
            get
            {
                if (compositionContainer == null)
                {
                    var catalog = new AggregateCatalog();
                    catalog.Catalogs.Add(new AssemblyCatalog(typeof(CompositionRoot).Assembly));
                    //catalog.Catalogs.Add(new DirectoryCatalog(".", "*.Plugin.dll"));

                    compositionContainer = new CompositionContainer(catalog);
                }
                return compositionContainer;
            }
        }

    }
}
