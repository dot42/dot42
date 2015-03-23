using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Dot42.DexLib.Extensions;
using Dot42.DexLib.IO.Collectors;
using Dot42.DexLib.IO.Markers;
using Dot42.DexLib.Instructions;
using Dot42.DexLib.Metadata;
using StringComparer = Dot42.DexLib.IO.Collectors.StringComparer;

namespace Dot42.DexLib.IO
{
    /// <summary>
    /// Write .dex files.
    /// </summary>
    internal class DexWriter
    {
        private readonly SHA1 hash = new SHA1CryptoServiceProvider();
        private readonly Dictionary<Annotation, UIntMarker> annotationSetMarkers = new Dictionary<Annotation, UIntMarker>();
        private readonly Dictionary<Parameter, UIntMarker> annotationSetRefListMarkers = new Dictionary<Parameter, UIntMarker>();
        private readonly Dictionary<MethodDefinition, uint> annotationSetRefLists = new Dictionary<MethodDefinition, uint>();
        private readonly Dictionary<AnnotationSet, uint> annotationSets = new Dictionary<AnnotationSet, uint>();
        private readonly List<ClassDefinitionMarkers> classDefinitionsMarkers = new List<ClassDefinitionMarkers>();
        private readonly Dictionary<MethodDefinition, uint> codes = new Dictionary<MethodDefinition, uint>();
        private readonly Dictionary<MethodDefinition, UIntMarker> debugMarkers = new Dictionary<MethodDefinition, UIntMarker>();

        private readonly Dex dex;

        private readonly HeaderMarkers headerMarkers = new HeaderMarkers();
        private readonly Map map = new Map();
        private readonly List<UIntMarker> prototypeTypeListMarkers = new List<UIntMarker>();
        private readonly List<UIntMarker> stringMarkers = new List<UIntMarker>();

        private readonly Dictionary<string, uint> typeLists = new Dictionary<string, uint>();
        private Dictionary<FieldReference, uint> fieldLookup;
        private List<ClassDefinition> flatClasses;
        private Dictionary<MethodReference, uint> methodLookup;
        private Dictionary<Prototype, int> prototypeLookup;
        private Dictionary<string, uint> stringLookup;
        private List<FieldReference> fieldReferences;
        private List<MethodReference> methodReferences;
        private List<TypeReference> typeReferences;
        private List<string> strings;
        private List<Prototype> prototypes;
        private Dictionary<TypeReference, uint> typeLookup;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal DexWriter(Dex dex)
        {
            this.dex = dex;
        }

        /// <summary>
        /// Write the entire structure to the given writer.
        /// </summary>
        internal void WriteTo(BinaryWriter writer)
        {
            new ModelSorter().Collect(dex);
            //new ModelShuffler().Collect(Dex);
            map.Clear();

            stringLookup = CollectStrings();
            typeLookup = CollectTypeReferences();
            methodLookup = CollectMethodReferences();
            fieldLookup = CollectFieldReferences();
            prototypeLookup = CollectPrototypes();
            flatClasses = ClassDefinition.Flattenize(dex.Classes);

            // Standard sort then topological sort
            var tsorter = new TopologicalSorter();
            var classDefinitionComparer = new ClassDefinitionComparer();
            flatClasses.Sort(classDefinitionComparer);
            flatClasses = new List<ClassDefinition>(tsorter.TopologicalSort(flatClasses, classDefinitionComparer));

            WriteHeader(writer);
            WriteStringId(writer);
            WriteTypeId(writer);
            WritePrototypeId(writer);
            WriteFieldId(writer);
            WriteMethodId(writer);
            WriteClassDef(writer);

            WriteAnnotationSetRefList(writer);
            WriteAnnotationSet(writer);
            WriteCodeItems(writer);
            WriteAnnotationsDirectoryItems(writer);
            WriteTypeLists(writer);
            WriteStringData(writer);
            WriteDebugInfo(writer);
            WriteAnnotations(writer);
            WriteEncodedArray(writer);
            WriteClassData(writer);

            WriteMapList(writer);

            ComputeSHA1Signature(writer);
            ComputeAdlerCheckSum(writer);
        }

        /// <summary>
        /// Write a header_item
        /// </summary>
        private void WriteHeader(BinaryWriter writer)
        {
            if (writer.BaseStream.Position != 0)
                throw new ArgumentException("Writer should be at position 0");
            map.Add(TypeCodes.Header, 1, 0);

            writer.Write(DexConsts.FileMagic);
            headerMarkers.CheckSumMarker = writer.MarkUInt();
            headerMarkers.SignatureMarker = writer.MarkSignature();
            headerMarkers.FileSizeMarker = writer.MarkUInt();
            writer.Write(DexConsts.HeaderSize);
            writer.Write(DexConsts.Endian);
            headerMarkers.LinkMarker = writer.MarkSizeOffset();
            headerMarkers.MapMarker = writer.MarkUInt();

            headerMarkers.StringsMarker = writer.MarkSizeOffset();
            headerMarkers.TypeReferencesMarker = writer.MarkSizeOffset();
            headerMarkers.PrototypesMarker = writer.MarkSizeOffset();
            headerMarkers.FieldReferencesMarker = writer.MarkSizeOffset();
            headerMarkers.MethodReferencesMarker = writer.MarkSizeOffset();
            headerMarkers.ClassDefinitionsMarker = writer.MarkSizeOffset();
            headerMarkers.DataMarker = writer.MarkSizeOffset();
        }

        /// <summary>
        /// Collect all strings used into the strings lookup table.
        /// </summary>
        private Dictionary<string, uint> CollectStrings()
        {
            var collector = new StringCollector();
            collector.Collect(dex);

            strings = new List<string>(collector.Items.Keys);
            strings.Sort(new StringComparer());

            uint index = 0;
            return strings.ToDictionary(s => s, s => index++);
        }

        /// <summary>
        /// Collect all field references used into the field reference lookup table.
        /// </summary>
        private Dictionary<FieldReference, uint> CollectFieldReferences()
        {
            var collector = new FieldReferenceCollector();
            collector.Collect(dex);

            fieldReferences = new List<FieldReference>(collector.Items.Keys);
            fieldReferences.Sort(new FieldReferenceComparer());
            
            uint index = 0;
            return fieldReferences.ToDictionary(s => s, s => index++);
        }

        /// <summary>
        /// Collect all method references used into the method reference lookup table.
        /// </summary>
        private Dictionary<MethodReference, uint> CollectMethodReferences()
        {
            var collector = new MethodReferenceCollector();
            collector.Collect(dex);

            methodReferences = new List<MethodReference>(collector.Items.Keys);
            methodReferences.Sort(new MethodReferenceComparer());

            uint index = 0;
            return methodReferences.ToDictionary(s => s, s => index++);
        }

        /// <summary>
        /// Collect all type references used into the type reference lookup table.
        /// </summary>
        private Dictionary<TypeReference, uint> CollectTypeReferences()
        {
            var collector = new TypeReferenceCollector();
            collector.Collect(dex);

            typeReferences = new List<TypeReference>(collector.Items.Keys);
            typeReferences.Sort(new TypeReferenceComparer());

            uint index = 0;
            return typeReferences.ToDictionary(s => s, s => index++);
        }

        private void WriteStringId(BinaryWriter writer)
        {
            if (strings.Count > 0)
            {
                headerMarkers.StringsMarker.Value = new SizeOffset((uint)strings.Count, (uint)writer.BaseStream.Position);
                map.Add(TypeCodes.StringId, (uint)strings.Count, (uint)writer.BaseStream.Position);
            }

            strings.ForEach(x => stringMarkers.Add(writer.MarkUInt()));
        }

        private void WriteStringData(BinaryWriter writer)
        {
            if (strings.Count > 0)
                map.Add(TypeCodes.StringData, (uint)strings.Count, (uint)writer.BaseStream.Position);

            for (int i = 0; i < strings.Count; i++)
            {
                stringMarkers[i].Value = (uint)writer.BaseStream.Position;
                writer.WriteMUTF8String(strings[i]);
            }
        }

        /// <summary>
        /// Collect all prototypes into the prototype lookup table.
        /// </summary>
        private Dictionary<Prototype, int> CollectPrototypes()
        {
            var collector = new PrototypeCollector();
            collector.Collect(dex);

            prototypes = collector.ToList(new PrototypeComparer());

            var lookup = new Dictionary<Prototype, int>();
            for (int i = 0; i < prototypes.Count; i++)
                lookup.Add(prototypes[i], i);

            return lookup;
        }

        /// <summary>
        /// Write proto_id_item structures for each prototype.
        /// </summary>
        private void WritePrototypeId(BinaryWriter writer)
        {
            var count = prototypes.Count;
            if (count > 0)
            {
                headerMarkers.PrototypesMarker.Value = new SizeOffset((uint)count, (uint)writer.BaseStream.Position);
                map.Add(TypeCodes.ProtoId, (uint)count, (uint)writer.BaseStream.Position);
            }

            foreach (var prototype in prototypes)
            {
                writer.Write(LookupStringId(TypeDescriptor.Encode(prototype)));
                writer.Write((uint)typeReferences.IndexOf(prototype.ReturnType));
                prototypeTypeListMarkers.Add(writer.MarkUInt());
            }
        }

        private void WriteTypeList(BinaryWriter writer, uint sectionOffset, ushort[] typelist, UIntMarker marker)
        {
            if (typelist.Length <= 0) 
                return;

            var key = GetTypeListKey(typelist);
            if (!typeLists.ContainsKey(key))
            {
                writer.EnsureAlignment(sectionOffset, 4);
                typeLists.Add(key, (uint) writer.BaseStream.Position);
                writer.Write((uint) typelist.Length);

                foreach (var item in typelist)
                {
                    writer.Write(item);
                }
            }
            if (marker != null)
            {
                marker.Value = typeLists[key];
            }
        }

        private void WriteTypeLists(BinaryWriter writer)
        {
            var offset = (uint)writer.BaseStream.Position;

            for (int i = 0; i < flatClasses.Count; i++)
                WriteTypeList(writer, offset, ComputeTypeList(flatClasses[i].Interfaces),
                              classDefinitionsMarkers[i].InterfacesMarker);

            for (int i = 0; i < prototypes.Count; i++)
                WriteTypeList(writer, offset, ComputeTypeList(prototypes[i].Parameters),
                              prototypeTypeListMarkers[i]);

            if (typeLists.Count > 0)
            {
                map.Add(TypeCodes.TypeList, (uint) typeLists.Count, offset);
            }
        }

        private ushort[] ComputeTypeList(IEnumerable<Parameter> parameters)
        {
            return parameters.Select(parameter => (ushort) typeLookup[parameter.Type]).ToArray();
        }

        private ushort[] ComputeTypeList(IEnumerable<ClassReference> classes)
        {
            return classes.Select(@class => (ushort) typeLookup[@class]).ToArray();
        }

        /// <summary>
        /// Write field_id_item structures for each field reference.
        /// </summary>
        private void WriteFieldId(BinaryWriter writer)
        {
            var count = fieldReferences.Count;
            if (count > 0)
            {
                headerMarkers.FieldReferencesMarker.Value = new SizeOffset((uint)count, (uint)writer.BaseStream.Position);
                map.Add(TypeCodes.FieldId, (uint)count, (uint)writer.BaseStream.Position);
            }

            foreach (var field in fieldReferences)
            {
                writer.Write((ushort)typeLookup[field.Owner]);
                writer.Write((ushort)typeLookup[field.Type]);
                writer.Write(LookupStringId(field.Name));
            }
        }

        /// <summary>
        /// Write method_id_item structures for each method reference.
        /// </summary>
        private void WriteMethodId(BinaryWriter writer)
        {
            var count = methodReferences.Count;
            if (count > 0)
            {
                headerMarkers.MethodReferencesMarker.Value = new SizeOffset((uint)count, (uint)writer.BaseStream.Position);
                map.Add(TypeCodes.MethodId, (uint)count, (uint)writer.BaseStream.Position);
            }

            foreach (var method in methodReferences)
            {
                writer.Write((ushort)typeLookup[method.Owner]);
                writer.Write((ushort)prototypeLookup[method.Prototype]);
                writer.Write(LookupStringId(method.Name));
            }
        }

        /// <summary>
        /// Write type_id_item structures for each type reference used in the dex file.
        /// </summary>
        private void WriteTypeId(BinaryWriter writer)
        {
            var count = typeReferences.Count;
            if (count > 0)
            {
                headerMarkers.TypeReferencesMarker.Value = new SizeOffset((uint)count, (uint)writer.BaseStream.Position);
                map.Add(TypeCodes.TypeId, (uint)count, (uint)writer.BaseStream.Position);
            }

            foreach (var tref in typeReferences)
            {
                writer.Write(LookupStringId(TypeDescriptor.Encode(tref)));
            }
        }

        private void WriteClassDef(BinaryWriter writer)
        {
            var count = flatClasses.Count;
            if (count > 0)
            {
                headerMarkers.ClassDefinitionsMarker.Value = new SizeOffset((uint)count, (uint)writer.BaseStream.Position);
                map.Add(TypeCodes.ClassDef, (uint)count, (uint)writer.BaseStream.Position);
            }

            var sectionOffset = (uint)writer.BaseStream.Position;
            foreach (var iterator in flatClasses)
            {
                var @class = iterator;
                writer.EnsureAlignment(sectionOffset, 4);
                var markers = new ClassDefinitionMarkers();
                classDefinitionsMarkers.Add(markers);

                writer.Write(typeLookup[@class]);
                writer.Write((uint) (@class.AccessFlags & ~(AccessFlags.Private | AccessFlags.Protected)));
                writer.Write(@class.SuperClass == null ? DexConsts.NoIndex : (uint) typeLookup[@class.SuperClass]);
                markers.InterfacesMarker = writer.MarkUInt();
                writer.Write(LookupStringId(@class.SourceFile));
                markers.AnnotationsMarker = writer.MarkUInt();
                markers.ClassDataMarker = writer.MarkUInt();
                markers.StaticValuesMarker = writer.MarkUInt();
            }
        }

        private void WriteAnnotationSetRefList(BinaryWriter writer)
        {
            var sectionOffset = (uint)writer.BaseStream.Position;

            foreach (var iterator in flatClasses)
            {
                var @class = iterator;
                writer.EnsureAlignment(sectionOffset, 4);
                foreach (var mdef in @class.Methods.Where(mdef => (mdef.Prototype.ContainsAnnotation())))
                {
                    annotationSetRefLists.Add(mdef, (uint) writer.BaseStream.Position);
                    writer.Write(mdef.Prototype.Parameters.Count);
                    foreach (var parameter in mdef.Prototype.Parameters)
                    {
                        annotationSetRefListMarkers.Add(parameter, writer.MarkUInt());
                    }
                }
            }

            if (annotationSetRefLists.Count > 0)
            {
                map.Add(TypeCodes.AnnotationSetRefList, (uint) annotationSetRefLists.Count, sectionOffset);
            }
        }

        private void WriteAnnotationSet(BinaryWriter writer)
        {
            var sectionOffset = (uint)writer.BaseStream.Position;

            foreach (var @class in flatClasses)
            {
                WriteAnnotationSet(writer, sectionOffset, @class, false);

                @class.Fields.ForEach(x => WriteAnnotationSet(writer, sectionOffset, x, false));
                @class.Methods.ForEach(x => WriteAnnotationSet(writer, sectionOffset, x, false));
            }

            foreach (var @class in flatClasses)
            {
                foreach (var method in @class.Methods.Where(x => x.Prototype.ContainsAnnotation()))
                {
                    foreach (var parameter in method.Prototype.Parameters)
                    {
                        annotationSetRefListMarkers[parameter].Value = WriteAnnotationSet(writer, sectionOffset, parameter, true);
                    }
                }
            }

            if (annotationSets.Count > 0)
            {
                map.Add(TypeCodes.AnnotationSet, (uint) annotationSets.Count, sectionOffset);
            }
        }

        private uint WriteAnnotationSet(BinaryWriter writer, uint sectionOffset, IAnnotationProvider provider, bool writezero)
        {
            var key = new AnnotationSet(provider);
            uint offset;

            if (!annotationSets.ContainsKey(key))
            {
                writer.EnsureAlignment(sectionOffset, 4);
                offset = (uint) writer.BaseStream.Position;

                if (provider.Annotations.Count > 0 || writezero)
                {
                    writer.Write(provider.Annotations.Count);
                }

                foreach (var annotation in provider.Annotations)
                {
                    if (annotationSetMarkers.ContainsKey(annotation))
                    {
                        annotationSetMarkers[annotation].CloneMarker();
                    }
                    else
                    {
                        annotationSetMarkers.Add(annotation, writer.MarkUInt());
                    }
                }

                if (provider.Annotations.Count > 0 || writezero)
                {
                    annotationSets.Add(key, offset);
                }
                else
                {
                    offset = DexConsts.NoIndex;
                }
            }
            else
            {
                offset = annotationSets[key];
            }

            return offset;
        }

        private void WriteAnnotation(BinaryWriter writer, Annotation annotation)
        {
            writer.Write((byte)annotation.Visibility);
            WriteEncodedAnnotation(writer, annotation);
        }

        private void WriteEncodedAnnotation(BinaryWriter writer, Annotation annotation)
        {
            writer.WriteULEB128((uint)typeLookup[annotation.Type]);
            writer.WriteULEB128((uint)annotation.Arguments.Count);

            foreach (var tuple in annotation.Arguments.Select(x => Tuple.Create(LookupStringId(x.Name), x.Value)).OrderBy(x => x.Item1))
            {
                writer.WriteULEB128(tuple.Item1);
                WriteValue(writer, tuple.Item2);
            }
        }

        private void WriteValue(BinaryWriter writer, object value)
        {
            var valueArgument = 0;
            var format = ValueFormat.GetFormat(value);
            switch (format)
            {
                case ValueFormats.Char:
                    valueArgument = VerifyMaxEncodedValueSize(writer.GetByteCountForUnsignedPackedNumber(Convert.ToInt64(value)) - 1, 1, format);
                    break;
                case ValueFormats.Byte:
                    valueArgument = VerifyMaxEncodedValueSize(writer.GetByteCountForSignedPackedNumber(Convert.ToInt64(value)) - 1, 0, format);
                    break;
                case ValueFormats.Short:
                    valueArgument = VerifyMaxEncodedValueSize(writer.GetByteCountForSignedPackedNumber(Convert.ToInt64(value)) - 1, 1, format);
                    break;
                case ValueFormats.Int:
                    valueArgument = VerifyMaxEncodedValueSize(writer.GetByteCountForSignedPackedNumber(Convert.ToInt64(value)) - 1, 3, format);
                    break;
                case ValueFormats.Long:
                    valueArgument = VerifyMaxEncodedValueSize(writer.GetByteCountForSignedPackedNumber(Convert.ToInt64(value)) - 1, 7, format);
                    break;
                case ValueFormats.Float:
                    valueArgument =
                        writer.GetByteCountForSignedPackedNumber(
                            BitConverter.ToInt32(BitConverter.GetBytes(Convert.ToSingle(value)), 0)) - 1;
                    break;
                case ValueFormats.Double:
                    valueArgument =
                        writer.GetByteCountForSignedPackedNumber(BitConverter.DoubleToInt64Bits(Convert.ToDouble(value))) -
                        1;
                    break;
                case ValueFormats.String:
                    valueArgument = writer.GetByteCountForUnsignedPackedNumber(stringLookup[(String)value]) - 1;
                    break;
                case ValueFormats.Type:
                    valueArgument = writer.GetByteCountForUnsignedPackedNumber(typeLookup[(TypeReference)value]) - 1;
                    break;
                case ValueFormats.Field:
                case ValueFormats.Enum:
                    valueArgument = writer.GetByteCountForUnsignedPackedNumber(fieldLookup[(FieldReference)value]) - 1;
                    break;
                case ValueFormats.Method:
                    valueArgument = writer.GetByteCountForUnsignedPackedNumber(methodLookup[(MethodReference)value]) - 1;
                    break;
                case ValueFormats.Boolean:
                    valueArgument = Convert.ToInt32(Convert.ToBoolean(value));
                    break;
            }

            var data = (byte)(valueArgument << 5);
            data |= (byte)format;
            writer.Write(data);

            switch (format)
            {
                case ValueFormats.Char:
                    writer.WriteUnsignedPackedNumber(Convert.ToInt64(value));
                    break;
                case ValueFormats.Short:
                case ValueFormats.Byte:
                case ValueFormats.Int:
                case ValueFormats.Long:
                    writer.WritePackedSignedNumber(Convert.ToInt64(value));
                    break;
                case ValueFormats.Float:
                    writer.WritePackedSignedNumber(BitConverter.ToInt32(BitConverter.GetBytes(Convert.ToSingle(value)), 0));
                    break;
                case ValueFormats.Double:
                    writer.WritePackedSignedNumber(BitConverter.DoubleToInt64Bits(Convert.ToDouble(value)));
                    break;
                case ValueFormats.String:
                    writer.WriteUnsignedPackedNumber(stringLookup[(String)value]);
                    break;
                case ValueFormats.Type:
                    writer.WriteUnsignedPackedNumber(typeLookup[(TypeReference)value]);
                    break;
                case ValueFormats.Field:
                case ValueFormats.Enum:
                    writer.WriteUnsignedPackedNumber(fieldLookup[(FieldReference)value]);
                    break;
                case ValueFormats.Method:
                    writer.WriteUnsignedPackedNumber(methodLookup[(MethodReference)value]);
                    break;
                case ValueFormats.Array:
                    WriteValues(writer, (object[])value);
                    break;
                case ValueFormats.Annotation:
                    WriteEncodedAnnotation(writer, value as Annotation);
                    break;
                case ValueFormats.Null:
                case ValueFormats.Boolean:
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        private static int VerifyMaxEncodedValueSize(int actual, int max, ValueFormats format)
        {
            if (actual > max)
                throw new ArgumentException(string.Format("Encoded value size ({0}) for type 0x{1:x2} is greater then {2}", actual, (int)format, max));
            return actual;
        }

        private void WriteValues(BinaryWriter writer, ICollection<object> values)
        {
            writer.WriteULEB128((uint)values.Count);
            foreach (var value in values)
            {
                WriteValue(writer, value);
            }
        }

        private void WriteAnnotations(BinaryWriter writer)
        {
            var offset = (uint)writer.BaseStream.Position;

            var annotations = new Dictionary<Annotation, uint>();

            foreach (var annotationset in annotationSets.Keys.ToList())
            {
                foreach (var annotation in annotationset)
                {
                    if (annotations.ContainsKey(annotation)) 
                        continue;

                    var annoffset = (uint)writer.BaseStream.Position;
                    WriteAnnotation(writer, annotation);
                    annotations.Add(annotation, annoffset);

                    annotationSetMarkers[annotation].Value = annoffset;
                }
            }

            if (annotations.Count > 0)
            {
                map.Add(TypeCodes.Annotation, (uint) annotations.Count, offset);
            }
        }

        public Dictionary<T, int> Collect<T>(List<T> container, IComparer<T> comparer)
        {
            var result = new Dictionary<T, int>();
            container.Sort(comparer);

            for (int i = 0; i < container.Count; i++)
                result.Add(container[i], i);

            return result;
        }

        /// <summary>
        /// Write the annotation_directory_item structures for all classes.
        /// </summary>
        private void WriteAnnotationsDirectoryItems(BinaryWriter writer)
        {
            var sectionOffset = (uint)writer.BaseStream.Position;
            uint count = 0;

            var classAnnotationSets = new Dictionary<AnnotationSet, uint>();
            for (var i = 0; i < flatClasses.Count; i++)
            {
                writer.EnsureAlignment(sectionOffset, 4);
                WriteAnnotationDirectoryItem(writer, i, ref count, classAnnotationSets, flatClasses[i]);
            }

            if (count > 0)
            {
                map.Add(TypeCodes.AnnotationDirectory, count, sectionOffset);
            }
        }

        /// <summary>
        /// Write a single annotation_directory_item structure
        /// </summary>
        private void WriteAnnotationDirectoryItem(BinaryWriter writer, int index, ref uint count, Dictionary<AnnotationSet, uint> classAnnotationSets, ClassDefinition @class)
        {
            var annotatedFields = @class.Fields.Where(x => x.Annotations.Any()).ToList();
            var annotatedMethods = @class.Methods.Where(x => x.Annotations.Any()).ToList();
            var annotatedParametersList = @class.Methods.Where(x => x.Prototype.ContainsAnnotation()).ToList();

            var total = @class.Annotations.Count + annotatedFields.Count + annotatedMethods.Count + annotatedParametersList.Count;
            if (total <= 0) 
                return;

            // all datas except class annotations are specific.
            if (total == @class.Annotations.Count)
            {
                var set = new AnnotationSet(@class);
                if (classAnnotationSets.ContainsKey(set))
                {
                    classDefinitionsMarkers[index].AnnotationsMarker.Value = classAnnotationSets[set];
                    return;
                }
                classAnnotationSets.Add(set, (uint) writer.BaseStream.Position);
            }

            classDefinitionsMarkers[index].AnnotationsMarker.Value = (uint) writer.BaseStream.Position;
            count++;

            if (@class.Annotations.Count > 0)
            {
                writer.Write(annotationSets[new AnnotationSet(@class)]);
            }
            else
            {
                writer.Write((uint)0);
            }

            writer.Write(annotatedFields.Count);
            writer.Write(annotatedMethods.Count);
            writer.Write(annotatedParametersList.Count);

            var fields = new List<FieldReference>(annotatedFields);
            fields.Sort(new FieldReferenceComparer());
            foreach (FieldDefinition field in fields)
            {
                writer.Write(fieldLookup[field]);
                writer.Write(annotationSets[new AnnotationSet(field)]);
            }

            var methods = new List<MethodReference>(annotatedMethods);
            methods.Sort(new MethodReferenceComparer());
            foreach (MethodDefinition method in methods)
            {
                writer.Write(methodLookup[method]);
                writer.Write(annotationSets[new AnnotationSet(method)]);
            }

            methods = new List<MethodReference>(annotatedParametersList);
            methods.Sort(new MethodReferenceComparer());
            foreach (MethodDefinition method in methods)
            {
                writer.Write(methodLookup[method]);
                writer.Write(annotationSetRefLists[method]);
            }
        }

        /// <summary>
        /// Writes all code_item structures
        /// </summary>
        private void WriteCodeItems(BinaryWriter writer)
        {
            var sectionOffset = (uint)writer.BaseStream.Position;
            var count = flatClasses.SelectMany(x => x.Methods).Count(x => WriteCodeItem(writer, x, sectionOffset));
            if (count > 0)
            {
                map.Add(TypeCodes.Code, (uint) count, sectionOffset);
            }
        }

        /// <summary>
        /// Write a single code_item structure
        /// </summary>
        /// <returns>True if a code_item was written, false otherwise</returns>
        private bool WriteCodeItem(BinaryWriter writer, MethodDefinition method, uint sectionOffset)
        {
            if (codes.ContainsKey(method))
                throw new AmbiguousMatchException("method renamer failed to find a uniquely name for " + method);    
            
            codes.Add(method, 0);

            var body = method.Body;
            if (body == null)
                return false;

            body.UpdateInstructionOffsets();
            writer.EnsureAlignment(sectionOffset, 4);
            codes[method] = (uint) writer.BaseStream.Position;

            writer.Write((ushort) body.Registers.Count);
            writer.Write(body.IncomingArguments);
            writer.Write(body.OutgoingArguments);
            writer.Write((ushort) body.Exceptions.Count);
            debugMarkers.Add(method, writer.MarkUInt());

            var iwriter = new InstructionWriter(this, method);
            iwriter.WriteTo(writer);

            if ((body.Exceptions.Count != 0) &&
                (iwriter.Codes.Length%2 != 0))
                writer.Write((ushort) 0);
            // padding (tries 4-byte alignment)

            var catchHandlers = new Dictionary<CatchSet, List<ExceptionHandler>>();
            var exceptionsMarkers = new Dictionary<ExceptionHandler, UShortMarker>();
            foreach (var handler in body.Exceptions)
            {
                writer.Write(handler.TryStart.Offset);
                writer.Write((ushort) (iwriter.LookupLast[handler.TryEnd] - handler.TryStart.Offset + 1));
                exceptionsMarkers.Add(handler, writer.MarkUShort());

                var set = new CatchSet(handler);
                if (!catchHandlers.ContainsKey(set))
                    catchHandlers.Add(set, new List<ExceptionHandler>());
                catchHandlers[set].Add(handler);
            }

            var catchSets = catchHandlers.Keys.ToList();
            catchSets.Sort(new CatchSetComparer());

            if (catchSets.Count > 0)
            {
                var baseOffset = writer.BaseStream.Position;
                writer.WriteULEB128((uint) catchSets.Count);
                foreach (var set in catchSets)
                {
                    var itemoffset = writer.BaseStream.Position - baseOffset;

                    if (set.CatchAll != null)
                        writer.WriteSLEB128(-set.Count);
                    else
                        writer.WriteSLEB128(set.Count);

                    foreach (var handler in catchHandlers[set])
                    {
                        exceptionsMarkers[handler].Value = (ushort) itemoffset;
                    }

                    foreach (var @catch in set)
                    {
                        writer.WriteULEB128((uint) typeLookup[@catch.Type]);
                        writer.WriteULEB128((uint) @catch.Instruction.Offset);
                    }

                    if (set.CatchAll != null)
                    {
                        writer.WriteULEB128((uint) set.CatchAll.Offset);
                    }
                }
            }
            return true;
        }

        private static void CheckOperand(DebugInstruction ins, int operandCount, params Type[] types)
        {
            if (ins.Operands.Count != operandCount)
                throw new DebugInstructionException(ins, string.Format("Expecting {0} operands", operandCount));

            for (var i = 0; i < ins.Operands.Count; i++)
            {
                try
                {
                    if (types[i].IsInstanceOfType(ins.Operands[i]))
                        continue;
                    Convert.ChangeType(ins.Operands[i], types[i]);
                }
                catch (Exception)
                {
                    throw new DebugInstructionException(ins, string.Format("Expecting '{0}' Type (or compatible) for Operands[{1}]", types[i], i));
                }
            }
        }

        private void WriteDebugInfo(BinaryWriter writer)
        {
            var offset = (uint)writer.BaseStream.Position;
            uint count = 0;

            foreach (var @class in flatClasses)
            {
                foreach (var method in @class.Methods)
                {
                    var body = method.Body;
                    if (body != null && body.DebugInfo != null)
                    {
                        debugMarkers[method].Value = (uint)writer.BaseStream.Position;
                        count++;

                        // byte aligned
                        var debugInfo = body.DebugInfo;
                        writer.WriteULEB128(debugInfo.LineStart);

                        if (debugInfo.Parameters.Count != method.Prototype.Parameters.Count)
                            throw new MalformedException(
                                "Unexpected parameter count in DebugInfo, must match with prototype");

                        writer.WriteULEB128((uint)debugInfo.Parameters.Count);
                        foreach (var parameter in debugInfo.Parameters)
                        {
                            writer.WriteULEB128p1(LookupStringId(parameter));
                        }

                        foreach (var ins in debugInfo.DebugInstructions)
                        {
                            string name;

                            writer.Write((byte)ins.OpCode);
                            switch (ins.OpCode)
                            {
                                case DebugOpCodes.AdvancePc:
                                    // uleb128 addr_diff
                                    CheckOperand(ins, 1, typeof(uint));
                                    writer.WriteULEB128(Convert.ToUInt32(ins.Operands[0]));
                                    break;
                                case DebugOpCodes.AdvanceLine:
                                    // sleb128 line_diff
                                    CheckOperand(ins, 1, typeof(int));
                                    writer.WriteSLEB128(Convert.ToInt32(ins.Operands[0]));
                                    break;
                                case DebugOpCodes.EndLocal:
                                case DebugOpCodes.RestartLocal:
                                    // uleb128 register_num
                                    CheckOperand(ins, 1, typeof(Register));
                                    writer.WriteULEB128((uint)((Register)ins.Operands[0]).Index);
                                    break;
                                case DebugOpCodes.SetFile:
                                    // uleb128p1 name_idx
                                    CheckOperand(ins, 1, typeof(string));
                                    name = (string)ins.Operands[0];
                                    writer.WriteULEB128p1(LookupStringId(name));
                                    break;
                                case DebugOpCodes.StartLocalExtended:
                                case DebugOpCodes.StartLocal:
                                    // StartLocalExtended : uleb128 register_num, uleb128p1 name_idx, uleb128p1 type_idx, uleb128p1 sig_idx
                                    // StartLocal : uleb128 register_num, uleb128p1 name_idx, uleb128p1 type_idx
                                    Boolean isExtended = ins.OpCode == DebugOpCodes.StartLocalExtended;

                                    if (isExtended)
                                        CheckOperand(ins, 4, typeof(Register), typeof(String), typeof(TypeReference),
                                                     typeof(String));
                                    else
                                        CheckOperand(ins, 3, typeof(Register), typeof(String), typeof(TypeReference));

                                    writer.WriteULEB128((uint)((Register)ins.Operands[0]).Index);

                                    name = (string)ins.Operands[1];
                                    writer.WriteULEB128p1(LookupStringId(name));

                                    var type = (TypeReference)ins.Operands[2];
                                    if (type == null)
                                        writer.WriteULEB128p1(DexConsts.NoIndex);
                                    else
                                        writer.WriteULEB128p1(typeLookup[type]);

                                    if (isExtended)
                                    {
                                        var signature = (string)ins.Operands[3];
                                        writer.WriteULEB128p1(LookupStringId(signature));
                                    }

                                    break;
                                case DebugOpCodes.EndSequence:
                                case DebugOpCodes.Special:
                                // between 0x0a and 0xff (inclusive)
                                case DebugOpCodes.SetPrologueEnd:
                                case DebugOpCodes.SetEpilogueBegin:
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            if (count > 0)
            {
                map.Add(TypeCodes.DebugInfo, count, offset);
            }
        }

        private void WriteEncodedArray(BinaryWriter writer)
        {
            var buffers = new Dictionary<string, uint>();
            var offset = (uint)writer.BaseStream.Position;
            uint count = 0;

            var memoryStream = new MemoryStream();
            using (var memoryWriter = new BinaryWriter(memoryStream))
            {
                for (var c = 0; c < flatClasses.Count; c++)
                {
                    var @class = flatClasses[c];
                    var values = new List<object>();
                    var lastNonNullIndex = -1;

                    for (var i = 0; i < @class.Fields.Count; i++)
                    {
                        var field = @class.Fields[i];
                        switch (ValueFormat.GetFormat(field.Value))
                        {
                            case ValueFormats.Annotation:
                            case ValueFormats.Array:
                            case ValueFormats.Method:
                            case ValueFormats.Type:
                            case ValueFormats.String:
                            case ValueFormats.Enum:
                            case ValueFormats.Field:
                                // always set
                                lastNonNullIndex = i;
                                break;
                            case ValueFormats.Null:
                                // never set
                                break;
                            case ValueFormats.Double:
                            case ValueFormats.Float:
                                if (Convert.ToDouble(field.Value) != 0)
                                    lastNonNullIndex = i;
                                break;
                            case ValueFormats.Boolean:
                            case ValueFormats.Byte:
                            case ValueFormats.Char:
                            case ValueFormats.Int:
                            case ValueFormats.Long:
                            case ValueFormats.Short:
                                if (Convert.ToInt64(field.Value) != 0)
                                    lastNonNullIndex = i;
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        values.Add(field.Value);
                    }

                    if (lastNonNullIndex != -1)
                    {
                        memoryStream.Position = 0;
                        memoryStream.SetLength(0);

                        WriteValues(memoryWriter, values.Take(lastNonNullIndex + 1).ToArray());
                        var buffer = memoryStream.ToArray();
                        var key = GetByteArrayKey(buffer);

                        if (!buffers.ContainsKey(key))
                        {
                            count++;
                            buffers.Add(key, (uint)writer.BaseStream.Position);
                            writer.Write(buffer);
                        }

                        classDefinitionsMarkers[c].StaticValuesMarker.Value = buffers[key];
                    }
                }
            }

            if (count > 0)
            {
                map.Add(TypeCodes.EncodedArray, count, offset);
            }
        }

        /// <summary>
        /// Write class_data_item structures for each class.
        /// </summary>
        private void WriteClassData(BinaryWriter writer)
        {
            var offset = (uint)writer.BaseStream.Position;
            var count = 0;
            var index = 0;
            foreach (var classDef in flatClasses)
            {
                var staticFields = (classDef.Fields.Where(x => x.IsStatic).OrderBy(x => fieldLookup[x])).ToList();
                var instanceFields = (classDef.Fields.Except(staticFields).OrderBy(x => fieldLookup[x])).ToList();
                var directMethods = (classDef.Methods.Where(x => x.IsDirect).OrderBy(x => methodLookup[x])).ToList();
                var virtualMethods = (classDef.Methods.Except(directMethods).OrderBy(x => methodLookup[x])).ToList();

                if ((staticFields.Count + instanceFields.Count + virtualMethods.Count + directMethods.Count) > 0)
                {
                    classDefinitionsMarkers[index].ClassDataMarker.Value = (uint)writer.BaseStream.Position;
                    count++;

                    writer.WriteULEB128((uint)staticFields.Count);
                    writer.WriteULEB128((uint)instanceFields.Count);
                    writer.WriteULEB128((uint)directMethods.Count);
                    writer.WriteULEB128((uint)virtualMethods.Count);

                    WriteEncodedFields(writer, staticFields);
                    WriteEncodedFields(writer, instanceFields);
                    WriteEncodedMethods(writer, directMethods);
                    WriteEncodedMethods(writer, virtualMethods);
                }

                index++;
            }

            // File "global" alignment (EnsureAlignment is used for local alignment)
            while ((writer.BaseStream.Position % 4) != 0)
            {
                writer.Write((byte) 0);
            }

            if (count > 0)
            {
                map.Add(TypeCodes.ClassData, (uint) count, offset);
            }
        }

        /// <summary>
        /// Write encoded_field structures for each given method.
        /// </summary>
        private void WriteEncodedFields(BinaryWriter writer, IEnumerable<FieldDefinition> fields)
        {
            uint lastIndex = 0;
            foreach (var t in fields)
            {
                var fieldIndex = fieldLookup[t];
                writer.WriteULEB128((uint)(fieldIndex - lastIndex));
                writer.WriteULEB128((uint)t.AccessFlags);

                lastIndex = fieldIndex;
            }
        }

        /// <summary>
        /// Write encoded_method structures for each given method.
        /// </summary>
        private void WriteEncodedMethods(BinaryWriter writer, IEnumerable<MethodDefinition> methods)
        {
            uint lastIndex = 0;
            foreach (var method in methods)
            {
                var methodIndex = methodLookup[method];
                writer.WriteULEB128((uint)(methodIndex - lastIndex));
                writer.WriteULEB128((uint)method.AccessFlags);
                writer.WriteULEB128(codes[method]);

                lastIndex = methodIndex;
            }
        }

        /// <summary>
        /// Write the map_list
        /// </summary>
        private void WriteMapList(BinaryWriter writer)
        {
            var sectionOffset = (uint) writer.BaseStream.Position;
            headerMarkers.MapMarker.Value = sectionOffset;
            map.Add(TypeCodes.MapList, 1, sectionOffset);

            writer.EnsureAlignment(sectionOffset, 4);
            writer.Write(map.Count);
            foreach (var item in map.Values)
            {
                writer.Write((ushort) item.Type);
                writer.Write((ushort) 0); // unused
                writer.Write(item.Size);
                writer.Write(item.Offset);
            }

            var filesize = (uint) writer.BaseStream.Position;
            headerMarkers.FileSizeMarker.Value = filesize;

            var lastEntry = TypeCodes.Header;
            foreach (var type in map.Keys)
            {
                if (lastEntry == TypeCodes.ClassDef)
                {
                    headerMarkers.DataMarker.Value = new SizeOffset(filesize - map[type].Offset, map[type].Offset);
                }
                lastEntry = type;
            }
        }

        /// <summary>
        /// Compute the SHA signature of the image and store it in the header.
        /// </summary>
        private void ComputeSHA1Signature(BinaryWriter writer)
        {
            writer.Seek((int)headerMarkers.SignatureMarker.FirstPosition + DexConsts.SignatureSize, SeekOrigin.Begin);
            var crypto = new SHA1CryptoServiceProvider();
            var signature = crypto.ComputeHash(writer.BaseStream);
            headerMarkers.SignatureMarker.Value = signature;
        }

        /// <summary>
        /// Compute the checksum of the image and store it in the header.
        /// </summary>
        private void ComputeAdlerCheckSum(BinaryWriter writer)
        {
            writer.Seek((int)headerMarkers.SignatureMarker.FirstPosition, SeekOrigin.Begin);
            ushort s1 = 1;
            ushort s2 = 0;
            int value;
            while ((value = writer.BaseStream.ReadByte()) != -1)
            {
                s1 = (ushort)((s1 + value) % 65521);
                s2 = (ushort)((s1 + s2) % 65521);
            }
            var checksum = (uint)(s2 << 16 | s1);
            headerMarkers.CheckSumMarker.Value = checksum;
        }

        /// <summary>
        /// Lookup the id of the given field ref.
        /// </summary>
        internal uint LookupFieldId(FieldReference field)
        {
            return fieldLookup[field];
        }

        /// <summary>
        /// Lookup the id of the given method ref.
        /// </summary>
        internal uint LookupMethodId(MethodReference method)
        {
            return methodLookup[method];
        }

        /// <summary>
        /// Lookup the id of the given type ref.
        /// </summary>
        internal uint LookupTypeId(TypeReference type)
        {
            return typeLookup[type];
        }

        /// <summary>
        /// Lookup the id of the given string.
        /// </summary>
        internal uint LookupStringId(string value)
        {
            //if (string.IsNullOrEmpty(value))
            if (value == null)
                return DexConsts.NoIndex;
            return stringLookup[value];
        }

        /// <summary>
        /// Create a key to uniquely identify the given byte array.
        /// </summary>
        private string GetByteArrayKey(byte[] bytes)
        {
            return Convert.ToBase64String(hash.ComputeHash(bytes));
        }

        /// <summary>
        /// Create a key to uniqueliy identify the given type list.
        /// </summary>
        private static string GetTypeListKey(IEnumerable<ushort> typelist)
        {
            return string.Join(",", typelist.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray());
        }
    }
}