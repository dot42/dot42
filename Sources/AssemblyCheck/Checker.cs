using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dot42.CecilExtensions;
using Dot42.Documentation;
using Dot42.FrameworkDefinitions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.AssemblyCheck
{
    internal class Checker
    {
        private readonly string path;
        private readonly Action<MessageTypes, string, IMetadataScope, string> log;
        private readonly Action<int, int> setProgress;
        private readonly AssemblyResolver resolver;
        private int errors;
        private readonly Dictionary<string, CheckResolveData> resolveErrorTypeNames = new Dictionary<string, CheckResolveData>();
        private readonly Dictionary<string, CheckResolveData> resolveErrorFieldNames = new Dictionary<string, CheckResolveData>();
        private readonly Dictionary<string, CheckResolveData> resolveErrorMethodNames = new Dictionary<string, CheckResolveData>();
        private readonly FrameworkInfo framework;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal Checker(string path, FrameworkInfo framework, Action<MessageTypes, string, IMetadataScope, string> log, Action<int, int> setProgress)
        {
            this.path = path;
            this.framework = framework;
            this.log = log;
            this.setProgress = setProgress;
            var localFolder = Path.GetDirectoryName(path);
            resolver = new AssemblyResolver(localFolder, framework.Folder);
        }

        /// <summary>
        /// Check the assembly in the given path.
        /// </summary>
        public void Check()
        {
            AssemblyDefinition assembly;
            try
            {
                errors = 0;
                resolveErrorFieldNames.Clear();
                resolveErrorMethodNames.Clear();
                resolveErrorTypeNames.Clear();
                Log(MessageTypes.General, null, null, "Checking {0} to framework {1}", path, framework.Folder);
                assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters(ReadingMode.Immediate) { AssemblyResolver = resolver });
            }
            catch (Exception ex)
            {
                Error(MessageTypes.General, null, null, "Failed to open assembly: {0} because {1}", path, ex.Message);
                return;
            }

            var allTypeCount = assembly.Modules.Sum(x => x.Types.Count);
            var progress = 0;
            Check((ICustomAttributeProvider)assembly, assembly.Name.Name);
            foreach (var module in assembly.Modules)
            {
                Check(module, ref progress, allTypeCount);
            }

            // Done
            var allCheckData = resolveErrorFieldNames.Values.Concat(resolveErrorMethodNames.Values).Concat(resolveErrorTypeNames.Values).ToList();
            var allCheckCount = allCheckData.Sum(x => x.CheckCount);
            var errorCheckCount = allCheckData.Where(x => !x.IsAvailable).Sum(x => x.CheckCount);
            Log(MessageTypes.General, null, null, errors == 0 ? 
                "No problems have been found" : 
                string.Format("{0} separate problems have been found. {1}% of the used external references is not available in Dot42 {2}.", errors, (int)((errorCheckCount * 100.0) / allCheckCount), framework.Name));
        }

        /// <summary>
        /// Check customer attributes
        /// </summary>
        private void Check(ICustomAttributeProvider provider, string context)
        {
            foreach (var ca in provider.CustomAttributes)
            {
                Check(ca.AttributeType, context);
            }
        }

        /// <summary>
        /// Check the given module.
        /// </summary>
        private void Check(ModuleDefinition module, ref int progress, int allTopLevelTypeCount)
        {
            foreach (var type in module.Types)
            {
                setProgress(progress++, allTopLevelTypeCount);
                Check(type);
            }
        }

        /// <summary>
        /// Check the given type.
        /// </summary>
        private void Check(TypeDefinition type)
        {
            var context = type.FullName;
            if (type.BaseType != null)
                Check(type.BaseType, context);
            Check((ICustomAttributeProvider)type, context);
            foreach (var nestedType in type.NestedTypes)
            {
                Check(nestedType);
            }
            foreach (var @event in type.Events)
            {
                Check(@event);
            }
            foreach (var field in type.Fields)
            {
                Check(field);
            }
            foreach (var method in type.Methods)
            {
                Check(method);
            }
            foreach (var prop in type.Properties)
            {
                Check(prop);
            }
        }

        /// <summary>
        /// Check the given event.
        /// </summary>
        private void Check(EventDefinition @event)
        {
            var context = Format(@event);
            Check(@event.EventType, context);
            Check((ICustomAttributeProvider)@event, context);
        }

        /// <summary>
        /// Check the given field.
        /// </summary>
        private void Check(FieldDefinition field)
        {
            var context = Format(field);
            Check(field.FieldType, context);
            Check((ICustomAttributeProvider)field, context);
        }

        /// <summary>
        /// Check the given property.
        /// </summary>
        private void Check(PropertyDefinition prop)
        {
            var context = Format(prop);
            Check(prop.PropertyType, context);
            Check((ICustomAttributeProvider)prop, context);
        }

        /// <summary>
        /// Check the given method.
        /// </summary>
        private void Check(MethodDefinition method)
        {
            var context = Format(method, true);            
            Check(method.ReturnType, context);
            Check((ICustomAttributeProvider)method, context);
            foreach (var p in method.Parameters)
            {
                Check(p, context);
            }
            foreach (var gp in method.GenericParameters)
            {
                Check(gp, context);
            }
            if (method.HasBody)
                Check(method.Body);
        }

        /// <summary>
        /// Check the given method body and code.
        /// </summary>
        private void Check(MethodBody body)
        {
            var context = Format(body.Method, true);
            foreach (var v in body.Variables)
            {
                Check(v.VariableType, context);
            }
            foreach (var ins in body.Instructions)
            {
                var fieldRef = ins.Operand as FieldReference;
                if (fieldRef != null) Check(fieldRef, context);
                var methodRef = ins.Operand as MethodReference;
                if (methodRef != null) Check(methodRef, context);
                var typeRef = ins.Operand as TypeReference;
                if (typeRef != null) Check(typeRef, context);
            }
        }

        private void Check(ParameterDefinition p, string context)
        {
            Check(p.ParameterType, context);
            Check((ICustomAttributeProvider)p, context);
        }

        /// <summary>
        /// Check the given field reference.
        /// </summary>
        private void Check(FieldReference field, string context)
        {
            if (!Check(field.DeclaringType, context))
                return;
            var error = false;
            try
            {
                var fieldDef = field.Resolve();
                error = (fieldDef == null);
            }
            catch (Exception)
            {
                error = true;
            }
            CheckResolveData data;
            var key = field.FullName;
            if (!resolveErrorFieldNames.TryGetValue(key, out data))
            {
                data = new CheckResolveData { IsAvailable = !error };
                resolveErrorFieldNames.Add(key, data);
                if (error)
                {
                    Error(MessageTypes.MissingField, Format(field), field.DeclaringType.Scope, context);
                }
            }
            data.CheckCount++;
        }

        /// <summary>
        /// Check the given method reference.
        /// </summary>
        private void Check(MethodReference method, string context)
        {
            if (!Check(method.DeclaringType, context))
                return;
            var error = false;
            try
            {
                if (method.IsGenericInstance)
                {
                    Check((GenericInstanceMethod)method, context);
                }
                else
                {
                    var typeDef = method.DeclaringType.GetElementType().Resolve();
                    var emethod = method.GetElementMethod();
                    var methodDef = (typeDef != null) ? typeDef.Methods.FirstOrDefault(x => x.AreSameExcludingGenericArguments(emethod, null)) : null;
                    error = (methodDef == null);
                }
            }
            catch (Exception)
            {
                error = true;
            }
            CheckResolveData data;
            var key = method.FullName;
            if (!resolveErrorMethodNames.TryGetValue(key, out data))
            {
                data = new CheckResolveData { IsAvailable = !error };
                resolveErrorMethodNames.Add(key, data);
                if (error)
                {
                    Error(MessageTypes.MissingMethod, Format(method, false), method.DeclaringType.Scope, context);
                }
            }
            data.CheckCount++;
        }

        /// <summary>
        /// Check the given method.
        /// </summary>
        private void Check(GenericInstanceMethod method, string context)
        {
            foreach (var arg in method.GenericArguments)
            {
                Check(arg, method.FullName);
            }
            Check(method.ElementMethod, context);
        }

        /// <summary>
        /// Check the given type reference.
        /// </summary>
        /// <returns>True on success, false otherwise</returns>
        private bool Check(TypeReference type, string context)
        {
            bool error;
            try
            {
                if (type.IsGenericInstance)
                {
                    return Check((GenericInstanceType)type, context);
                }
                if (type.IsGenericParameter)
                {
                    return Check((GenericParameter)type, context);
                }
                if (type.IsArray)
                {
                    return Check((ArrayType)type, context);
                }
                if (type.IsByReference)
                {
                    return Check((ByReferenceType) type, context);
                }
                if (type.IsFunctionPointer)
                {
                    Error(MessageTypes.UnsupportedFeature, type.FullName, type.Scope, "Function pointer types are not supported (in {0})", context);
                    return false;
                }
                if (type.IsPinned)
                {
                    Error(MessageTypes.UnsupportedFeature, type.FullName, type.Scope, "Pinned are not supported ({0})", context);
                    return false;
                }
                if (type.IsOptionalModifier)
                {
                    Error(MessageTypes.UnsupportedFeature, type.FullName, type.Scope, "Optional modifier types are not supported (in {0})", context);
                    return false;
                }
                if (type.IsRequiredModifier)
                {
                    Error(MessageTypes.UnsupportedFeature, type.FullName, type.Scope, "Required modifier types are not supported (in {0})", context);
                    return false;
                }
                var typeDef = type.Resolve();
                error = (typeDef == null);
            }
            catch (Exception)
            {
                error = true;
            }
            CheckResolveData data;
            var key = type.FullName;
            if (!resolveErrorTypeNames.TryGetValue(key, out data))
            {
                data = new CheckResolveData { IsAvailable = !error };
                resolveErrorTypeNames.Add(key, data);
                if (error)
                {
                    Error(MessageTypes.MissingType, CecilFormat.GetTypeName(type), type.Scope, context);
                }
            }
            data.CheckCount++;
            return !error;
        }

        /// <summary>
        /// Check the given type.
        /// </summary>
        private bool Check(GenericInstanceType type, string context)
        {
            var result = true;
            foreach (var arg in type.GenericArguments)
            {
                if (!Check(arg, type.FullName)) result = false;
            }
            if (!Check(type.ElementType, context)) result = false;
            return result;
        }

        /// <summary>
        /// Check the given type.
        /// </summary>
        private bool Check(GenericParameter type, string context)
        {
            var result = true;
            foreach (var arg in type.Constraints)
            {
                if (!Check(arg, type.FullName)) result = false;
            }
            return result;
        }

        /// <summary>
        /// Check the given type.
        /// </summary>
        private bool Check(ArrayType type, string context)
        {
            return Check(type.ElementType, context);
        }

        /// <summary>
        /// Check the given type.
        /// </summary>
        private bool Check(ByReferenceType type, string context)
        {
            return Check(type.ElementType, context);
        }

        /// <summary>
        /// Log the given message.
        /// </summary>
        private void Error(MessageTypes type, string member, IMetadataScope  scope, string format, params object[] args)
        {
            errors++;
            Log(type, member, scope, format, args);
        }

        /// <summary>
        /// Log the given message.
        /// </summary>
        private void Log(MessageTypes type, string member, IMetadataScope scope, string format, params object[] args)
        {
            string msg;
            if ((args == null) || (args.Length == 0))
                msg = format;
            else
            {
                msg = string.Format(format, args);
            }
            log(type, member, scope, msg);
        }

        private static string Format(FieldReference field)
        {
            var sb = new StringBuilder();
            sb.Append(CecilFormat.GetTypeName(field.DeclaringType));
            sb.Append('.');
            sb.Append(field.Name);
            return sb.ToString();
        }

        private static string Format(EventReference @event)
        {
            var sb = new StringBuilder();
            sb.Append(CecilFormat.GetTypeName(@event.DeclaringType));
            sb.Append('.');
            sb.Append(@event.Name);
            return sb.ToString();
        }

        private static string Format(PropertyReference property)
        {
            var sb = new StringBuilder();
            sb.Append(CecilFormat.GetTypeName(property.DeclaringType));
            sb.Append('.');
            sb.Append(property.Name);
            return sb.ToString();
        }

        private static string Format(MethodReference method, bool includeGenericArguments)
        {
            var sb = new StringBuilder();
            sb.Append(CecilFormat.GetTypeName(method.DeclaringType, true, includeGenericArguments));
            sb.Append('.');
            sb.Append(method.Name);
            sb.Append(Descriptor.Format(method.GenericParameters));
            sb.Append('(');
            sb.Append(string.Join(", ", method.Parameters.Select(x => Descriptor.Format(x, includeGenericArguments))));
            sb.Append(')');
            return sb.ToString();
        }

        private class CheckResolveData
        {
            public int CheckCount;
            public bool IsAvailable;
        }
    }
}
