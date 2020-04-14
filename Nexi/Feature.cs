using System.Collections.Generic;

namespace Nexi {
    
    public class Feature {

        public List<ISystem> allSystems;

        public Feature(params ISystem[] systems) {
            allSystems = new List<ISystem>();
            foreach(ISystem system in systems) {
                Add(system);
            }
        }

        public void Add(ISystem system) {
            allSystems.Add(system);
        }
    }
}
