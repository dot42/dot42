using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Mail;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Reachable;
using Mono.Cecil;

namespace Dot42.CompilerLib.ILConversion
{
    /// <summary>
    /// Change base class of NUnit TextFixture's.
    /// </summary>
    [Export(typeof (ILConverterFactory))]
    internal class NUnitTextFixtureBaseClass : ILConverterFactory
    {
        /// <summary>
        /// Low values come first
        /// </summary>
        public int Priority
        {
            get { return 100; }
        }

        /// <summary>
        /// Create the converter
        /// </summary>
        public ILConverter Create()
        {
            return new Converter();
        }

        private class Converter : ILConverter
        {
            private const string NUnitTestCaseFullName = "Dot42.Test.NUnitTestCase";
            private ReachableContext reachableContext;
            private TypeDefinition baseType;


            /// <summary>
            /// Convert calls to android extension ctors.
            /// </summary>
            public void Convert(ReachableContext reachableContext)
            {
                // Save context
                this.reachableContext = reachableContext;

                // Collect all names
                var textFixtures = reachableContext.ReachableTypes.Where(x => x.HasNUnitTestFixtureAttribute()).ToList();
                if (textFixtures.Count == 0)
                    return;

                foreach (var type in textFixtures)
                {
                    ConvertType(type);
                }
            }

            /// <summary>
            /// Convert base class of given type.
            /// </summary>
            private void ConvertType(TypeDefinition testFixtureType)
            {
                if (IsNUnitTestCase(testFixtureType))
                    return; // Done
                if (!testFixtureType.BaseType.IsSystemObject())
                    throw new CompilerException(string.Format("Test fixture {0} must derive from Object or {1}",
                                                              testFixtureType.FullName, NUnitTestCaseFullName));

                // Set base type
                baseType = baseType ?? reachableContext.ReachableTypes.Single(x => x.FullName == NUnitTestCaseFullName);
                testFixtureType.BaseType = baseType;

                // Alter ctors
                foreach (var ctor in testFixtureType.Methods.Where(x => x.IsConstructor))
                {
                    ConvertCtor(ctor);
                }

                
                // make sure the class doesn't override any of our methods.
                foreach (var method in testFixtureType.Methods)
                {
                    if (new[] {"SetUp", "TearDown", "CountTestCases", "RunTest"}.Contains(method.Name))
                        method.SetName(method.Name + "Impl");
                }
            }

            /// <summary>
            /// Change the call to the base ctor
            /// </summary>
            private void ConvertCtor(MethodDefinition ctor)
            {
                if (!ctor.HasBody)
                    return;
                foreach (var ins in ctor.Body.Instructions)
                {
                    var methodRef = ins.Operand as MethodReference;
                    if (methodRef == null)
                        continue;
                    if (methodRef.Name != ".ctor")
                        continue;
                    if (!methodRef.DeclaringType.IsSystemObject())
                        continue;
                    // ctor must have 0 arguments
                    if (methodRef.Parameters.Count != 0)
                        throw new CompilerException(
                            string.Format("Test fixture type {0} must only call default constructors.",
                                          ctor.DeclaringType.FullName));
                    ins.Operand =
                        baseType.Methods.First(x => x.IsConstructor && (x.Parameters.Count == 0) && (x.Name == ".ctor"));
                }
            }

            /// <summary>
            /// Is the given type (or its base types) Junit.Framework.TestCase?
            /// </summary>
            private static bool IsNUnitTestCase(TypeDefinition type)
            {
                while (type != null)
                {
                    if (type.FullName == NUnitTestCaseFullName)
                        return true;
                    if (type.BaseType == null)
                        return false;
                    type = type.BaseType.GetElementType().Resolve();
                }
                return false;
            }
        }
    }
}