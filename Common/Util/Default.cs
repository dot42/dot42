namespace TallComponents.Common.Util
{
    public static class Default<T>
        where T : new()
    {
        /// <summary>
        /// Single instance
        /// </summary>
        public static readonly T Instance = new T();
    }
}
