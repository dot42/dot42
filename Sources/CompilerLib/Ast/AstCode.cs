namespace Dot42.CompilerLib.Ast
{
    public enum AstCode
    {
        // For convenience, the start is exactly identical to Mono.Cecil.Cil.Code
        // Instructions that should not be used are prepended by __
        Nop,
        Break,
        __Ldarg_0,
        __Ldarg_1,
        __Ldarg_2,
        __Ldarg_3,
        __Ldloc_0,
        __Ldloc_1,
        __Ldloc_2,
        __Ldloc_3,
        __Stloc_0,
        __Stloc_1,
        __Stloc_2,
        __Stloc_3,
        __Ldarg_S,
        __Ldarga_S,
        __Starg_S,
        __Ldloc_S,
        __Ldloca_S,
        __Stloc_S,
        Ldnull,
        __Ldc_I4_M1,
        __Ldc_I4_0,
        __Ldc_I4_1,
        __Ldc_I4_2,
        __Ldc_I4_3,
        __Ldc_I4_4,
        __Ldc_I4_5,
        __Ldc_I4_6,
        __Ldc_I4_7,
        __Ldc_I4_8,
        __Ldc_I4_S,
        Ldc_I4,
        Ldc_I8,
        Ldc_R4,
        Ldc_R8,
        Dup,
        Pop,
        Jmp,
        Call,
        Calli,
        Ret,
        __Br_S,
        __Brfalse_S,
        __Brtrue_S,
        __Beq_S,
        __Bge_S,
        __Bgt_S,
        __Ble_S,
        __Blt_S,
        __Bne_Un_S,
        __Bge_Un_S,
        __Bgt_Un_S,
        __Ble_Un_S,
        __Blt_Un_S,
        Br,
        Brfalse,
        Brtrue,
        __Beq,
        __Bge,
        __Bgt,
        __Ble,
        __Blt,
        __Bne_Un,
        __Bge_Un,
        __Bgt_Un,
        __Ble_Un,
        __Blt_Un,
        Switch,
        __Ldind_I1,
        __Ldind_U1,
        __Ldind_I2,
        __Ldind_U2,
        __Ldind_I4,
        __Ldind_U4,
        __Ldind_I8,
        __Ldind_I,
        __Ldind_R4,
        __Ldind_R8,
        Ldind_Ref,
        Stind_Ref,
        __Stind_I1,
        __Stind_I2,
        __Stind_I4,
        __Stind_I8,
        __Stind_R4,
        __Stind_R8,
        Add,
        Sub,
        Mul,
        Div,
        Div_Un,
        Rem,
        Rem_Un,
        And,
        Or,
        Xor,
        Shl,
        Shr,
        Shr_Un,
        Neg,
        Not,
        Conv_I1,
        Conv_I2,
        Conv_I4,
        Conv_I8,
        Conv_R4,
        Conv_R8,
        Conv_U4,
        Conv_U8,
        Callvirt,
        Cpobj,
        Ldobj,
        Ldstr,
        Newobj,
        Castclass,
        Isinst,
        Conv_R_Un,
        Unbox,
        Throw,
        Ldfld,
        Ldflda,
        Stfld,
        Ldsfld,
        Ldsflda,
        Stsfld,
        Stobj,
        Conv_Ovf_I1_Un,
        Conv_Ovf_I2_Un,
        Conv_Ovf_I4_Un,
        Conv_Ovf_I8_Un,
        Conv_Ovf_U1_Un,
        Conv_Ovf_U2_Un,
        Conv_Ovf_U4_Un,
        Conv_Ovf_U8_Un,
        Conv_Ovf_I_Un,
        Conv_Ovf_U_Un,
        Box,
        Newarr,
        Ldlen,
        Ldelema,
        Ldelem_I1,
        Ldelem_U1,
        Ldelem_I2,
        Ldelem_U2,
        Ldelem_I4,
        Ldelem_U4,
        Ldelem_I8,
        Ldelem_I,
        Ldelem_R4,
        Ldelem_R8,
        Ldelem_Ref,
        Stelem_I,
        Stelem_I1,
        Stelem_I2,
        Stelem_I4,
        Stelem_I8,
        Stelem_R4,
        Stelem_R8,
        Stelem_Ref,
        Ldelem_Any,
        Stelem_Any,
        Unbox_Any,
        Conv_Ovf_I1,
        Conv_Ovf_U1,
        Conv_Ovf_I2,
        Conv_Ovf_U2,
        Conv_Ovf_I4,
        Conv_Ovf_U4,
        Conv_Ovf_I8,
        Conv_Ovf_U8,
        Refanyval,
        Ckfinite,
        Mkrefany,
        Ldtoken,
        Conv_U2,
        Conv_U1,
        Conv_I,
        Conv_Ovf_I,
        Conv_Ovf_U,
        Add_Ovf,
        Add_Ovf_Un,
        Mul_Ovf,
        Mul_Ovf_Un,
        Sub_Ovf,
        Sub_Ovf_Un,
        Endfinally,
        Leave,
        __Leave_S,
        __Stind_I,
        Conv_U,
        Arglist,
        Ceq,
        Cgt,
        Cgt_Un,
        Clt,
        Clt_Un,
        Ldftn,
        Ldvirtftn,
        __Ldarg,
        __Ldarga,
        __Starg,
        Ldloc,
        Ldloca,
        Stloc,
        Localloc,
        Endfilter,
        Unaligned,
        Volatile,
        Tail,
        Initobj,
        Constrained,
        Cpblk,
        Initblk,
        No,
        Rethrow,
        Sizeof,
        Refanytype,
        Readonly,
		
        // Virtual codes - defined for convenience
        Cne,
        Cge,
        Cge_Un,
        Cle,
        Cle_Un,
        CIsNull, // (operand == null) ? 1 : 0
        CIsNotNull, // (operand == null) ? 0 : 1
        Ldexception,  // Operand holds the CatchType for catch handler, null for filter
        NullCoalescing,
        InitArray, // Array Initializer (operand must be of InitArrayData type)
        InitArrayFromArguments, // Initialize array from argument expressions. Operand must be of XArrayType
        InstanceOf, // Operand holds type references

        CompoundAdd, // (x,y) => x += y
        CompoundSub, // (x,y) => x -= y
        CompoundMul, // (x,y) => x *= y
        CompoundDiv, // (x,y) => x /= y
        CompoundRem, // (x,y) => x %= y
        CompoundAnd, // (x,y) => x &= y
        CompoundOr, // (x,y) => x |= y
        CompoundXor, // (x,y) => x ^= y
        CompoundShl, // (x,y) => x <<= y
        CompoundShr, // (x,y) => x >>= y
        CompoundShr_Un, // (x,y) => x >>>= y

        /// <summary>
        /// Defines a barrier between the parent expression and the argument expression that prevents combining them
        /// </summary>
        Wrap,
				
        Delegate,
        TypeOf,
        LoopOrSwitchBreak,
        Ldc_Decimal,
        /// <summary>
        /// Represents the 'default(T)' instruction.
        /// </summary>
        /// <remarks>Introduced by SimplifyLdObjAndStObj step</remarks>
        DefaultValue,
        /// <summary>
        /// AstExpression with a single child: binary operator.
        /// This expression means that the binary operator will also assign the new value to its left-hand side.
        /// 'CompoundAssignment' must not be used for local variables, as inlining (and other) optimizations don't know that it modifies the variable.
        /// </summary>
        /// <remarks>Introduced by MakeCompoundAssignments step</remarks>
        CompoundAssignment,
        /// <summary>
        /// Represents the post-increment operator.
        /// The first argument is the address of the variable to increment (ldloca instruction).
        /// The second arugment is the amount the variable is incremented by (ldc.i4 instruction)
        /// </summary>
        /// <remarks>Introduced by IntroducePostIncrement step</remarks>
        PostIncrement,
        PostIncrement_Ovf, // checked variant of PostIncrement
        PostIncrement_Ovf_Un, // checked variant of PostIncrement, for unsigned integers
        /// <summary>Simulates getting the address of the argument instruction.</summary>
        /// <remarks>
        /// Used for postincrement for properties, and to represent the Address() method on multi-dimensional arrays.
        /// Also used when inlining a method call on a value type: "stloc(v, ...); call(M, ldloca(v));" becomes "call(M, AddressOf(...))"
        /// </remarks>
        AddressOf,
        /// <summary>Simulates getting the value of a lifted operator's nullable argument</summary>
        /// <remarks>
        /// For example "stloc(v1, ...); stloc(v2, ...); logicand(ceq(call(Nullable`1::GetValueOrDefault, ldloca(v1)), ldloc(v2)), callgetter(Nullable`1::get_HasValue, ldloca(v1)))" becomes "wrap(ceq(ValueOf(...), ...))"
        /// </remarks>
        ValueOf,
        /// <summary>Simulates creating a new nullable value from a value type argument</summary>
        /// <remarks>
        /// For example "stloc(v1, ...); stloc(v2, ...); ternaryop(callgetter(Nullable`1::get_HasValue, ldloca(v1)), newobj(Nullable`1::.ctor, add(call(Nullable`1::GetValueOrDefault, ldloca(v1)), ldloc(v2))), defaultvalue(Nullable`1))"
        /// becomes "NullableOf(add(valueof(...), ...))"
        /// </remarks>
        NullableOf,
        /// <summary>
        /// Declares parameters that are used in an expression tree.
        /// The last child of this node is the call constructing the expression tree, all other children are the
        /// assignments to the ParameterExpression variables.
        /// </summary>
        ExpressionTreeParameterDeclarations,

        /// <summary>
        /// Box argument to a byref array
        /// </summary>
        ByRefArray,

        /// <summary>
        /// Box argument to a byref [out only] array
        /// </summary>
        ByRefOutArray,

        // Compare float/double
        // value1, value2 => result
        CmpLFloat,
        CmpGFloat,

        // Compare long
        // value1, value2 => result
        CmpLong,

        // New object (Java type NEW) (Operand is TypeReference)
        // => objectref
        New,

        /// <summary>
        /// Branch for java opcodes IFLT, IFGE, IFGT, IFLE
        /// </summary>
        BrIfEq,
        BrIfNe,
        BrIfLt,
        BrIfGe,
        BrIfGt,
        BrIfLe,

        // Load java class
        LdClass,

        // Key based switch
        LookupSwitch,

        // Java stack operations
        Dup_x1,
        Dup_x2,
        Dup2,
        Dup2_x1,
        Dup2_x2,
        Swap,
        Pop2,

        // Java call interface
        CallIntf,
        // Java invoke-special
        CallSpecial,

        // Convert enum instance to int/long value
        Enum_to_int,
        Enum_to_long,        
        // Convert int/long value to enum instance
        Int_to_enum,
        Long_to_enum,

        // Convert signed int value to ubyte/ushort (after loading from signed storage)
        Int_to_ubyte,
        Int_to_ushort,

        // Load this parameter
        Ldthis,

        // Java multianewarray
        MultiNewarr,

        // Initialize the elements of an new array of structs
        InitStructArray,
        // Initialize the elements of an new array of enum's
        InitEnumArray,

        // Load the value of the GenericInstanceField
        LdGenericInstanceField,

        // Store the value of the GenericInstanceField
        StGenericInstanceField,

        // Load the value of the GenericInstanceTypeArgument
        LdGenericInstanceTypeArgument,

        // Load the value of the GenericInstanceMethodArgument
        LdGenericInstanceMethodArgument,

        // Gets the BoxedType (Type instance) of the given XTypeReference operand
        BoxedTypeOf,

        // Call TypeHelper.ArrayNewInstance(Class, length)
        ArrayNewInstance,

        // Call TypeHelper.ArrayNewInstance2(Class, int[])
        ArrayNewInstance2,

        // arg[0] ? arg[1] : arg[2] (arg[1] and arg[2] are evaluated only when needed)
        Conditional,

        // Simple versions (without a need for further conversion) of codes
        SimpleCastclass,
        SimpleInstanceOf,

        // Unox the argument from a generic value.
        UnboxFromGeneric,

        // Class the ctor of the base class. (int)Operand is the number of parameters.
        CallBaseCtor,
    }
}