using System;
using System.Collections.Generic;

namespace Nexi {
    
    public struct Filter {

        public FilterEntry[] Entries { get; private set; }
        public Type[] ComponentTypes { get; private set; }

        public Filter(params FilterEntry[] entries) {
            Entries = entries;

            List<Type> types = new List<Type>();
            foreach(FilterEntry entry in entries) {
                types.Add(entry.Component);
            }
            ComponentTypes = types.ToArray();
        }

        public override string ToString() {
            return string.Join(", ", Entries);
        }

        public bool Matches(World world, uint entity) {
            //world.Log($"Testing {entity} against filter\n{ToString()})");

            foreach(FilterEntry entry in Entries) {
                if(entry.Style == FilterStyle.Include && !world.HasComponent(entity, entry.Component)) {
                    return false;
                }
                if(entry.Style == FilterStyle.Exclude && world.HasComponent(entity, entry.Component)) {
                    return false;
                }
            }
            return true;
        }

        public bool FiltersType(Type type) {
            return Array.IndexOf(ComponentTypes, type) != -1;
        }
    }

    public struct FilterEntry {

        public Type Component { get; private set; }
        public FilterStyle Style { get; private set; }

        public FilterEntry(FilterStyle style, Type component) {
            Style = style;
            Component = component;
        }

        public override string ToString() {
            return $"{Style.ToString()}:{Component.Name}";
        }
    }

    public enum FilterStyle {
        Include, Exclude
    }
}
