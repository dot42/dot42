using System;
using Dot42.DexLib;

namespace Dot42.CompilerLib.DexVerifier
{
    internal class Verifier
    {
        private readonly Action<string> onError;

        /// <summary>
        /// Default ctor
        /// </summary>
        private Verifier(Action<string> onError)
        {
            this.onError = onError;
        }

        /// <summary>
        /// Verify the given dex structure.
        /// </summary>
        internal static void Verify(Dex dex, Action<string> onError)
        {
            var v = new Verifier(onError);
            v.VerifyMethods(dex);
        }

        /// <summary>
        /// Verify all methods
        /// </summary>
        private void VerifyMethods(Dex dex)
        {
            foreach (var @class in dex.GetClasses())
            {
                VerifyClass(@class);
            }
        }

        /// <summary>
        /// Verify the given class
        /// </summary>
        private void VerifyClass(ClassDefinition @class)
        {
            foreach (var method in @class.Methods)
            {
                VerifyMethod(method);
            }            
            foreach (var inner in @class.InnerClasses)
            {
                VerifyClass(inner);
            }
        }

        /// <summary>
        /// Verify the given method
        /// </summary>
        private void VerifyMethod(MethodDefinition method)
        {
            if (method.Body == null)
            {
                if (method.IsConstructor)
                {
                    onError(string.Format("Constructor has no body: {0}", method));
                }
            }
            else
            {
                foreach (var handler in method.Body.Exceptions)
                {
                    if (handler.TryEnd.Offset < handler.TryStart.Offset)
                    {
                        onError(string.Format("Invalid exception handler 0x{0:X4}-0x{1:X4} in {2}", handler.TryStart.Offset,
                                              handler.TryEnd.Offset, method.Name));
                    }
                }
            }
        }
    }
}
