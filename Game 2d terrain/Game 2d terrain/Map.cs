using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Game_2d_terrain
{
    public class Map
    {
        
        Random rand = new Random();
       public enum Tile
        {
            AIR = 0,
            GRASS = 1,
            HILL = 2,
            CLIFF = 3,
            MOUNTAIN = 4,
            SOIL = 5,
            WATER = 6

        };

        
        public int[,] heightmap;
        
        

        const int PEAK_MAX = 8;
        public int currentZ = 0;

        public const int GRIDSIZE = 513;
        //public const int GRIDSIZE = 1025;
        public int MAX_X = GRIDSIZE;
        public int MAX_Y = GRIDSIZE;
        public int MAX_Z = 32;
        int ROUGHNESS = GRIDSIZE/2;
        public int[, ,] _m;
        public int[, ,] _stuff;
        int peakSize = PEAK_MAX;
        public Map()
        {
            _m = new int[MAX_X, MAX_Y, MAX_Z] ;
            _stuff = new int[MAX_X, MAX_Y, MAX_Z];
            heightmap = new int[MAX_X, MAX_Y];

        }

        private void ClearMap()
        {
            for (int i = 0; i < MAX_X; ++i)
            {
                for (int j = 0; j < MAX_Y; ++j)
                {
                    for (int k = 0; k < MAX_Z; ++k)
                    {
                        _m[i, j, k] = 0;
                    }
                }
            }
        }

        private void ClearHeightMap()
        {

            for (int i = 0; i < MAX_X; ++i)
            {
                for (int j = 0; j < MAX_Y; ++j)
                {
                    heightmap[i, j] = 0;
                }
            }
        }
        // set up map, load separate areas/regions
        private void GenerateHeightMap()
        {
            
            heightmap[0, 0] = heightmap[0, MAX_Y - 1] = heightmap[MAX_X - 1, 0] = heightmap[MAX_X - 1, MAX_Y - 1] = (int)peakSize;



            for (int i = 0; i < MAX_X; ++i)
            {
                for (int j = 0; j < MAX_Y; ++j)
                {
                    heightmap[i, j] = rand.Next(peakSize / 2, peakSize);
                }
            } 
            peakSize = MidpointDisplacement();
        }

        private int MidpointDisplacement()
        {
            int rough = ROUGHNESS;
            for (int m = MAX_X - 1; m >= 2; m /= 2, rough/=2)
            {
                int halfside = m / 2;
                for (int x = 0; x < MAX_X - 1; x += m)
                {
                    for (int y = 0; y < MAX_Y - 1; y += m)
                    {
                        int avg = (heightmap[x, y + m] + heightmap[x + m, y] + heightmap[x, y] + heightmap[x + m, y + m]) / 4;
                        avg = avg + (int)(rand.NextDouble()  *2 * rough) - rough;
                        heightmap[x + halfside, y + halfside] =  (int)MathHelper.Clamp(avg, 0, peakSize);
                        if (heightmap[x + halfside, y + halfside] > peakSize) peakSize = heightmap[x + halfside, y + halfside];
                    }
                }

                //generate the diamond values
                //since the diamonds are staggered we only move x
                //by half side
                //NOTE: if the data shouldn't wrap then x < MAX_X
                //to generate the far edge values
                for (int x = 0; x < MAX_X - 1; x += halfside)
                {
                    //and y is x offset by half a side, but moved by
                    //the full side length
                    //NOTE: if the data shouldn't wrap then y < MAX_X
                    //to generate the far edge values
                    for (int y = (x + halfside) % m; y < MAX_Y - 1; y += m)
                    {
                        //x, y is center of diamond
                        //note we must use mod  and add MAX_X for subtraction 
                        //so that we can wrap around the array to find the corners
                        int avg =
                          heightmap[(x - halfside + (MAX_X-1)) % (MAX_X-1),y] + //left of center
                         heightmap[(x + halfside) % MAX_X-1,y] + //right of center
                          heightmap[x,(y + halfside) % MAX_X-1] + //below center
                          heightmap[x,(y - halfside + (MAX_X-1)) % (MAX_X-1)]; //above center
                        avg /= 4;

                        //new value = average plus random offset
                        //We calculate random value in range of 2h
                        //and then subtract h so the end value is
                        //in the range (-h, +h)
                        avg = avg + (int)(rand.NextDouble() *2* rough)-rough;
                        //update value for center of diamond
                        heightmap[x, y] = (int)MathHelper.Clamp(avg,0,peakSize);
                        if (heightmap[x, y] > peakSize) peakSize = heightmap[x, y];

                        //wrap values on the edges, remove
                        //this and adjust loop condition above
                        //for non-wrapping values.
                        if (x == 0) heightmap[MAX_X - 1, y] =  (int)MathHelper.Clamp(avg, 0, peakSize);
                        if (y == 0) heightmap[x, MAX_X - 1] = (int)MathHelper.Clamp(avg, 0, peakSize);
                    }
                }
            }
            return peakSize;
        }
    
        

        public void SmoothMap()
        {
            int[,] tempMap = heightmap;
            for (int i = 0; i < MAX_X; ++i)
            {
                for (int j = 0; j < MAX_Y; ++j)
                {
                    tempMap[i,j] = AvgPoint(i, j);
                }
            }
            heightmap = tempMap;

        }

        //average the heightmap point for point.
        private int AvgPoint(int x, int y)
        {
           // bit of cellular automata.

           // rules - if (neighbour height >= own height) for 7 neighbours, do nothing. else decrement own height by 1;
            int tempX, tempY;
            int sum = 0;
            int isTaller = 0;
            for (int di = -1; di <= 1; di++)
            {
                tempX = x + di;

                for (int dj = -1; dj <= 1; ++dj)
                {
                    tempY = y + dj;


                    if (!(tempX == x && tempY == y) && //check not central square
                        (tempX >= 0 && tempX < MAX_X) && (tempY >= 0 && tempY < MAX_Y))
                    {
                        if (heightmap[tempX, tempY] >= heightmap[x, y])
                        {
                            isTaller++;
                        }
                    }
                   
                }
            }
            if (isTaller > 7)
            {
                sum = heightmap[x,y];
                return sum;
            }

            else
            {
                sum = heightmap[x, y]-1;
                return sum <0? 0:sum;
            }


        }

        public Tile ChooseTile(int height)
        {
            
            return Tile.AIR;
        }
        public void Load()
        {
            peakSize = PEAK_MAX;
            
            ClearHeightMap();
            
            GenerateHeightMap();
            AssignTiles();
           //SmoothMap();
           //SmoothMap();
            //currentZ = 0;     
        }

        public void AssignTiles()
        {
            ClearMap();
           
            int currentTile = 0;

            for (int i = 0; i < MAX_X; ++i)
            {
                for (int j = 0; j < MAX_Y; ++j)
                {
                    currentTile = heightmap[i, j];
                    //for (int k = 0; k < MAX_Z; ++k)
                    //{
                    //    // we want Z=0 to be the top of our map, showing only the highest peaks. k=0 => peakSize
                    //    // if (k <= peakSize)
                    //    // {
                    //    if (currentTile == peakSize - k)
                    //    {
                    //        _m[i, j, k] = currentTile;
                    //    }
                    //    else if (currentTile < peakSize - k)
                    //    {
                    //        // this is AIR SPACE
                    //        _m[i, j, k] = -1;
                    //    }
                    //    else
                    //    {
                    //        // UNDERGROUND
                    //        _m[i, j, k] = 200;
                    //    }
                    //    #region tile
                    //    //Tile tile;
                    //    //if (currentTile >= 6) tile = Tile.MOUNTAIN;
                    //    //else if (currentTile > 3) tile = Tile.CLIFF;
                    //    //else if (currentTile > 1) tile = Tile.HILL;
                    //    //else if (currentTile > 0) tile = Tile.GRASS;
                    //    //else if (currentTile == 0) tile = Tile.SOIL;
                    //    //else tile = Tile.AIR;

                    //    //    switch (tile)
                    //    //{
                    //    //    case Tile.AIR:
                    //    //        _m[i, j, k] = 0;
                    //    //        break;
                    //    //    case Tile.GRASS:
                    //    //        _m[i, j, k] = 1;
                    //    //        break;
                    //    //    case Tile.HILL:
                    //    //        _m[i, j, k] = 2;
                    //    //        break;
                    //    //    case Tile.CLIFF:
                    //    //        _m[i, j, k] = 3;
                    //    //        break;
                    //    //    case Tile.MOUNTAIN:
                    //    //        _m[i, j, k] = 4;
                    //    //        break;
                    //    //    case Tile.SOIL:
                    //    //        _m[i, j, k] = 5;
                    //    //        break;
                    //    //    case Tile.WATER:
                    //    //        _m[i, j, k] = 6;
                    //    //        break;
                    //    //    default:
                    //    //        _m[i, j, k] = 0;
                    //    //        break;
                    //    //}

                    //    ////}
                    //    #endregion
                        

                    //}
                }
            }
        }

        public int Update()
        {
            //AddWater(0);
            return 0;
        }
        public void AddWater(int z)
        {
            int size = rand.Next(0,2);
            int x = 30;
            int y = 30;
            for (int dy = -5; dy < 4; dy++)
            {
                y += size + dy ;
                for (int dx = 2; dx < 3; dx++)
                {
                    x += size + dx;



                    _m[x, y, z] = (int)Tile.WATER;

                }
            }
        }
    }
}

