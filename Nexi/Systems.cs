namespace Nexi {

    public interface ISystem { }

    public interface IAllSystems {
        void Initialise(World world);
        void Tick(World world);
        void React(World world);
        void Cleanup();
        void ComponentAdded(World world, uint entity);
        void ComponentRemoved(World world, uint entity);
    }

    public interface IInitialiseSystem : ISystem {
        void Initialise(World world);
    }

    public interface ITickSystem : ISystem {
        void Tick(World world);
    }

    public interface IReactiveSystem : ISystem {
        //Trigger Trigger { get; }
        Filter Filter { get; }
        void React(World world, uint[] entities);
    }
}
