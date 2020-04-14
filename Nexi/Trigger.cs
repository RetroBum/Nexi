using System;

namespace Nexi {
    
    public struct Trigger {

        public Type Component { get; private set; }
        public TriggerStyle Style { get; private set; }
        
        public Trigger(TriggerStyle style, Type component) {
            Style = style;
            Component = component;
        }

        public override string ToString() {
            return $"{Style.ToString()}:{Component.Name}";
        }
    }

    public enum TriggerStyle {
        Added, Removed
    }
}
