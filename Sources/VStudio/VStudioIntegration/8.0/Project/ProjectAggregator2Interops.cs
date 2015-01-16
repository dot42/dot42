//***************************************************************************
//
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//***************************************************************************

namespace Microsoft.VisualStudio.ProjectAggregator2.Interop
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [ComImport]
    [Guid("D6CEA324-8E81-4e0e-91DE-E5D7394A45CE")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IVsProjectAggregator2
    {
        #region IVsProjectAggregator2 Members

        [PreserveSig]
        int SetInner(IntPtr innerIUnknown);
        [PreserveSig]
        int SetMyProject(IntPtr projectIUnknown);

        #endregion
    }
}
