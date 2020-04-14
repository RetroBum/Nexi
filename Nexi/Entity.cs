using System;
using System.Collections.Generic;

namespace Nexi {
    
    public class Entity {

        public uint Uid { get; private set; }
        public bool[] Components { get; private set; }

        public void Init(int totalNumComponents, uint uid) {
            Uid = uid;
            Components = new bool[totalNumComponents];
        }

        public void ComponentAdded(int componentTypeIndex) {
            Components[componentTypeIndex] = true;
        }

        public void ComponentRemoved(int componentTypeIndex) {
            Components[componentTypeIndex] = false;
        }

        public string ListComponents(Type[] componentTypes) {
            List<string> compNames = new List<string>();
            for(int i = 0; i < Components.Length; i++) {
                if(Components[i]) {
                    compNames.Add(componentTypes[i].Name);
                }
            }
            return string.Join(", ", compNames);
        }
    }
}
