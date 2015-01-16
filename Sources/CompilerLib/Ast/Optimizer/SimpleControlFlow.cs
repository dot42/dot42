using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.XModel;

namespace Dot42.CompilerLib.Ast.Optimizer
{
    internal class SimpleControlFlow
	{
        readonly Dictionary<AstLabel, int> labelGlobalRefCount = new Dictionary<AstLabel, int>();
        readonly Dictionary<AstLabel, AstBasicBlock> labelToBasicBlock = new Dictionary<AstLabel, AstBasicBlock>();
		
		DecompilerContext context;
		XTypeSystem typeSystem;
		
		public SimpleControlFlow(DecompilerContext context, AstBlock method)
		{
			this.context = context;
			this.typeSystem = context.CurrentModule.TypeSystem;
			
			foreach(AstLabel target in method.GetSelfAndChildrenRecursive<AstExpression>(e => e.IsBranch()).SelectMany(e => e.GetBranchTargets())) {
				labelGlobalRefCount[target] = labelGlobalRefCount.GetOrDefault(target) + 1;
			}
			foreach(AstBasicBlock bb in method.GetSelfAndChildrenRecursive<AstBasicBlock>()) {
				foreach(AstLabel label in bb.GetChildren().OfType<AstLabel>()) {
					labelToBasicBlock[label] = bb;
				}
			}
		}
		
		public bool SimplifyNullCoalescing(List<AstNode> body, AstBasicBlock head, int pos)
		{
			// ...
			// v = ldloc(leftVar)
			// brtrue(endBBLabel, ldloc(leftVar))
			// br(rightBBLabel)
			//
			// rightBBLabel:
			// v = rightExpr
			// br(endBBLabel)
			// ...
			// =>
			// ...
			// v = NullCoalescing(ldloc(leftVar), rightExpr)
			// br(endBBLabel)
			
			AstVariable v, v2;
			AstExpression leftExpr, leftExpr2;
			AstVariable leftVar;
			AstLabel endBBLabel, endBBLabel2;
			AstLabel rightBBLabel;
			AstBasicBlock rightBB;
			AstExpression rightExpr;
			if (head.Body.Count >= 3 &&
			    head.Body[head.Body.Count - 3].Match(AstCode.Stloc, out v, out leftExpr) &&
			    leftExpr.Match(AstCode.Ldloc, out leftVar) &&
			    head.MatchLastAndBr(AstCode.Brtrue, out endBBLabel, out leftExpr2, out rightBBLabel) &&
			    leftExpr2.MatchLdloc(leftVar) &&
			    labelToBasicBlock.TryGetValue(rightBBLabel, out rightBB) &&
			    rightBB.MatchSingleAndBr(AstCode.Stloc, out v2, out rightExpr, out endBBLabel2) &&
			    v == v2 &&
			    endBBLabel == endBBLabel2 &&
			    labelGlobalRefCount.GetOrDefault(rightBBLabel) == 1 &&
			    body.Contains(rightBB)
			   )
			{
				head.Body.RemoveTail(AstCode.Stloc, AstCode.Brtrue, AstCode.Br);
				head.Body.Add(new AstExpression(leftExpr.SourceLocation, AstCode.Stloc, v, new AstExpression(leftExpr.SourceLocation, AstCode.NullCoalescing, null, leftExpr, rightExpr)));
				head.Body.Add(new AstExpression(leftExpr.SourceLocation, AstCode.Br, endBBLabel));
				
				body.RemoveOrThrow(labelToBasicBlock[rightBBLabel]);
				return true;
			}
			return false;
		}


        public bool JoinBasicBlocks(List<AstNode> body, AstBasicBlock head, int pos)
		{
			AstLabel nextLabel;
			AstBasicBlock nextBB;
			if (!head.Body.ElementAtOrDefault(head.Body.Count - 2).IsConditionalControlFlow() &&
			    head.Body.Last().Match(AstCode.Br, out nextLabel) &&
			    labelGlobalRefCount[nextLabel] == 1 &&
			    labelToBasicBlock.TryGetValue(nextLabel, out nextBB) &&
			    body.Contains(nextBB) &&
			    nextBB.Body.First() == nextLabel &&
			    !nextBB.Body.OfType<AstTryCatchBlock>().Any()
			   )
			{
				head.Body.RemoveTail(AstCode.Br);
				nextBB.Body.RemoveAt(0);  // Remove label
				head.Body.AddRange(nextBB.Body);
				
				body.RemoveOrThrow(nextBB);
				return true;
			}
			return false;
		}
	}
}
