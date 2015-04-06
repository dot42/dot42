using System;
using System.ComponentModel.Composition;
using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Mapped;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;

namespace Dot42.FrameworkBuilder.Android
{
    [Export(typeof(IMappedTypeBuilder))]
    internal sealed class JavaNioByteBufferBuilder: AndroidBuilder
    {
        public JavaNioByteBufferBuilder() : this(ClassFile.Empty) { }

        internal JavaNioByteBufferBuilder(ClassFile cf)
            : base(cf, "java/nio/ByteBuffer")
        {
        }

        protected override PropertyBuilder CreatePropertyBuilder(NetTypeDefinition typeDef)
        {
            return new SkipSpecifiedPropertyBuilder(typeDef, this, m=>m.OriginalJavaName.StartsWith("get")?(bool?)false:null);
        }
    }
}
