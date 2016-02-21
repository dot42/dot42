using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Optimizer
{
    public partial class AstOptimizer
    {
        #region TransformDecimalCtorToConstant
        static bool TransformDecimalCtorToConstant(List<AstNode> body, AstExpression expr, int pos)
        {
            XMethodReference r;
            List<AstExpression> args;
            if (expr.Match(AstCode.Newobj, out r, out args) && r.DeclaringType.IsSystemDecimal())
            {
                if (args.Count == 1)
                {
                    // redirect non-resolvable / clashing constructor calls
                    var method = ((XMethodReference) expr.Operand).Resolve();
                    var parameterType = method.Parameters[0].ParameterType;
                    if (parameterType.IsUInt32() || parameterType.IsUInt64())
                    {
                        expr.Code = AstCode.Call;
                        expr.Operand = r.DeclaringType.Resolve().Methods.Single(
                            p => p.Name.StartsWith("op_Implicit") // startsWith as the method has been renamed...
                              && p.IsStatic 
                              && p.Parameters.Count == 1 
                              && p.Parameters[0].ParameterType.IsSame(parameterType));
                        expr.InferredType = r.DeclaringType;
                    }
                }
                else if (args.Count == 5)
                {
                    int lo, mid, hi, isNegative, scale;
                    if (expr.Arguments[0].Match(AstCode.Ldc_I4, out lo) &&
                        expr.Arguments[1].Match(AstCode.Ldc_I4, out mid) &&
                        expr.Arguments[2].Match(AstCode.Ldc_I4, out hi) &&
                        expr.Arguments[3].Match(AstCode.Ldc_I4, out isNegative) &&
                        expr.Arguments[4].Match(AstCode.Ldc_I4, out scale))
                    {
                        // convert to a static string conversion method.
                        var dec = new decimal(lo, mid, hi, isNegative != 0, (byte)scale);
                        var str = dec.ToString(CultureInfo.InvariantCulture);

                        expr.Code = AstCode.Call;
                        expr.Operand = r.DeclaringType.Resolve().Methods.Single(p => p.Name == "FromInvariantString" && p.IsStatic && p.Parameters.Count == 1);
                        expr.Arguments.Clear();
                        expr.Arguments.Add(new AstExpression(expr.SourceLocation, AstCode.Ldstr, str));
                        expr.InferredType = r.DeclaringType;
                        return true;
                    }
                }
            }
            bool modified = false;
            foreach (AstExpression arg in expr.Arguments)
            {
                modified |= TransformDecimalCtorToConstant(null, arg, -1);
            }
            return modified;
        }
        #endregion

        #region SimplifyLdObjAndStObj
        static bool SimplifyLdObjAndStObj(List<AstNode> body, AstExpression expr, int pos)
        {
            bool modified = false;
            expr = SimplifyLdObjAndStObj(expr, ref modified);
            if (modified && body != null)
                body[pos] = expr;
            for (int i = 0; i < expr.Arguments.Count; i++)
            {
                expr.Arguments[i] = SimplifyLdObjAndStObj(expr.Arguments[i], ref modified);
                modified |= SimplifyLdObjAndStObj(null, expr.Arguments[i], -1);
            }
            return modified;
        }

        static AstExpression SimplifyLdObjAndStObj(AstExpression expr, ref bool modified)
        {
            if (expr.Code == AstCode.Initobj)
            {
                expr.Code = AstCode.Stobj;
                expr.Arguments.Add(new AstExpression(expr.SourceLocation, AstCode.DefaultValue, expr.Operand));
                modified = true;
            }
            else if (expr.Code == AstCode.Cpobj)
            {
                expr.Code = AstCode.Stobj;
                expr.Arguments[1] = new AstExpression(expr.SourceLocation, AstCode.Ldobj, expr.Operand, expr.Arguments[1]);
                modified = true;
            }
            AstExpression arg, arg2;
            XTypeReference type;
            AstCode? newCode = null;
            if (expr.Match(AstCode.Stobj, out type, out arg, out arg2))
            {
                switch (arg.Code)
                {
                    case AstCode.Ldelema: newCode = AstCode.Stelem_Any; break;
                    case AstCode.Ldloca: newCode = AstCode.Stloc; break;
                    case AstCode.Ldflda: newCode = AstCode.Stfld; break;
                    case AstCode.Ldsflda: newCode = AstCode.Stsfld; break;
                }
            }
            else if (expr.Match(AstCode.Ldobj, out type, out arg))
            {
                switch (arg.Code)
                {
                    case AstCode.Ldelema: newCode = AstCode.Ldelem_Any; break;
                    case AstCode.Ldloca: newCode = AstCode.Ldloc; break;
                    case AstCode.Ldflda: newCode = AstCode.Ldfld; break;
                    case AstCode.Ldsflda: newCode = AstCode.Ldsfld; break;
                }
            }
            if (newCode != null)
            {
                arg.Code = newCode.Value;
                if (expr.Code == AstCode.Stobj)
                {
                    arg.InferredType = expr.InferredType;
                    arg.ExpectedType = expr.ExpectedType;
                    arg.Arguments.Add(arg2);
                }
                arg.ILRanges.AddRange(expr.ILRanges);
                modified = true;
                return arg;
            }
            else
            {
                return expr;
            }
        }
        #endregion

        #region SimplifyLdcI4ConvI8
        static bool SimplifyLdcI4ConvI8(List<AstNode> body, AstExpression expr, int pos)
        {
            AstExpression ldc;
            int val;
            if (expr.Match(AstCode.Conv_I8, out ldc) && ldc.Match(AstCode.Ldc_I4, out val))
            {
                expr.Code = AstCode.Ldc_I8;
                expr.Operand = (long)val;
                expr.Arguments.Clear();
                return true;
            }
            bool modified = false;
            foreach (AstExpression arg in expr.Arguments)
            {
                modified |= SimplifyLdcI4ConvI8(null, arg, -1);
            }
            return modified;
        }
        #endregion

        #region MakeAssignmentExpression
        bool MakeAssignmentExpression(List<AstNode> body, AstExpression expr, int pos)
        {
            // exprVar = ...
            // stloc(v, exprVar)
            // ->
            // exprVar = stloc(v, ...))
            AstVariable exprVar;
            AstExpression initializer;
            if (!(expr.Match(AstCode.Stloc, out exprVar, out initializer) && exprVar.IsGenerated))
                return false;
            var nextExpr = body.ElementAtOrDefault(pos + 1) as AstExpression;
            AstVariable v;
            AstExpression stLocArg;
            if (nextExpr.Match(AstCode.Stloc, out v, out stLocArg) && stLocArg.MatchLdloc(exprVar))
            {
                var store2 = body.ElementAtOrDefault(pos + 2) as AstExpression;
                if (StoreCanBeConvertedToAssignment(store2, exprVar))
                {
                    // expr_44 = ...
                    // stloc(v1, expr_44)
                    // anystore(v2, expr_44)
                    // ->
                    // stloc(v1, anystore(v2, ...))
                    var inlining = new AstInlining(method);
                    if (inlining.numLdloc.GetOrDefault(exprVar) == 2 && inlining.numStloc.GetOrDefault(exprVar) == 1)
                    {
                        body.RemoveAt(pos + 2); // remove store2
                        body.RemoveAt(pos); // remove expr = ...
                        nextExpr.Arguments[0] = store2;
                        store2.Arguments[store2.Arguments.Count - 1] = initializer;

                        inlining.InlineIfPossible(body, ref pos);

                        return true;
                    }
                }

                body.RemoveAt(pos + 1); // remove stloc
                nextExpr.Arguments[0] = initializer;
                ((AstExpression)body[pos]).Arguments[0] = nextExpr;
                return true;
            }
            
            if ((nextExpr != null) && (nextExpr.Code == AstCode.Stsfld) && nextExpr.Arguments.Count == 1)
            {
                // exprVar = ...
                // stsfld(fld, exprVar)
                // ->
                // exprVar = stsfld(fld, ...))
                if (nextExpr.Arguments[0].MatchLdloc(exprVar))
                {
                    body.RemoveAt(pos + 1); // remove stsfld
                    nextExpr.Arguments[0] = initializer;
                    ((AstExpression)body[pos]).Arguments[0] = nextExpr;
                    return true;
                }
            }
            return false;
        }

        bool StoreCanBeConvertedToAssignment(AstExpression store, AstVariable exprVar)
        {
            if (store == null)
                return false;
            switch (store.Code)
            {
                case AstCode.Stloc:
                case AstCode.Stfld:
                case AstCode.Stsfld:
                case AstCode.Stobj:
                    break;
                default:
                    if (!store.Code.IsStoreToArray())
                        return false;
                    break;
            }
            return store.Arguments.Last().Code == AstCode.Ldloc && store.Arguments.Last().Operand == exprVar;
        }
        #endregion

        #region MakeCompoundAssignments
        bool MakeCompoundAssignments(List<AstNode> body, AstExpression expr, int pos)
        {
            bool modified = false;
            modified |= MakeCompoundAssignment(expr);
            // Static fields and local variables are not handled here - those are expressions without side effects
            // and get handled by ReplaceMethodCallsWithOperators
            // (which does a reversible transform to the short operator form, as the introduction of checked/unchecked might have to revert to the long form).
            foreach (AstExpression arg in expr.Arguments)
            {
                modified |= MakeCompoundAssignments(null, arg, -1);
            }
            if (modified && body != null)
                new AstInlining(method).InlineInto(body, pos, aggressive: false);
            return modified;
        }

        bool MakeCompoundAssignment(AstExpression expr)
        {
            // stelem.any(T, ldloc(array), ldloc(pos), <OP>(ldelem.any(T, ldloc(array), ldloc(pos)), <RIGHT>))
            // or
            // stobj(T, ldloc(ptr), <OP>(ldobj(T, ldloc(ptr)), <RIGHT>))
            AstCode expectedLdelemCode;
            switch (expr.Code)
            {
                case AstCode.Stelem_Any:
                    expectedLdelemCode = AstCode.Ldelem_Any;
                    break;
                case AstCode.Stfld:
                    expectedLdelemCode = AstCode.Ldfld;
                    break;
                case AstCode.Stobj:
                    expectedLdelemCode = AstCode.Ldobj;
                    break;
                default:
                    return false;
            }

            // all arguments except the last (so either array+pos, or ptr):
            bool hasGeneratedVar = false;
            for (int i = 0; i < expr.Arguments.Count - 1; i++)
            {
                AstVariable inputVar;
                if (!expr.Arguments[i].Match(AstCode.Ldloc, out inputVar))
                    return false;
                hasGeneratedVar |= inputVar.IsGenerated;
            }
            // At least one of the variables must be generated; otherwise we just keep the expanded form.
            // We do this because we want compound assignments to be represented in ILAst only when strictly necessary;
            // other compound assignments will be introduced by ReplaceMethodCallsWithOperator
            // (which uses a reversible transformation, see ReplaceMethodCallsWithOperator.RestoreOriginalAssignOperatorAnnotation)
            if (!hasGeneratedVar)
                return false;

            AstExpression op = expr.Arguments.Last();
            // in case of compound assignments with a lifted operator the result is inside NullableOf and the operand is inside ValueOf
            bool liftedOperator = false;
            if (op.Code == AstCode.NullableOf)
            {
                op = op.Arguments[0];
                liftedOperator = true;
            }
            if (!CanBeRepresentedAsCompoundAssignment(op))
                return false;

            AstExpression ldelem = op.Arguments[0];
            if (liftedOperator)
            {
                if (ldelem.Code != AstCode.ValueOf)
                    return false;
                ldelem = ldelem.Arguments[0];
            }
            if (ldelem.Code != expectedLdelemCode)
                return false;
            Debug.Assert(ldelem.Arguments.Count == expr.Arguments.Count - 1);
            for (int i = 0; i < ldelem.Arguments.Count; i++)
            {
                if (!ldelem.Arguments[i].MatchLdloc((AstVariable)expr.Arguments[i].Operand))
                    return false;
            }
            expr.Code = AstCode.CompoundAssignment;
            expr.Operand = null;
            expr.Arguments.RemoveRange(0, ldelem.Arguments.Count);
            // result is "CompoundAssignment(<OP>(ldelem.any(...), <RIGHT>))"
            return true;
        }

        static bool CanBeRepresentedAsCompoundAssignment(AstExpression expr)
        {
            switch (expr.Code)
            {
                case AstCode.Add:
                case AstCode.Add_Ovf:
                case AstCode.Add_Ovf_Un:
                case AstCode.Sub:
                case AstCode.Sub_Ovf:
                case AstCode.Sub_Ovf_Un:
                case AstCode.Mul:
                case AstCode.Mul_Ovf:
                case AstCode.Mul_Ovf_Un:
                case AstCode.Div:
                case AstCode.Div_Un:
                case AstCode.Rem:
                case AstCode.Rem_Un:
                case AstCode.And:
                case AstCode.Or:
                case AstCode.Xor:
                case AstCode.Shl:
                case AstCode.Shr:
                case AstCode.Shr_Un:
                    return true;
                case AstCode.Call:
                    var m = expr.Operand as XMethodReference;
                    if (m == null || m.HasThis || expr.Arguments.Count != 2) return false;
                    switch (m.Name)
                    {
                        case "op_Addition":
                        case "op_Subtraction":
                        case "op_Multiply":
                        case "op_Division":
                        case "op_Modulus":
                        case "op_BitwiseAnd":
                        case "op_BitwiseOr":
                        case "op_ExclusiveOr":
                        case "op_LeftShift":
                        case "op_RightShift":
                            return true;
                        default:
                            return false;
                    }
                default:
                    return false;
            }
        }
        #endregion

        #region IntroducePostIncrement

#if POSTINCREMENT
		bool IntroducePostIncrement(List<AstNode> body, AstExpression expr, int pos)
		{
			bool modified = IntroducePostIncrementForVariables(body, expr, pos);
			Debug.Assert(body[pos] == expr); // IntroducePostIncrementForVariables shouldn't change the expression reference
			AstExpression newExpr = IntroducePostIncrementForInstanceFields(expr);
			if (newExpr != null) {
				modified = true;
				body[pos] = newExpr;
				new AstInlining(method).InlineIfPossible(body, ref pos);
			}
			return modified;
		}

		bool IntroducePostIncrementForVariables(List<AstNode> body, AstExpression expr, int pos)
		{
			// Works for variables and static fields/properties
			
			// expr = ldloc(i)
			// stloc(i, add(expr, ldc.i4(1)))
			// ->
			// expr = postincrement(1, ldloca(i))
			AstVariable exprVar;
			AstExpression exprInit;
			if (!(expr.Match(AstCode.Stloc, out exprVar, out exprInit) && exprVar.IsGenerated))
				return false;
			
			//The next expression
			AstExpression nextExpr = body.ElementAtOrDefault(pos + 1) as AstExpression;
			if (nextExpr == null)
				return false;
			
			AstCode loadInstruction = exprInit.Code;
			AstCode storeInstruction = nextExpr.Code;
			bool recombineVariable = false;
			
			// We only recognise local variables, static fields, and static getters with no arguments
			switch (loadInstruction) {
				case AstCode.Ldloc:
					//Must be a matching store type
					if (storeInstruction != AstCode.Stloc)
						return false;
					AstVariable loadVar = (AstVariable)exprInit.Operand;
					AstVariable storeVar = (AstVariable)nextExpr.Operand;
					if (loadVar != storeVar) {
						if (loadVar.OriginalVariable != null && loadVar.OriginalVariable == storeVar.OriginalVariable)
							recombineVariable = true;
						else
							return false;
					}
					break;
				case AstCode.Ldsfld:
					if (storeInstruction != AstCode.Stsfld)
						return false;
					if (exprInit.Operand != nextExpr.Operand)
						return false;
					break;
				default:
					return false;
			}
			
			AstExpression addExpr = nextExpr.Arguments[0];
			
			int incrementAmount;
			AstCode incrementCode = GetIncrementCode(addExpr, out incrementAmount);
			if (!(incrementAmount != 0 && addExpr.Arguments[0].MatchLdloc(exprVar)))
				return false;
			
			if (recombineVariable) {
				// Split local variable, unsplit these two instances
				// replace nextExpr.Operand with exprInit.Operand
				ReplaceVariables(method, oldVar => oldVar == nextExpr.Operand ? (AstVariable)exprInit.Operand : oldVar);
			}
			
			switch (loadInstruction) {
				case AstCode.Ldloc:
					exprInit.Code = AstCode.Ldloca;
					break;
				case AstCode.Ldsfld:
					exprInit.Code = AstCode.Ldsflda;
					break;
			}
			expr.Arguments[0] = new AstExpression(incrementCode, incrementAmount, exprInit);
			body.RemoveAt(pos + 1); // TODO ILRanges
			return true;
		}
		
		static bool IsGetterSetterPair(object getterOperand, object setterOperand)
		{
			MethodReference getter = getterOperand as MethodReference;
			MethodReference setter = setterOperand as MethodReference;
			if (getter == null || setter == null)
				return false;
			if (!TypeAnalysis.IsSameType(getter.DeclaringType, setter.DeclaringType))
				return false;
			MethodDefinition getterDef = getter.Resolve();
			MethodDefinition setterDef = setter.Resolve();
			if (getterDef == null || setterDef == null)
				return false;
			foreach (PropertyDefinition prop in getterDef.DeclaringType.Properties) {
				if (prop.GetMethod == getterDef)
					return prop.SetMethod == setterDef;
			}
			return false;
		}
		
		AstExpression IntroducePostIncrementForInstanceFields(AstExpression expr)
		{
			// stfld(field, ldloc(instance), add(stloc(helperVar, ldfld(field, ldloc(instance))), ldc.i4(1)))
			// -> stloc(helperVar, postincrement(1, ldflda(field, ldloc(instance))))
			
			// Also works for array elements and pointers:
			
			// stelem.any(T, ldloc(instance), ldloc(pos), add(stloc(helperVar, ldelem.any(T, ldloc(instance), ldloc(pos))), ldc.i4(1)))
			// -> stloc(helperVar, postincrement(1, ldelema(ldloc(instance), ldloc(pos))))
			
			// stobj(T, ldloc(ptr), add(stloc(helperVar, ldobj(T, ldloc(ptr)), ldc.i4(1))))
			// -> stloc(helperVar, postIncrement(1, ldloc(ptr)))
			
			// callsetter(set_P, ldloc(instance), add(stloc(helperVar, callgetter(get_P, ldloc(instance))), ldc.i4(1)))
			// -> stloc(helperVar, postIncrement(1, propertyaddress. callgetter(get_P, ldloc(instance))))
			
			if (!(expr.Code == AstCode.Stfld || expr.Code.IsStoreToArray() || expr.Code == AstCode.Stobj))
				return null;
			
			// Test that all arguments except the last are ldloc (1 arg for fields and pointers, 2 args for arrays)
			for (int i = 0; i < expr.Arguments.Count - 1; i++) {
				if (expr.Arguments[i].Code != AstCode.Ldloc)
					return null;
			}
			
			AstExpression addExpr = expr.Arguments[expr.Arguments.Count - 1];
			int incrementAmount;
			AstCode incrementCode = GetIncrementCode(addExpr, out incrementAmount);
			AstVariable helperVar;
			AstExpression initialValue;
			if (!(incrementAmount != 0 && addExpr.Arguments[0].Match(AstCode.Stloc, out helperVar, out initialValue)))
				return null;
			
			if (expr.Code == AstCode.Stfld) {
				if (initialValue.Code != AstCode.Ldfld)
					return null;
				// There might be two different FieldReference instances, so we compare the field's signatures:
				FieldReference getField = (FieldReference)initialValue.Operand;
				FieldReference setField = (FieldReference)expr.Operand;
				if (!(TypeAnalysis.IsSameType(getField.DeclaringType, setField.DeclaringType)
				      && getField.Name == setField.Name && TypeAnalysis.IsSameType(getField.FieldType, setField.FieldType)))
				{
					return null;
				}
			} else if (expr.Code == AstCode.Stobj) {
				if (!(initialValue.Code == AstCode.Ldobj && initialValue.Operand == expr.Operand))
					return null;
			} else {
				if (!initialValue.Code.IsLoadFromArray())
					return null;
			}
			Debug.Assert(expr.Arguments.Count - 1 == initialValue.Arguments.Count);
			for (int i = 0; i < initialValue.Arguments.Count; i++) {
				if (!initialValue.Arguments[i].MatchLdloc((AstVariable)expr.Arguments[i].Operand))
					return null;
			}
			
			AstExpression stloc = addExpr.Arguments[0];
			if (expr.Code == AstCode.Stobj) {
				stloc.Arguments[0] = new AstExpression(AstCode.PostIncrement, incrementAmount, initialValue.Arguments[0]);
			} else {
				stloc.Arguments[0] = new AstExpression(AstCode.PostIncrement, incrementAmount, initialValue);
				initialValue.Code = (expr.Code == AstCode.Stfld ? AstCode.Ldflda : AstCode.Ldelema);
			}
			// TODO: ILRanges?
			
			return stloc;
		}
		
		AstCode GetIncrementCode(AstExpression addExpr, out int incrementAmount)
		{
			AstCode incrementCode;
			bool decrement = false;
			switch (addExpr.Code) {
				case AstCode.Add:
					incrementCode = AstCode.PostIncrement;
					break;
				case AstCode.Add_Ovf:
					incrementCode = AstCode.PostIncrement_Ovf;
					break;
				case AstCode.Add_Ovf_Un:
					incrementCode = AstCode.PostIncrement_Ovf_Un;
					break;
				case AstCode.Sub:
					incrementCode = AstCode.PostIncrement;
					decrement = true;
					break;
				case AstCode.Sub_Ovf:
					incrementCode = AstCode.PostIncrement_Ovf;
					decrement = true;
					break;
				case AstCode.Sub_Ovf_Un:
					incrementCode = AstCode.PostIncrement_Ovf_Un;
					decrement = true;
					break;
				default:
					incrementAmount = 0;
					return AstCode.Nop;
			}
			if (addExpr.Arguments[1].Match(AstCode.Ldc_I4, out incrementAmount)) {
				if (incrementAmount == -1 || incrementAmount == 1) { // TODO pointer increment?
					if (decrement)
						incrementAmount = -incrementAmount;
					return incrementCode;
				}
			}
			incrementAmount = 0;
			return AstCode.Nop;
		}
#endif // POSTINCREMENT
        #endregion

        #region SimplifyShiftOperators
        static bool SimplifyShiftOperators(List<AstNode> body, AstExpression expr, int pos)
        {
            // C# compiles "a << b" to "a << (b & 31)", so we will remove the "& 31" if possible.
            bool modified = false;
            SimplifyShiftOperators(expr, ref modified);
            return modified;
        }

        static void SimplifyShiftOperators(AstExpression expr, ref bool modified)
        {
            for (int i = 0; i < expr.Arguments.Count; i++)
                SimplifyShiftOperators(expr.Arguments[i], ref modified);
            if (expr.Code != AstCode.Shl && expr.Code != AstCode.Shr && expr.Code != AstCode.Shr_Un)
                return;
            var a = expr.Arguments[1];
            if (a.Code != AstCode.And || a.Arguments[1].Code != AstCode.Ldc_I4 || expr.InferredType == null)
                return;
            int mask;
            switch (expr.InferredType.Kind)
            {
                case XTypeReferenceKind.Int:
                case XTypeReferenceKind.UInt:
                    mask = 31;
                    break;
                case XTypeReferenceKind.Long:
                case XTypeReferenceKind.ULong:
                    mask = 63;
                    break;
                default: return;
            }
            if ((int)a.Arguments[1].Operand != mask) return;
            var res = a.Arguments[0];
            res.ILRanges.AddRange(a.ILRanges);
            res.ILRanges.AddRange(a.Arguments[1].ILRanges);
            expr.Arguments[1] = res;
            modified = true;
        }
        #endregion

        #region InlineExpressionTreeParameterDeclarations
        bool InlineExpressionTreeParameterDeclarations(List<AstNode> body, AstExpression expr, int pos)
        {
            // When there is a Expression.Lambda() call, and the parameters are declared in the
            // IL statement immediately prior to the one containing the Lambda() call,
            // using this code for the3 declaration:
            //   stloc(v, call(Expression::Parameter, call(Type::GetTypeFromHandle, ldtoken(...)), ldstr(...)))
            // and the variables v are assigned only once (in that statements), and read only in a Expression::Lambda
            // call that immediately follows the assignment statements, then we will inline those assignments
            // into the Lambda call using AstCode.ExpressionTreeParameterDeclarations.

            // This is sufficient to allow inlining over the expression tree construction. The remaining translation
            // of expression trees into C# will be performed by a C# AST transformer.

            for (int i = expr.Arguments.Count - 1; i >= 0; i--)
            {
                if (InlineExpressionTreeParameterDeclarations(body, expr.Arguments[i], pos))
                    return true;
            }

            XMethodReference mr;
            AstExpression lambdaBodyExpr, parameterArray;
            if (!(expr.Match(AstCode.Call, out mr, out lambdaBodyExpr, out parameterArray) && mr.Name == "Lambda"))
                return false;
            if (!(parameterArray.Code == AstCode.InitArray && mr.DeclaringType.FullName == "System.Linq.Expressions.Expression"))
                return false;
            int firstParameterPos = pos - parameterArray.Arguments.Count;
            if (firstParameterPos < 0)
                return false;

            AstExpression[] parameterInitExpressions = new AstExpression[parameterArray.Arguments.Count + 1];
            for (int i = 0; i < parameterArray.Arguments.Count; i++)
            {
                parameterInitExpressions[i] = body[firstParameterPos + i] as AstExpression;
                if (!MatchParameterVariableAssignment(parameterInitExpressions[i]))
                    return false;
                AstVariable v = (AstVariable)parameterInitExpressions[i].Operand;
                if (!parameterArray.Arguments[i].MatchLdloc(v))
                    return false;
                // TODO: validate that the variable is only used here and within 'body'
            }

            parameterInitExpressions[parameterInitExpressions.Length - 1] = lambdaBodyExpr;
            Debug.Assert(expr.Arguments[0] == lambdaBodyExpr);
            expr.Arguments[0] = new AstExpression(expr.SourceLocation, AstCode.ExpressionTreeParameterDeclarations, null, parameterInitExpressions);

            body.RemoveRange(firstParameterPos, parameterArray.Arguments.Count);

            return true;
        }

        bool MatchParameterVariableAssignment(AstExpression expr)
        {
            // stloc(v, call(Expression::Parameter, call(Type::GetTypeFromHandle, ldtoken(...)), ldstr(...)))
            AstVariable v;
            AstExpression init;
            if (!expr.Match(AstCode.Stloc, out v, out init))
                return false;
            if (v.IsGenerated || v.IsParameter || v.IsPinned)
                return false;
            if (v.Type == null || v.Type.FullName != "System.Linq.Expressions.ParameterExpression")
                return false;
            XMethodReference parameterMethod;
            AstExpression typeArg, nameArg;
            if (!init.Match(AstCode.Call, out parameterMethod, out typeArg, out nameArg))
                return false;
            if (!(parameterMethod.Name == "Parameter" && parameterMethod.DeclaringType.FullName == "System.Linq.Expressions.Expression"))
                return false;
            XMethodReference getTypeFromHandle;
            AstExpression typeToken;
            if (!typeArg.Match(AstCode.Call, out getTypeFromHandle, out typeToken))
                return false;
            if (!(getTypeFromHandle.Name == "GetTypeFromHandle" && getTypeFromHandle.DeclaringType.FullName == "System.Type"))
                return false;
            return typeToken.Code == AstCode.Ldtoken && nameArg.Code == AstCode.Ldstr;
        }
        #endregion
    }
}
