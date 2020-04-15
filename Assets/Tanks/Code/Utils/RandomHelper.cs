using System;
using System.Collections.Generic;
using System.Linq;

namespace Tanks.Utils {
    public static class RandomHelper {
        public static bool RandomBool() {
            return UnityEngine.Random.value > 0.5f;
        }
    
        public static T Random<T>(this IEnumerable<T> enumerable) {
            if (!TryRandom(enumerable, out var ret))
                throw new ArgumentOutOfRangeException();
            return ret;
        }

        public static bool TryRandom<T>(this IEnumerable<T> enumerable, out T ret) {
            var count = 0;
            if (enumerable is ICollection<T> collection) {
                count = collection.Count;
            } else {
                var array = enumerable.ToArray();
                count = array.Length;
                enumerable = array;
            }
            
            if (count == 0) {
                ret = default;
                return false;
            }
            var rand = UnityEngine.Random.Range(0, count);
            ret = enumerable.ElementAt(rand);
            return true;
        }
    }
}