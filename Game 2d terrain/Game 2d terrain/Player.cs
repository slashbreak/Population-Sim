using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game_2d_terrain
{
    class Player : Entity
    {
        public Player(int speed)
        {
            _updateSpeed = speed;
            _position = new Microsoft.Xna.Framework.Vector3(2, 2, 0);
        }

        public void  Update(Microsoft.Xna.Framework.GameTime gt, ref Map m, Microsoft.Xna.Framework.Input.MouseState mouse, int tilesize, int dx, int dy)
        {
            
            timer += gt.ElapsedGameTime.Milliseconds;
            if (timer > _updateSpeed)
            {
                timer = 0;
                this._position.X += dx;
                this._position.Y += dy;
            }
 	         //base.Update(gt, ref m, mouse, tilesize);
        }
  
    }
}
