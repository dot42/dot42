using System;
using System.Linq;
using Dot42.ImportJarLib;
using Dot42.ImportJarLib.Model;

namespace Dot42.FrameworkBuilder.Android
{
    /// <summary>
    /// Custom property builder.
    /// </summary>
    internal sealed class SkipSpecifiedPropertyBuilder : PropertyBuilder
    {
        private readonly Func<NetMethodDefinition, bool?> _isPropertyPredicate;
        private readonly string[] _skipOriginalJavaProperties;

        /// <summary>
        /// Default ctor
        /// </summary>
        public SkipSpecifiedPropertyBuilder(NetTypeDefinition typeDef, TypeBuilder declaringTypeBuilder, params string[] skipOriginalJavaProperties)
            : base(typeDef, declaringTypeBuilder)
        {
            _skipOriginalJavaProperties = skipOriginalJavaProperties;
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public SkipSpecifiedPropertyBuilder(NetTypeDefinition typeDef, TypeBuilder declaringTypeBuilder, Func<NetMethodDefinition, bool?> isPropertyPredicate)
            : base(typeDef, declaringTypeBuilder)
        {
            _isPropertyPredicate = isPropertyPredicate;
        }


        /// <summary>
        /// Is the given method a property get method?
        /// </summary>
        protected override bool IsGetter(NetMethodDefinition method)
        {
            var name = method.OriginalJavaName;
            if (_skipOriginalJavaProperties != null && _skipOriginalJavaProperties.Contains(name))
                return false;

            if (_isPropertyPredicate != null)
            {
                var ret = _isPropertyPredicate(method);
                if (ret != null) return ret.Value;
            }

            return base.IsGetter(method);
        }
        /// <summary>
        /// Is the given method a property get method?
        /// </summary>
        protected override bool IsSetter(NetMethodDefinition method)
        {
            var name = method.OriginalJavaName;
            if (_skipOriginalJavaProperties != null && _skipOriginalJavaProperties.Contains(name))
                return false;
            if (_isPropertyPredicate != null)
            {
                var ret = _isPropertyPredicate(method);
                if (ret != null) return ret.Value;
            }

            return base.IsSetter(method);
        }
    }
}