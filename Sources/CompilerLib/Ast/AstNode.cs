using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dot42.Utility;

namespace Dot42.CompilerLib.Ast
{
    [DebuggerDisplay("{@ToString()}")]
    public abstract class AstNode
    {
        public static readonly ISourceLocation NoSource = null;

        private readonly ISourceLocation sourceLocation;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected AstNode(ISourceLocation sourceLocation)
        {
            this.sourceLocation = sourceLocation;
        }

        /// <summary>
        /// Source code reference
        /// </summary>
        public ISourceLocation SourceLocation { get { return sourceLocation; } }

        /// <summary>
        /// Return the set of this and all (recursive) children of type T that match the given predicate.
        /// </summary>
		public IEnumerable<T> GetSelfAndChildrenRecursive<T>(Func<T, bool> predicate = null)
            where T : AstNode
		{
			var result = new List<T>(16);
            if (predicate != null)
			    AccumulateSelfAndChildrenRecursive(result, predicate);
            else
                AccumulateSelfAndChildrenRecursive(result);
            return result;
		}

        /// <summary>
        /// Return the set of this and all (recursive) children of type AstExpression that match the given predicate.
        /// </summary>
        public IEnumerable<AstExpression> GetExpressions(Func<AstExpression, bool> predicate = null)
        {
            var result = new List<AstExpression>(16);
            if (predicate != null)
                AccumulateSelfAndChildrenRecursive(result, predicate);
            else
                AccumulateSelfAndChildrenRecursive(result);
            return result;
        }

        /// <summary>
        /// Return the set of this and all (recursive) children of type AstExpression that match the given code.
        /// </summary>
        public IEnumerable<AstExpression> GetExpressions(AstCode code)
        {
            return GetExpressions(x => x.Code == code);
        }

        /// <summary>
        /// Return the set of this and all (recursive) children-parent pairs of type AstExpression that match the given predicate.
        /// </summary>
        public IEnumerable<AstExpressionPair> GetExpressionPairs(Func<AstExpression, bool> predicate = null)
        {
            var result = new List<AstExpressionPair>(16);
            if (predicate != null)
                AccumulateExpressions(result, null, predicate);
            else
                AccumulateExpressions(result, null);
            return result;
        }

        /// <summary>
        /// Fill the list with all items in the set of this and all (recursive) children of type T that match the given predicate.
        /// </summary>
        private void AccumulateSelfAndChildrenRecursive<T>(List<T> list, Func<T, bool> predicate) 
            where T : AstNode
		{
			// Note: RemoveEndFinally depends on self coming before children
			var thisAsT = this as T;
			if ((thisAsT != null) && predicate(thisAsT))
				list.Add(thisAsT);
			foreach (var node in GetChildren())
			{
                if (node != null)
    			    node.AccumulateSelfAndChildrenRecursive(list, predicate);
			}
		}

        /// <summary>
        /// Fill the list with all items in the set of this and all (recursive) children of type T.
        /// </summary>
        private void AccumulateSelfAndChildrenRecursive<T>(List<T> list)
            where T : AstNode
        {
            // Note: RemoveEndFinally depends on self coming before children
            var thisAsT = this as T;
            if (thisAsT != null) list.Add(thisAsT);
            foreach (var node in GetChildren())
            {
                if (node != null) 
                    node.AccumulateSelfAndChildrenRecursive(list);
            }
        }

        /// <summary>
        /// Fill the list with all items in the set of this and all (recursive) children of type T that match the given predicate.
        /// </summary>
        private void AccumulateExpressions(List<AstExpressionPair> list, AstExpression parent, Func<AstExpression, bool> predicate)
        {
            // Note: RemoveEndFinally depends on self coming before children
            var thisAsT = this as AstExpression;
            if ((thisAsT != null) && predicate(thisAsT))
                list.Add(new AstExpressionPair(thisAsT, parent));
            foreach (var node in GetChildren())
            {
                if (node != null)
                    node.AccumulateExpressions(list, thisAsT, predicate);
            }
        }

        /// <summary>
        /// Fill the list with all items in the set of this and all (recursive) children of type AstExpression.
        /// </summary>
        private void AccumulateExpressions(List<AstExpressionPair> list, AstExpression parent)
        {
            var thisAsT = this as AstExpression;
            if (thisAsT != null) list.Add(new AstExpressionPair(thisAsT, parent));
            foreach (var node in GetChildren())
            {
                if (node != null)
                    node.AccumulateExpressions(list, thisAsT);
            }
        }

        /// <summary>
        /// Gets all direct child nodes.
        /// </summary>
		public virtual IEnumerable<AstNode> GetChildren()
		{
			yield break;
		}

		/// <summary>
		/// Convert to human readable string
		/// </summary>
		public override string ToString()
		{
			var w = new StringWriter();
			WriteTo(new PlainTextOutput(w));
			return w.ToString().Replace("\r\n", "; ");
		}

		/// <summary>
		/// Write human readable output.
		/// </summary>
		public abstract void WriteTo(ITextOutput output);

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public abstract TReturn Accept<TReturn, TData>(AstNodeVisitor<TReturn, TData> visitor, TData data);

        /// <summary>
        /// Remove all results
        /// </summary>
        public void CleanResults()
        {
            GetSelfAndChildrenRecursive<AstExpression>().ForEach(x => x.Result = null);
        }
	}

    /// <summary>
    /// AST Extension methods
    /// </summary>
    public static class AstNodeExtensions
    {
        /// <summary>
        /// Accept a visit by the given visitor.
        /// Returns default(TReturn) if node is null.
        /// </summary>
        public static TReturn AcceptOrDefault<TReturn, TData>( this AstNode node, AstNodeVisitor<TReturn, TData> visitor, TData data)
        {
            if (node == null)
                return default(TReturn);
            return node.Accept(visitor, data);
        }
    }
}