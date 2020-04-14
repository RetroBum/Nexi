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
        private List<Group> groups;
        
        private List<ISystem> allSystems;
        private List<IInitialiseSystem> initSystems;
        private List<ITickSystem> tickSystems;
        private List<IReactiveSystem> reactSystems;

        private Dictionary<Group, IReactiveSystem> groupToSystem;

        public void Initialise(uint maxEntities, Type[] componentTypes, ISystem[] systems) {
            this.componentTypes = componentTypes;
            nextUid = 0;
            freeUids = new Stack<uint>();
            entities = new Entity[maxEntities];
            components = new IComponent[componentTypes.Length][];
            groups = new List<Group>();
            
            for(int i = 0; i < componentTypes.Length; i++) {
                if(!typeof(IComponent).IsAssignableFrom(componentTypes[i])) {
                    throw new Exception("Components must implement IComponent.");
                }
                components[i] = new IComponent[maxEntities];
            }

            allSystems = new List<ISystem>();
            initSystems = new List<IInitialiseSystem>();
            tickSystems = new List<ITickSystem>();
            reactSystems = new List<IReactiveSystem>();
            groupToSystem = new Dictionary<Group, IReactiveSystem>();
            foreach(ISystem system in systems) {
                allSystems.Add(system);
                if(system is IInitialiseSystem) initSystems.Add(system as IInitialiseSystem);
                if(system is ITickSystem) tickSystems.Add(system as ITickSystem);
                if(system is IReactiveSystem) {
                    reactSystems.Add(system as IReactiveSystem);
                    // Create group
                    Group g = new Group((system as IReactiveSystem).Filter);
                    groups.Add(g);
                    groupToSystem.Add(g, system as IReactiveSystem);
                }
            }

            // Init
            foreach(IInitialiseSystem system in initSystems) {
                system.Initialise(this);
            }

            Log?.Invoke($"Registered {componentTypes.Length} components and {allSystems.Count} systems.");
        }

        public void AddFeature(Feature feature) {
            foreach(ISystem system in feature.allSystems) {
                allSystems.Add(system);
                if(system is IInitialiseSystem) initSystems.Add(system as IInitialiseSystem);
                if(system is ITickSystem) tickSystems.Add(system as ITickSystem);
                if(system is IReactiveSystem) {
                    reactSystems.Add(system as IReactiveSystem);
                    // Create group
                    Group g = new Group((system as IReactiveSystem).Filter);
                    groups.Add(g);
                    groupToSystem.Add(g, system as IReactiveSystem);
                }
            }
        }

        public void Update() {
            // Tick
            foreach(ITickSystem system in tickSystems) {
                system.Tick(this);
            }
        }

        public void React() {
            foreach(Group g in groups) {
                if(g.HasReactions()) {
                    if(groupToSystem.ContainsKey(g)) groupToSystem[g].React(this, g.ChangedEntities.ToArray());
                }
                g.UpdateCheck();
            }
        }

        #region Debug
        public void PrintEntities() {
            string msg = "\nEntities:\n";
            for(int i = 0; i < entities.Length; i++) {
                if(entities[i] != null) {
                    msg += entities[i].Uid + ": " + entities[i].ListComponents(componentTypes) + "\n";
                }
            }
            Log(msg);
        }

        public void PrintGroupCounts() {
            Log(string.Join("\n", groups.Select(x => x.ToString())));
        }
        #endregion

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
            int cti = GetComponentTypeIndex(component);
            //if(components[cti][entity] != null) throw new Exception($"Can't add {component.GetType().Name} to entity {entity}. Component already exists on entity.");
            components[cti][entity] = component;

            // Add to entity
            entities[entity].ComponentAdded(cti);
            
            // Add to groups
            foreach(Group group in groups) {
                group.TestEntity(this, entity);
            }
        }

        public void RemoveComponent(uint entity, int componentTypeIndex) {
            if(entities[entity] == null) throw new Exception($"Can't remove component from entity {entity}. Entity does not exist.");
            if(components[componentTypeIndex][entity] == null) throw new Exception($"Can't remove component {componentTypes[componentTypeIndex].Name} form entity {entity}. Component does not exist on entity.");
            components[componentTypeIndex][entity] = null;

            // Remove from entity
            entities[entity].ComponentRemoved(componentTypeIndex);

            // Remove from groups
            foreach(Group group in groups) {
                group.TestEntity(this, entity);
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
