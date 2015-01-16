namespace Dot42.CompilerLib.Ast
{
    /// <summary>
    /// Generic visitor
    /// </summary>
    public class AstNodeVisitor<TReturn, TData>
    {
        public virtual TReturn Visit(AstBlock node, TData data)
        {
            return default(TReturn);
        }

        public virtual TReturn Visit(AstSwitch.CaseBlock node, TData data)
        {
            return Visit((AstBlock)node, data);
        }

        public virtual TReturn Visit(AstTryCatchBlock.CatchBlock node, TData data)
        {
            return Visit((AstBlock)node, data);
        }

        public virtual TReturn Visit(AstBasicBlock node, TData data)
        {
            return default(TReturn);
        }

        public virtual TReturn Visit(AstExpression node, TData data)
        {
            return default(TReturn);
        }

        public virtual TReturn Visit(AstLabel node, TData data)
        {
            return default(TReturn);
        }

        public virtual TReturn Visit(AstSwitch node, TData data)
        {
            return default(TReturn);
        }

        public virtual TReturn Visit(AstTryCatchBlock node, TData data)
        {
            return default(TReturn);
        }
    }
}
