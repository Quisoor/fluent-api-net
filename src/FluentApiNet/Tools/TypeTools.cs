using System;
using System.Linq;

namespace FluentApiNet.Tools
{
    /// <summary>
    /// Class type tools
    /// </summary>
    public static class TypeTools
    {
        /// <summary>
        /// Determines whether the specified type is simple.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is simple; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSimple(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(type.GetGenericArguments()[0]);
            }
            return type.IsPrimitive
              || type.IsEnum
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal))
              || type.Equals(typeof(DateTime));
        }

        /// <summary>
        /// Determines whether the specified type is list.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is list; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsList(Type type)
        {
            return type.GenericTypeArguments.Count() == 1;
        }

        /// <summary>
        /// Gets the type of the list.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Type GetListType(Type type)
        {
            if (IsList(type))
            {
                return type.GenericTypeArguments.First();
            }
            else
            {
                return null;
            }
        }
    }
}
