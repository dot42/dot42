using System;
using System.Diagnostics;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Ast2RLCompiler.Extensions;
using Dot42.CompilerLib.Java2Ast;
using Dot42.CompilerLib.XModel;
using TallApplications.Dot42;
using ILMethodDefinition = Mono.Cecil.MethodDefinition;
using JavaMethodDefinition = Dot42.JvmClassLib.MethodDefinition;


namespace Dot42.CompilerLib
{
    [DebuggerDisplay("{FullName}")]
    public class MethodSource
    {
        private readonly XMethodDefinition method;
        private readonly AstBlock ast;
        private readonly ILMethodDefinition ilMethod;
        private readonly JavaMethodDefinition javaMethod;

        /// <summary>
        /// IL ctor
        /// </summary>
        public MethodSource(XMethodDefinition method, ILMethodDefinition ilMethod)
        {
            if (ilMethod == null)
                throw new ArgumentNullException();
            this.method = method;
            this.ilMethod = ilMethod;
        }

        /// <summary>
        /// Java ctor
        /// </summary>
        public MethodSource(XMethodDefinition method, JavaMethodDefinition javaMethod)
        {
            if (javaMethod == null)
                throw new ArgumentNullException();
            this.method = method;
            this.javaMethod = javaMethod;
        }

        /// <summary>
        /// AST ctor
        /// </summary>
        public MethodSource(XMethodDefinition method, AstBlock ast)
        {
            if (ast == null)
                throw new ArgumentNullException();
            this.method = method;
            this.ast = ast;
        }

        public XMethodDefinition Method { get { return method; } }
        
        public bool IsDotNet { get { return (ilMethod != null); } }
        public bool IsJava { get { return (javaMethod != null); } }
        public bool IsAst { get { return (ast != null); } }

        public ILMethodDefinition ILMethod { get { return ilMethod; } }
        public JavaMethodDefinition JavaMethod { get { return javaMethod; } }
        public AstBlock Ast { get { return ast; } }

        /// <summary>
        /// Gets the method name
        /// </summary>
        public string Name
        {
            get { return (ilMethod != null) ? ilMethod.Name : (javaMethod != null) ? javaMethod.Name : method.Name; }
        }

        /// <summary>
        /// Gets the fullname of the method.
        /// </summary>
        public string FullName
        {
            get { return (ilMethod != null) ? ilMethod.FullName : (javaMethod != null) ? javaMethod.FullName : method.FullName; }
        }

        /// <summary>
        /// Gets the fullname of the declaring type.
        /// </summary>
        public string DeclaringTypeFullName
        {
            get { return (ilMethod != null) ? ilMethod.DeclaringType.FullName : (javaMethod != null) ? javaMethod.DeclaringClass.ClassName : method.DeclaringType.FullName; }
        }

        /// <summary>
        /// Does this method return void?
        /// </summary>
        public bool ReturnsVoid
        {
            get { return (ilMethod != null) ? ilMethod.ReturnType.IsVoid() : (javaMethod != null) ? javaMethod.ReturnType.IsVoid : method.ReturnType.IsVoid(); }
        }

        /// <summary>
        /// Does this method return a dex wide type?
        /// </summary>
        public bool ReturnsDexWide
        {
            get { return (ilMethod != null) ? ilMethod.ReturnType.IsDexWide() : (javaMethod != null) ? javaMethod.ReturnType.IsDexWide() : method.ReturnType.IsDexWide(); }
        }

        /// <summary>
        /// Does this method return a dex value type?
        /// </summary>
        public bool ReturnsDexValue
        {
            get { return (ilMethod != null) ? ilMethod.ReturnType.IsDexValue() : (javaMethod != null) ? javaMethod.ReturnType.IsDexValue() : method.ReturnType.IsDexValue(); }
        }

        /// <summary>
        /// Does this method return a dex object type?
        /// </summary>
        public bool ReturnsDexObject
        {
            get { return (ilMethod != null) ? ilMethod.ReturnType.IsDexObject() : (javaMethod != null) ? javaMethod.ReturnType.IsDexObject() : method.ReturnType.IsDexObject(); }
        }

        /// <summary>
        /// Gets the last source location that is specified in the method.
        /// </summary>
        public ISourceLocation GetLastSourceLine()
        {
            if (ilMethod != null)
            {
                if (!ilMethod.HasBody)
                    return null;
                var seqPoints = ilMethod.Body.Instructions.Select(x => x.SequencePoint(ilMethod.Body)).Where(x => (x != null) && !x.IsSpecial());
                return SequencePointWrapper.Wrap(seqPoints.OrderByDescending(x => x.StartLine).FirstOrDefault());
            }
            if (javaMethod != null)
            {
                var body = javaMethod.Body;
                if (body == null)
                    return null;
                var lastLocIns = body.Instructions.OrderByDescending(x => x.LineNumber).FirstOrDefault();
                if (lastLocIns != null)
                    return new SourceLocation(body, lastLocIns);
            }
            if (ast != null)
            {
                return ast.GetSelfAndChildrenRecursive<AstNode>().Select(x => x.SourceLocation).Where(x => x != null).OrderByDescending(x => x.StartLine).FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Is this a static method
        /// </summary>
        public bool IsStatic
        {
            get { return (ilMethod != null) ? ilMethod.IsStatic : (javaMethod != null) ? javaMethod.IsStatic : method.IsStatic; }
        }

        /// <summary>
        /// Is this a class ctor?
        /// </summary>
        public bool IsClassCtor
        {
            get { return (ilMethod != null) ? ilMethod.IsClassCtor() : (javaMethod != null) ? (javaMethod.Name == "<clinit>") : (method.IsConstructor && method.IsStatic); }
        }

        /// <summary>
        /// Is this a ctor (non-class)?
        /// </summary>
        public bool IsCtor
        {
            get { return (ilMethod != null) ? ilMethod.IsConstructor && !ILMethod.IsStatic : (javaMethod != null) ? (javaMethod.Name == "<init>") : (method.IsConstructor && method.IsStatic); }
        }
    }
}
