using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_2d_terrain
{
    // contains all body info on an entity as well as health, inventory, skills, exp.
    public class EntityBody
    {
        public struct BodyParts
        {
            int head;
            int body;
            int arms;
            int leg;
        };

        int _maxWeight = 100;
        int _currentWeight = 0;
        int _experience = 0;
        int _hunger = 0;
        List<Game1.Item> _inventory;
        BodyParts _bodyParts;
        public EntityBody()
        {
            _inventory = new List<Game1.Item>();
        }
        public bool PickupItem(Game1.Item item)
        {
            if (item.weight < _maxWeight - _currentWeight)
            {
                _inventory.Add(item);
                return true;
            }
            return false;
        }
    }
}
