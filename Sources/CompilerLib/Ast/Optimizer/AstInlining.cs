using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Optimizer
{
	/// <summary>
	/// Performs inlining transformations.
	/// </summary>
	internal class AstInlining
	{
		readonly AstBlock method;
		internal Dictionary<AstVariable, int> numStloc  = new Dictionary<AstVariable, int>();
		internal Dictionary<AstVariable, int> numLdloc  = new Dictionary<AstVariable, int>();
		internal Dictionary<AstVariable, int> numLdloca = new Dictionary<AstVariable, int>();
		
		public AstInlining(AstBlock method)
		{
			this.method = method;
			AnalyzeMethod();
		}
		
		void AnalyzeMethod()
		{
			numStloc.Clear();
			numLdloc.Clear();
			numLdloca.Clear();
			
			// Analyse the whole method
			AnalyzeNode(method);
		}
		
		void AnalyzeNode(AstNode node)
		{
			AstExpression expr = node as AstExpression;
			if (expr != null) {
				AstVariable locVar = expr.Operand as AstVariable;
				if (locVar != null) {
					if (expr.Code == AstCode.Stloc) {
						numStloc[locVar] = numStloc.GetOrDefault(locVar) + 1;
					} else if (expr.Code == AstCode.Ldloc) {
						numLdloc[locVar] = numLdloc.GetOrDefault(locVar) + 1;
					} else if (expr.Code == AstCode.Ldloca) {
						numLdloca[locVar] = numLdloca.GetOrDefault(locVar) + 1;
					} else {
						throw new NotSupportedException(expr.Code.ToString());
					}
				}
				foreach (AstExpression child in expr.Arguments)
					AnalyzeNode(child);
			} else {
				var catchBlock = node as AstTryCatchBlock.CatchBlock;
				if (catchBlock != null && catchBlock.ExceptionVariable != null) {
					numStloc[catchBlock.ExceptionVariable] = numStloc.GetOrDefault(catchBlock.ExceptionVariable) + 1;
				}
				
				foreach (AstNode child in node.GetChildren())
					AnalyzeNode(child);
			}
		}
		
		public bool InlineAllVariables()
		{
			bool modified = false;
			AstInlining i = new AstInlining(method);
			foreach (AstBlock block in method.GetSelfAndChildrenRecursive<AstBlock>())
				modified |= i.InlineAllInBlock(block);
			return modified;
		}
		
		public bool InlineAllInBlock(AstBlock block)
		{
			bool modified = false;
			List<AstNode> body = block.Body;
			if (block is AstTryCatchBlock.CatchBlock && body.Count > 1) {
				AstVariable v = ((AstTryCatchBlock.CatchBlock)block).ExceptionVariable;
				if (v != null && v.IsGenerated) {
					if (numLdloca.GetOrDefault(v) == 0 && numStloc.GetOrDefault(v) == 1 && numLdloc.GetOrDefault(v) == 1) {
						AstVariable v2;
						AstExpression ldException;
						if (body[0].Match(AstCode.Stloc, out v2, out ldException) && ldException.MatchLdloc(v)) {
							body.RemoveAt(0);
							((AstTryCatchBlock.CatchBlock)block).ExceptionVariable = v2;
							modified = true;
						}
					}
				}
			}
			for(int i = 0; i < body.Count - 1;) {
				AstVariable locVar;
				AstExpression expr;
				if (body[i].Match(AstCode.Stloc, out locVar, out expr) && InlineOneIfPossible(block.Body, i, aggressive: false)) {
					modified = true;
					i = Math.Max(0, i - 1); // Go back one step
				} else {
					i++;
				}
			}
			foreach(AstBasicBlock bb in body.OfType<AstBasicBlock>()) {
				modified |= InlineAllInBasicBlock(bb);
			}
			return modified;
		}
		
		public bool InlineAllInBasicBlock(AstBasicBlock bb)
		{
			bool modified = false;
			List<AstNode> body = bb.Body;
			for(int i = 0; i < body.Count;) {
				AstVariable locVar;
				AstExpression expr;
				if (body[i].Match(AstCode.Stloc, out locVar, out expr) && InlineOneIfPossible(bb.Body, i, aggressive: false)) {
					modified = true;
					i = Math.Max(0, i - 1); // Go back one step
				} else {
					i++;
				}
			}
			return modified;
		}
		
		/// <summary>
		/// Inlines instructions before pos into block.Body[pos].
		/// </summary>
		/// <returns>The number of instructions that were inlined.</returns>
		public int InlineInto(List<AstNode> body, int pos, bool aggressive)
		{
			if (pos >= body.Count)
				return 0;
			int count = 0;
			while (--pos >= 0) {
				AstExpression expr = body[pos] as AstExpression;
				if (expr == null || expr.Code != AstCode.Stloc)
					break;
				if (InlineOneIfPossible(body, pos, aggressive))
					count++;
				else
					break;
			}
			return count;
		}
		
		/// <summary>
		/// Aggressively inlines the stloc instruction at block.Body[pos] into the next instruction, if possible.
		/// If inlining was possible; we will continue to inline (non-aggressively) into the the combined instruction.
		/// </summary>
		/// <remarks>
		/// After the operation, pos will point to the new combined instruction.
		/// </remarks>
		public bool InlineIfPossible(List<AstNode> body, ref int pos)
		{
			if (InlineOneIfPossible(body, pos, true)) {
				pos -= InlineInto(body, pos, false);
				return true;
			}
			return false;
		}
		
		/// <summary>
		/// Inlines the stloc instruction at block.Body[pos] into the next instruction, if possible.
		/// </summary>
		public bool InlineOneIfPossible(List<AstNode> body, int pos, bool aggressive)
		{
			AstVariable v;
			AstExpression inlinedExpression;
			if (body[pos].Match(AstCode.Stloc, out v, out inlinedExpression) && !v.IsPinned) {
				if (InlineIfPossible(v, inlinedExpression, body.ElementAtOrDefault(pos+1), aggressive)) {
					// Assign the ranges of the stloc instruction:
					inlinedExpression.ILRanges.AddRange(((AstExpression)body[pos]).ILRanges);
					// Remove the stloc instruction:
					body.RemoveAt(pos);
					return true;
				} else if (numLdloc.GetOrDefault(v) == 0 && numLdloca.GetOrDefault(v) == 0) {
					// The variable is never loaded
					if (inlinedExpression.HasNoSideEffects()) {
						// Remove completely
						body.RemoveAt(pos);
						return true;
					} else if (inlinedExpression.CanBeExpressionStatement() && v.IsGenerated) {
						// Assign the ranges of the stloc instruction:
						inlinedExpression.ILRanges.AddRange(((AstExpression)body[pos]).ILRanges);
						// Remove the stloc, but keep the inner expression
						body[pos] = inlinedExpression;
						return true;
					}
				}
			}
			return false;
		}
		
		/// <summary>
		/// Inlines 'expr' into 'next', if possible.
		/// </summary>
		bool InlineIfPossible(AstVariable v, AstExpression inlinedExpression, AstNode next, bool aggressive)
		{
			// ensure the variable is accessed only a single time
			if (numStloc.GetOrDefault(v) != 1)
				return false;
			int ldloc = numLdloc.GetOrDefault(v);
			if (ldloc > 1 || ldloc + numLdloca.GetOrDefault(v) != 1)
				return false;

		    AstExpression parent;
			int pos;
			if (FindLoadInNext(next as AstExpression, v, inlinedExpression, out parent, out pos) == true) {
				if (ldloc == 0) {
					if (!IsGeneratedValueTypeTemporary((AstExpression)next, parent, pos, v, inlinedExpression))
						return false;
				} else {
					if (!aggressive && !v.IsGenerated && !NonAggressiveInlineInto((AstExpression)next, parent, inlinedExpression))
						return false;
				}
				
				// Assign the ranges of the ldloc instruction:
				inlinedExpression.ILRanges.AddRange(parent.Arguments[pos].ILRanges);
				
				if (ldloc == 0) {
					// it was an ldloca instruction, so we need to use the pseudo-opcode 'addressof' so that the types
					// comes out correctly
					parent.Arguments[pos] = new AstExpression(inlinedExpression.SourceLocation, AstCode.AddressOf, null, inlinedExpression);
				} else {
					parent.Arguments[pos] = inlinedExpression;
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Is this a temporary variable generated by the C# compiler for instance method calls on value type values
		/// </summary>
		/// <param name="next">The next top-level expression</param>
		/// <param name="parent">The direct parent of the load within 'next'</param>
		/// <param name="pos">Index of the load within 'parent'</param>
		/// <param name="v">The variable being inlined.</param>
		/// <param name="inlinedExpression">The expression being inlined</param>
		bool IsGeneratedValueTypeTemporary(AstExpression next, AstExpression parent, int pos, AstVariable v, AstExpression inlinedExpression)
		{
			if (pos == 0 && v.Type != null && v.Type.IsValueType) {
				// Inlining a value type variable is allowed only if the resulting code will maintain the semantics
				// that the method is operating on a copy.
				// Thus, we have to disallow inlining of other locals, fields, array elements, dereferenced pointers
				switch (inlinedExpression.Code) {
					case AstCode.Ldloc:
					case AstCode.Stloc:
					case AstCode.CompoundAssignment:
					case AstCode.Ldelem_Any:
					case AstCode.Ldelem_I:
					case AstCode.Ldelem_I1:
					case AstCode.Ldelem_I2:
					case AstCode.Ldelem_I4:
					case AstCode.Ldelem_I8:
					case AstCode.Ldelem_R4:
					case AstCode.Ldelem_R8:
					case AstCode.Ldelem_Ref:
					case AstCode.Ldelem_U1:
					case AstCode.Ldelem_U2:
					case AstCode.Ldelem_U4:
					case AstCode.Ldobj:
					case AstCode.Ldind_Ref:
						return false;
					case AstCode.Ldfld:
					case AstCode.Stfld:
					case AstCode.Ldsfld:
					case AstCode.Stsfld:
						// allow inlining field access only if it's a readonly field
						var f = ((XFieldReference)inlinedExpression.Operand).Resolve();
						if (!(f != null && f.IsReadOnly))
							return false;
						break;
					case AstCode.Call:
						// inlining runs both before and after IntroducePropertyAccessInstructions,
						// so we have to handle both 'call' and 'callgetter'
						var mr = (XMethodReference)inlinedExpression.Operand;
						// ensure that it's not an multi-dimensional array getter
						if (mr.DeclaringType is XArrayType)
							return false;
						goto case AstCode.Callvirt;
					case AstCode.Callvirt:
                    case AstCode.CallIntf:
                    case AstCode.CallSpecial:
                        // don't inline foreach loop variables:
						mr = (XMethodReference)inlinedExpression.Operand;
						if (mr.Name == "get_Current" && mr.HasThis)
							return false;
						break;
					case AstCode.Castclass:
					case AstCode.Unbox_Any:
						// These are valid, but might occur as part of a foreach loop variable.
						AstExpression arg = inlinedExpression.Arguments[0];
						if (arg.Code == AstCode.Call || arg.Code == AstCode.Callvirt || arg.Code == AstCode.CallIntf || arg.Code == AstCode.CallSpecial) {
							mr = (XMethodReference)arg.Operand;
							if (mr.Name == "get_Current" && mr.HasThis)
								return false; // looks like a foreach loop variable, so don't inline it
						}
						break;
				}
				
				// inline the compiler-generated variable that are used when accessing a member on a value type:
				switch (parent.Code) {
					case AstCode.Call:
					case AstCode.Callvirt:
                    case AstCode.CallIntf:
                    case AstCode.CallSpecial:
                        var mr = (XMethodReference)parent.Operand;
						return mr.HasThis;
					case AstCode.Stfld:
					case AstCode.Ldfld:
					case AstCode.Ldflda:
						return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// Determines whether a variable should be inlined in non-aggressive mode, even though it is not a generated variable.
		/// </summary>
		/// <param name="next">The next top-level expression</param>
		/// <param name="parent">The direct parent of the load within 'next'</param>
		/// <param name="inlinedExpression">The expression being inlined</param>
		bool NonAggressiveInlineInto(AstExpression next, AstExpression parent, AstExpression inlinedExpression)
		{
			if (inlinedExpression.Code == AstCode.DefaultValue)
				return true;
			
			switch (next.Code) {
				case AstCode.Ret:
				case AstCode.Brtrue:
					return parent == next;
				case AstCode.Switch:
                case AstCode.LookupSwitch:
                    return parent == next || (parent.Code == AstCode.Sub && parent == next.Arguments[0]);
				default:
					return false;
			}
		}
		
		/// <summary>
		/// Gets whether 'expressionBeingMoved' can be inlined into 'expr'.
		/// </summary>
		public bool CanInlineInto(AstExpression expr, AstVariable v, AstExpression expressionBeingMoved)
		{
			AstExpression parent;
			int pos;
			return FindLoadInNext(expr, v, expressionBeingMoved, out parent, out pos) == true;
		}
		
		/// <summary>
		/// Finds the position to inline to.
		/// </summary>
		/// <returns>true = found; false = cannot continue search; null = not found</returns>
		bool? FindLoadInNext(AstExpression expr, AstVariable v, AstExpression expressionBeingMoved, out AstExpression parent, out int pos)
		{
			parent = null;
			pos = 0;
			if (expr == null)
				return false;
			for (int i = 0; i < expr.Arguments.Count; i++) {
				// Stop when seeing an opcode that does not guarantee that its operands will be evaluated.
				// Inlining in that case might result in the inlined expresion not being evaluted.
				if (i == 1 && (expr.Code == AstCode.NullCoalescing))
					return false;
				
				AstExpression arg = expr.Arguments[i];
				
				if ((arg.Code == AstCode.Ldloc || arg.Code == AstCode.Ldloca) && arg.Operand == v) {
					parent = expr;
					pos = i;
					return true;
				}
				bool? r = FindLoadInNext(arg, v, expressionBeingMoved, out parent, out pos);
				if (r != null)
					return r;
			}
			if (IsSafeForInlineOver(expr, expressionBeingMoved))
				return null; // continue searching
			else
				return false; // abort, inlining not possible
		}
		
		/// <summary>
		/// Determines whether it is safe to move 'expressionBeingMoved' past 'expr'
		/// </summary>
		bool IsSafeForInlineOver(AstExpression expr, AstExpression expressionBeingMoved)
		{
			switch (expr.Code) {
				case AstCode.Ldloc:
					AstVariable loadedVar = (AstVariable)expr.Operand;
					if (numLdloca.GetOrDefault(loadedVar) != 0) {
						// abort, inlining is not possible
						return false;
					}
					foreach (AstExpression potentialStore in expressionBeingMoved.GetSelfAndChildrenRecursive<AstExpression>()) {
						if (potentialStore.Code == AstCode.Stloc && potentialStore.Operand == loadedVar)
							return false;
					}
					// the expression is loading a non-forbidden variable
					return true;
				case AstCode.Ldloca:
				case AstCode.Ldflda:
				case AstCode.Ldsflda:
				case AstCode.Ldelema:
				case AstCode.AddressOf:
				case AstCode.ValueOf:
				case AstCode.NullableOf:
					// address-loading instructions are safe if their arguments are safe
					foreach (AstExpression arg in expr.Arguments) {
						if (!IsSafeForInlineOver(arg, expressionBeingMoved))
							return false;
					}
					return true;
				default:
					// instructions with no side-effects are safe (except for Ldloc and Ldloca which are handled separately)
					return expr.HasNoSideEffects();
			}
		}
		
		/// <summary>
		/// Runs a very simple form of copy propagation.
		/// Copy propagation is used in two cases:
		/// 1) assignments from arguments to local variables
		///    If the target variable is assigned to only once (so always is that argument) and the argument is never changed (no ldarga/starg),
		///    then we can replace the variable with the argument.
		/// 2) assignments of address-loading instructions to local variables
		/// </summary>
		public void CopyPropagation()
		{
			foreach (AstBlock block in method.GetSelfAndChildrenRecursive<AstBlock>()) {
				for (int i = 0; i < block.Body.Count; i++) {
					AstVariable v;
					AstExpression copiedExpr;
					if (block.Body[i].Match(AstCode.Stloc, out v, out copiedExpr)
					    && !v.IsParameter && numStloc.GetOrDefault(v) == 1 && numLdloca.GetOrDefault(v) == 0
					    && CanPerformCopyPropagation(copiedExpr, v))
					{
						// un-inline the arguments of the ldArg instruction
						var uninlinedArgs = new AstVariable[copiedExpr.Arguments.Count];
						for (int j = 0; j < uninlinedArgs.Length; j++) {
                            uninlinedArgs[j] = new AstGeneratedVariable(v.Name + "_cp_" + j, v.OriginalName);
							block.Body.Insert(i++, new AstExpression(copiedExpr.SourceLocation, AstCode.Stloc, uninlinedArgs[j], copiedExpr.Arguments[j]));
						}
						
						// perform copy propagation:
						foreach (var expr in method.GetSelfAndChildrenRecursive<AstExpression>()) {
							if (expr.Code == AstCode.Ldloc && expr.Operand == v) {
								expr.Code = copiedExpr.Code;
								expr.Operand = copiedExpr.Operand;
								for (int j = 0; j < uninlinedArgs.Length; j++) {
									expr.Arguments.Add(new AstExpression(copiedExpr.SourceLocation, AstCode.Ldloc, uninlinedArgs[j]));
								}
							}
						}
						
						block.Body.RemoveAt(i);
						if (uninlinedArgs.Length > 0) {
							// if we un-inlined stuff; we need to update the usage counters
							AnalyzeMethod();
						}
						InlineInto(block.Body, i, aggressive: false); // maybe inlining gets possible after the removal of block.Body[i]
						i -= uninlinedArgs.Length + 1;
					}
				}
			}
		}
		
		bool CanPerformCopyPropagation(AstExpression expr, AstVariable copyVariable)
		{
			switch (expr.Code) {
				case AstCode.Ldloca:
				case AstCode.Ldelema:
				case AstCode.Ldflda:
				case AstCode.Ldsflda:
					// All address-loading instructions always return the same value for a given operand/argument combination,
					// so they can be safely copied.
					return true;
				case AstCode.Ldloc:
					AstVariable v = (AstVariable)expr.Operand;
					if (v.IsParameter) {
						// Parameters can be copied only if they aren't assigned to (directly or indirectly via ldarga)
						return numLdloca.GetOrDefault(v) == 0 && numStloc.GetOrDefault(v) == 0;
					} else {
						// Variables are be copied only if both they and the target copy variable are generated,
						// and if the variable has only a single assignment
						return v.IsGenerated && copyVariable.IsGenerated && numLdloca.GetOrDefault(v) == 0 && numStloc.GetOrDefault(v) == 1;
					}
				default:
					return false;
			}
		}
	}
}
