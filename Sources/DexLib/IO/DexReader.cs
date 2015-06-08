using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Dot42.DexLib.Extensions;
using Dot42.DexLib.Instructions;
using Dot42.DexLib.Metadata;

namespace Dot42.DexLib.IO
{
    internal sealed class DexReader
    {
        private readonly DexHeader header = new DexHeader();
        private readonly Map map = new Map();
        private readonly List<string> strings = new List<string>();
        private readonly List<FieldReference> fieldReferences = new List<FieldReference>();
        private readonly List<MethodReference> methodReferences = new List<MethodReference>();
        private readonly List<TypeReference> typeReferences = new List<TypeReference>();
        private readonly List<Prototype> prototypes = new List<Prototype>();
        private readonly List<ClassDefinition> classes = new List<ClassDefinition>();

        private void ReadHeader(BinaryReader reader)
        {
            header.Magic = reader.ReadBytes(DexConsts.FileMagic.Length);

            if (!header.Magic.Match(DexConsts.FileMagic, 0))
                throw new MalformedException("Unexpected Magic number");


            header.CheckSum = reader.ReadUInt32();
            header.Signature = reader.ReadBytes(DexConsts.SignatureSize);

            header.FileSize = reader.ReadUInt32();
            header.HeaderSize = reader.ReadUInt32();
            header.EndianTag = reader.ReadUInt32();

            if (header.EndianTag != DexConsts.Endian)
                throw new MalformedException("Only Endian-encoded files are supported");

            header.LinkSize = reader.ReadUInt32();
            header.LinkOffset = reader.ReadUInt32();

            header.MapOffset = reader.ReadUInt32();

            header.StringsSize = reader.ReadUInt32();
            header.StringsOffset = reader.ReadUInt32();

            header.TypeReferencesSize = reader.ReadUInt32();
            header.TypeReferencesOffset = reader.ReadUInt32();

            header.PrototypesSize = reader.ReadUInt32();
            header.PrototypesOffset = reader.ReadUInt32();

            header.FieldReferencesSize = reader.ReadUInt32();
            header.FieldReferencesOffset = reader.ReadUInt32();

            header.MethodReferencesSize = reader.ReadUInt32();
            header.MethodReferencesOffset = reader.ReadUInt32();

            header.ClassDefinitionsSize = reader.ReadUInt32();
            header.ClassDefinitionsOffset = reader.ReadUInt32();

            header.DataSize = reader.ReadUInt32();
            header.DataOffset = reader.ReadUInt32();
        }

        private void ReadMapList(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(header.MapOffset, () =>
            {
                var mapsize = reader.ReadUInt32();
                for (var i = 0; i < mapsize; i++)
                {
                    var type = (TypeCodes) reader.ReadUInt16();
                    reader.ReadUInt16(); // unused
                    var size = reader.ReadUInt32();
                    var offset = reader.ReadUInt32();

                    map.Add(type, size, offset);
                }
            });
        }

        private void ReadStrings(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(header.StringsOffset, () =>
            {
                uint StringsDataOffset = reader.ReadUInt32();
                reader.BaseStream.Seek(StringsDataOffset, SeekOrigin.Begin);
                for (int i = 0; i < header.StringsSize; i++)
                {
                    strings.Add(reader.ReadMUTF8String());
                }
            });
        }

        private void PrefetchTypeReferences(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(header.TypeReferencesOffset, () =>
            {
                reader.BaseStream.Seek(header.TypeReferencesOffset, SeekOrigin.Begin);

                for (int i = 0; i < header.TypeReferencesSize; i++)
                {
                    int descriptorIndex = reader.ReadInt32();
                    string descriptor = strings[descriptorIndex];
                    typeReferences.Add(TypeDescriptor.Allocate(descriptor));
                }
            });
        }

        private void PrefetchFieldDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint fieldcount)
        {
            int fieldIndex = 0;
            for (int i = 0; i < fieldcount; i++)
            {
                if (i == 0)
                {
                    fieldIndex = (int) reader.ReadULEB128();
                }
                else
                {
                    fieldIndex += (int) reader.ReadULEB128();
                }
                reader.ReadULEB128();
                var fdef = new FieldDefinition(fieldReferences[fieldIndex]);
                fieldReferences[fieldIndex] = fdef;
            }
        }

        private void PrefetchMethodDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint methodcount)
        {
            int methodIndex = 0;
            for (int i = 0; i < methodcount; i++)
            {
                if (i == 0)
                {
                    methodIndex = (int) reader.ReadULEB128();
                }
                else
                {
                    methodIndex += (int) reader.ReadULEB128();
                }
                reader.ReadULEB128();
                reader.ReadULEB128();
                var mdef = new MethodDefinition(methodReferences[methodIndex]);
                methodReferences[methodIndex] = mdef;
            }
        }

        private void PrefetchClassDefinition(BinaryReader reader, ClassDefinition classDefinition, uint classDataOffset)
        {
            reader.PreserveCurrentPosition(classDataOffset, () =>
            {
                uint staticFieldSize = reader.ReadULEB128();
                uint instanceFieldSize = reader.ReadULEB128();
                uint directMethodSize = reader.ReadULEB128();
                uint virtualMethodSize = reader.ReadULEB128();

                PrefetchFieldDefinitions(reader, classDefinition, staticFieldSize);
                PrefetchFieldDefinitions(reader, classDefinition, instanceFieldSize);
                PrefetchMethodDefinitions(reader, classDefinition, directMethodSize);
                PrefetchMethodDefinitions(reader, classDefinition, virtualMethodSize);
            });
        }

        private void PrefetchClassDefinitions(BinaryReader reader, bool prefetchMembers)
        {
            reader.PreserveCurrentPosition(header.ClassDefinitionsOffset, () =>
            {
                for (int i = 0; i < header.ClassDefinitionsSize; i++)
                {
                    int classIndex = reader.ReadInt32();

                    ClassDefinition cdef;
                    if (typeReferences[classIndex] is ClassDefinition)
                    {
                        cdef = (ClassDefinition) typeReferences[classIndex];
                    }
                    else
                    {
                        cdef = new ClassDefinition((ClassReference) typeReferences[classIndex]);
                        typeReferences[classIndex] = cdef;
                        classes.Add(cdef);
                    }

                    reader.ReadInt32();
                    // skip access_flags
                    reader.ReadInt32();
                    // skip superclass_idx
                    reader.ReadInt32();
                    // skip interfaces_off
                    reader.ReadInt32();
                    // skip source_file_idx
                    reader.ReadInt32();
                    // skip annotations_off

                    uint classDataOffset = reader.ReadUInt32();
                    if ((classDataOffset > 0) && prefetchMembers)
                    {
                        PrefetchClassDefinition(reader, cdef, classDataOffset);
                    }

                    reader.ReadInt32();
                    // skip static_values_off
                }
            });
        }

        private void ReadAnnotationDirectory(BinaryReader reader, ClassDefinition classDefinition, uint annotationOffset)
        {
            reader.PreserveCurrentPosition(annotationOffset, () =>
            {
                uint classAnnotationOffset = reader.ReadUInt32();
                uint annotatedFieldsSize = reader.ReadUInt32();
                uint annotatedMethodsSize = reader.ReadUInt32();
                uint annotatedParametersSize = reader.ReadUInt32();

                if (classAnnotationOffset > 0)
                    classDefinition.Annotations =
                        ReadAnnotationSet(reader,
                            classAnnotationOffset);

                for (int j = 0; j < annotatedFieldsSize; j++)
                    ((FieldDefinition) fieldReferences[reader.ReadInt32()]).Annotations =
                        ReadAnnotationSet(reader, reader.ReadUInt32());

                for (int j = 0; j < annotatedMethodsSize; j++)
                    ((MethodDefinition) methodReferences[reader.ReadInt32()]).Annotations =
                        ReadAnnotationSet(reader, reader.ReadUInt32());

                for (int j = 0; j < annotatedParametersSize; j++)
                {
                    int methodIndex = reader.ReadInt32();
                    uint offset = reader.ReadUInt32();
                    var annotations = ReadAnnotationSetRefList(reader, offset);
                    var mdef = (methodReferences[methodIndex] as MethodDefinition);

                    for (int i = 0; i < annotations.Count; i++)
                    {
                        if (annotations[i].Count > 0)
                        {
                            mdef.Prototype.Parameters[i].Annotations = annotations[i];
                        }
                    }
                }
            });
        }

        private List<List<Annotation>> ReadAnnotationSetRefList(BinaryReader reader, uint annotationOffset)
        {
            var result = new List<List<Annotation>>();
            reader.PreserveCurrentPosition(annotationOffset, () =>
            {
                uint size = reader.ReadUInt32();
                for (uint i = 0; i < size; i++)
                {
                    uint offset = reader.ReadUInt32();
                    result.Add(ReadAnnotationSet(reader, offset));
                }
            });
            return result;
        }

        private List<Annotation> ReadAnnotationSet(BinaryReader reader, uint annotationOffset)
        {
            var result = new List<Annotation>();
            reader.PreserveCurrentPosition(annotationOffset, () =>
            {
                uint size = reader.ReadUInt32();
                for (uint i = 0; i < size; i++)
                {
                    uint offset = reader.ReadUInt32();
                    result.Add(ReadAnnotation(reader, offset));
                }
            });
            return result;
        }

        private Annotation ReadAnnotation(BinaryReader reader, uint annotationOffset)
        {
            Annotation annotation = null;
            reader.PreserveCurrentPosition(annotationOffset, () =>
            {
                byte visibility = reader.ReadByte();
                annotation = ReadEncodedAnnotation(reader);
                annotation.Visibility =
                    (AnnotationVisibility) visibility;
            });
            return annotation;
        }

        private Annotation ReadEncodedAnnotation(BinaryReader reader)
        {
            var typeIndex = (int) reader.ReadULEB128();
            var elementSize = (int) reader.ReadULEB128();

            var annotation = new Annotation();
            annotation.Type = (ClassReference) typeReferences[typeIndex];

            for (int i = 0; i < elementSize; i++)
            {
                var nameIndex = (int) reader.ReadULEB128();
                var name = strings[nameIndex];
                var value = ReadValue(reader);
                var argument = new AnnotationArgument(name, value);
                annotation.Arguments.Add(argument);
            }

            return annotation;
        }

        private void ReadPrototypes(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(header.PrototypesOffset, () =>
            {
                for (int i = 0; i < header.PrototypesSize; i++)
                {
                    long thisOffset = reader.BaseStream.Position;
                    int shortyIndex = reader.ReadInt32();
                    int returnTypeIndex = reader.ReadInt32();
                    uint parametersOffset = reader.ReadUInt32();

                    var prototype = new Prototype();
                    prototype.ReturnType = typeReferences[returnTypeIndex];

                    if (parametersOffset > 0)
                    {
                        ReadParameters(reader, prototype, parametersOffset);
                    }

                    prototype.Freeze();

                    // cache the signature and hashcode.
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    prototype.GetHashCode();
                    prototype.ToSignature();
                    
                    prototypes.Add(prototype);
                }
            });
        }

        private void ReadParameters(BinaryReader reader, Prototype prototype, uint parametersOffset)
        {
            reader.PreserveCurrentPosition(parametersOffset, () =>
            {
                uint typecount = reader.ReadUInt32();
                for (int j = 0; j < typecount; j++)
                {
                    var parameter = new Parameter();
                    ushort typeIndex = reader.ReadUInt16();
                    parameter.Type = typeReferences[typeIndex];
                    prototype.Parameters.Add(parameter);
                }
            });
        }

        private void ReadClassDefinitions(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(header.ClassDefinitionsOffset, () =>
            {
                for (int i = 0; i < header.ClassDefinitionsSize; i++)
                {
                    uint classIndex = reader.ReadUInt32();

                    var cdef = (ClassDefinition) typeReferences[(int) classIndex];
                    cdef.AccessFlags = (AccessFlags) reader.ReadUInt32();

                    uint superClassIndex = reader.ReadUInt32();
                    if (superClassIndex != DexConsts.NoIndex)
                        cdef.SuperClass = (ClassReference) typeReferences[(int) superClassIndex];

                    uint interfaceOffset = reader.ReadUInt32();
                    uint sourceFileIndex = reader.ReadUInt32();
                    uint annotationOffset= reader.ReadUInt32();
                    uint classDataOffset = reader.ReadUInt32();
                    uint staticValuesOffset = reader.ReadUInt32();

                    if (interfaceOffset > 0) ReadInterfaces(reader, cdef, interfaceOffset);

                    if (sourceFileIndex != DexConsts.NoIndex)
                        cdef.SourceFile = strings[(int) sourceFileIndex];

                    if (classDataOffset > 0)
                        ReadClassDefinition(reader, cdef, classDataOffset);

                    if (annotationOffset > 0)
                        ReadAnnotationDirectory(reader, cdef, annotationOffset);

                    if (staticValuesOffset > 0)
                    {
                        ReadStaticValues(reader, cdef, staticValuesOffset);
                    }
                }
            });
        }

        private void ReadStaticValues(BinaryReader reader, ClassDefinition classDefinition, uint staticValuesOffset)
        {
            reader.PreserveCurrentPosition(staticValuesOffset, () =>
            {
                object[] values = ReadValues(reader);
                for (int j = 0; j < values.Length; j++)
                {
                    classDefinition.Fields[j].Value = values[j];
                }
            });
        }

        private void ReadInterfaces(BinaryReader reader, ClassDefinition classDefinition, uint interfaceOffset)
        {
            reader.PreserveCurrentPosition(interfaceOffset, () =>
            {
                uint typecount = reader.ReadUInt32();
                for (int j = 0; j < typecount; j++)
                {
                    ushort typeIndex = reader.ReadUInt16();
                    classDefinition.Interfaces.Add((ClassReference)typeReferences[typeIndex]);
                }
            });
        }

        private object[] ReadValues(BinaryReader reader)
        {
            uint size = reader.ReadULEB128();
            var array = new ArrayList();
            for (uint i = 0; i < size; i++)
            {
                array.Add(ReadValue(reader));
            }
            return array.ToArray();
        }

        private object ReadValue(BinaryReader reader)
        {
            int data = reader.ReadByte();
            int valueFormat = data & 0x1F;
            int valueArgument = data >> 5;

            switch ((ValueFormats) valueFormat)
            {
                case ValueFormats.Byte:
                    return (sbyte) reader.ReadSignedPackedNumber(valueArgument + 1);
                case ValueFormats.Short:
                    return (short) reader.ReadSignedPackedNumber(valueArgument + 1);
                case ValueFormats.Char:
                    return (char) reader.ReadUnsignedPackedNumber(valueArgument + 1);
                case ValueFormats.Int:
                    return (int) reader.ReadSignedPackedNumber(valueArgument + 1);
                case ValueFormats.Long:
                    return (long) reader.ReadSignedPackedNumber(valueArgument + 1);
                case ValueFormats.Float:
                    return
                        BitConverter.ToSingle(
                            BitConverter.GetBytes((int) reader.ReadSignedPackedNumber(valueArgument + 1)), 0);
                case ValueFormats.Double:
                    return BitConverter.Int64BitsToDouble(reader.ReadSignedPackedNumber(valueArgument + 1));
                case ValueFormats.String:
                    return strings[(int) reader.ReadUnsignedPackedNumber(valueArgument + 1)];
                case ValueFormats.Type:
                    return typeReferences[(int) reader.ReadUnsignedPackedNumber(valueArgument + 1)];
                case ValueFormats.Field:
                case ValueFormats.Enum:
                    return fieldReferences[(int) reader.ReadUnsignedPackedNumber(valueArgument + 1)];
                case ValueFormats.Method:
                    return methodReferences[(int) reader.ReadUnsignedPackedNumber(valueArgument + 1)];
                case ValueFormats.Array:
                    return ReadValues(reader);
                case ValueFormats.Annotation:
                    return ReadEncodedAnnotation(reader);
                case ValueFormats.Null:
                    return null;
                case ValueFormats.Boolean:
                    return valueArgument != 0;
                default:
                    throw new ArgumentException();
            }
        }

        private void ReadFieldDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint fieldcount)
        {
            uint fieldIndex = 0;
            for (int i = 0; i < fieldcount; i++)
            {
                fieldIndex += reader.ReadULEB128();
                uint accessFlags = reader.ReadULEB128();

                var fdef = (FieldDefinition) fieldReferences[(int) fieldIndex];
                fdef.AccessFlags = (AccessFlags) accessFlags;
                fdef.Owner = classDefinition;

                classDefinition.Fields.Add(fdef);
            }
        }

        private void ReadMethodDefinitions(BinaryReader reader, ClassDefinition classDefinition, uint methodcount,
            bool isVirtual)
        {
            uint methodIndex = 0;
            for (int i = 0; i < methodcount; i++)
            {
                methodIndex += reader.ReadULEB128();
                uint accessFlags = reader.ReadULEB128();
                uint codeOffset = reader.ReadULEB128();

                var mdef = (MethodDefinition) methodReferences[(int) methodIndex];
                mdef.AccessFlags = (AccessFlags) accessFlags;
                mdef.Owner = classDefinition;
                mdef.IsVirtual = isVirtual;

                classDefinition.Methods.Add(mdef);

                if (codeOffset > 0)
                    ReadMethodBody(reader, mdef, codeOffset);
            }
        }

        private void ReadMethodBody(BinaryReader reader, MethodDefinition mdef, uint codeOffset)
        {
            reader.PreserveCurrentPosition(codeOffset, () =>
            {
                ushort registersSize = reader.ReadUInt16();
                ushort incomingArgsSize = reader.ReadUInt16();
                ushort outgoingArgsSize = reader.ReadUInt16();
                ushort triesSize = reader.ReadUInt16();
                uint debugOffset = reader.ReadUInt32();

                mdef.Body = new MethodBody(mdef, registersSize);
                mdef.Body.IncomingArguments = incomingArgsSize;
                mdef.Body.OutgoingArguments = outgoingArgsSize;

                var ireader = new InstructionReader(this, mdef);
                ireader.ReadFrom(reader);

                if ((triesSize != 0) && (ireader.Codes.Length%2 != 0))
                    reader.ReadUInt16(); // padding (4-byte alignment)

                if (triesSize != 0)
                    ReadExceptionHandlers(reader, mdef, ireader,
                        triesSize);

                if (debugOffset != 0)
                    ReadDebugInfo(reader, mdef, ireader, debugOffset);
            });
        }

        private void ReadDebugInfo(BinaryReader reader, MethodDefinition mdef, InstructionReader instructionReader,
            uint debugOffset)
        {
            reader.PreserveCurrentPosition(debugOffset, () =>
            {
                var debugInfo = new DebugInfo(mdef.Body);
                mdef.Body.DebugInfo = debugInfo;

                uint lineStart = reader.ReadULEB128();
                debugInfo.LineStart = lineStart;

                uint parametersSize = reader.ReadULEB128();
                for (int i = 0; i < parametersSize; i++)
                {
                    long index = reader.ReadULEB128p1();
                    string name = null;
                    if (index != DexConsts.NoIndex && index >= 0)
                        name = strings[(int) index];
                    debugInfo.Parameters.Add(name);
                }

                while (true)
                {
                    var ins = new DebugInstruction((DebugOpCodes) reader.ReadByte());
                    debugInfo.DebugInstructions.Add(ins);

                    uint registerIndex;
                    uint addrDiff;
                    long nameIndex;
                    long typeIndex;
                    long signatureIndex;
                    int lineDiff;
                    string name;

                    switch (ins.OpCode)
                    {
                        case DebugOpCodes.AdvancePc:
                            // uleb128 addr_diff
                            addrDiff = reader.ReadULEB128();
                            ins.Operands.Add(addrDiff);
                            break;
                        case DebugOpCodes.AdvanceLine:
                            // sleb128 line_diff
                            lineDiff = reader.ReadSLEB128();
                            ins.Operands.Add(lineDiff);
                            break;
                        case DebugOpCodes.EndLocal:
                        case DebugOpCodes.RestartLocal:
                            // uleb128 register_num
                            registerIndex = reader.ReadULEB128();
                            ins.Operands.Add(mdef.Body.Registers[(int) registerIndex]);
                            break;
                        case DebugOpCodes.SetFile:
                            // uleb128p1 name_idx
                            nameIndex = reader.ReadULEB128p1();
                            name = null;
                            if (nameIndex != DexConsts.NoIndex && nameIndex >= 0)
                                name = strings[(int) nameIndex];
                            ins.Operands.Add(name);
                            break;
                        case DebugOpCodes.StartLocalExtended:
                        case DebugOpCodes.StartLocal:
                            // StartLocalExtended : uleb128 register_num, uleb128p1 name_idx, uleb128p1 type_idx, uleb128p1 sig_idx
                            // StartLocal : uleb128 register_num, uleb128p1 name_idx, uleb128p1 type_idx
                            Boolean isExtended = ins.OpCode ==
                                                 DebugOpCodes.
                                                     StartLocalExtended;

                            registerIndex = reader.ReadULEB128();
                            ins.Operands.Add(mdef.Body.Registers[(int) registerIndex]);

                            nameIndex = reader.ReadULEB128p1();
                            name = null;
                            if (nameIndex != DexConsts.NoIndex && nameIndex >= 0)
                                name = strings[(int) nameIndex];
                            ins.Operands.Add(name);

                            typeIndex = reader.ReadULEB128p1();
                            TypeReference type = null;
                            if (typeIndex != DexConsts.NoIndex && typeIndex >= 0)
                                type = typeReferences[(int) typeIndex];
                            ins.Operands.Add(type);

                            if (isExtended)
                            {
                                signatureIndex = reader.ReadULEB128p1();
                                string signature = null;
                                if (signatureIndex != DexConsts.NoIndex && signatureIndex >= 0)
                                    signature = strings[(int) signatureIndex];
                                ins.Operands.Add(signature);
                            }

                            break;
                        case DebugOpCodes.EndSequence:
                            return;
                        case DebugOpCodes.Special:
                        // between 0x0a and 0xff (inclusive)
                        case DebugOpCodes.SetPrologueEnd:
                        case DebugOpCodes.SetEpilogueBegin:
                        default:
                            break;
                    }
                }
            });
        }

        private void ReadExceptionHandlers(BinaryReader reader, MethodDefinition mdef,
            InstructionReader instructionReader, ushort triesSize)
        {
            var exceptionLookup = new Dictionary<uint, List<ExceptionHandler>>();
            for (int i = 0; i < triesSize; i++)
            {
                uint startOffset = reader.ReadUInt32();
                uint insCount = reader.ReadUInt16();
                uint endOffset = startOffset + insCount - 1;
                uint handlerOffset = reader.ReadUInt16();

                var ehandler = new ExceptionHandler();
                mdef.Body.Exceptions.Add(ehandler);
                if (!exceptionLookup.ContainsKey(handlerOffset))
                {
                    exceptionLookup.Add(handlerOffset, new List<ExceptionHandler>());
                }
                exceptionLookup[handlerOffset].Add(ehandler);
                ehandler.TryStart = instructionReader.Lookup[(int) startOffset];
                // The last code unit covered (inclusive) is start_addr + insn_count - 1
                ehandler.TryEnd = instructionReader.LookupLast[(int) endOffset];
            }

            long baseOffset = reader.BaseStream.Position;
            uint catchHandlersSize = reader.ReadULEB128();
            for (int i = 0; i < catchHandlersSize; i++)
            {
                long itemoffset = reader.BaseStream.Position - baseOffset;
                int catchTypes = reader.ReadSLEB128();
                bool catchAllPresent = catchTypes <= 0;
                catchTypes = Math.Abs(catchTypes);

                for (int j = 0; j < catchTypes; j++)
                {
                    uint typeIndex = reader.ReadULEB128();
                    uint offset = reader.ReadULEB128();
                    var @catch = new Catch();
                    @catch.Type = typeReferences[(int) typeIndex];
                    @catch.Instruction = instructionReader.Lookup[(int) offset];

                    // As catch handler can be used in several tries, let's clone the catch
                    foreach (ExceptionHandler ehandler in exceptionLookup[(uint) itemoffset])
                        ehandler.Catches.Add(@catch.Clone());
                }

                if (catchAllPresent)
                {
                    uint offset = reader.ReadULEB128();
                    foreach (ExceptionHandler ehandler in exceptionLookup[(uint) itemoffset])
                        ehandler.CatchAll = instructionReader.Lookup[(int) offset];
                }
            }
        }

        private void ReadClassDefinition(BinaryReader reader, ClassDefinition classDefinition, uint classDataOffset)
        {
            reader.PreserveCurrentPosition(classDataOffset, () =>
            {
                uint staticFieldSize = reader.ReadULEB128();
                uint instanceFieldSize = reader.ReadULEB128();
                uint directMethodSize = reader.ReadULEB128();
                uint virtualMethodSize = reader.ReadULEB128();

                ReadFieldDefinitions(reader, classDefinition, staticFieldSize);
                ReadFieldDefinitions(reader, classDefinition, instanceFieldSize);
                ReadMethodDefinitions(reader, classDefinition, directMethodSize, false);
                ReadMethodDefinitions(reader, classDefinition, virtualMethodSize, true);
            });
        }

        private void ReadMethodReferences(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(header.MethodReferencesOffset, () =>
            {
                for (int i = 0; i < header.MethodReferencesSize; i++)
                {
                    int classIndex = reader.ReadUInt16();
                    int prototypeIndex = reader.ReadUInt16();
                    int nameIndex = reader.ReadInt32();

                    var mref = new MethodReference();
                    mref.Owner = (CompositeType) typeReferences[classIndex];
                    // Clone the prototype so we can annotate & update it easily
                    mref.Prototype = prototypes[prototypeIndex].Clone();
                    mref.Name = strings[nameIndex];

                    methodReferences.Add(mref);
                }
            });
        }

        private void ReadFieldReferences(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(header.FieldReferencesOffset, () =>
            {
                for (int i = 0; i < header.FieldReferencesSize; i++)
                {
                    int classIndex = reader.ReadUInt16();
                    int typeIndex  = reader.ReadUInt16();
                    int nameIndex  = reader.ReadInt32();

                    var fref = new FieldReference();

                    fref.Owner = (ClassReference) typeReferences[classIndex];
                    fref.Type  = typeReferences[typeIndex];
                    fref.Name  = strings[nameIndex];

                    fieldReferences.Add(fref);
                }
            });
        }

        private void ReadTypesReferences(BinaryReader reader)
        {
            reader.PreserveCurrentPosition(header.TypeReferencesOffset, () =>
            {
                for (int i = 0; i < header.TypeReferencesSize; i++)
                {
                    int descriptorIndex = reader.ReadInt32();
                    string descriptor = strings[descriptorIndex];
                    TypeDescriptor.Fill(descriptor, typeReferences[i]);

                    // freeze the references and cache the encoded value.
                    typeReferences[i].Freeze();
                    TypeDescriptor.Encode(typeReferences[i]);
                }
            });
        }

        internal Dex ReadFrom(BinaryReader reader)
        {
            ReadHeader(reader);
            ReadMapList(reader);
            ReadStrings(reader);

            PrefetchTypeReferences(reader);
            PrefetchClassDefinitions(reader, false);

            ReadTypesReferences(reader);
            ReadPrototypes(reader);
            ReadFieldReferences(reader);
            ReadMethodReferences(reader);

            PrefetchClassDefinitions(reader, true);
            ReadClassDefinitions(reader);

            var topLevelClasses = ClassDefinition.Hierarchicalize(classes);
            var dex = new Dex();
            dex.AddRange(topLevelClasses);
            return dex;
        }

        /// <summary>
        /// Gets a field reference by index from the <see cref="fieldReferences"/> table.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal FieldReference GetFieldReference(int index)
        {
            return fieldReferences[index];
        }

        /// <summary>
        /// Gets a method reference by index from the <see cref="methodReferences"/> table.
        /// </summary>
        internal MethodReference GetMethodReference(int index)
        {
            return methodReferences[index];
        }

        /// <summary>
        /// Gets a type reference by index from the <see cref="typeReferences"/> table.
        /// </summary>
        internal TypeReference GetTypeReference(int index)
        {
            return typeReferences[index];
        }

        /// <summary>
        /// Gets a string by index from the <see cref="strings"/> table.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal string GetString(int index)
        {
            return strings[index];
        }
    }
}