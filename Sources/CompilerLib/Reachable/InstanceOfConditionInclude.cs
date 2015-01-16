using System;
using System.Diagnostics;
using Dot42.CompilerLib.Extensions;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.Extensions;
using Mono.Cecil;
using TypeReference = Mono.Cecil.TypeReference;

namespace Dot42.CompilerLib.Reachable
{
    [DebuggerDisplay("{instanceOfCondition}")]
    internal sealed class InstanceOfConditionInclude
    {
        private readonly TypeDefinition instanceOfCondition;
        private readonly string className;

        /// <summary>
        /// Default ctor
        /// </summary>
        public InstanceOfConditionInclude(TypeReference instanceOfCondition)
        {
            if (instanceOfCondition == null)
                throw new ArgumentNullException("instanceOfCondition");
            this.instanceOfCondition = instanceOfCondition.GetElementType().Resolve();
            if (this.instanceOfCondition == null) 
                throw new CompilerException(string.Format("Cannot resolve InstanceOfCondition {0}", instanceOfCondition.FullName));

            var attr = this.instanceOfCondition.GetDexOrJavaImportAttribute();
            className = (attr != null) ? (string)attr.ConstructorArguments[0].Value : null;
        }

        /// <summary>
        /// The given type has been made reachable.
        /// </summary>
        /// <returns>True if the type was marked reachable, false otherwise</returns>
        internal bool IncludeIfNeeded(ReachableContext context, TypeDefinition type)
        {
            if (type.IsReachable)
                return true; // Already included

            if (instanceOfCondition.IsInterface)
            {
                // Check implements
                if (Implements(context, type))
                {
                    type.MarkReachable(context);
                    return true;
                }
            }
            else
            {
                // Check extends
                if (Extends(context, type))
                {
                    type.MarkReachable(context);
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// The given type has been made reachable.
        /// </summary>
        /// <returns>True if the class was marked reachable, false otherwise</returns>
        internal bool IncludeIfNeeded(ReachableContext context, ClassFile javaClass)
        {
            if (javaClass.IsReachable)
                return true; // Already included
            if (className == null)
                return false;

            if (instanceOfCondition.IsInterface)
            {
                // Check implements
                if (Implements(context, javaClass))
                {
                    javaClass.MarkReachable(context);
                    return true;
                }
            }
            else
            {
                // Check extends
                if (Extends(javaClass))
                {
                    javaClass.MarkReachable(context);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Is our <see cref="instanceOfCondition"/> a base class of the given type?
        /// </summary>
        private bool Extends(ReachableContext context, TypeDefinition type)
        {
            while (type != null)
            {
                if (type == instanceOfCondition)
                    return true;
                if (type.BaseType == null)
                    return false;
                type = type.BaseType.GetElementType().Resolve(context);
            }
            return false;
        }

        /// <summary>
        /// Is our <see cref="instanceOfCondition"/> a base class of the given type?
        /// </summary>
        private bool Extends(ClassFile javaClass)
        {
            while (javaClass != null)
            {
                if (javaClass.ClassName == className)
                    return true;
                ClassFile superClass;
                if (!javaClass.TryGetSuperClass(out superClass))
                    return false;
                javaClass = superClass;
            }
            return false;
        }

        /// <summary>
        /// Is our <see cref="instanceOfCondition"/> an interface implemented by the given type (or one of it's base types)?
        /// </summary>
        private bool Implements(ReachableContext context, TypeDefinition type)
        {
            while (type != null)
            {
                if (type == instanceOfCondition)
                    return true;

                if (type.HasInterfaces)
                {
                    foreach (var intfRef in type.Interfaces)
                    {
                        var intf = intfRef.Interface.GetElementType().Resolve(context);
                        if (Implements(context, intf))
                            return true;
                    }
                }

                if (type.BaseType == null)
                    return false;
                type = type.BaseType.GetElementType().Resolve(context);
            }
            return false;
        }

        /// <summary>
        /// Is our <see cref="instanceOfCondition"/> an interface implemented by the given type (or one of it's base types)?
        /// </summary>
        private bool Implements(ReachableContext context, ClassFile javaClass)
        {
            while (javaClass != null)
            {
                if (javaClass.ClassName == className)
                    return true;

                if (javaClass.Interfaces != null)
                {
                    foreach (var intfRef in javaClass.Interfaces)
                    {
                        ClassFile intf;
                        if (context.TryLoadClass(intfRef.ClassName, out intf))
                        {
                            if (Implements(context, intf))
                                return true;
                        }
                    }
                }

                ClassFile superClass;
                if (!javaClass.TryGetSuperClass(out superClass))
                    return false;
                javaClass = superClass;
            }
            return false;
        }
    }
}
