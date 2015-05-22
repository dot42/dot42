using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.CompilerLib.XModel;
using Dot42.Utility;

namespace Dot42.CompilerLib.Ast
{
    public sealed class AstExpression : AstNode
    {
        public AstCode Code { get; set; }
        public object Operand { get; set; }
        public List<AstExpression> Arguments { get; set; }
        public AstExpressionPrefix[] Prefixes { get; set; }
        // Mapping to the original instructions (useful for debugging)
        public List<InstructionRange> ILRanges { get; private set; }

        /// <summary>
        /// Set by the RCode compiler
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// Used by arguments of byref parameters
        /// </summary>
        public AstExpression StoreByRefExpression { get; set; }

        public XTypeReference ExpectedType { get; set; }
        public XTypeReference InferredType { get; set; }

        /// <summary>
        /// Number of arguments added because of generic instance argument parameters (0..2)
        /// </summary>
        public int GenericInstanceArgCount { get; set; }

        public static readonly object AnyOperand = new object();

        /// <summary>
        /// Copy ctor
        /// </summary>
        public AstExpression(AstExpression source)
            : base(source.SourceLocation)
        {
            CopyFrom(source);
        }

        public AstExpression(ISourceLocation sourceLocation, AstCode code, object operand, List<AstExpression> args)
            : base(sourceLocation)
        {
            if (operand is AstExpression)
                throw new ArgumentException("operand");

            Code = code;
            Operand = operand;
            Arguments = new List<AstExpression>(args);
            ILRanges = new List<InstructionRange>(1);
        }

        public AstExpression(ISourceLocation sourceLocation, AstCode code, object operand, params AstExpression[] args)
            : base(sourceLocation)
        {
            if (operand is AstExpression)
                throw new ArgumentException("operand");

            Code = code;
            Operand = operand;
            Arguments = new List<AstExpression>(args);
            ILRanges = new List<InstructionRange>(1);
        }

        /// <summary>
        /// Copy all setting except source location from source into this.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="useBestSourceLocation">if true, copies the source location if 
        /// the this.SourceLocation is null or IsSpecial</param>
        /// <returns>this</returns>
        public AstExpression CopyFrom(AstExpression source, bool useBestSourceLocation = false)
        {
            Code = source.Code;
            Operand = source.Operand;
            Arguments = (source.Arguments != null) ? new List<AstExpression>(source.Arguments) : null;
            Prefixes = source.Prefixes;
            ILRanges = source.ILRanges;
            Result = source.Result;
            StoreByRefExpression = source.StoreByRefExpression;
            ExpectedType = source.ExpectedType;
            InferredType = source.InferredType;

            if (useBestSourceLocation)
            {
                if (SourceLocation == null || SourceLocation.IsSpecial)
                    SourceLocation = source.SourceLocation;
            }
            return this;
        }

        /// <summary>
        /// Initialize the inferred type.
        /// </summary>
        /// <returns>this</returns>
        public AstExpression SetType(XTypeReference type)
        {
            InferredType = type;
            ExpectedType = null;
            return this;
        }

        /// <summary>
        /// Override the code.
        /// </summary>
        /// <returns>this</returns>
        public AstExpression SetCode(AstCode code)
        {
            Code = code;
            return this;
        }

        /// <summary>
        /// Override the arguments.
        /// </summary>
        /// <returns>this</returns>
        public AstExpression SetArguments(params AstExpression[] arguments)
        {
            Arguments.Clear();
            if (arguments != null)
            {
                Arguments.AddRange(arguments);
            }
            return this;
        }

        /// <summary>
        /// Override the arguments.
        /// </summary>
        /// <returns>this</returns>
        public AstExpression SetArguments(IEnumerable<AstExpression> arguments)
        {
            Arguments.Clear();
            if (arguments != null)
            {
                Arguments.AddRange(arguments);
            }
            return this;
        }

        public void AddPrefix(AstExpressionPrefix prefix)
        {
            var arr = Prefixes;
            if (arr == null)
                arr = new AstExpressionPrefix[1];
            else
                Array.Resize(ref arr, arr.Length + 1);
            arr[arr.Length - 1] = prefix;
            Prefixes = arr;
        }

        /// <summary>
        /// Gets the first prefix of the given code.
        /// </summary>
        public AstExpressionPrefix GetPrefix(AstCode code)
        {
            var prefixes = Prefixes;
            return (prefixes != null) ? prefixes.FirstOrDefault(p => p.Code == code) : null;
        }

        /// <summary>
        /// Gets all direct child nodes.
        /// </summary>
        public override IEnumerable<AstNode> GetChildren()
        {
            return Arguments;
        }

        /// <summary>
        /// Is this expression a branch to another node?
        /// </summary>
        public bool IsBranch()
        {
            return Operand is AstLabel || Operand is AstLabel[] || Operand is AstLabelKeyPair[];
        }

        /// <summary>
        /// Gets all possible branch targets.
        /// </summary>
        public IEnumerable<AstLabel> GetBranchTargets()
        {
            var label = Operand as AstLabel;
            if (label != null)
            {
                return new[] { label };
            }
            var labels = Operand as AstLabel[];
            if (labels != null)
            {
                return labels;
            }
            var pairs = Operand as AstLabelKeyPair[];
            if (pairs != null)
            {
                return pairs.Select(x => x.Label);
            }
            return Enumerable.Empty<AstLabel>();
        }

        /// <summary>
        /// Write human readable output.
        /// </summary>
        public override void WriteTo(ITextOutput output, FormattingOptions format)
        {
            if(format.HasFlag(FormattingOptions.ShowHasSeqPoint) && SourceLocation != NoSource)
                output.Write(SourceLocation.IsSpecial ? "!" : "~");

            if (Operand is AstVariable && ((AstVariable)Operand).IsGenerated)
            {
                if (Code == AstCode.Stloc && InferredType == null)
                {
                    output.Write(((AstVariable)Operand).Name);
                    output.Write(" = ");
                    Arguments.First().WriteTo(output, format);
                    return;
                }
                if (Code == AstCode.Ldloc)
                {
                    output.Write(((AstVariable)Operand).Name);
                    if (InferredType != null)
                    {
                        output.Write(':');
                        InferredType.WriteTo(output, AstNameSyntax.ShortTypeName);
                        if ((ExpectedType != null) && (ExpectedType.FullName != this.InferredType.FullName))
                        {
                            output.Write("[exp:");
                            ExpectedType.WriteTo(output, AstNameSyntax.ShortTypeName);
                            output.Write(']');
                        }
                    }
                    return;
                }
            }

            if (Prefixes != null)
            {
                foreach (var prefix in Prefixes)
                {
                    output.Write(prefix.Code.GetName());
                    output.Write(". ");
                }
            }

            output.Write(Code.GetName());
            if (InferredType != null)
            {
                output.Write(':');
                InferredType.WriteTo(output, AstNameSyntax.ShortTypeName);
                if ((ExpectedType != null) && (ExpectedType.FullName != InferredType.FullName))
                {
                    output.Write("[exp:");
                    ExpectedType.WriteTo(output, AstNameSyntax.ShortTypeName);
                    output.Write(']');
                }
            }
            else if (ExpectedType != null)
            {
                output.Write("[exp:");
                ExpectedType.WriteTo(output, AstNameSyntax.ShortTypeName);
                output.Write(']');
            }
            output.Write('(');

            var first = true;
            if (Operand != null)
            {
                if (Operand is AstLabel)
                {
                    output.WriteReference(((AstLabel)Operand).Name, Operand);
                }
                else if (Operand is AstLabel[])
                {
                    var labels = (AstLabel[])Operand;
                    for (var i = 0; i < labels.Length; i++)
                    {
                        if (i > 0)
                            output.Write(", ");
                        output.WriteReference(labels[i].Name, labels[i]);
                    }
                }
                else if (Operand is AstLabelKeyPair[])
                {
                    var pairs = (AstLabelKeyPair[])Operand;
                    for (var i = 0; i < pairs.Length; i++)
                    {
                        if (i > 0)
                            output.Write(", ");
                        output.Write("{0} -> ", pairs[i].Key);
                        output.WriteReference(pairs[i].Label.Name, pairs[i].Label);
                    }                    
                }
                else if (Operand is XMethodReference)
                {
                    var method = (XMethodReference)Operand;
                    if (method.DeclaringType != null)
                    {
                        method.DeclaringType.WriteTo(output, AstNameSyntax.ShortTypeName);
                        output.Write("::");
                    }
                    output.WriteReference(method.Name, method);
                }
                else if (Operand is XFieldReference)
                {
                    var field = (XFieldReference)Operand;
                    field.DeclaringType.WriteTo(output, AstNameSyntax.ShortTypeName);
                    output.Write("::");
                    output.WriteReference(field.Name, field);
                }
                else
                {
                    DisassemblerHelpers.WriteOperand(output, Operand);
                }
                first = false;
            }

            bool firstArg = true;
            foreach (var arg in Arguments)
            {
                if (!first) output.Write(", ");

                if ((format & FormattingOptions.BreakExpressions) != 0)
                {
                    if (firstArg)
                        output.Indent();

                    output.WriteLine();
                }

                arg.WriteTo(output, format);
                first = false;
                firstArg = false;
            }
            output.Write(')');

            if ((format & FormattingOptions.BreakExpressions) != 0 && !firstArg)
            {
                output.Unindent();
            }
        }

        /// <summary>
        /// Accept a visit by the given visitor.
        /// </summary>
        public override TReturn Accept<TReturn, TData>(AstNodeVisitor<TReturn, TData> visitor, TData data)
        {
            return visitor.Visit(this, data);
        }
    }
}