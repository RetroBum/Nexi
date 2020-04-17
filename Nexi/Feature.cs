using System.Collections.Generic;

namespace Nexi {
    
    public class Feature : IAllSystems {

        private List<ISystem> allSystems;
        private List<IInitialiseSystem> initSystems;
        private List<ITickSystem> tickSystems;
        private List<IReactiveSystem> reactSystems;

        private List<Group> groups;
        private Dictionary<Group, IReactiveSystem> groupToSystems;

        public int InitSystemsCount => initSystems.Count;
        public int TickSystemsCount => tickSystems.Count;
        public int ReactSystemsCount => reactSystems.Count;
        public int AllSystemsCount => allSystems.Count;

        public Feature(params ISystem[] systems) {
            allSystems = new List<ISystem>();
            initSystems = new List<IInitialiseSystem>();
            tickSystems = new List<ITickSystem>();
            reactSystems = new List<IReactiveSystem>();
            groups = new List<Group>();
            groupToSystems = new Dictionary<Group, IReactiveSystem>();

            foreach(ISystem system in systems) Add(system);
        }

        public void Add(ISystem system) {
            allSystems.Add(system);
            if(system is IInitialiseSystem) initSystems.Add(system as IInitialiseSystem);
            if(system is ITickSystem) tickSystems.Add(system as ITickSystem);
            if(system is IReactiveSystem) {
                reactSystems.Add(system as IReactiveSystem);

                Group g = new Group((system as IReactiveSystem).Filter);
                groups.Add(g);
                groupToSystems.Add(g, system as IReactiveSystem);
            }
        }

        public void Initialise(World world) {
            foreach(IInitialiseSystem system in initSystems) {
                system.Initialise(world);
            }
        }

        public void Tick(World world) {
            foreach(ITickSystem system in tickSystems) {
                system.Tick(world);
            }
        }
        
        public void React(World world) {
            foreach(Group g in groups) {
                if(g.HasReactions() && groupToSystems.ContainsKey(g)) {
                    groupToSystems[g].React(world, g.ChangedEntities.ToArray());
                }
            }
        }

        public void Cleanup() {
            foreach(Group g in groups) {
                g.ClearChanged();
            }
        }

        public void ComponentAdded(World world, uint entity) {
            foreach(Group g in groups) {
                g.TestEntity(world, entity);
            }
        }

        public void ComponentRemoved(World world, uint entity) {
            foreach(Group g in groups) {
                g.TestEntity(world, entity);
            }
        }
    }
}
