// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Dot42.CompilerLib.Ast
{
    /// <summary>
    /// Node pattern matching extension methods
    /// </summary>
    internal static class PatternMatching
    {
        /// <summary>
        /// Match expression with given code.
        /// </summary>
        public static bool Match(this AstNode node, AstCode code)
        {
            var expr = node as AstExpression;
            return (expr != null) && (expr.Prefixes == null) && (expr.Code == code);
        }

        /// <summary>
        /// Match expression with given code and no arguments.
        /// </summary>
        public static bool Match<T>(this AstNode node, AstCode code, out T operand)
        {
            var expr = node as AstExpression;
            if ((expr != null) && (expr.Prefixes == null) && (expr.Code == code) && (expr.Arguments.Count == 0))
            {
                operand = (T)expr.Operand;
                return true;
            }
            operand = default(T);
            return false;
        }

        /// <summary>
        /// Match expression with given code and zero or more arguments.
        /// </summary>
        public static bool Match(this AstNode node, AstCode code, out List<AstExpression> args)
        {
            var expr = node as AstExpression;
            if ((expr != null) && (expr.Prefixes == null) && (expr.Code == code))
            {
                Debug.Assert(expr.Operand == null);
                args = expr.Arguments;
                return true;
            }
            args = null;
            return false;
        }

        /// <summary>
        /// Match expression with given code and one argument.
        /// </summary>
        public static bool Match(this AstNode node, AstCode code, out AstExpression arg)
        {
            List<AstExpression> args;
            if (node.Match(code, out args) && args.Count == 1)
            {
                arg = args[0];
                return true;
            }
            arg = null;
            return false;
        }

        /// <summary>
        /// Match expression with given code and zero or more arguments.
        /// </summary>
        public static bool Match<T>(this AstNode node, AstCode code, out T operand, out List<AstExpression> args)
        {
            var expr = node as AstExpression;
            if ((expr != null) && (expr.Prefixes == null) && (expr.Code == code))
            {
                operand = (T)expr.Operand;
                args = expr.Arguments;
                return true;
            }
            operand = default(T);
            args = null;
            return false;
        }

        /// <summary>
        /// Match expression with given code and one argument.
        /// </summary>
        public static bool Match<T>(this AstNode node, AstCode code, out T operand, out AstExpression arg)
        {
            List<AstExpression> args;
            if (node.Match(code, out operand, out args) && args.Count == 1)
            {
                arg = args[0];
                return true;
            }
            arg = null;
            return false;
        }

        /// <summary>
        /// Match expression with given code and two arguments.
        /// </summary>
        public static bool Match<T>(this AstNode node, AstCode code, out T operand, out AstExpression arg1, out AstExpression arg2)
        {
            List<AstExpression> args;
            if (node.Match(code, out operand, out args) && args.Count == 2)
            {
                arg1 = args[0];
                arg2 = args[1];
                return true;
            }
            arg1 = null;
            arg2 = null;
            return false;
        }

        /// <summary>
        /// Match basic block with one label followed by an expression with code code and one argument.
        /// </summary>
        public static bool MatchSingle<T>(this AstBasicBlock bb, AstCode code, out T operand, out AstExpression arg)
        {
            if (bb.Body.Count == 2 &&
                bb.Body[0] is AstLabel &&
                bb.Body[1].Match(code, out operand, out arg))
            {
                return true;
            }
            operand = default(T);
            arg = null;
            return false;
        }


        /// <summary>
        /// Match basic block with one label followed by an expression with code code and one argument, followed by a branch.
        /// </summary>
        public static bool MatchSingleAndBr<T>(this AstBasicBlock bb, AstCode code, out T operand, out AstExpression arg, out AstLabel brLabel)
        {
            if (bb.Body.Count == 3 &&
                bb.Body[0] is AstLabel &&
                bb.Body[1].Match(code, out operand, out arg) &&
                bb.Body[2].Match(AstCode.Br, out brLabel))
            {
                return true;
            }
            operand = default(T);
            arg = null;
            brLabel = null;
            return false;
        }


        /// <summary>
        /// Match basic block ending with an expression with code code and one argument, followed by a branch.
        /// </summary>
        public static bool MatchLastAndBr<T>(this AstBasicBlock bb, AstCode code, out T operand, out AstExpression arg, out AstLabel brLabel)
        {
            if (bb.Body.ElementAtOrDefault(bb.Body.Count - 2).Match(code, out operand, out arg) &&
                bb.Body.LastOrDefault().Match(AstCode.Br, out brLabel))
            {
                return true;
            }
            operand = default(T);
            arg = null;
            brLabel = null;
            return false;
        }


        /// <summary>
        /// Match load this.
        /// </summary>
        public static bool MatchThis(this AstNode node)
        {
            AstVariable v;
            return node.Match(AstCode.Ldloc, out v) && v.IsThis;
        }


        /// <summary>
        /// Match load given variable.
        /// </summary>
        public static bool MatchLdloc(this AstNode node, AstVariable expectedVar)
        {
            AstVariable v;
            return node.Match(AstCode.Ldloc, out v) && (v == expectedVar);
        }
    }
}
