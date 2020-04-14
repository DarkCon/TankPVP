namespace Tanks.Utils {
    public abstract class ObjectsPool {

        private static IObjectsPool _main;

        public static IObjectsPool Main {
            set {
                _main = value;
            }
            get {
                if (_main == null) {
                    _main = new ObjectsPoolEnableDisable();
                }
                return _main;
            }
        }
    }
}