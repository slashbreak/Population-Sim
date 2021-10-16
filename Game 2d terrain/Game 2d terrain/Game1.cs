using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Game_2d_terrain
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        const int WIDTH = 1280;
        const int HEIGHT = 800;
        Random random = new Random();
        Player p;
        enum GameState
        {
            Playing,
            Waiting,
            Menu
        };


        public enum ITEMS
        {
            Nothing,
            Plant,
            Meat
        };
        public struct Item
        {
            public ITEMS id;
            public int weight;
            public string description;

        };
        int offsetX = 0;
        int offsetY = 0;
        int viewX = 0;
        int viewY = 0;
        int drawX;
        int drawY;
        int MAINSIZE = 2;

        //powers of 2
        public int TILE_SIZE = 16;
        public int TEXT_SIZE = 16;

        int nTilesCol = 900;
        int nTilesRow = 512;
        float mouseX;
        float mouseY;


        Rectangle textureColorRect;
        Rectangle drawRect;

        Vector2 drawPositions = Vector2.Zero;
        Color tileColor = Color.White;
        GameState gamestate = GameState.Playing;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        KeyboardState prevState;
        MouseState prevMouse;
        // all map textures
        List<Texture2D> texlist;


        Map map;

        //actors
        List<Entity> creatures;
        List<Entity> deadCreatures;
        int CREATURE_ID = 0; // increment for new creatures added

        //fps
        int _total_frames = 0;
        float _elapsed_time = 0.0f;
        int _fps = 0;

        // Draw toggle
        bool mapDraw = false;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here


            //this.IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = WIDTH;
            graphics.PreferredBackBufferHeight = HEIGHT;

            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            map = new Map();
            textureColorRect = new Rectangle(0, 0, 16, 16);
            drawRect = new Rectangle(0, 0, 16, 16);
            nTilesCol /= TILE_SIZE;
            nTilesRow /= TILE_SIZE;
            //SETUP INITIAL VIEWPORT
            //viewX = map.MAX_Y / 3 - 1;
            //viewY = map.MAX_X / 3 - 1;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            texlist = new List<Texture2D>();
            texlist.Add(Content.Load<Texture2D>("tiles"));
            texlist.Add(Content.Load<Texture2D>("food"));
            texlist.Add(Content.Load<Texture2D>("boy"));
            texlist.Add(Content.Load<Texture2D>("girl"));
            texlist.Add(Content.Load<Texture2D>("player"));
            texlist.Add(Content.Load<Texture2D>("mapcolor"));

            //add creatures
            creatures = new List<Entity>();
            p = new Player(10);
            //creatures.Add(new Entity(ref map, true, 2, new Vector3(0, 5, 0), new Vector3(map.MAX_X, map.MAX_Y, map.MAX_Z),200,TILE_SIZE));

            deadCreatures = new List<Entity>();
            font = Content.Load<SpriteFont>("SpriteFont1");
            map.Load();
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        float timer = 0;
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            // Update

            MouseState mstate = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();
            #region fpscounter
            _elapsed_time += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // 1 Second has passed
            if (_elapsed_time >= 1000.0f)
            {
                _fps = _total_frames;
                _total_frames = 0;
                _elapsed_time = 0;
            }
            #endregion

            #region MAP RESET/ TOGGLE
            if (keyState.IsKeyDown(Keys.R) &&
                    prevState.IsKeyUp(Keys.R))
            {

                map.Load();
            }


            if (keyState.IsKeyDown(Keys.M) &&
                 prevState.IsKeyUp(Keys.M))
            {

                mapDraw = !mapDraw;
            }
            #endregion



            #region PLAY PAUSE

            /////////////////
            // TURN BASED TOGGLE
           // gamestate = GameState.Waiting;

            ///////////////////
            if (keyState.IsKeyDown(Keys.P) &&
                 prevState.IsKeyUp(Keys.P))
            {
                gamestate = GameState.Playing;
            }
            if (keyState.IsKeyDown(Keys.L) &&
                 prevState.IsKeyUp(Keys.L))
            {
                gamestate = GameState.Waiting;
            }
            #endregion

            int dX = 0;
            int dY = 0;
            if (keyState.IsKeyDown(Keys.W) &&
                    prevState.IsKeyUp(Keys.W))
            {
                dY = -1;
                gamestate = GameState.Playing;
            }
            if (keyState.IsKeyDown(Keys.A) &&
                    prevState.IsKeyUp(Keys.A))
            {
                dX = -1;
                gamestate = GameState.Playing;
            }
            if (keyState.IsKeyDown(Keys.S) &&
                    prevState.IsKeyUp(Keys.S))
            {
                dY = 1;
                gamestate = GameState.Playing;
            }
            if (keyState.IsKeyDown(Keys.D) &&
                    prevState.IsKeyUp(Keys.D))
            {
                dX = 1;
                gamestate = GameState.Playing;
            }
            // Z level
            if (keyState.IsKeyDown(Keys.PageDown) &&
                 prevState.IsKeyUp(Keys.PageDown))
            {
                map.currentZ = (int)MathHelper.Clamp(++map.currentZ, 0, map.MAX_Z - 1);
            }

            if (keyState.IsKeyDown(Keys.PageUp) &&
                 prevState.IsKeyUp(Keys.PageUp))
            {
                map.currentZ = (int)MathHelper.Clamp(--map.currentZ, 0, map.MAX_Z - 1);

            }

            #region viewport movement

            if (keyState.IsKeyDown(Keys.OemPlus) &&
                 prevState.IsKeyUp(Keys.OemPlus))
            {
                TILE_SIZE *= 2;
                TEXT_SIZE *= 2;
                nTilesCol = 1024 / TILE_SIZE;
                nTilesRow = 512 / TILE_SIZE;
                //textureColorRect = new Rectangle(0, 0, TILE_SIZE, TILE_SIZE);

            }
            if (keyState.IsKeyDown(Keys.OemMinus) &&
                 prevState.IsKeyUp(Keys.OemMinus))
            {
                TILE_SIZE /= 2;
                TEXT_SIZE /= 2;
                nTilesCol = 1024 / TILE_SIZE;
                nTilesRow = 512 / TILE_SIZE;
                //textureColorRect = new Rectangle(0, 0, TILE_SIZE, TILE_SIZE);

            }

            if (keyState.IsKeyDown(Keys.Down))
            {
                viewY = (int)MathHelper.Clamp(viewY += 8, 0, map.MAX_X);
            }
            else if (keyState.IsKeyDown(Keys.Up))
            {
                viewY = (int)MathHelper.Clamp(viewY -= 8, 0, map.MAX_X);
            }
            else if (keyState.IsKeyDown(Keys.Left))
            {
                viewX = (int)MathHelper.Clamp(viewX -= 8, 0, map.MAX_Y);
            }
            else if (keyState.IsKeyDown(Keys.Right))
            {
                viewX = (int)MathHelper.Clamp(viewX += 8, 0, map.MAX_Y);
            }
            #endregion

            #region gamestates
            switch (gamestate)
            {
                case GameState.Waiting:

                    break;
                case GameState.Playing:

                    //timer += 100;
                    if (timer > 0)
                    {
                        //redo alive creature list each pass
                        List<Entity> alive = new List<Entity>();
                        foreach (Entity e in creatures)
                        {
                            e.Update(gameTime, ref map, mstate, TILE_SIZE);
                            if (e.alive == true)
                            {
                                alive.Add(e);
                            }
                            else
                            {
                                deadCreatures.Add(e);
                                // add food where creature died

                                map._stuff[(int)e._position.Y, (int)e._position.X, (int)e._position.Z] = (int)ITEMS.Plant;
                            }
                        }
                        creatures = alive;

                        p.Update(gameTime, ref map, mstate, TILE_SIZE, dX, dY);

                        timer = 0;
                    }
                    break;
            }
            #endregion

            #region debug commands
            // add girls
            if (keyState.IsKeyDown(Keys.Y) &&
                    prevState.IsKeyUp(Keys.Y))
            {
                for (int i = 0; i < 500; i++)
                    creatures.Add(
                        new Entity(
                            ++CREATURE_ID,
                            ref map,
                            false,
                            3,
                            new Vector3(random.Next(0, map.MAX_X - 1),
                            random.Next(0, map.MAX_Y - 1), map.currentZ),
                            new Vector3(map.MAX_X - 1, map.MAX_Y - 1, map.MAX_Z - 1),
                            random.Next(10, 150),
                            TILE_SIZE,
                            random.Next(1, 10)));
            }

            //add boys
            if (keyState.IsKeyDown(Keys.U) &&
                    prevState.IsKeyUp(Keys.U))
            {
                for (int i = 0; i < 500; i++)
                    creatures.Add(new Entity(++CREATURE_ID, ref map, true, 2,
                        new Vector3(random.Next(0, map.MAX_X - 1),
                            random.Next(0, map.MAX_Y - 1), map.currentZ),
                            new Vector3(map.MAX_X - 1, map.MAX_Y - 1, map.MAX_Z - 1), random.Next(10, 150), TILE_SIZE, random.Next(1, 10)));
            }

            if (keyState.IsKeyDown(Keys.J) &&
                    prevState.IsKeyUp(Keys.J))
            {
                map.SmoothMap();
                map.AssignTiles();
            }

            if (keyState.IsKeyDown(Keys.F) &&
             prevState.IsKeyUp(Keys.F))
            {
                int i;
                for (i = 1; i < 1000; i++)
                {
                    map._stuff[random.Next(1, map.MAX_X), random.Next(1, map.MAX_Y), map.currentZ] = (int)ITEMS.Plant;
                }
            }


            #region MOUSE INPUT
            mouseX = (mstate.X / TILE_SIZE) * TILE_SIZE;
            mouseY = (mstate.Y / TILE_SIZE) * TILE_SIZE;
            //mouseX = MathHelper.Clamp(mouseX, 0, WIDTH);
            //mouseY = MathHelper.Clamp(mouseY, 0, HEIGHT);

            //toggle creature info
            if ((mstate.LeftButton == ButtonState.Pressed) && (prevMouse.LeftButton == ButtonState.Released))
            {
                foreach (Entity e in creatures)
                {
                    if ((mouseX / TILE_SIZE + viewX == e._position.X) && (mouseY / TILE_SIZE + viewY == e._position.Y))
                    {
                        e.showInfo = !e.showInfo;
                    }
                }
            }
            if ((mstate.RightButton == ButtonState.Pressed))
            {

                map._stuff[(int)(mouseY / TILE_SIZE + viewY), (int)(mouseX / TILE_SIZE + viewX), map.currentZ] = (int)ITEMS.Plant;

            }


            #endregion


            prevState = keyState;
            prevMouse = mstate;

            #endregion

            map.Update();
            timer += gameTime.ElapsedGameTime.Milliseconds;
            base.Update(gameTime);
        }

        // sort out positions relative to screen and drawing
        protected override void Draw(GameTime gameTime)
        {
            _total_frames++;
            GraphicsDevice.Clear(Color.RosyBrown);
            spriteBatch.Begin();

            drawX = 0;
            drawY = 0;
            //draw map -- REMEMBER SCREEN OFFSETS


            #region draw loop
            for (int i = 0; i < nTilesRow; ++i)
            {
                drawY = i + viewY;
                for (int j = 0; j < nTilesCol; ++j)
                {
                    drawX = j + viewX;
                    #region map draw
                    if (drawY < map.MAX_Y - 1 && drawX < map.MAX_X - 1)
                    {
                        //above current Z
                        drawRect.X = offsetX + j * TILE_SIZE;
                        drawRect.Y = offsetY + i * TILE_SIZE;
                        drawRect.Width = TILE_SIZE;
                        drawRect.Height = TILE_SIZE;


                        //tileColor.R = 90;
                        //tileColor.G = (byte)(90 + map.heightmap[drawX, drawY]);
                        //tileColor.B = (byte)(49 + map.heightmap[drawX, drawY]);
                        textureColorRect.X = (map.heightmap[drawX, drawY] % 8) * 16;
                        if (mapDraw)
                        {
                            spriteBatch.Draw(
                                    texlist[0],
                                    drawRect,
                                    textureColorRect,
                                    Color.White
                                    );
                            //spriteBatch.Draw()
                        }
                        #region DRAW FOOD AND ITEMS
                        if (map._stuff[drawY, drawX, map.currentZ] != (int)ITEMS.Nothing)
                        {
                            spriteBatch.Draw(texlist[1], drawRect, Color.White);
                        }
                        #endregion
                    }
                    else
                    {
                        //outside map
                        spriteBatch.Draw(texlist[2], new Microsoft.Xna.Framework.Rectangle(offsetX + j * TEXT_SIZE, offsetY + i * TEXT_SIZE, TILE_SIZE, TILE_SIZE),
                                   Microsoft.Xna.Framework.Color.White);
                    }

                    #endregion






                    //DEAD AREA = OUTSIDE OF MAPSIZE




                }
            #endregion
            }
            #region x

            #region creaturedraw

            //// draw creatures-- REMEMBER SCREEN OFFSETS
            foreach (Entity e in creatures)
            {
                // draw creature
                if (e._position.Z == map.currentZ)
                {
                    if ((e._position.X - viewX < nTilesCol && e._position.Y - viewY < nTilesRow) &&
                        (e._position.X - viewX >= 0 && e._position.Y - viewY >= 0))
                    {
                        e.draw = true;
                        spriteBatch.Draw(texlist[e._color],
                            new Rectangle(
                                offsetX + (int)(e._position.X - viewX) * TILE_SIZE,
                                offsetY + (int)(e._position.Y - viewY) * TILE_SIZE,
                                TILE_SIZE, TILE_SIZE), e.state == Entity.EntityState.Idle ? Color.White : Color.Blue);
                    }
                }
                // creature info
                if (e.showInfo == true && e.draw == true)
                {
                    spriteBatch.DrawString(font,
                        "NAME:" + e._id +
                        "\nX: " + e._position.X.ToString() +
                        "Y: " + e._position.Y.ToString() +
                        "Z: " + e._position.Z.ToString() +
                         "\nSpeed: " + e._updateSpeed.ToString() +
                          "\nSight: " + e._sight.ToString() +

                        "\n" + (e.gender == true ? "BOY" : "GIRL") +
                        "\nHUNGER: " + e._hunger.ToString(),

                        new Vector2(
                            e._position.X * TILE_SIZE - viewX * TILE_SIZE,
                            e._position.Y * TILE_SIZE - viewY * TILE_SIZE),
                            e.state == Entity.EntityState.Idle ? Color.White : Color.Blue);

                    e.draw = false;
                }
            }

            #endregion
            if ((p._position.X - viewX < nTilesCol && p._position.Y - viewY < nTilesRow) &&
                        (p._position.X - viewX >= 0 && p._position.Y - viewY >= 0))
            {
                spriteBatch.Draw(
                    texlist[4],
                    new Rectangle(offsetX + (int)(p._position.X - viewX) * TILE_SIZE,
                                  offsetY + (int)(p._position.Y - viewY) * TILE_SIZE, TILE_SIZE, TILE_SIZE), Color.White);
            }
            // debug log info
            spriteBatch.DrawString(font, "depth: " + map.currentZ.ToString(), new Vector2(900, 10), Color.Black);
            spriteBatch.DrawString(font, "alive: " + creatures.Count.ToString(), new Vector2(900, 20), Color.Black);
            spriteBatch.DrawString(font, "dead: " + deadCreatures.Count.ToString(), new Vector2(900, 30), Color.Black);
            spriteBatch.DrawString(font, "X: " + mouseX.ToString() + " Y: " + mouseY.ToString(), new Vector2(900, 40), Color.Black);
            spriteBatch.DrawString(font, "tilesize: " + TILE_SIZE.ToString(), new Vector2(900, 50), Color.Black);
            //spriteBatch.DrawString(font, "dead: " + deadCreatures.Count.ToString(), new Vector2(900, 60), Color.Black);


            //draw mouse pointer
            spriteBatch.Draw(texlist[0], new Rectangle((int)mouseX, (int)mouseY, TILE_SIZE, TILE_SIZE), Color.Black);




            //// map value logger
            //float offX = 700;
            //float offY = 300;
            //for (int i = 0; i < map.MAX_X; i++)
            //{
            //    for (int j = 0; j < map.MAX_Y; j++)
            //    {
            //        //spriteBatch.DrawString(font, map._m[i, j, map.currentZ].ToString() + " ",new Vector2(offX+j*20,offY+i*20), Color.Black);
            //        spriteBatch.DrawString(font, map.heightmap[i, j].ToString() + " ", new Vector2(offX + j * 10, offY + i * 10), Color.Black);
            //    }
            //    //spriteBatch.DrawString(font, " ", new Vector2(offX + i * 20, offY + j * 20), Color.Black);
            //}


            //// FPS DRAW 
            spriteBatch.DrawString(font, string.Format("FPS=  {0}", _fps),
                new Vector2(10.0f, 700.0f), Color.White);

            #endregion

            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
