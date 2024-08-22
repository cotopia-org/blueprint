using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace srtool
{
    public static class ObjectExtensions
    {
        public static T DeepClone<T>(this T obj)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(obj));
            }

            if (obj == null)
            {
                return default;
            }

            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }

        }

        public static List<T> OrderedDistinct<T>(this IEnumerable<T> list)
        {
            var seenElements = new HashSet<T>();
            var distinctList = new List<T>();

            foreach (T element in list)
            {
                if (seenElements.Add(element))
                {
                    distinctList.Add(element);
                }
            }

            return distinctList;
        }


    }
}