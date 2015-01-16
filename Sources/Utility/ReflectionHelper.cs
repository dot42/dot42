using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Dot42.Utility
{
    /// <summary>
    /// Represents a set of helpers for .net reflection
    ///  </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// Gets a MethodInfo object from specified expression
        ///  </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="methodExpression"></param>
        /// <returns></returns>
        public static MethodInfo MethodOf<TResult>(Expression<Func<TResult>> methodExpression)
        {
            return ((MethodCallExpression)methodExpression.Body).Method;
        }

        /// <summary>
        /// Gets a MethodInfo object from specified expression
        ///  </summary>
        /// <param name="methodExpression"></param>
        /// <returns></returns>
        public static MethodInfo MethodOf(Expression<Action> methodExpression)
        {
            return ((MethodCallExpression)methodExpression.Body).Method;
        }

        /// <summary>
        /// Gets a MethodInfo object from specified expression
        ///  </summary>
        /// <param name="methodExpression"></param>
        /// <returns></returns>
        public static MethodInfo MethodOf<TInstance, TResult>(Expression<Func<TInstance, TResult>> methodExpression)
        {
            return ((MethodCallExpression)methodExpression.Body).Method;
        }

        /// <summary>
        /// Gets a MethodInfo object from specified expression
        ///  </summary>
        /// <param name="methodExpression"></param>
        /// <returns></returns>
        public static MethodInfo MethodOf<TInstance>(Expression<Action<TInstance>> methodExpression)
        {
            return ((MethodCallExpression)methodExpression.Body).Method;
        }

        /// <summary>
        /// Gets a PropertyInfo object from specified expression
        ///  </summary>
        /// <param name="propertyGetExpression"></param>
        /// <returns></returns>
        public static PropertyInfo PropertyOf<TProperty>(Expression<Func<TProperty>> propertyGetExpression)
        {
            return ((MemberExpression)propertyGetExpression.Body).Member as PropertyInfo;
        }

        /// <summary>
        /// Gets a PropertyInfo object from specified expression
        ///  </summary>
        /// <param name="propertyGetExpression"></param>
        /// <returns></returns>
        public static PropertyInfo PropertyOf<TInstance, TProperty>(Expression<Func<TInstance, TProperty>> propertyGetExpression)
        {
            return ((MemberExpression)propertyGetExpression.Body).Member as PropertyInfo;
        }

        /// <summary>
        /// Gets a FieldInfo object from specified expression
        ///  </summary>
        /// <param name="fieldAccessExpression"></param>
        /// <returns></returns>
        public static FieldInfo FieldsOf<TProperty>(Expression<Func<TProperty>> fieldAccessExpression)
        {
            return ((MemberExpression)fieldAccessExpression.Body).Member as FieldInfo;
        }
    }
}
