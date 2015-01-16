using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TallComponents.Common.Extensions
{
    internal static class ExceptionExtensions
    {
        /// <summary>
        /// Gets a proper message for the given exception.
        /// Various exception types are handled.
        /// </summary>
        internal static string GetMessage(this Exception ex)
        {
#if !DOTNET2
            var aex = ex as AggregateException;
            if (aex != null)
            {
                ex = aex = aex.Flatten();
                var inner = aex.InnerExceptions;
                if (inner.Count == 1)
                {
                    return inner[0].GetMessage();
                }
                if (inner.Count > 1)
                {
                    return string.Join(", ", inner.Take(5).Select(x => x.GetMessage()));
                }
            }
#endif
            return ex.Message;
        }
    }
}
