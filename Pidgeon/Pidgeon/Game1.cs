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

namespace Pidgeon
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        #region Fields

        float maxHeight;

        CModel flame1;
        CModel flame2;
        CModel flame3;
        CModel currentflame;

        GraphicsDeviceManager graphics;

        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        int lives, score;

        Texture2D life, speed, newGameTex, paused;

        BasicEffect boxEffect;

        Texture2D minimap;
        Texture2D briefcase;
        Texture2D dude;
        private Texture2D controls;
        private Texture2D deathScreen;
        private Texture2D creditsTex;

        CModel skyBox;

        // Create a list of building objects so we can do fancy stuff like shade the correct building red or some bollocks liek that

        List<Buildings> _buildings = new List<Buildings>();
        List<PickupObject> _objects = new List<PickupObject>();
        List<HudText> _hudTextStuff = new List<HudText>();
        KeyboardState lastKeyboardState = new KeyboardState();
        GamePadState lastGamePadState = new GamePadState();
        MouseState lastMousState = new MouseState();
        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();
        MouseState currentMouseState = new MouseState();

        TimeSpan timeLeft;
        private TimeSpan creditTime = TimeSpan.FromSeconds(10);
        private TimeSpan controlTimer = TimeSpan.FromSeconds(6);
        private TimeSpan crashTimer = TimeSpan.FromSeconds(2);

        bool showCollisions;

        Ship ship;
        ChaseCamera camera;
        private BuisnessMan man;

        private int timerIncrease;

        CModel ground;

        Random _random = new Random();

        #region Audio strings

        private const string PickupSound = "Sounds/SFX/pickup";
        private const string ManSpawnSound = "Sounds/Man/manspawn";
        private const string BgMusic = "Sounds/Music/bg2";

        #endregion Audio strings

        bool gameOver;

        int rI;

        int buildingDistance;

        private enum State
        {
            Playing,
            Paused,
            Gameover,
            NewGame,
            Credits
        }

        private State _gameState;

        Model manRunning;
        Model manStanding;
        Model manFalling;

        #endregion Fields

        #region Initialization

        public Game1()
        {
            showCollisions = false;
            graphics = new GraphicsDeviceManager(this);
            graphics.SupportedOrientations = DisplayOrientation.Portrait;

            maxHeight = 0;

            lives = 3;
            score = 0;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Initialize the sound manager
            SoundManager.Initialize(this);

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;

            timerIncrease = 30;

            // Create the chase camera
            camera = new ChaseCamera();

            // Set the camera offsets
            camera.DesiredPositionOffset = new Vector3(0.0f, 2000.0f, 3500.0f);
            camera.LookAtOffset = new Vector3(0.0f, 150.0f, 0.0f);

            // Set camera perspective
            camera.NearPlaneDistance = 10.0f;
            camera.FarPlaneDistance = 10000000.0f;

            buildingDistance = 200000;
            //TODO: Set any other camera invariants here such as field of view

            // If you can't understand what this line means, you need lamped you dozy bastard
            _gameState = State.NewGame;
        }

        /// <summary>
        /// Initalize the game
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            gameOver = false;

            timeLeft = TimeSpan.FromSeconds(60);

            // Set the camera aspect ratio
            // This must be done after the class to base.Initalize() which will
            // initialize the graphics device.
            camera.AspectRatio = (float)graphics.GraphicsDevice.Viewport.Width /
                graphics.GraphicsDevice.Viewport.Height;

            // Perform an inital reset on the camera so that it starts at the resting
            // position. If we don't do this, the camera will start at the origin and
            // race across the world to get behind the chased object.
            // This is performed here because the aspect ratio is needed by Reset.
            UpdateCameraChaseTarget();
            camera.Reset();

            for (int i = 0; i < 45; i++)
                // Get a random element in the list and spawn the player on top of that element
                NewManPos();
            // Spawn objects
            SpawnObjects();

            // If you can't understand what this line means, you need lamped you dozy bastard
            SoundManager.LoadSound(PickupSound);
            SoundManager.LoadSound(ManSpawnSound);
            NewManPos();
        }

        /// <summary>
        /// Load graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            flame1 = new CModel(Content.Load<Model>("Models/Flame_1"), GraphicsDevice, Matrix.Identity, boxEffect);
            flame2 = new CModel(Content.Load<Model>("Models/Flame_2"), GraphicsDevice, Matrix.Identity, boxEffect);
            flame3 = new CModel(Content.Load<Model>("Models/Flame_3"), GraphicsDevice, Matrix.Identity, boxEffect);

            currentflame = flame3;

            boxEffect = new BasicEffect(GraphicsDevice);

            life = Content.Load<Texture2D>("Textures/Hud/life");
            briefcase = Content.Load<Texture2D>("Textures/Hud/Breifcase_UI");
            speed = Content.Load<Texture2D>("Textures/Hud/speed");
            minimap = Content.Load<Texture2D>("Textures/Hud/minimap");
            dude = Content.Load<Texture2D>("Textures/Hud/dude");
            controls = Content.Load<Texture2D>("Textures/Hud/Controls");

            newGameTex = Content.Load<Texture2D>("Textures/Screens/NewGame");
            paused = Content.Load<Texture2D>("Textures/Screens/paused");
            deathScreen = Content.Load<Texture2D>("Textures/Screens/Death_Screen");
            creditsTex = Content.Load<Texture2D>("Textures/Screens/Credits");

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("gameFont");

            ship = new Ship(Content.Load<Model>("Models/Actors/Pidgeon_1"), GraphicsDevice, boxEffect);
            ground = new CModel(Content.Load<Model>("ground"), GraphicsDevice, Matrix.CreateScale(500.0f, 500.0f, 500.0f), boxEffect);

            manStanding = Content.Load<Model>("Dude_Standing");
            manRunning = Content.Load<Model>("Dude_Running");
            manFalling = Content.Load<Model>("Dude_Falling");

            Model load;
            for (int count = 1; count < 5; count++)
                load = Content.Load<Model>("Models/object_" + count.ToString());

            man = new BuisnessMan(manStanding, GraphicsDevice, Matrix.Identity, boxEffect);
            man.collision = new CModel(Content.Load<Model>("Collison_Cylinder"), GraphicsDevice, Matrix.Identity, boxEffect);
            skyBox = new CModel(Content.Load<Model>("skybox"), GraphicsDevice, Matrix.Identity, boxEffect);

            #region Init Buildings

            // Add the buildings to the list with a random type
            // (i.e, the random building could be a small one with no windows, or a really tall one with loads of windows!

            for (int i = 0; i < 100; i++)
            {
                Buildings building = null;
                bool collision = true;
                while (collision == true)
                {
                    collision = false;
                    var randomB = _random.Next(2, 9);
                    building = new Buildings(Content.Load<Model>("Models/Buildings/Building_" + randomB),
                        GraphicsDevice,
                        Matrix.CreateScale(20f) *
                        Matrix.CreateTranslation(_random.Next(-buildingDistance, buildingDistance), 0, _random.Next(-buildingDistance, buildingDistance)),
                        boxEffect);

                    building.UpdateCollisions();

                    // Need to check if the spawning model is intersecting with a previously spawned model and chuck it if it is
                    foreach (var b in _buildings)
                    {
                        if (building.CheckCollisionWith(b))
                        {
                            collision = true;
                        }
                    }
                }

                if (building.boundingBoxes.Max().Max.Y > maxHeight)
                    maxHeight = building.boundingBoxes.Max().Max.Y;

                _buildings.Add(building);
                Console.WriteLine("Spawning building at new random position");
            }

            ship.SpawnHeight = maxHeight + 1000;

            ship.Reset();

            #endregion Init Buildings
        }

        #endregion Initialization

        #region Update and Draw

        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            lastKeyboardState = currentKeyboardState;
            lastGamePadState = currentGamePadState;
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            if (ship.thrustAmount > 1.5f)
                currentflame = flame1;
            else if (ship.thrustAmount > 1.0f)
                currentflame = flame2;
            else if (ship.thrustAmount > 0.5f)
                currentflame = flame3;

            #region If State = New Game

            if (_gameState == State.NewGame)
            {
                if (currentKeyboardState.IsKeyUp(Keys.Enter) && lastKeyboardState.IsKeyDown(Keys.Enter))
                {
                    _gameState = State.Playing;
                    return;
                }
            }
            if (_gameState == State.Gameover)
            {
                if (currentKeyboardState.IsKeyUp(Keys.Enter) && lastKeyboardState.IsKeyDown(Keys.Enter))
                {
                    timeLeft = TimeSpan.FromSeconds(60);
                    man.Model = manStanding;
                    camera.DesiredPositionOffset = new Vector3(0.0f, 2000.0f, 3500.0f);
                    gameOver = false;
                    _gameState = State.Playing;
                    ship.Reset();
                    camera.Reset();
                    lives = 3;
                    _objects.Clear();
                    SpawnObjects();
                    NewManPos();
                    return;
                }

                if (currentKeyboardState.IsKeyUp(Keys.Escape) && lastKeyboardState.IsKeyDown(Keys.Escape))
                {
                    _gameState = State.Credits;
                }
                return;
            }

            #endregion If State = New Game

            #region If State = Paused

            if (_gameState == State.Paused)
            {
                if (currentKeyboardState.IsKeyUp(Keys.P) && lastKeyboardState.IsKeyDown(Keys.P))
                    _gameState = State.Playing;
                return;
            }

            #endregion If State = Paused

            #region if state = playing

            if (_gameState == State.Playing)
            {
                if (lastKeyboardState.IsKeyUp(Keys.I) && currentKeyboardState.IsKeyDown(Keys.I))
                    ship.inverted = !ship.inverted;
                timeLeft -= gameTime.ElapsedGameTime;
                controlTimer -= gameTime.ElapsedGameTime;

                if (timeLeft.TotalMilliseconds <= 0)
                {
                    gameOver = true;
                }

                if (currentKeyboardState.IsKeyDown(Keys.G))
                    gameOver = true;

                if (currentKeyboardState.IsKeyUp(Keys.P) && lastKeyboardState.IsKeyDown(Keys.P))
                    _gameState = State.Paused;

                if (currentKeyboardState.IsKeyUp(Keys.Enter) && lastKeyboardState.IsKeyDown(Keys.Enter))
                    _gameState = State.Paused;

                if (gameOver)
                {
                    UpdateCameraChaseTarget();
                    camera.Reset();
                    camera.DesiredPositionOffset = new Vector3(0.0f, 40000.0f, 70000.0f);

                    if (man.World.Translation.Z + (man.Width / 2) <=
                        _buildings[rI].World.Translation.Z + (_buildings[rI].Width / 2))
                    {
                        man.Model = manRunning;
                        man.World *= Matrix.CreateTranslation(0.0f, 0.0f, 50.0f);
                    }
                    else if (man.World.Translation.Y > 0)
                    {
                        man.Model = manFalling;
                        man.World *= Matrix.CreateTranslation(0.0f, -100.0f, 0.0f);
                    }
                    else
                        _gameState = State.Gameover;

                    base.Update(gameTime);
                    return;
                }

                if ((currentKeyboardState.IsKeyDown(Keys.F1) && lastKeyboardState.IsKeyUp(Keys.F1)) ||
                    (currentGamePadState.IsButtonDown(Buttons.Back) && lastGamePadState.IsButtonUp(Buttons.Back)))
                {
                    showCollisions = !showCollisions;
                }
                // Exit when the Escape key or Back button is pressed
                if (currentKeyboardState.IsKeyDown(Keys.Escape) || currentGamePadState.IsButtonDown(Buttons.Start))
                {
                    Exit();
                }

                bool touchTopLeft = currentMouseState.LeftButton == ButtonState.Pressed &&
                                    lastMousState.LeftButton != ButtonState.Pressed &&
                                    currentMouseState.X < GraphicsDevice.Viewport.Width / 10 &&
                                    currentMouseState.Y < GraphicsDevice.Viewport.Height / 10;

                if (lastKeyboardState.IsKeyUp(Keys.E) &&
                    (currentKeyboardState.IsKeyDown(Keys.E)))
                    NewManPos();

                // Reset the ship on R key or right thumb stick clicked
                if (currentKeyboardState.IsKeyDown(Keys.R))
                {
                    ship.Reset();
                    camera.Reset();
                }

                // Update the ship
                ship.Update(gameTime);

                // Update the camera to chase the new target
                UpdateCameraChaseTarget();

                // Update the business man's objects
                UpdateBusObjects();

                // The chase camera's update behavior is the springs
                camera.Update(gameTime);

                if (_hudTextStuff.Count > 0)
                {
                    for (int i = _hudTextStuff.Count - 1; i >= 0; i--)
                    {
                        _hudTextStuff[i].Update(gameTime);
                        if (_hudTextStuff[i].DoneDrawing)
                            _hudTextStuff.RemoveAt(i);
                    }
                }

                foreach (Buildings check in _buildings)
                {
                    int lol;
                    if (check == _buildings[rI])
                        lol = 1;
                    check.UpdateCollisions();
                    if (ship.CheckCollisionWith(check))
                    {
                        _hudTextStuff.Add(new HudText((Content.Load<Texture2D>("Textures/Hud/crashed")), TimeSpan.FromSeconds(3), new Vector2(400, 300)));
                        lives--;
                        if (lives <= 0)
                            gameOver = true;

                        ship.Reset();
                    }
                }

                if (ship.CheckCollisionWith(man))
                    if (_objects.Count == 0)
                    {
                        score += 100;
                        timeLeft += TimeSpan.FromSeconds(40);
                        Console.WriteLine("New Score : " + score);
                        // Spawn the objects again at NEW random locations
                        // Make a new Business man at a new random location
                        SpawnObjects();
                        NewManPos();
                    }

                skyBox.World = Matrix.CreateTranslation(camera.Position - new Vector3(0.0f, 100.0f, 0.0f));
                base.Update(gameTime);
            }

            #endregion if state = playing

            if (_gameState == State.Credits)
            {
                creditTime -= gameTime.ElapsedGameTime;
                if (creditTime.TotalSeconds <= 0)
                    Exit();
            }
        }

        private void UpdateBusObjects()
        {
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                _objects[i].pickUp.World = Matrix.CreateRotationY(0.05f) * _objects[i].pickUp.World;

                if (ship.CheckCollisionWith(_objects[i].collision))
                {
                    _hudTextStuff.Add(new HudText(Content.Load<Texture2D>("Textures/Hud/PickupItem"), TimeSpan.FromSeconds(3), new Vector2(400, 300)));
                    Console.WriteLine("Bird has collided with an object, PARTY TYME");
                    timeLeft += TimeSpan.FromSeconds(2);
                    SoundManager.PlaySound(PickupSound);
                    _objects.RemoveAt(i);

                    if (_objects.Count == 0)
                    {
                        _hudTextStuff.Add(new HudText((Content.Load<Texture2D>("Textures/Hud/return")), TimeSpan.FromSeconds(3), new Vector2(400, 200)));
                    }
                    score += 30;
                }
            }
        }

        /// <summary>
        /// Update the values to be chased by the camera
        /// </summary>
        private void UpdateCameraChaseTarget()
        {
            if (gameOver)
            {
                camera.ChasePosition = man.World.Translation;
                camera.Up = Vector3.Up;
                return;
            }
            camera.ChasePosition = ship.Position;
            camera.ChaseDirection = ship.Direction;
            camera.Up = ship.Up;
        }

        /// <summary>
        /// Draws the ship and ground.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            device.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            GraphicsDevice.DepthStencilState = DepthStencilState.None;

            #region If State = Paused

            if (_gameState == State.Paused)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(paused, new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2), null, Color.White, 0f, Vector2.Zero, new Vector2(2, 2), SpriteEffects.None, 1f);
                spriteBatch.End();
            }

            #endregion If State = Paused

            if (_gameState == State.NewGame)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(newGameTex, Vector2.Zero, Color.White);
                spriteBatch.End();
            }

            #region If State = Playing

            if (_gameState == State.Playing)
            {
                Matrix skyView = camera.View;

                //skyView.M41 = 0;
                //skyView.M42 = 0;
                //skyView.M43 = 0;

                skyBox.Lighting = false;

                skyBox.Draw(skyView, camera.Projection, GraphicsDevice);

                SoundManager.PlayMusic(BgMusic);

                GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                ship.Draw(camera.View, camera.Projection, GraphicsDevice);
                if (showCollisions)
                    ship.DrawCollisions(camera, GraphicsDevice);

                ground.Draw(camera.View, camera.Projection, GraphicsDevice);

                man.Draw(camera.View, camera.Projection, GraphicsDevice);
                if (showCollisions)
                    man.DrawCollisions(camera, GraphicsDevice);

                if (_objects.Count == 0)
                {
                    man.collision.Draw(camera.View, camera.Projection, GraphicsDevice);
                    if (showCollisions)
                        man.collision.DrawCollisions(camera, GraphicsDevice);
                }

                // Draw all the buildings in the list
                foreach (var b in _buildings)
                {
                    b.Draw(camera.View, camera.Projection, GraphicsDevice);

                    if (showCollisions)
                        b.DrawCollisions(camera, GraphicsDevice);
                }

                foreach (var o in _objects)
                {
                    o.pickUp.Draw(camera.View, camera.Projection, GraphicsDevice);
                    if (showCollisions)
                        o.pickUp.DrawCollisions(camera, GraphicsDevice);
                }

                foreach (var o in _objects)
                {
                    GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                    o.collision.Draw(camera.View, camera.Projection, GraphicsDevice);
                    o.collision.UpdateCollisions();
                    if (showCollisions)
                        o.collision.DrawCollisions(camera, GraphicsDevice);
                    GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
                }

                currentflame.World = ship.World;
                currentflame.Draw(camera.View, camera.Projection, GraphicsDevice);

                foreach (var hudText in _hudTextStuff)
                {
                    hudText.Draw(spriteBatch);
                }

                DrawOverlay();

                base.Draw(gameTime);
            }

            #endregion If State = Playing

            if (_gameState == State.Gameover)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(deathScreen, Vector2.Zero, Color.White);
                spriteBatch.End();
            }

            if (_gameState == State.Credits)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(creditsTex, Vector2.Zero, Color.White);
                spriteBatch.End();
            }
        }

        /// <summary>
        /// Displays an overlay showing what the controls are,
        /// and which settings are currently selected.
        /// </summary>
        private void DrawOverlay()
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            if (controlTimer.TotalSeconds > 0)
            {
                spriteBatch.Draw(controls, Vector2.Zero, Color.White);
            }

            spriteBatch.DrawString(spriteFont, timeLeft.ToString(@"mm\:ss"),
                new Vector2((graphics.PreferredBackBufferWidth / 2) - (spriteFont.MeasureString(timeLeft.ToString(@"mm\:ss")).X / 2),
                graphics.PreferredBackBufferHeight - spriteFont.MeasureString(timeLeft.ToString(@"mm\:ss")).Y - 20), Color.White);
            spriteBatch.DrawString(spriteFont, "Score: " + score.ToString(),
                new Vector2(20, graphics.PreferredBackBufferHeight - spriteFont.MeasureString("Score: " + score.ToString()).Y - 20), Color.White);

            for (int i = 0; i < lives; i++)
            {
                spriteBatch.Draw(life, new Rectangle(20 + (i * 75), 20, 75, 75), Color.White);
            }

            for (int i = 0; i < _objects.Count; i++)
            {
                spriteBatch.Draw(briefcase, new Rectangle(graphics.PreferredBackBufferWidth - (i * 75) - 95, 20, 75, 75), Color.White);
            }

            DrawMiniMap();

            spriteBatch.End();
        }

        private void DrawMiniMap()
        {
            spriteBatch.Draw(speed, new Rectangle(300, 20, (int)(200 * (ship.thrustAmount / Ship.MAX_SPEED)), 50), new Rectangle(0, 0, (int)((ship.thrustAmount / Ship.MAX_SPEED) * speed.Width), 0), Color.White);

            spriteBatch.Draw(minimap, new Rectangle(graphics.PreferredBackBufferWidth - 150, graphics.PreferredBackBufferHeight - 150, 150, 150), Color.White);

            Vector2 mapcentre = new Vector2(graphics.PreferredBackBufferWidth - 75, graphics.PreferredBackBufferHeight - 75);

            int itemSize = 10;

            foreach (var o in _objects)
            {
                Vector2 distance = (new Vector2(o.pickUp.World.Translation.X, o.pickUp.World.Translation.Z) / new Vector2(buildingDistance));

                if (distance.X > 0.8f)
                    distance.X = 0.8f;
                if (distance.Y > 0.8f)
                    distance.Y = 0.8f;


                if (distance.X < -0.8f)
                    distance.X = -0.8f;
                if (distance.Y < -0.8f)
                    distance.Y = -0.8f;

                spriteBatch.Draw(briefcase, new Rectangle((int)(mapcentre.X + (distance.X * 75) - itemSize), (int)(mapcentre.Y + (distance.Y * 75) - itemSize), itemSize, itemSize), Color.White);
            }

            Vector2 shipDistance = (new Vector2(ship.World.Translation.X, ship.World.Translation.Z) / new Vector2(buildingDistance));

            if (shipDistance.X > 0.8f)
                shipDistance.X = 0.8f;
            if (shipDistance.Y > 0.8f)
                shipDistance.Y = 0.8f;

            if (shipDistance.X < -0.8f)
                shipDistance.X = -0.8f;
            if (shipDistance.Y < -0.8f)
                shipDistance.Y = -0.8f;

            spriteBatch.Draw(life, new Rectangle((int)(mapcentre.X + (shipDistance.X * 75) - itemSize), (int)(mapcentre.Y + (shipDistance.Y * 75) - itemSize), itemSize, itemSize), Color.White);

            shipDistance = (new Vector2(man.World.Translation.X, man.World.Translation.Z) / new Vector2(buildingDistance));

            if (shipDistance.X > 0.8f)
                shipDistance.X = 0.8f;
            if (shipDistance.Y > 0.8f)
                shipDistance.Y = 0.8f;

            if (shipDistance.X < -0.8f)
                shipDistance.X = -0.8f;
            if (shipDistance.Y < -0.8f)
                shipDistance.Y = -0.8f;

            spriteBatch.Draw(dude, new Rectangle((int)(mapcentre.X + (shipDistance.X * 75) - itemSize), (int)(mapcentre.Y + (shipDistance.Y * 75) - itemSize), itemSize, itemSize), Color.White);
        }

        #endregion Update and Draw

        private void NewManPos()
        {
            // He's gonna kill himself
            SoundManager.PlaySound(ManSpawnSound);
            // Get a random element in the list and spawn the player on top of that element
            rI = _random.Next(0, _buildings.Count);

            man.World = _buildings[rI].World
                * Matrix.CreateTranslation(0f, _buildings[rI].boundingBoxes.Max().Max.Y, 0);
            man.UpdateCollisions();
            man.collision.World = man.World;
            man.UpdateCollisions();
        }

        private void SpawnObjects()
        {
            #region objects (rough)

            // Create the objects and have them spawn on top of a random skyscraper
            for (int i = 0; i < 3; i++)
            {
                PickupObject obj = new PickupObject(i);
                obj.NumOF = _random.Next(0, _buildings.Count);
                if (obj._id > 1)
                {
                    foreach (var o in _objects)
                    {
                        // Check if the building already has object on it, if it does get a new number
                        if (o.NumOF == rI)
                        {
                            obj.NumOF = _random.Next(0, _buildings.Count);
                        }
                    }
                }

                obj.collision = new CModel(Content.Load<Model>("Collison_Cylinder"), GraphicsDevice, Matrix.Identity, boxEffect);
                obj.pickUp = new CModel(Content.Load<Model>("Models/object_" + _random.Next(1, 4)),
                    GraphicsDevice,
                    Matrix.CreateScale(5) *
                    Matrix.CreateTranslation(_buildings[obj.NumOF].World.Translation) * Matrix.CreateTranslation(0f, _buildings[obj.NumOF].boundingBoxes.Max().Max.Y, 0), boxEffect);
                obj.collision.World = Matrix.CreateScale(10) *
                    Matrix.CreateTranslation(_buildings[obj.NumOF].World.Translation) * Matrix.CreateTranslation(0f, _buildings[obj.NumOF].boundingBoxes.Max().Max.Y, 0);

                _objects.Add(obj);
            }

            #endregion objects (rough)
        }
    }
}