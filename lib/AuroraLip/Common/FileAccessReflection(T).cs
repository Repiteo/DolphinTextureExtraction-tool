﻿namespace AuroraLib.Common
{
    public class FileAccessReflection<T> where T : IFileAccess
    {
        #region constructor

        private static IEnumerable<Type> _AvailableTypes { get; set; }

        private static IEnumerable<IFileAccess> _Instances { get; set; }

        static FileAccessReflection()
        {
            _AvailableTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes().Where(s => typeof(IFileAccess).IsAssignableFrom(s) && !s.IsInterface && !s.IsAbstract));
            _Instances = _AvailableTypes.Select(x => (IFileAccess)Activator.CreateInstance(x));
        }

        public FileAccessReflection()
        {
            AvailableTypes = _AvailableTypes.Where(s => typeof(T).IsAssignableFrom(s) && !s.IsInterface && !s.IsAbstract);
            Instances = _Instances.Where(x => x is T).Select(x => (T)x);
        }

        #endregion constructor

        /// <summary>
        /// A list of all available T Types
        /// </summary>
        public IEnumerable<Type> AvailableTypes { get; private set; }

        /// <summary>
        /// Instances are only used to retrieve pseudo static data.
        /// </summary>
        protected IEnumerable<T> Instances { get; private set; }

        /// <summary>
        /// A list of all readable types of available T.
        /// </summary>
        /// <returns>List of readable types of T</returns>
        public IEnumerable<Type> GetReadable() => Instances.Where(x => x.CanRead).Select(x => x.GetType());

        /// <summary>
        /// A list of all writable types of available T.
        /// </summary>
        /// <returns>List of writable types of T</returns>
        public IEnumerable<Type> GetWritable() => Instances.Where(x => x.CanWrite).Select(x => x.GetType());

        /// <summary>
        /// Get the type that matches with the Identify.
        /// </summary>
        /// <returns>List of readable types of T</returns>
        public Type GetByMagic(string magic) => Instances.Where(x => x is IMagicIdentify).First(x => ((IMagicIdentify)x).Magic == magic).GetType();

        /// <summary>
        /// Get the type by the name.
        /// </summary>
        /// <returns>List of readable types of T</returns>
        public Type GetByName(string name) => AvailableTypes.First(x => x.Name == name);

        /// <summary>
        /// Get the Instance of the type.
        /// </summary>
        /// <returns>Instance of T</returns>
        internal T GetInstance(Type type) => Instances.First(x => x.GetType() == type);

        /// <summary>
        /// Trying to find an T that Match to the data
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="type"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public bool TryToFindMatch(Stream stream, out Type type, string extension = "")
        {
            foreach (var instance in Instances)
            {
                stream.Seek(0, SeekOrigin.Begin);
                if (instance.IsMatch(stream, extension))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    type = instance.GetType();
                    return true;
                }
            }
            type = null;
            return false;
        }
    }
}
