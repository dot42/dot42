using System;
using System.Linq;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.JvmClassLib;
using Dot42.JvmClassLib.Attributes;
using Dot42.JvmClassLib.Structures;
using Dot42.Utility;

namespace Dot42.CompilerLib.Reachable.Java
{
    /// <summary>
    /// Helper used to find all reachable classes.
    /// </summary>
    internal sealed class ReachableWalker
    {
        /// <summary>
        /// Walk the given member.
        /// </summary>
        internal static void Walk(ReachableContext context, AbstractReference member)
        {
            if (member == null)
                return;

            ClassFile classFile;
            FieldDefinition field;
            MethodDefinition method;
            TypeReference typeRef;
            if ((classFile = member as ClassFile) != null)
            {
                Walk(context, classFile);
            }
            else if ((method = member as MethodDefinition) != null)
            {
                Walk(context, method);
            }
            else if ((field = member as FieldDefinition) != null)
            {
                Walk(context, field);
            }
            else if ((typeRef = member as TypeReference) != null)
            {
                Walk(context, typeRef);
            }
        }

        /// <summary>
        /// Mark all base types and externally visible members reachable
        /// </summary>
        private static void Walk(ReachableContext context, TypeReference type)
        {
            if (type == null)
                return;

            if (type.IsArray)
            {
                var aType = (ArrayTypeReference)type;
                Walk(context, aType.ElementType);
            }
            else if (type.IsObjectType)
            {
                var oType = (ObjectTypeReference)type;
                foreach (var typeArg in oType.Arguments)
                {
                    Walk(context, typeArg.Signature);
                }
                ClassFile classFile;
                if (context.TryLoadClass(oType.ClassName, out classFile))
                {
                    classFile.MarkReachable(context);
                }
            }
            else if (type.IsBaseType || type.IsVoid || type.IsTypeVariable)
            {
                // Not need to mark anything
            }
            else
            {
                throw new ArgumentException("Unknown type: " + type);
            }
        }

        /// <summary>
        /// Mark all eachable items in argument as such.
        /// </summary>
        private static void Walk(ReachableContext context, ClassFile classFile)
        {
            // Mark owner
            classFile.DeclaringClass.MarkReachable(context);

            // Mark base class
            classFile.SuperClass.MarkReachable(context);

            // Mark interfaces
            if (classFile.Interfaces != null)
            {
                foreach (var intf in classFile.Interfaces)
                {
                    intf.MarkReachable(context);
                }
            }

            // Mark class ctor
            classFile.Methods.FirstOrDefault(x => x.Name == "<clinit>").MarkReachable(context);

            // Mark methods of imported classes reachable
            if (classFile.IsCreatedByLoader)
            {
                classFile.Methods.Where(x => x.IsPublic || x.IsProtected).ForEach(x => x.MarkReachable(context));
            }

            // If this is a class that does not have an imported C# wrapper, be safe and include everything from this class.
            if (!context.HasDexImport(classFile))
            {
                context.MarkAsRoot(classFile);
            }

            // Record in context
            context.RecordReachableType(classFile);
        }

        /// <summary>
        /// Mark all eachable items in argument as such.
        /// </summary>
        private static void Walk(ReachableContext context, FieldDefinition field)
        {
            field.FieldType.MarkReachable(context);
            field.DeclaringClass.MarkReachable(context);
        }

        /// <summary>
        /// Mark all eachable items in argument as such.
        /// </summary>
        private static void Walk(ReachableContext context, MethodDefinition method)
        {
            method.DeclaringClass.MarkReachable(context);
            method.ReturnType.MarkReachable(context);

            // All parameters
            foreach (var param in method.Parameters)
            {
                param.MarkReachable(context);
            }

            // Base methods
            if (!method.IsStatic && !method.IsFinal)
            {
                MethodDefinition baseMethod;
                if ((baseMethod = method.GetBaseMethod()) != null)
                {
                    if (context.Contains(baseMethod.DeclaringClass))
                    {
                        baseMethod.MarkReachable(context);
                    }
                }
            }

            Walk(context, method.Body);
        }

        /// <summary>
        /// Mark all eachable items in argument as such.
        /// </summary>
        private static void Walk(ReachableContext context, CodeAttribute code)
        {
            if (code == null) 
                return;

            // Exception handlers
            foreach (var handler in code.ExceptionHandlers)
            {
                handler.CatchType.MarkReachable(context);
            }

            // Local variables
            /*foreach (var var in code.Variables)
                {
                    var.VariableType.MarkReachable(context);
                }*/

            // Instructions
            foreach (var ins in code.Instructions)
            {
                object operand = ins.Operand;
                if (operand != null)
                {
                    ConstantPoolClass cpClass;
                    ConstantPoolFieldRef cpField;
                    ConstantPoolMethodRef cpMethod;

                    if ((cpClass = operand as ConstantPoolClass) != null)
                    {
                        ClassFile cf;
                        if (cpClass.TryResolve(out cf))
                        {
                            cf.MarkReachable(context);
                        }
                    }
                    else if ((cpField = operand as ConstantPoolFieldRef) != null)
                    {
                        FieldDefinition fieldDef;
                        if (cpField.TryResolve(out fieldDef))
                        {
                            fieldDef.MarkReachable(context);
                        }
                    }
                    else if ((cpMethod = operand as ConstantPoolMethodRef) != null)
                    {
                        MethodDefinition method;
                        if (cpMethod.TryResolve(out method))
                        {
                            method.MarkReachable(context);
                        }
                        else
                        {
                            
                        }
                    }
                }

            }
        }
    }
}
