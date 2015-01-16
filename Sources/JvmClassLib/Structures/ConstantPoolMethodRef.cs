using System.Collections.Generic;
using System.Linq;

namespace Dot42.JvmClassLib.Structures
{
    public class ConstantPoolMethodRef : ConstantPoolMemberRef
    {
        private MethodDefinition resolved;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ConstantPoolMethodRef(ConstantPool constantPool, int classIndex, int nameAndTypeIndex)
            : base(constantPool, classIndex, nameAndTypeIndex)
        {
        }

        /// <summary>
        /// Resolve this reference.
        /// </summary>
        public MethodDefinition Resolve()
        {
            MethodDefinition method;
            if (TryResolve(out method))
                return method;
            throw new ResolveException(string.Format("Method {0} not found", this));
        }

        /// <summary>
        /// Resolve this reference.
        /// </summary>
        public bool TryResolve(out MethodDefinition method)
        {
            if (resolved == null)
            {
                ClassFile @class;
                if (TryResolveClass(out @class))
                {
                    // Look in method of the class and it's super classes
                    foreach (var currentClass in SelfBaseAndImplementedClasses(@class))
                    {
                        resolved = currentClass.Methods.FirstOrDefault(IsMatchByNameAndDescriptor);
                        if (resolved != null)
                        {
                            break;
                        }
                    }
                }
            }
            method = resolved;
            return (method != null);
        }

        /// <summary>
        /// Gets all classes that the given classes extends or implements
        /// </summary>
        private IEnumerable<ClassFile> SelfBaseAndImplementedClasses(ClassFile original)
        {
            // Return self
            yield return original;

            // Base classes
            var currentClass = original;
            while (currentClass != null)
            {
                if (currentClass.TryGetSuperClass(out currentClass))
                {
                    yield return currentClass;
                }
                else
                {
                    break;
                }
            }

            // Implemented interfaces
            currentClass = original;
            while (currentClass != null)
            {
                if (currentClass.Interfaces != null)
                {
                    foreach (var intfRef in currentClass.Interfaces)
                    {
                        ClassFile intfClass;
                        if (Loader.TryLoadClass(intfRef.ClassName, out intfClass))
                        {
                            foreach (var x in SelfBaseAndImplementedClasses(intfClass))
                            {
                                yield return x;
                            }
                        }
                    }
                }
                if (currentClass.TryGetSuperClass(out currentClass))
                {
                    yield return currentClass;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Match the given method on name and descriptor.
        /// </summary>
        private bool IsMatchByNameAndDescriptor(MethodDefinition method)
        {
            return (Name == method.Name) && (Descriptor == method.Descriptor);
        }

        /// <summary>
        /// Gets the tag of this entry
        /// </summary>
        public override ConstantPoolTags Tag
        {
            get { return ConstantPoolTags.Methodref; }
        }
    }
}
