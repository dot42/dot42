using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Package;
using VSXmlEditorService = Microsoft.VisualStudio.XmlEditor.XmlEditorService;

namespace Dot42.VStudio.XmlEditor
{
    public class XmlEditorService : IXmlEditorService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly VSXmlEditorService xmlEditorService;
        private IXmlLanguageService languageService;

        /// <summary>
        /// Default ctor
        /// </summary>
        public XmlEditorService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            xmlEditorService = serviceProvider.GetService(typeof(VSXmlEditorService)) as VSXmlEditorService;
            if (xmlEditorService == null)
                throw new ArgumentException("XmlEditorService not found");
        }

        IXmlStore IXmlEditorService.CreateXmlStore()
        {
            var store = xmlEditorService.CreateXmlStore();
            if (store == null)
                return null;
            return new XmlStore(store);
        }

        IXmlLanguageService IXmlEditorService.GetLanguageService()
        {
            if (languageService != null)
                return languageService;

            var internalLanguageService = GetXmlLanguageService();
            if (internalLanguageService == null)
                return null;

            languageService = new XmlLanguageService(internalLanguageService);
            return languageService;
        }

        /// <summary>
        /// Get the XML Editor language service
        /// </summary>
        /// <returns></returns>
        private LanguageService GetXmlLanguageService()
        {
            var vssp = serviceProvider.GetService(typeof (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)) as
                Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
            var xmlEditorGuid = new Guid("f6819a78-a205-47b5-be1c-675b3c7f0b8e");
            var iunknown = new Guid("00000000-0000-0000-C000-000000000046");
            IntPtr ptr;
            if ((vssp != null) && (vssp.QueryService(ref xmlEditorGuid, ref iunknown, out ptr) == 0))
            {
                try
                {
                    return Marshal.GetObjectForIUnknown(ptr) as LanguageService;
                }
                finally
                {
                    Marshal.Release(ptr);
                }
            }
            return null;
        }
    }
}
