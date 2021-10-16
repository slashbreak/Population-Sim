using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Game_2d_terrain
{
    public class Entity
    {
        public int _id; // unique id number
        public bool draw = false;
        public bool alive = true;
        public bool showInfo = false; // for drawing creature info
        Random r = new Random();
        public float timer = 0;
        public int _health = 100;
        public int _hunger = 100;
        public float _updateSpeed = 100; // higher number is slower. 1000 = once per second.
        public int _color = 3;
        public Vector3 _position;
        public int _xBoundary;
        public int _yBoundary;
        public int _zBoundary;
        public int _sight = 10;
        public float moveCounter = 0.0f;
        public bool gender = false; // 0 - male 1 - female
        Map map;
        bool foodSpotted = false;
        Vector2 food = Vector2.Zero; // closest bit of food

        public int _tilesize = 2;

        public enum EntityState 
        {
            Hunt,
            Sleep,
            Run,
            Eat,
            FollowMouse,
            Idle
        } ;
        public EntityState state = EntityState.Idle;

        public EntityBody _body = new EntityBody();

        public Entity()
        {
            _health = 100;
            _hunger = 100;
            _color = 0;
            _updateSpeed = 10;
            _position = new Vector3(0, 0,0);
            
        }

        public Entity(int id, ref Map m, bool g, int color, Vector3 pos,Vector3 bounds, float speed, int tilesize, int sight)
        {
            _id = id;
            _color = color;
            _position = pos;
            _xBoundary = (int)bounds.X;
            _yBoundary = (int)bounds.Y;
            _zBoundary = (int)bounds.Z;
            gender = g;
            map = m;
            _updateSpeed = speed;
            _tilesize = tilesize;
            _sight = sight;
        }
        public virtual void Update(GameTime gt, ref Map m, MouseState mouse, int tilesize)
        {
            timer += gt.ElapsedGameTime.Milliseconds;
            _tilesize = tilesize;
            //int mX = mouse.X / _tilesize;
            //int mY = mouse.Y / _tilesize;
            map = m;
            if (timer > _updateSpeed)
            {
                timer = 0;
                _hunger -= 1;
                if (_hunger <= 0)
                {
                    alive = false;
                    return;
                }
                else if (_hunger <= 100)
                {
                    state = EntityState.Hunt;

                }
                else if (_hunger > 100)
                {
                       //state = EntityState.FollowMouse;
                }

                //if (state == EntityState.FollowMouse)
                //{

                //    // note dirty swapping of x
                //    if (mX == _position.X && mY == _position.Y)
                //    {
                //        state = EntityState.Idle;
                //        return;
                //    }
                //    if (mY < _position.Y)
                //    {
                //        _position.Y -= 1;
                //    }
                //    else if (mY > _position.Y)
                //    {
                //        _position.Y += 1;
                //    }
                //    if (mX < _position.X)
                //    {
                //        _position.X -= 1;
                //    }
                //    else if (mX > _position.X)
                //    {
                //        _position.X += 1 ;
                //    }
                //    _position.X = MathHelper.Clamp(_position.X, 0, _xBoundary);
                //    _position.Y = MathHelper.Clamp(_position.Y, 0, _yBoundary);
                //    _position.Z = MathHelper.Clamp(_position.Z, 0, _zBoundary);
                //}
                if (state == EntityState.Idle)
                {
                    _position.X += r.Next(-1, 2);
                    _position.Y += r.Next(-1, 2);
                    _position.X = MathHelper.Clamp(_position.X, 0, _xBoundary);
                    _position.Y = MathHelper.Clamp(_position.Y, 0, _yBoundary);
                    _position.Z = MathHelper.Clamp(_position.Z, 0, _zBoundary);
                }

                if (state == EntityState.Hunt)
                {

                    if (!foodSpotted)
                    {
                        food = FoodPos(_position, _sight);
                        if (food != Vector2.Zero)
                        {
                            foodSpotted = true;
                        }
                    }
                    if (_position.X == food.X && _position.Y == food.Y)
                    {
                        if (map._stuff[(int)food.Y, (int)food.X, map.currentZ] == (int)Game1.ITEMS.Plant)
                        {
                            state = EntityState.Eat;
                        }
                        // uh oh, someone ate our food.
                        else
                        {
                            foodSpotted = false;
                        }

                    }
                    else if (food != Vector2.Zero)
                    {

                        if (food.X > _position.X)
                        {
                            _position.X += 1;
                        }
                        else if (food.X < _position.X)
                        {
                            _position.X -= 1;
                        }
                        if (food.Y > _position.Y)
                        {
                            _position.Y += 1;
                        }
                        else if (food.Y < _position.Y)
                        {
                            _position.Y -= 1;
                        }
                        _position.X = MathHelper.Clamp(_position.X, 0, _xBoundary);
                        _position.Y = MathHelper.Clamp(_position.Y, 0, _yBoundary);
                        _position.Z = MathHelper.Clamp(_position.Z, 0, _zBoundary);
                    }

                }
                if (state == EntityState.Eat)
                {
                    _hunger += 150;
                    map._stuff[(int)_position.Y, (int)_position.X, (int)_position.Z] = (int)Game1.ITEMS.Nothing;
                    state = EntityState.Idle;
                    foodSpotted = false;
                }
            }
        }


        // A Spiral search looking for food
        private Vector2 FoodPos(Vector3 entPos, int sight)
        {
            int x = (int)entPos.X;
            int y = (int)entPos.Y;
            int z = (int)entPos.Z;
           
            // we're standing on food. 
            if (CheckGridItem(entPos,Game1.ITEMS.Plant))
            {
                return new Vector2(x, y);
            }

            for (int d = 0; d <= sight; d++)
            {
                for (int i = x - d; i < x + d + 1; i++)
                {
                    if ((i > 0 && i < map.MAX_X - 1) && (y-d>0 && y+d < map.MAX_X-1))
                    {
                        if (CheckGridItem(new Vector3(i, y - d, z), Game1.ITEMS.Plant))
                            return new Vector2(i, y - d);
                        if (CheckGridItem(new Vector3(i, y + d, z), Game1.ITEMS.Plant))
                            return new Vector2(i, y + d);
                    }
                }
                for (int j = y - d +1; j < y + d; j++)
                {
                    if ((j > 0 && j < map.MAX_Y-1) && (x-d>0 && x+d < map.MAX_X-1))
                    {
                        if (CheckGridItem(new Vector3(x - d, j, z), Game1.ITEMS.Plant))
                            return new Vector2(x - d, j);
                        if (CheckGridItem(new Vector3(x + d, j, z), Game1.ITEMS.Plant))
                            return new Vector2(x + d, j);
                    }
                }
            }

            
            return Vector2.Zero;
        }

        // looks in grid for specified item
        private bool CheckGridItem(Vector3 vec, Game1.ITEMS item)
        {
            if (map._stuff[(int)vec.Y, (int)vec.X, (int)vec.Z] == (int)item)
            {
                return true;
            }
            return false;
        }


    }
}
