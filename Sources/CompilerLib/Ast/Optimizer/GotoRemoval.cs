using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Dot42.CompilerLib.Ast.Optimizer
{
    internal class GotoRemoval
	{
		private readonly Dictionary<AstNode, AstNode> parent = new Dictionary<AstNode, AstNode>();
		private readonly Dictionary<AstNode, AstNode> nextSibling = new Dictionary<AstNode, AstNode>();
		
		public void RemoveGotos(AstBlock method)
		{
			// Build the navigation data
			parent[method] = null;
			foreach (var node in method.GetSelfAndChildrenRecursive<AstNode>()) {
				AstNode previousChild = null;
				foreach (var child in node.GetChildren()) {
					if (parent.ContainsKey(child))
						throw new Exception("The following expression is linked from several locations: " + child);
					parent[child] = node;
					if (previousChild != null)
						nextSibling[previousChild] = child;
					previousChild = child;
				}
				if (previousChild != null)
					nextSibling[previousChild] = null;
			}
			
			// Simplify gotos
			bool modified;
			do {
				modified = false;
				foreach (var gotoExpr in method.GetSelfAndChildrenRecursive<AstExpression>(e => e.Code == AstCode.Br || e.Code == AstCode.Leave)) {
					modified |= TrySimplifyGoto(gotoExpr);
				}
			} while(modified);
			
			RemoveRedundantCode(method);
		}
		
		public static void RemoveRedundantCode(AstBlock method)
		{
			// Remove dead lables and nops
			HashSet<AstLabel> liveLabels = new HashSet<AstLabel>(method.GetSelfAndChildrenRecursive<AstExpression>(e => e.IsBranch()).SelectMany(e => e.GetBranchTargets()));
			foreach(AstBlock block in method.GetSelfAndChildrenRecursive<AstBlock>()) {
				block.Body = block.Body.Where(n => !n.Match(AstCode.Nop) && !(n is AstLabel && !liveLabels.Contains((AstLabel)n))).ToList();
			}
				
			// Remove redundant break at the end of case
			// Remove redundant case blocks altogether
			foreach(AstSwitch ilSwitch in method.GetSelfAndChildrenRecursive<AstSwitch>()) {
				foreach(AstBlock ilCase in ilSwitch.CaseBlocks) {
					Debug.Assert(ilCase.EntryGoto == null);
					
					int count = ilCase.Body.Count;
					if (count >= 2) {
						if (ilCase.Body[count - 2].IsUnconditionalControlFlow() &&
						    ilCase.Body[count - 1].Match(AstCode.LoopOrSwitchBreak)) 
						{
							ilCase.Body.RemoveAt(count - 1);
						}
					}
				}
				
				var defaultCase = ilSwitch.CaseBlocks.SingleOrDefault(cb => cb.Values == null);
				// If there is no default block, remove empty case blocks
				if (defaultCase == null || (defaultCase.Body.Count == 1 && defaultCase.Body.Single().Match(AstCode.LoopOrSwitchBreak))) {
					ilSwitch.CaseBlocks.RemoveAll(b => b.Body.Count == 1 && b.Body.Single().Match(AstCode.LoopOrSwitchBreak));
				}
			}
			
			// Remove redundant return at the end of method
			if (method.Body.Count > 0 && method.Body.Last().Match(AstCode.Ret) && ((AstExpression)method.Body.Last()).Arguments.Count == 0) {
				method.Body.RemoveAt(method.Body.Count - 1);
			}
			
			// Remove unreachable return statements
			bool modified = false;
			foreach(AstBlock block in method.GetSelfAndChildrenRecursive<AstBlock>()) {
				for (int i = 0; i < block.Body.Count - 1;) {
					if (block.Body[i].IsUnconditionalControlFlow() && block.Body[i+1].Match(AstCode.Ret)) {
						modified = true;
						block.Body.RemoveAt(i+1);
					} else {
						i++;
					}
				}
			}
			if (modified) {
				// More removals might be possible
				new GotoRemoval().RemoveGotos(method);
			}
		}
		
		IEnumerable<AstNode> GetParents(AstNode node)
		{
			var current = node;
			while(true) {
				current = parent[current];
				if (current == null)
					yield break;
				yield return current;
			}
		}
		
		bool TrySimplifyGoto(AstExpression gotoExpr)
		{
			Debug.Assert(gotoExpr.Code == AstCode.Br || gotoExpr.Code == AstCode.Leave);
			Debug.Assert(gotoExpr.Prefixes == null);
			Debug.Assert(gotoExpr.Operand != null);
			
			AstNode target = Enter(gotoExpr, new HashSet<AstNode>());
			if (target == null)
				return false;
			
			// The gotoExper is marked as visited because we do not want to
			// walk over node which we plan to modify
			
			// The simulated path always has to start in the same try-block
			// in other for the same finally blocks to be executed.
			
			if (target == Exit(gotoExpr, new HashSet<AstNode>() { gotoExpr })) {
				gotoExpr.Code = AstCode.Nop;
				gotoExpr.Operand = null;
				if (target is AstExpression)
					((AstExpression)target).ILRanges.AddRange(gotoExpr.ILRanges);
				gotoExpr.ILRanges.Clear();
				return true;
			}
			
			AstNode breakBlock = GetParents(gotoExpr).FirstOrDefault(n => n is AstSwitch);
			if (breakBlock != null && target == Exit(breakBlock, new HashSet<AstNode>() { gotoExpr })) {
				gotoExpr.Code = AstCode.LoopOrSwitchBreak;
				gotoExpr.Operand = null;
				return true;
			}
					
			return false;
		}
		
		/// <summary>
		/// Get the first expression to be excecuted if the instruction pointer is at the start of the given node.
		/// Try blocks may not be entered in any way.  If possible, the try block is returned as the node to be executed.
		/// </summary>
		AstNode Enter(AstNode node, HashSet<AstNode> visitedNodes)
		{
			if (node == null)
				throw new ArgumentNullException();
			
			if (!visitedNodes.Add(node))
				return null;  // Infinite loop
			
			AstLabel label = node as AstLabel;
			if (label != null) {
				return Exit(label, visitedNodes);
			}
			
			AstExpression expr = node as AstExpression;
			if (expr != null) {
				if (expr.Code == AstCode.Br || expr.Code == AstCode.Leave) {
					AstLabel target = (AstLabel)expr.Operand;
					// Early exit - same try-block
					if (GetParents(expr).OfType<AstTryCatchBlock>().FirstOrDefault() == GetParents(target).OfType<AstTryCatchBlock>().FirstOrDefault())
						return Enter(target, visitedNodes);
					// Make sure we are not entering any try-block
					var srcTryBlocks = GetParents(expr).OfType<AstTryCatchBlock>().Reverse().ToList();
					var dstTryBlocks = GetParents(target).OfType<AstTryCatchBlock>().Reverse().ToList();
					// Skip blocks that we are already in
					int i = 0;
					while(i < srcTryBlocks.Count && i < dstTryBlocks.Count && srcTryBlocks[i] == dstTryBlocks[i]) i++;
					if (i == dstTryBlocks.Count) {
						return Enter(target, visitedNodes);
					} else {
						AstTryCatchBlock dstTryBlock = dstTryBlocks[i];
						// Check that the goto points to the start
						AstTryCatchBlock current = dstTryBlock;
						while(current != null) {
							foreach(AstNode n in current.TryBlock.Body) {
								if (n is AstLabel) {
									if (n == target)
										return dstTryBlock;
								} else if (!n.Match(AstCode.Nop)) {
									current = n as AstTryCatchBlock;
									break;
								}
							}
						}
						return null;
					}
				} else if (expr.Code == AstCode.Nop) {
					return Exit(expr, visitedNodes);
				} else if (expr.Code == AstCode.LoopOrSwitchBreak) {
					AstNode breakBlock = GetParents(expr).First(n => n is AstSwitch);
					return Exit(breakBlock, new HashSet<AstNode>() { expr });
				} else {
					return expr;
				}
			}
			
			AstBlock block = node as AstBlock;
			if (block != null) {
				if (block.EntryGoto != null) {
					return Enter(block.EntryGoto, visitedNodes);
				} else if (block.Body.Count > 0) {
					return Enter(block.Body[0], visitedNodes);
				} else {
					return Exit(block, visitedNodes);
				}
			}
					
			AstTryCatchBlock tryCatch = node as AstTryCatchBlock;
			if (tryCatch != null) {
				return tryCatch;
			}
			
			AstSwitch astSwitch = node as AstSwitch;
			if (astSwitch != null) {
				return astSwitch.Condition;
			}
			
			throw new NotSupportedException(node.GetType().ToString());
		}
		
		/// <summary>
		/// Get the first expression to be excecuted if the instruction pointer is at the end of the given node
		/// </summary>
		AstNode Exit(AstNode node, HashSet<AstNode> visitedNodes)
		{
			if (node == null)
				throw new ArgumentNullException();
			
			AstNode nodeParent = parent[node];
			if (nodeParent == null)
				return null;  // Exited main body
			
			if (nodeParent is AstBlock) {
				AstNode nextNode = nextSibling[node];
				if (nextNode != null) {
					return Enter(nextNode, visitedNodes);
				} else {
					return Exit(nodeParent, visitedNodes);
				}
			}
					
			if (nodeParent is AstTryCatchBlock) {
				// Finally blocks are completely ignored.
				// We rely on the fact that try blocks can not be entered.
				return Exit(nodeParent, visitedNodes);
			}
			
			if (nodeParent is AstSwitch) {
				return null;  // Implicit exit from switch is not allowed
			}
					
			throw new NotSupportedException(nodeParent.GetType().ToString());
		}
	}
}
