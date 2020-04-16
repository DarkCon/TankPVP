using System;
using System.Linq;
using Morpeh;
using UnityEngine;

namespace Tanks.Utils {
    [RequireComponent(typeof(Installer))]
    public class CleanLevelOnDestroy : MonoBehaviour {
        private void OnDestroy() {
            var installer = GetComponent<Installer>();

            var disposableSystems = installer.initializers.Cast<IDisposable>()
                /*.Concat(installer.updateSystems.Select(p => p.System))
                .Concat(installer.fixedUpdateSystems.Select(p => p.System))
                .Concat(installer.lateUpdateSystems.Select(p => p.System))*/;

            foreach (var disposable in disposableSystems) {
                disposable.Dispose();
            }
            
            //TODO: also how clean world?
        }
    }
}