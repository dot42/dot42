using System.ComponentModel.Composition;
using System.Linq;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Reachable;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.ILConversion
{
    /// <summary>
    /// Will make fields and methods public when they are private or family and
    /// accessed from a subclass.
    /// 
    /// uses the marker "IsFamilyOrAssembly" to mark members that can be protected
    /// because they reside in the same namespace, and "IsPublic" otherwise.
    /// </summary>
    [Export(typeof(ILConverterFactory))]
    internal class FixMemberAccess : ILConverterFactory
    {
        /// <summary>
        /// Low values come first
        /// </summary>
        public int Priority
        {
            get { return 50; }
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
            /// <summary>
            /// Create static ctors for types that have dllimport methods.
            /// </summary>
            public void Convert(ReachableContext reachableContext)
            {
                var rechableMethods = reachableContext.ReachableTypes.SelectMany(r=>r.Methods)
                                                      .Where(m=> m.IsReachable && m.HasBody);

                foreach (var method in rechableMethods)
                {
                   FixAccesses(method.Body);
                }
            }

            private void FixAccesses(MethodBody body)
            {
                var methodDeclaringType = body.Method.DeclaringType;
                bool isMethodInDexImport = methodDeclaringType.HasDexImportAttribute();

                foreach (var ins in body.Instructions)
                {
                    var op = ins.Operand as MemberReference;
                    if (op == null || op.DeclaringType == null)
                        continue;

                    var memberDeclaringType = op.DeclaringType.Resolve();
                    if (memberDeclaringType.IsGenericInstance)
                        memberDeclaringType = memberDeclaringType.GetElementType().Resolve();

                    if (memberDeclaringType == methodDeclaringType)
                        continue;

                    bool isMemberInDexImport = memberDeclaringType.HasDexImportAttribute();
                    
                    if (isMethodInDexImport && isMemberInDexImport) 
                        continue; // these end up in the same class.

                    var fieldRef = op as FieldReference;
                    var methodRef = op as MethodReference;

                    if (fieldRef != null)
                    {
                        var field = fieldRef.Resolve();
                        if (field == null) continue;
                        if (field.IsPublic) continue;

                        if ((field.IsFamily || field.IsFamilyOrAssembly)
                            && IsSubclass(memberDeclaringType, methodDeclaringType))
                            continue;

                        bool isSameNamespace = IsSameNamespace(isMethodInDexImport, isMemberInDexImport, memberDeclaringType, methodDeclaringType);

                        // reset member access mask
                        field.Attributes &= ~FieldAttributes.FieldAccessMask;
                        if (isSameNamespace) field.IsFamilyOrAssembly  = true;
                        else                 field.IsPublic = true;

                       
                    }
                    else if (methodRef != null)
                    {
                        var method = methodRef.Resolve();
                        if (method == null) continue;
                        if (method.IsPublic) continue;

                        if ((method.IsFamily || method.IsFamilyOrAssembly) 
                            && IsSubclass(methodDeclaringType, memberDeclaringType))
                            continue;

                        bool isSameNamespace = IsSameNamespace(isMethodInDexImport, isMemberInDexImport, memberDeclaringType, methodDeclaringType);

                        // reset member access mask
                        method.Attributes &= ~MethodAttributes.MemberAccessMask;
                        if (isSameNamespace) method.IsFamilyOrAssembly = true;
                        else                 method.IsPublic = true;
                    }
                }
            }

            private bool IsSameNamespace(bool isMethodInDexImport, bool isMemberInDexImport, TypeDefinition memberDeclaringType, TypeDefinition methodDeclaringType)
            {
                return (!isMethodInDexImport && !isMemberInDexImport
                        && memberDeclaringType.Scope.Name == methodDeclaringType.Scope.Name)
                       && GetNameSpace(memberDeclaringType) == GetNameSpace(methodDeclaringType);
            }

            private string GetNameSpace(TypeDefinition type)
            {
                if (type.IsNested)
                    return GetNameSpace(type.DeclaringType);
                return type.Namespace;
            }

            private bool IsSubclass(TypeReference subClassReference, TypeDefinition baseClass)
            {
                while (subClassReference != null)
                {
                    var subClass = subClassReference.Resolve();
                    if (subClass == baseClass)
                        return true;
                    subClassReference = subClass.BaseType;
                }
                return false;
            }
        }
    }    
}