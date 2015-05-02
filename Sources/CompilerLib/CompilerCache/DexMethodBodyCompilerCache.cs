using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel.DotNet;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;
using Dot42.Mapping;
using Dot42.Utility;
using Mono.Cecil;
using ArrayType = Dot42.DexLib.ArrayType;
using ByReferenceType = Dot42.DexLib.ByReferenceType;
using FieldReference = Dot42.DexLib.FieldReference;
using MethodDefinition = Dot42.DexLib.MethodDefinition;
using MethodReference = Dot42.DexLib.MethodReference;
using TypeReference = Dot42.DexLib.TypeReference;


namespace Dot42.CompilerLib.CompilerCache
{
    public class DexMethodBodyCompilerCache
    {
        private readonly AssemblyTypeByScopeIdCache _assemblyCache;
        private readonly Dex _dex;

        private readonly DexLookup _dexLookup;
        private readonly MapFileLookup _map;

        private readonly Dictionary<Tuple<string, string, string>, Tuple<TypeEntry, MethodEntry>> _methodsByMetadataToken = new Dictionary<Tuple<string, string, string>, Tuple<TypeEntry, MethodEntry>>();
        private int statCacheHits;
        private int statCacheMisses;

        public bool IsEnabled { get { return _dex != null && _map != null; } }

        public AssemblyTypeByScopeIdCache AssemblyCache { get { return _assemblyCache; } }

        public DexMethodBodyCompilerCache()
        {
        }

        public DexMethodBodyCompilerCache(string cacheDirectory, Func<AssemblyDefinition, string> filenameFromAssembly, string dexFilename = "classes.dex")
        {
            dexFilename = Path.Combine(cacheDirectory, dexFilename);
            var mapfile = Path.ChangeExtension(dexFilename, ".d42map");

            if (!File.Exists(dexFilename) || !File.Exists(mapfile))
                return;

            try
            {
                var dex = Dex.Read(dexFilename);
                var map = new MapFileLookup(new MapFile(mapfile));

                _assemblyCache = new AssemblyTypeByScopeIdCache(filenameFromAssembly, map);

                foreach (var type in map.TypeEntries)
                {
                    if(type.ScopeId == null)
                        continue;
                    foreach (var method in type.Methods)
                    {
                        if (type.ScopeId == null)
                            continue;
                        _methodsByMetadataToken[Tuple.Create(type.Scope, type.ScopeId, method.ScopeId)] = 
                                                                            Tuple.Create(type, method);
                    }
                }

                _dexLookup = new DexLookup(dex);

                _dex = dex;
                _map = map;
            }
            catch (Exception ex)
            {
                DLog.Warning(DContext.CompilerCodeGenerator, "unable to initialize compiler cache", ex);
                throw;
            }
        }

        public void PrintStatistics()
        {
            if(IsEnabled)
                DLog.Warning(DContext.CompilerCodeGenerator, "Dex method body compiler cache: {0} hits and {1} misses.", statCacheHits, statCacheMisses);
        }

        public MethodBody GetFromCache(MethodDefinition targetMethod, Mono.Cecil.MethodDefinition sourceMethod, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            return null;
            var ret = GetFromCacheImpl(targetMethod, sourceMethod, compiler, targetPackage);
            
            if (ret != null) Interlocked.Increment(ref statCacheHits);
            else             Interlocked.Increment(ref statCacheMisses);
            
            return ret;
        }

        public MethodBody GetFromCacheImpl(MethodDefinition targetMethod, Mono.Cecil.MethodDefinition sourceMethod, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            if (!IsEnabled)
                return null;

            var assembly = sourceMethod.Module.Assembly;

            if (_assemblyCache.IsModified(assembly))
                return null;

            Tuple<TypeEntry, MethodEntry> entry;

            string scope = sourceMethod.DeclaringType.Scope.Name;
            string typeScopeId = sourceMethod.DeclaringType.MetadataToken.ToScopeId();
            string methodScopeId = sourceMethod.DeclaringType.Methods.IndexOf(sourceMethod).ToString(System.Globalization.CultureInfo.InvariantCulture);

            if (!_methodsByMetadataToken.TryGetValue(Tuple.Create(scope, typeScopeId, methodScopeId), out entry))
                return null;

            var cachedMethod = _dexLookup.FindMethod(entry.Item1.DexName, entry.Item2.DexName, entry.Item2.DexSignature);

            if (cachedMethod == null)
                return null;

            try
            {
                var body = DexMethodBodyCloner.Clone(targetMethod, cachedMethod);
                FixReferences(body, assembly, compiler, targetPackage);
                return body;
            }
            catch (Exception ex)
            {
                DLog.Warning(DContext.CompilerCodeGenerator, "Compiler cache: EXCEPTION while converting cached body: {0}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Operands refering to types, methods or fields need to be fixed, as they might have
        /// gotten another name in the target package. he same applies for catch references.
        /// </summary>
        private void FixReferences(MethodBody body, Mono.Cecil.AssemblyDefinition assembly, 
                                                AssemblyCompiler compiler,  DexTargetPackage targetPackage)
        {
            // fix operands
            foreach (var ins in body.Instructions)
            {
                var fieldRef = ins.Operand as FieldReference;
                var methodRef = ins.Operand as MethodReference;
                var classRef = ins.Operand as ClassReference;

                if (classRef != null)
                {
                    ins.Operand = ConvertClassReference(classRef, compiler, targetPackage);
                }
                else if (fieldRef != null)
                {
                    ins.Operand = ConvertFieldReference(fieldRef, compiler, targetPackage);
                }
                else if (methodRef != null)
                {
                    ins.Operand = ConvertMethodReference(methodRef, compiler, targetPackage);
                }
            }

            // fix catch clauses
            foreach (var @catch in body.Exceptions.SelectMany(e => e.Catches))
            {
                if (@catch.Type != null)
                    @catch.Type = ConvertTypeReference(@catch.Type, compiler, targetPackage);
            }

        }
     
        private TypeReference ConvertTypeReference(TypeReference sourceRef, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            if (sourceRef is PrimitiveType)
            {
                return sourceRef;
            }

            if (sourceRef is ByReferenceType)
            {
                var type = (ByReferenceType) sourceRef;
                var elementType = ConvertTypeReference(type.ElementType, compiler, targetPackage);
                return new ByReferenceType(elementType);
            }

            if (sourceRef is ArrayType)
            {
                var arrayType = (ArrayType) sourceRef;
                var elementType = ConvertTypeReference(arrayType.ElementType,compiler, targetPackage);
                return new ArrayType(elementType);
            }
            
            // must be ClassReference
            return ConvertClassReference((ClassReference)sourceRef, compiler, targetPackage);
        }

        private ClassReference ConvertClassReference(ClassReference sourceRef, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            TypeEntry type = _map.GetTypeByDexName(sourceRef.Fullname);
            if (type == null)
            {
                // has no dex name; assue that it is a native java type with
                // DexImport or JavaImport attribute who's name can not change.
                Trace.WriteLine("TypeEntry not found:   " + sourceRef);
                return new ClassReference(sourceRef.Fullname);

            }
            var cecilTypeDef = _assemblyCache.FindType(type);
            return cecilTypeDef.GetClassReference(targetPackage, compiler.Module);
        }

        private MethodReference ConvertMethodReference(MethodReference methodRef, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            MethodEntry methodEntry = _map.GetMethodByDexSignature(methodRef.Owner.Fullname, methodRef.Name, methodRef.Prototype.ToSignature());
            if (methodEntry == null)
            {
                // has no dex name; assue that it is a native java type with
                // DexImport or JavaImport attribute whose name can not change.
                Trace.WriteLine("MethodEntry not found: " + methodRef);
                return methodRef;
            }

            var cecilMethod = _assemblyCache.FindMethod(methodEntry);

            if (cecilMethod == null)
            {
                throw new CompilerCacheFixupException("Unable to resolve method reference: " + methodRef);
            }

            var xmethodReference = XBuilder.AsMethodReference(compiler.Module, cecilMethod);
            return xmethodReference.GetReference(targetPackage);
        }

        private object ConvertFieldReference(FieldReference fieldRef, AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            var classRef = ConvertClassReference(fieldRef.Owner, compiler, targetPackage);
            var typeRef = ConvertTypeReference(fieldRef.Type, compiler, targetPackage);
            // I don't believe we have to protect ourselfs from field name changes. 
            // Except for obfuscation, there is no reason to rename fields. They are 
            // independent of other classes.
            return new FieldReference(classRef, fieldRef.Name, typeRef);
        }

    }
}
