using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvShields
{
    public static class TypeStorage
    {
        private static Dictionary<string, HashSet<AdvShieldProjector>> StorageContainer { get; set; }

        static TypeStorage()
        {
            StorageContainer = new Dictionary<string, HashSet<AdvShieldProjector>>();
        }

        public static HashSet<AdvShieldProjector> GetObjects()
        {
            if (StorageContainer.TryGetValue(typeof(AdvShieldProjector).FullName, out var storage))
                return storage;
            else
                return new HashSet<AdvShieldProjector>();
        }

        public static void AddObject(AdvShieldProjector newValue)
        {            
            HashSet<AdvShieldProjector> storage;

            if (StorageContainer.TryGetValue(typeof(AdvShieldProjector).FullName, out var value))
            {
                storage = value;
            }
            else
            {
                storage = new HashSet<AdvShieldProjector>();
                StorageContainer.Add(typeof(AdvShieldProjector).FullName, storage);
            }

            storage.Add(newValue);
        }


        public static void RemoveObject(AdvShieldProjector oldValue)
        {
            if (StorageContainer.TryGetValue(typeof(AdvShieldProjector).FullName, out var value))
            {
                HashSet<AdvShieldProjector> storage = value;

                if (storage.Contains(oldValue))
                    storage.Remove(oldValue);
            }
        }
    }
}
