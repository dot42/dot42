using System.Collections.Generic;

namespace Dot42.DexLib
{
    /// <summary>
    /// Helper for quick lookups of classes or methods in a dex file.
    /// Assumes that the neither classes are added or removed from dex
    /// or methods added or removed from classes.
    /// </summary>
    public class DexLookup
    {
        private readonly Dictionary<string, MethodDefinition> _methodsByFullName = new Dictionary<string, MethodDefinition>();
        private readonly Dictionary<string, ClassDefinition> _classesByFullName = new Dictionary<string, ClassDefinition>();
        //private readonly Dictionary<int, ClassDefinition> _classesById = new Dictionary<int, ClassDefinition>();
        //private readonly Dictionary<int, MethodDefinition> _methodsById = new Dictionary<int, MethodDefinition>();

        public DexLookup(Dex dex)
        {
            foreach (var @class in dex.GetClasses())
            {
                AddClass(@class);
            }
        }

        private void AddClass(ClassDefinition @class)
        {
            _classesByFullName[@class.Fullname] = @class;
            //if (@class.MapFileId != 0)
            //    _classesById[@class.MapFileId] = @class;

            foreach (var method in @class.Methods)
            {
                _methodsByFullName[@class.Fullname + "::" + method.Name + method.Prototype.ToSignature()] = method;

                //if (method.MapFileId != 0)
                //    _methodsById[method.MapFileId] = method;
            }

            foreach(var inner in @class.InnerClasses)
                AddClass(inner);
        }

        ///// <summary>
        ///// returns null if not found
        ///// </summary>
        //public ClassDefinition FindClassById(int mapFileId)
        //{
        //    ClassDefinition ret;
        //    _classesById.TryGetValue(mapFileId, out ret);
        //    return ret;
        //}

        /// <summary>
        /// returns null if not found
        /// </summary>
        public ClassDefinition FindClass(string fullName)
        {
            ClassDefinition ret;
            _classesByFullName.TryGetValue(fullName, out ret);
            return ret;
        }

        /// <summary>
        /// returns null if not found
        /// </summary>
        public MethodDefinition FindMethod(string classFullName, string methodName, string methodSignature)
        {
            MethodDefinition ret;
            _methodsByFullName.TryGetValue(classFullName + "::" + methodName + methodSignature, out ret);
            return ret;
        }

        ///// <summary>
        ///// returns null if not found
        ///// </summary>
        //public MethodDefinition FindMethod(int mapFileId)
        //{
        //    MethodDefinition ret;
        //    _methodsById.TryGetValue(mapFileId, out ret);
        //    return ret;
        //}
    }
}
