using System;
using System.Collections.Generic;
using System.Linq;

namespace Nexi {
    
    public class World {

        public delegate void Logger(string message);
        public Logger Log { get; set; }

        private Type[] componentTypes;
        private uint nextUid;
        private Stack<uint> freeUids;
        private Entity[] entities;
        private IComponent[][] components;
        private List<Feature> features;

        public World(uint maxEntities, Type[] componentTypes) {
            this.componentTypes = componentTypes;
            nextUid = 0;
            freeUids = new Stack<uint>();
            entities = new Entity[maxEntities];
            components = new IComponent[componentTypes.Length][];
            features = new List<Feature>();
            
            for(int i = 0; i < componentTypes.Length; i++) {
                if(!typeof(IComponent).IsAssignableFrom(componentTypes[i])) {
                    throw new Exception("Components must implement IComponent.");
                }
                components[i] = new IComponent[maxEntities];
            }

            Log?.Invoke($"Registered {componentTypes.Length} components.");
        }

        public void AddFeature(Feature feature) {
            features.Add(feature);
        }

        public void Init() {
            int count = 0;
            foreach(Feature feature in features) {
                feature.Initialise(this);
                count += feature.InitSystemsCount;
            }

            Log?.Invoke($"Initialised {count} systems.");
        }

        public void Update() {
            // Tick
            foreach(Feature feature in features) {
                feature.Tick(this);
            }
        }

        public void React() {
            foreach(Feature feature in features) {
                feature.React(this);
            }
        }

        public void Cleanup() {
            foreach(Feature feature in features) {
                feature.Cleanup();
            }
        }

        #region Entities
        public Entity CreateEntity() {
            Entity entity = new Entity();
            uint uid = freeUids.Count > 0 ? freeUids.Pop() : nextUid++;
            entity.Init(componentTypes.Length, uid);
            entities[uid] = entity;
            return entity;
        }
        #endregion

        #region Components
        public void AddComponent(uint entity, IComponent component) {
            if(entities[entity] == null) throw new Exception($"Can't add component to entity {entity}. Entity does not exist.");
            Log($"Adding {component.GetType().Name} to entity {entity}");
            int cti = GetComponentTypeIndex(component);
            components[cti][entity] = component;

            // Add to entity
            entities[entity].ComponentAdded(cti);

            // Add to groups
            foreach(Feature feature in features) {
                feature.ComponentAdded(this, entity);
            }
        }

        public void RemoveComponent(uint entity, int componentTypeIndex) {
            if(entities[entity] == null) throw new Exception($"Can't remove component from entity {entity}. Entity does not exist.");
            Log($"Removing component index {componentTypeIndex} from entity {entity}");
            if(components[componentTypeIndex][entity] == null) throw new Exception($"Can't remove component {componentTypes[componentTypeIndex].Name} form entity {entity}. Component does not exist on entity.");
            components[componentTypeIndex][entity] = null;

            // Remove from entity
            entities[entity].ComponentRemoved(componentTypeIndex);

            // Remove from groups
            foreach(Feature feature in features) {
                feature.ComponentRemoved(this, entity);
            }
        }

        public T GetComponent<T>(uint entity) where T : IComponent {
            int cti = GetComponentTypeIndex(typeof(T));
            return (T)components[cti][entity];
        }

        public bool HasComponent(uint entity, Type componentType) {
            int cti = GetComponentTypeIndex(componentType);
            if(entities[entity] == null) throw new Exception($"Can't check if entity {entity} has component. Entity does not exist.");
            return entities[entity].Components[cti];
        }
        
        private int GetComponentTypeIndex(IComponent component) {
            return Array.IndexOf(componentTypes, component.GetType());
        }

        private int GetComponentTypeIndex(Type componentType) {
            return Array.IndexOf(componentTypes, componentType);
        }
        #endregion
    }
}
