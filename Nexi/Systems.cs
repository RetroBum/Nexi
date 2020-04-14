namespace Nexi {

    public interface ISystem { }

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
