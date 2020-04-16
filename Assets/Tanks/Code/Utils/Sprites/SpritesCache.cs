using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tanks.Sprites {
    public static class SpritesCache {
        private static readonly Dictionary<string, Sprite[]> spritesCache = new Dictionary<string, Sprite[]>();
        
        public static Sprite[] GetAllFor(string textureName) {
            if (!spritesCache.TryGetValue(textureName, out var sprites)) {
                sprites = Resources.LoadAll<Sprite>(textureName);
                spritesCache.Add(textureName, sprites);
            }
            return sprites;
        }

        public static IEnumerable<Sprite> GetAllLoaded() {
            return spritesCache.Values.SelectMany(array => array);
        }
        
        public static Sprite FindInLoaded(string name) {
            return GetAllLoaded().FirstOrDefault(s => s.name == name);
        }
    }
}