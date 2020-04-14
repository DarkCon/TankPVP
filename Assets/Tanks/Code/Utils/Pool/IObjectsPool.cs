using Morpeh;
using UnityEngine;

namespace Tanks.Utils {
    public interface IObjectsPool {
        void AddPrefabs(EntityProvider[] prefabs);
        void Clean();

        IEntity Take(string name, bool networkSpawn = false);
        IEntity Take(string name, Vector3 inPosition, bool networkSpawn = false);
        void Return(IEntity entity, World world);
    }
}