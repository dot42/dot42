using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.JvmClassLib.Attributes;
using Dot42.JvmClassLib.Structures;
using Dot42.Utility;

namespace Dot42.JvmClassLib
{
    /// <summary>
    /// Helper used to read classfile structures.
    /// </summary>
    internal class ClassFileReader
    {
        private readonly Stream stream;

        /// <summary>
        /// Default ctor
        /// </summary>
        public ClassFileReader(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// Read the header
        /// </summary>
        internal void ReadHeader(ClassFile cf)
        {
            var magic = stream.ReadU4();
            if (magic != ClassFile.Magic)
                throw new Dot42Exception("Invalid magic");

            cf.MinorVersion = stream.ReadU2();
            cf.MajorVersion = stream.ReadU2();

            var cpCount = stream.ReadU2();
            var cp = new ConstantPool(cpCount, cf.Loader);
            for (var i = 1; i < cpCount; i++)
            {
                var tag = (ConstantPoolTags) stream.ReadU1();
                ConstantPoolEntry entry;
                int tmp;
                switch (tag)
                {
                    case ConstantPoolTags.Class:
                        entry = new ConstantPoolClass(cp, stream.ReadU2());
                        break;
                    case ConstantPoolTags.Fieldref:
                        tmp = stream.ReadU2();
                        entry = new ConstantPoolFieldRef(cp, tmp, stream.ReadU2());
                        break;
                    case ConstantPoolTags.Methodref:
                        tmp = stream.ReadU2();
                        entry = new ConstantPoolMethodRef(cp, tmp, stream.ReadU2());
                        break;
                    case ConstantPoolTags.InterfaceMethodref:
                        tmp = stream.ReadU2();
                        entry = new ConstantPoolInterfaceMethodRef(cp, tmp, stream.ReadU2());
                        break;
                    case ConstantPoolTags.String:
                        entry = new ConstantPoolString(cp, stream.ReadU2());
                        break;
                    case ConstantPoolTags.Integer:
                        entry = new ConstantPoolInteger(cp, stream.ReadS4());
                        break;
                    case ConstantPoolTags.Float:
                        entry = new ConstantPoolFloat(cp, stream.ReadF4());
                        break;
                    case ConstantPoolTags.Long:
                        entry = new ConstantPoolLong(cp, stream.ReadS8());
                        break;
                    case ConstantPoolTags.Double:
                        entry = new ConstantPoolDouble(cp, stream.ReadF8());
                        break;
                    case ConstantPoolTags.NameAndType:
                        tmp = stream.ReadU2();
                        entry = new ConstantPoolNameAndType(cp, tmp, stream.ReadU2());
                        break;
                    case ConstantPoolTags.Utf8:
                        tmp = stream.ReadU2();
                        entry = new ConstantPoolUtf8(cp, stream.ReadUTF8(tmp));
                        break;
                    default:
                        throw new Dot42Exception("Unknown constant pool tag: " + (int)tag);
                }
                cp[i] = entry;
                if ((tag == ConstantPoolTags.Double) || (tag == ConstantPoolTags.Long))
                    i++;
            }

            cf.ClassAccessFlags = (ClassAccessFlags) stream.ReadU2();
            
            var index = stream.ReadU2();
            cf.ClassName = cp.GetEntry<ConstantPoolClass>(index).Name;
            index = stream.ReadU2();
            cf.SuperClass = (index == 0) ? null : new ObjectTypeReference(cp.GetEntry<ConstantPoolClass>(index).Name, null);

            // Interfaces
            var icount = stream.ReadU2();
            var interfaces = new string[icount];
            for (var i = 0; i < icount; i++)
            {
                index = stream.ReadU2();
                interfaces[i] = cp.GetEntry<ConstantPoolClass>(index).Name;
            }
            cf.Interfaces = interfaces.Select(x => new ObjectTypeReference(x, null)).ToArray();

            // Fields
            var fcount = stream.ReadU2();
            for (var i = 0; i < fcount; i++)
            {
                var accessFlags = (FieldAccessFlags) stream.ReadU2();
                var nameIndex = stream.ReadU2();
                var descriptorIndex = stream.ReadU2();
                var name = cp.GetEntry<ConstantPoolUtf8>(nameIndex).Value;
                var descriptor = cp.GetEntry<ConstantPoolUtf8>(descriptorIndex).Value;
                var field = new FieldDefinition(cf, accessFlags, name, descriptor, null);
                ReadAttributes(cp, field);
                cf.Fields.Add(field);
            }

            // Methods
            var mcount = stream.ReadU2();
            for (var i = 0; i < mcount; i++)
            {
                var accessFlags = (MethodAccessFlags)stream.ReadU2();
                var nameIndex = stream.ReadU2();
                var descriptorIndex = stream.ReadU2();
                var name = cp.GetEntry<ConstantPoolUtf8>(nameIndex).Value;
                var descriptor = cp.GetEntry<ConstantPoolUtf8>(descriptorIndex).Value;
                var method = new MethodDefinition(cf, accessFlags, name, descriptor, null);
                ReadAttributes(cp, method);
                cf.Methods.Add(method);
            }

            // Attributes
            ReadAttributes(cp, cf);
        }

        /// <summary>
        /// Read a list of attributes
        /// </summary>
        private void ReadAttributes(ConstantPool cp, IModifiableAttributeProvider provider)
        {
            var count = stream.ReadU2();
            for (var i = 0; i < count; i++)
            {
                var nameIndex = stream.ReadU2();
                var name = cp.GetEntry<ConstantPoolUtf8>(nameIndex).Value;
                var length = stream.ReadU4();

                Attribute attr;
                int tmp;
                switch (name)
                {
                    case CodeAttribute.AttributeName:
                        attr = ReadCodeAttribute((MethodDefinition)provider, cp);
                        break;
                    case ConstantValueAttribute.AttributeName:
                        tmp = stream.ReadU2();
                        attr = new ConstantValueAttribute(((IConstantPoolValue)cp[tmp]).Value);
                        break;
                    case ExceptionsAttribute.AttributeName:
                        attr = ReadExceptionsAttribute(cp);
                        break;
                    case InnerClassesAttribute.AttributeName:
                        attr = ReadInnerClassesAttribute(cp);
                       break;
                    case SyntheticAttribute.AttributeName:
                        attr = new SyntheticAttribute();
                        break;
                    case SourceFileAttribute.AttributeName:
                        tmp = stream.ReadU2();
                        attr = new SourceFileAttribute(cp.GetEntry<ConstantPoolUtf8>(tmp).Value);
                        break;
                    case LineNumberTableAttribute.AttributeName:
                        attr = ReadLineNumberTableAttribute();
                        break;
                    case LocalVariableTableAttribute.AttributeName:
                        attr = ReadLocalVariableTableAttribute(cp);
                        break;
                    case DeprecatedAttribute.AttributeName:
                        attr = new DeprecatedAttribute();
                        break;
                    case OverrideAttribute.AttributeName:
                        attr = new OverrideAttribute();
                        break;
                    case SignatureAttribute.AttributeName:
                        tmp = stream.ReadU2();
                        attr = new SignatureAttribute(cp.GetEntry<ConstantPoolUtf8>(tmp).Value);
                        break;
                    case RuntimeVisibleAnnotationsAttribute.AttributeName:
                        attr = new RuntimeVisibleAnnotationsAttribute(ReadAnnotationsAttribute(cp));
                        break;
                    case RuntimeInvisibleAnnotationsAttribute.AttributeName:
                        attr = new RuntimeInvisibleAnnotationsAttribute(ReadAnnotationsAttribute(cp));
                        break;
                    case RuntimeVisibleParameterAnnotationsAttribute.AttributeName:
                        attr = new RuntimeVisibleParameterAnnotationsAttribute(ReadAnnotationsAttribute(cp));
                        break;
                    case RuntimeInvisibleParameterAnnotationsAttribute.AttributeName:
                        attr = new RuntimeInvisibleParameterAnnotationsAttribute(ReadAnnotationsAttribute(cp));
                        break;
                    case AnnotationDefaultAttribute.AttributeName:
                        attr = new AnnotationDefaultAttribute(ReadElementValue(cp));
                        break;
                    default:
                        stream.Skip(length);
                        attr = new UnknownAttribute(name);
                        break;
                }
                provider.Add(attr);
            }
            provider.AttributesLoaded();
        }

        /// <summary>
        /// Read a Code attribute
        /// </summary>
        private CodeAttribute ReadCodeAttribute(MethodDefinition method, ConstantPool cp)
        {
            var maxStack = stream.ReadU2();
            var maxLocals = stream.ReadU2();
            var codeLength = (int)stream.ReadU4();
            var code = new byte[codeLength];
            stream.Read(code, 0, codeLength);
            var result = new CodeAttribute(method, cp, maxStack, maxLocals, code);

            var ecount = stream.ReadU2();
            for (var i = 0; i < ecount; i++)
            {
                var startPC = stream.ReadU2();
                var endPC = stream.ReadU2();
                var handlerPC = stream.ReadU2();
                var catchTypeIndex = stream.ReadU2();
                var catchType = (catchTypeIndex != 0) ? cp.GetEntry<ConstantPoolClass>(catchTypeIndex).Type : null;
                result.Add(new ExceptionHandler(result, startPC, endPC, handlerPC, catchType));
            }

            ReadAttributes(cp, result);

            return result;
        }

        /// <summary>
        /// Read the data of a RuntimeVisibleAnnotations or RuntimeInvisibleAnnotations attribute.
        /// </summary>
        private Annotation[] ReadAnnotationsAttribute(ConstantPool cp)
        {
            var count = stream.ReadU2();
            var result = new Annotation[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = ReadAnnotation(cp);
            }
            return result;
        }

        /// <summary>
        /// Read a single annotation structure.
        /// </summary>
        private Annotation ReadAnnotation(ConstantPool cp)
        {
            var typeIndex = stream.ReadU2();
            var numPairs = stream.ReadU2();
            var pairs = new ElementValuePair[numPairs];
            for (var j = 0; j < numPairs; j++)
            {
                var nameIndex = stream.ReadU2();
                var value = ReadElementValue(cp);
                pairs[j] = new ElementValuePair(cp, nameIndex, value);
            }
            return new Annotation(cp, typeIndex, pairs);
        }

        /// <summary>
        /// Read an element_value (4.7.16.1)
        /// </summary>
        private ElementValue ReadElementValue(ConstantPool cp)
        {
            var tag = (char) stream.ReadU1();
            switch (tag)
            {
                case 'B':
                case 'C':
                case 'D':
                case 'F':
                case 'I':
                case 'J':
                case 'S':
                case 'Z':
                case 's':
                    return new ConstElementValue(cp, tag, stream.ReadU2());
                case 'e':
                    {
                        var typeNameIndex = stream.ReadU2();
                        var constNameIndex = stream.ReadU2();
                        return new EnumConstElementValue(cp, typeNameIndex, constNameIndex);
                    }
                case 'c':
                    return new ClassElementValue(cp, stream.ReadU2());
                case '@':
                    return new AnnotationElementValue(ReadAnnotation(cp));
                case '[':
                    {
                        var numValues = stream.ReadU2();
                        var values = new ElementValue[numValues];
                        for (var i = 0; i < numValues; i++)
                        {
                            values[i] = ReadElementValue(cp);
                        }
                        return new ArrayElementValue(values);
                    }
                default:
                    throw new Dot42Exception(string.Format("Unknown element_value tag '{0}'", tag));
            }
        }

        /// <summary>
        /// Read a Exceptions attribute
        /// </summary>
        private ExceptionsAttribute ReadExceptionsAttribute(ConstantPool cp)
        {
            var result = new ExceptionsAttribute();
            var count = stream.ReadU2();
            for (var i = 0; i < count; i++)
            {
                var index = stream.ReadU2();
                result.Exceptions.Add(cp.GetEntry<ConstantPoolClass>(index).Type);
            }
            return result;
        }

        /// <summary>
        /// Read a InnerClasses attribute
        /// </summary>
        private InnerClassesAttribute ReadInnerClassesAttribute(ConstantPool cp)
        {
            var result = new InnerClassesAttribute();
            var count = stream.ReadU2();
            for (var i = 0; i < count; i++)
            {
                var innerClassIndex = stream.ReadU2();
                var innerClass = (innerClassIndex != 0) ? cp.GetEntry<ConstantPoolClass>(innerClassIndex).Type : null;
                var outerClassIndex = stream.ReadU2();
                var outerClass = (outerClassIndex != 0) ? cp.GetEntry<ConstantPoolClass>(outerClassIndex).Type : null;
                var innerNameIndex = stream.ReadU2();
                var innerName = (innerNameIndex != 0) ? cp.GetEntry<ConstantPoolUtf8>(innerNameIndex).Value : null;
                var accessFlags = (NestedClassAccessFlags)stream.ReadU2();
                result.Classes.Add(new InnerClass(cp.Loader, innerClass, outerClass, innerName, accessFlags));
            }
            return result;
        }

        /// <summary>
        /// Read a LineNumberTable attribute
        /// </summary>
        private LineNumberTableAttribute ReadLineNumberTableAttribute()
        {
            var result = new LineNumberTableAttribute();
            var count = stream.ReadU2();
            for (var i = 0; i < count; i++)
            {
                var startPC = stream.ReadU2();
                var lineNumber = stream.ReadU2();
                result.Add(new LineNumber(startPC, lineNumber));
            }
            return result;
        }

        /// <summary>
        /// Read a LocalVariableTable attribute
        /// </summary>
        private LocalVariableTableAttribute ReadLocalVariableTableAttribute(ConstantPool cp)
        {
            var result = new LocalVariableTableAttribute();
            var count = stream.ReadU2();
            for (var i = 0; i < count; i++)
            {
                var startPC = stream.ReadU2();
                var length = stream.ReadU2();
                var nameIndex = stream.ReadU2();
                var name = cp.GetEntry<ConstantPoolUtf8>(nameIndex).Value;
                var descriptorIndex = stream.ReadU2();
                var descriptor = cp.GetEntry<ConstantPoolUtf8>(descriptorIndex).Value;
                var variableType = Descriptors.ParseFieldType(descriptor);
                var index = stream.ReadU2();
                result.Variables.Add(new LocalVariable(startPC, length, name, variableType, index));
            }
            return result;
        }
    }
}
