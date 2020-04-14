using System.Collections.Generic;

namespace Nexi {
    
    public class Group {

        public Filter Filter { get; private set; }
        private List<uint> entities;
        public List<uint> ChangedEntities { get; private set; }

        public Group(Filter filter) {
            Filter = filter;
            entities = new List<uint>();
            ChangedEntities = new List<uint>();
        }
        
        /*
         * Current Filter.Matches() isn't taking in to account things that Include more than one.
         * So a filter(Include: SpriteResource, Include: Posiiton)
         * will fire if it has either SpriteResource OR Position
         * It should only fire if it has both!
         */

        public void TestEntity(World world, uint entity) {
            //world.Log($"Testing entity {entity} against {Filter.ToString()}");
            bool pass = Filter.Matches(world, entity);
            //world.Log($"Test passed:{pass}");
            if(pass) {
                if(!entities.Contains(entity)) entities.Add(entity);
                if(!ChangedEntities.Contains(entity)) ChangedEntities.Add(entity);
            }
            if(!pass) {
                if(entities.Contains(entity)) entities.Remove(entity);               
            }
        }

        public bool Contains(uint entity) {
            return entities.IndexOf(entity) != -1;
        }

        public void UpdateCheck() {
            ChangedEntities.Clear();
        }

        public bool HasReactions() {
            return ChangedEntities.Count > 0;
        }

        public override string ToString() {
            return $"Group(Filter:{Filter.ToString()}:{string.Join(", ", entities)})";
        }
    }
}
