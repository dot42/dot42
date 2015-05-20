using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Reachable;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.Synthetic;
using Dot42.DexLib;
using Dot42.Utility;
using Mono.Cecil;
using ILFieldDefinition = Mono.Cecil.FieldDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for existing enum types.
    /// </summary>
    internal class EnumClassBuilder : ClassBuilder
    {
        private readonly NullableEnumBaseClassBuilder nullableBaseClassBuilder;

        private XSyntheticMethodDefinition ctor;
        private EnumInfoClassBuilder enumInfoClassBuilder;
        private XSyntheticMethodDefinition unboxMethod;

        /// <summary>
        /// Default ctor
        /// </summary>
        public EnumClassBuilder(ReachableContext context, AssemblyCompiler compiler, TypeDefinition typeDef, NullableEnumBaseClassBuilder nullableBaseClassBuilder)
            : base(context, compiler, typeDef)
        {
            this.nullableBaseClassBuilder = nullableBaseClassBuilder;
        }

        /// <summary>
        /// Sorting low comes first
        /// </summary>
        protected override int SortPriority { get { return base.Type.UsedInNullableT ? -50 : 0; } }

        /// <summary>
        /// Gets the created instance ctor(name, ordinal, value).
        /// </summary>
        internal XMethodReference GetInstanceConstructorRef()
        {
            if (ctor != null)
                return ctor;
            throw new CompilerException("ctor not created yet");
        }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        protected override void CreateClassDefinition(DexTargetPackage targetPackage, ClassDefinition parent, TypeDefinition parentType, XTypeDefinition parentXType)
        {
            base.CreateClassDefinition(targetPackage, parent, parentType, parentXType);
            Class.IsEnum = true;
        }

        /// <summary>
        /// Implement java.lang.Cloneable
        /// </summary>
        protected override void ImplementCloneable(DexTargetPackage targetPackage)
        {
            // Do nothing
        }

        /// <summary>
        /// Set the super class of the class definition.
        /// </summary>
        protected override void ImplementSuperClass(DexTargetPackage targetPackage)
        {
            if (nullableBaseClassBuilder != null)
            {
                // Super class == nullable base class
                Class.SuperClass = nullableBaseClassBuilder.Class;
                Class.NullableMarkerClass = nullableBaseClassBuilder.Class;
            }
            else
            {
                // Super class is Dot42.Internal.Enum
                Class.SuperClass = Compiler.GetDot42InternalType("Enum").GetClassReference(targetPackage);
            }
        }

        /// <summary>
        /// Create the nested classes for this type.
        /// </summary>
        protected override IEnumerable<ClassBuilder> CreateNestedClassBuilders(ReachableContext context, DexTargetPackage targetPackage, ClassDefinition parent)
        {
            enumInfoClassBuilder = new EnumInfoClassBuilder(context, Compiler, Type, this);
            yield return enumInfoClassBuilder;
        }

        /// <summary>
        /// Should the given field be implemented?
        /// </summary>
        protected override bool ShouldImplementField(ILFieldDefinition field)
        {
            return (field.Name != NameConstants.Enum.ValueFieldName);
        }

        /// <summary>
        /// Implement the class now that all classes have been created
        /// </summary>
        protected override void CreateMembers(DexTargetPackage targetPackage)
        {
            // Build value__ field
            var module = Compiler.Module;
            var underlyingEnumType = Type.GetEnumUnderlyingType();
            var isWide = underlyingEnumType.IsWide();
            var xValueType = isWide ? module.TypeSystem.Long : module.TypeSystem.Int;
            var valueField = XSyntheticFieldDefinition.Create(XType, XSyntheticFieldFlags.Protected | XSyntheticFieldFlags.ReadOnly, NameConstants.Enum.ValueFieldName, xValueType);
            Class.Fields.Add(valueField.GetDexField(Class, targetPackage));

            // Create normal members
            base.CreateMembers(targetPackage);

            // Build value ctor
            ctor = XSyntheticMethodDefinition.Create(XType, XSyntheticMethodFlags.Constructor | XSyntheticMethodFlags.Protected, "<init>", null, module.TypeSystem.Void,
                XParameter.Create("name", module.TypeSystem.String),
                XParameter.Create("ordinal", module.TypeSystem.Int),
                XParameter.Create("value", xValueType));
            ctor.Body = CreateCtorBody(ctor);
            Class.Methods.Add(ctor.GetDexMethod(Class, targetPackage));

            // Build enumInfo field
            var internalEnumInfoType = Compiler.GetDot42InternalType("EnumInfo");
            var enumInfoField = XSyntheticFieldDefinition.Create(XType, XSyntheticFieldFlags.Static | XSyntheticFieldFlags.ReadOnly, NameConstants.Enum.InfoFieldName, internalEnumInfoType/* enumInfoClassBuilder.Class*/);
            Class.Fields.Add(enumInfoField.GetDexField(Class, targetPackage));

            // Build default$ field
            var defaultField = XSyntheticFieldDefinition.Create(XType, XSyntheticFieldFlags.Static | XSyntheticFieldFlags.ReadOnly, NameConstants.Enum.DefaultFieldName, XType);
            Class.Fields.Add(defaultField.GetDexField(Class, targetPackage));

            // Build class ctor
            var classCtor = XSyntheticMethodDefinition.Create(XType, XSyntheticMethodFlags.Static | XSyntheticMethodFlags.Constructor | XSyntheticMethodFlags.Private, "<clinit>", null, module.TypeSystem.Void);
            classCtor.Body = CreateClassCtorBody(isWide, enumInfoField, defaultField, enumInfoClassBuilder.DefaultCtor, xValueType, module.TypeSystem);
            Class.Methods.Add(classCtor.GetDexMethod(Class, targetPackage));

            if (!isWide)
            {
                // Build IntValue method
                var intValue = XSyntheticMethodDefinition.Create(XType, XSyntheticMethodFlags.Virtual, "IntValue", null, module.TypeSystem.Int);
                intValue.Body = CreateIntOrLongValueBody();
                Class.Methods.Add(intValue.GetDexMethod(Class, targetPackage));
            }
            else
            {
                // Build LongValue method
                var longValue = XSyntheticMethodDefinition.Create(XType, XSyntheticMethodFlags.Virtual, "LongValue", null, module.TypeSystem.Long);
                longValue.Body = CreateIntOrLongValueBody();
                Class.Methods.Add(longValue.GetDexMethod(Class, targetPackage));
            }

            // Build Unbox(object) method
            unboxMethod = XSyntheticMethodDefinition.Create(XType, XSyntheticMethodFlags.Static, NameConstants.Enum.UnboxMethodName, null, XType, XParameter.Create("value", Compiler.Module.TypeSystem.Object));
            unboxMethod.Body = CreateUnboxBody(unboxMethod, isWide, Compiler);
            Class.Methods.Add(unboxMethod.GetDexMethod(Class, targetPackage));
        }

        /// <summary>
        /// Generate code for all methods.
        /// </summary>
        public override void GenerateCode(DexTargetPackage targetPackage)
        {
            base.GenerateCode(targetPackage);

            // Compile synthetic methods
            XType.Methods.OfType<XSyntheticMethodDefinition>().ForEach(x => x.Compile(Compiler, targetPackage));
        }

        /// <summary>
        /// Create the body of the ctor.
        /// </summary>
        private AstBlock CreateCtorBody(XSyntheticMethodDefinition method)
        {           
            // Call base ctor
            var nameParam = method.AstParameters[0];
            var ordinalParam = method.AstParameters[1];
            var valueParam = method.AstParameters[2];
            var valueField = XType.Fields.First(x => !x.IsStatic && x.Name == NameConstants.Enum.ValueFieldName);

            return AstBlock.CreateOptimizedForTarget(
                // Call base
                new AstExpression(AstNode.NoSource, AstCode.CallBaseCtor, 2,
                    new AstExpression(AstNode.NoSource, AstCode.Ldthis, null),
                    new AstExpression(AstNode.NoSource, AstCode.Ldloc, nameParam),
                    new AstExpression(AstNode.NoSource, AstCode.Ldloc, ordinalParam)),
                // Initialize value__
                new AstExpression(AstNode.NoSource, AstCode.Stfld, valueField,
                    new AstExpression(AstNode.NoSource, AstCode.Ldthis, null),
                    new AstExpression(AstNode.NoSource, AstCode.Ldloc, valueParam)),
                // Return void
                new AstExpression(AstNode.NoSource, AstCode.Ret, null));
        }

        /// <summary>
        /// Create the body of the class ctor.
        /// </summary>
        private AstBlock CreateClassCtorBody(bool isWide, XFieldDefinition enumInfoField, XFieldDefinition defaultField, XMethodReference enumInfoCtor, XTypeReference valueType, XTypeSystem typeSystem)
        {
            var internalEnumType = Compiler.GetDot42InternalType("Enum");
            var internalEnumInfoType = Compiler.GetDot42InternalType("EnumInfo");
            var valueToFieldMap = new Dictionary<object, XFieldDefinition>();
            var ldc = isWide ? AstCode.Ldc_I8 : AstCode.Ldc_I4;

            var nameVar     = new AstGeneratedVariable("enumName", null) {Type = typeSystem.String};
            var enumInfoVar = new AstGeneratedVariable("enumInfo", null) {Type = internalEnumInfoType};
            var valVar      = new AstGeneratedVariable("val", null) { Type = enumInfoField.FieldType };

            var ast = AstBlock.CreateOptimizedForTarget(
                // Instantiate enum info field
                new AstExpression(AstNode.NoSource, AstCode.Stsfld, enumInfoField,
                    new AstExpression(AstNode.NoSource, AstCode.Stloc, enumInfoVar,
                        new AstExpression(AstNode.NoSource, AstCode.Newobj, enumInfoCtor))));

            // Instantiate values for each field
            var ordinal = 0;
            foreach (var field in XType.Fields.Where(x => x.IsStatic && !(x is XSyntheticFieldDefinition)))
            {
                // Find dex field
                object value;
                if (!field.TryGetEnumValue(out value))
                    throw new CompilerException(string.Format("Cannot get enum value from field {0}", field.FullName));
                value = isWide ? (object)XConvert.ToLong(value) : (object)XConvert.ToInt(value);
                XFieldDefinition existingField;
                AstExpression valueExpr;
                if (valueToFieldMap.TryGetValue(value, out existingField))
                {
                    // Re-use instance of existing field
                    valueExpr = new AstExpression(AstNode.NoSource, AstCode.Ldsfld, existingField);
                }
                else
                {
                    // Record
                    valueToFieldMap[value] = field;

                    // Call ctor
                    valueExpr = new AstExpression(AstNode.NoSource, AstCode.Newobj, ctor,
                        new AstExpression(AstNode.NoSource, AstCode.Stloc, nameVar,
                            new AstExpression(AstNode.NoSource, AstCode.Ldstr, field.Name)),
                        new AstExpression(AstNode.NoSource, AstCode.Ldc_I4, ordinal),
                        new AstExpression(AstNode.NoSource, AstCode.Stloc, valVar,
                            new AstExpression(AstNode.NoSource, ldc, value)));
                }

                // Initialize static field
                var storeExpression = new AstExpression(AstNode.NoSource, AstCode.Stsfld, field, valueExpr);

                // Add to info
                var addMethod = new XMethodReference.Simple("Add", true, typeSystem.Void, internalEnumInfoType,
                    XParameter.Create("value", valueType),
                    XParameter.Create("name", typeSystem.String),
                    XParameter.Create("instance", internalEnumType));
                ast.Body.Add(new AstExpression(AstNode.NoSource, AstCode.Call, addMethod,
                    new AstExpression(AstNode.NoSource, AstCode.Ldloc, enumInfoVar),
                    new AstExpression(AstNode.NoSource, AstCode.Ldloc, valVar),
                    new AstExpression(AstNode.NoSource, AstCode.Ldloc, nameVar),
                    storeExpression));

                // Increment ordinal
                ordinal++;
            }

            // Initialize default field
            var getValueMethod = new XMethodReference.Simple("GetValue", true, internalEnumType, internalEnumInfoType,
                                                             XParameter.Create("value", valueType));
            ast.Body.Add(new AstExpression(AstNode.NoSource, AstCode.Stsfld, defaultField,
                new AstExpression(AstNode.NoSource, AstCode.SimpleCastclass, XType,
                    new AstExpression(AstNode.NoSource, AstCode.Call, getValueMethod,
                        new AstExpression(AstNode.NoSource, AstCode.Ldsfld, enumInfoField),
                        new AstExpression(AstNode.NoSource, ldc, 0)))));

            // Return
            ast.Body.Add(new AstExpression(AstNode.NoSource, AstCode.Ret, null));
            return ast;
        }

        /// <summary>
        /// Create the body of the IntValue or LongValue method.
        /// </summary>
        private AstBlock CreateIntOrLongValueBody()
        {
            // Get value__
            var valueField = XType.Fields.First(x => !x.IsStatic && x.Name == NameConstants.Enum.ValueFieldName);
            return AstBlock.CreateOptimizedForTarget( 
                new AstExpression(AstNode.NoSource, AstCode.Ret, null,
                    new AstExpression(AstNode.NoSource, AstCode.Ldfld, valueField,
                        new AstExpression(AstNode.NoSource, AstCode.Ldthis, null))));
        }

        /// <summary>
        /// Create the body of the unbox(object) method.
        /// </summary>
        private AstBlock CreateUnboxBody(XSyntheticMethodDefinition method, bool isWide, AssemblyCompiler compiler)
        {
            // Prepare
            var loc = AstNode.NoSource;
            Func<AstExpression> ldValue = () => new AstExpression(loc, AstCode.Ldloc, method.AstParameters[0]);
            var afterMyEnum = new AstLabel(loc, "_afterMyEnum");

            // value instanceof MyEnumType?
            var ifNotInstanceOfMyEnum = new AstExpression(loc, AstCode.Brfalse, afterMyEnum, new AstExpression(loc, AstCode.SimpleInstanceOf, XType, ldValue()));
            var returnMyEnum = new AstExpression(loc, AstCode.Ret, null, new AstExpression(loc, AstCode.SimpleCastclass, XType, ldValue()));

            // boxToMyEnum(UnboxInteger(value))
            var boxingType = compiler.GetDot42InternalType("Boxing").Resolve();
            var unboxNumericMethod = boxingType.Methods.First(x => x.Name == (isWide ? "UnboxLong" : "UnboxInteger"));
            var unboxToNumeric = new AstExpression(loc, AstCode.Call, unboxNumericMethod, ldValue());
            var numericToMyEnum = new AstExpression(loc, isWide ? AstCode.Long_to_enum : AstCode.Int_to_enum, XType, unboxToNumeric).SetType(XType);
            var returnX = new AstExpression(loc, AstCode.Ret, null, numericToMyEnum);

            var ast = new AstBlock(loc, new AstNode[] {
                ifNotInstanceOfMyEnum,
                returnMyEnum,
                afterMyEnum,
                returnX
            });            
            return ast;
        }
    }
}
