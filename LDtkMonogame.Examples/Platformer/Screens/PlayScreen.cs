using Examples.GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading;
using LDtk;
using System.Collections.Generic;
using Penumbra;
using LDtk.Examples.Platformer;

namespace Examples.Screens
{
    class PlayScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        SpriteFont gameFont;

        // Camera
        protected Vector2 cameraPosition;
        protected Vector2 cameraOrigin;
        protected float pixelScale = 1f;
        protected bool freeCam = true;
        protected Texture2D texture;
        protected Matrix camera;

        // LDtk stuff
        private LDtkWorld world;
        private LevelManager levelManager;
        private KeyboardState oldKeyboard;
        private MouseState oldMouse;

        // Entities
        private Texture2D pixelTexture;
        private readonly List<Door> doors = new List<Door>();
        private readonly List<Crate> crates = new List<Crate>();
        private readonly List<Diamond> diamonds = new List<Diamond>();
        private Player LDtkPlayer;

        // UI
        private Texture2D diamondTexture;
        private Texture2D fontTexture;
        private int diamondsCollected;

        // Debug
        private bool showTileColliders = false;
        private bool showEntityColliders;
        private Door destinationDoor;

        float pauseAlpha;

        InputAction pauseAction;

        Vector2 enemyPosition = new Vector2(100, 100);
        Vector2 playerPosition = new Vector2(64, 64);

        Random random = new Random();

        Point MousePosition = new Point();

        // Store reference to lighting system.
        PenumbraComponent penumbra;

        // Create sample light source and shadow hull.
        Light light = new PointLight
        {
            Scale = new Vector2(200), // Range of the light source (how far the light will travel)
            ShadowType = ShadowType.Solid // Will not lit hulls themselves
        };
        //Hull hull = new Hull(new Vector2(1.0f), new Vector2(-1.0f, 1.0f), new Vector2(-1.0f), new Vector2(1.0f, -1.0f))
        //{
        //    Position = new Vector2(400f, 240f),
        //    Scale = new Vector2(50f)
        //};

        #endregion

        public PlayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            freeCam = false;

            pauseAction = new InputAction(
                new Buttons[] { Buttons.Start, Buttons.Back },
                new Keys[] { Keys.Escape },
                true);
        }

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services, "Content");

                gameFont = content.Load<SpriteFont>("Fonts/bitwise");

                // Camera Code
                texture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
                texture.SetData(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });

                penumbra = new PenumbraComponent(ScreenManager.Game);
                penumbra.Lights.Add(light);
                //penumbra.Hulls.Add(hull);
                penumbra.AmbientColor = Color.Black;

                world = content.Load<LDtkWorld>("LDtkMonogameExample");
                world.spriteBatch = ScreenManager.SpriteBatch;
                world.GraphicsDevice = ScreenManager.GraphicsDevice;
                levelManager = new LevelManager(world);

                levelManager.OnEnterNewLevel += (level) =>
                {
                    doors.AddRange(level.GetEntities<Door>());
                    for (int i = 0; i < doors.Count; i++)
                    {
                        doors[i].collider = new Rect(doors[i].Position.X - 4, doors[i].Position.Y - 2, 8, 4);
                    }

                    crates.AddRange(level.GetEntities<Crate>());
                    for (int i = 0; i < crates.Count; i++)
                    {
                        crates[i].collider = new Rect(crates[i].Position.X - 8, crates[i].Position.Y - 16, 16, 16);
                    }

                    diamonds.AddRange(level.GetEntities<Diamond>());
                    for (int i = 0; i < diamonds.Count; i++)
                    {
                        diamonds[i].collider = new Rect(diamonds[i].Position.X - 6, diamonds[i].Position.Y - 16, 12, 16);
                    }
                };

                levelManager.ChangeLevelTo("Level1");
                LDtkEntity startLocation = levelManager.CurrentLevel.GetEntity<LDtkEntity>("PlayerSpawn");

                LDtkPlayer = new Player
                {
                    Texture = content.Load<Texture2D>("Art/Characters/KingHuman"),
                    Position = startLocation.Position,
                    Pivot = startLocation.Pivot,
#if DEBUG
                    EditorVisualColor = startLocation.EditorVisualColor,
#endif
                    Tile = new Rectangle(0, 0, 78, 58),
                    Size = new Vector2(78, 58)
                };

                LDtkPlayer.animator.OnEnteredDoor += () =>
                {
                    LDtkPlayer.animator.SetState(Animator.Animation.ExitDoor);
                    levelManager.ChangeLevelTo(LDtkPlayer.door.levelIdentifier);
                    destinationDoor = levelManager.CurrentLevel.GetEntity<Door>();
                    LDtkPlayer.Position = destinationDoor.Position;
                };

                pixelTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
                pixelTexture.SetData(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });

                fontTexture = content.Load<Texture2D>("Art/Gui/Font");
                diamondTexture = content.Load<Texture2D>("Art/Gui/Diamond");

            // Initialize the lighting system.
            penumbra.Initialize();


                #region LDTK Code
                //ScreenManager.Game.Window.ClientSizeChanged += (o, e) => OnWindowResized();
                //OnWindowResized();

                #endregion

                // A real game would probably have more content than this sample, so
                // it would take longer to load. We simulate that by delaying for a
                // while, giving you a chance to admire the beautiful loading screen.
                Thread.Sleep(1000);

                // once the load has finished, we use ResetElapsedTime to tell the game's
                // timing mechanism that we have just finished a very long frame, and that
                // it should not try to catch up.
                //ScreenManager.Game.ResetElapsedTime();


            }
        }

        public override void Deactivate()
        {
#if WINDOWS_PHONE
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State["PlayerPosition"] = playerPosition;
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State["EnemyPosition"] = enemyPosition;
#endif

            base.Deactivate();
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void Unload()
        {
            content.Unload();

#if WINDOWS_PHONE
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State.Remove("PlayerPosition");
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State.Remove("EnemyPosition");
#endif
        }

        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;


                KeyboardState keyboard = Keyboard.GetState();
                MouseState mouse = Mouse.GetState();

                // Debug/Cheats
                if (keyboard.IsKeyDown(Keys.F1) && oldKeyboard.IsKeyDown(Keys.F1) == false)
                {
                    showTileColliders = !showTileColliders;
                }

                if (keyboard.IsKeyDown(Keys.F2) && oldKeyboard.IsKeyDown(Keys.F2) == false)
                {
                    showEntityColliders = !showEntityColliders;
                }

                if (keyboard.IsKeyDown(Keys.F4) && oldKeyboard.IsKeyDown(Keys.F4) == false)
                {
                    diamondsCollected++;
                }

                if (keyboard.IsKeyDown(Keys.F5) && oldKeyboard.IsKeyDown(Keys.F5) == false)
                {

                }

                levelManager.SetCenterPoint(LDtkPlayer.Position);
                levelManager.Update();
                LDtkPlayer.Update(keyboard, oldKeyboard, mouse, oldMouse, levelManager.CurrentLevel, deltaTime);

                LDtkPlayer.door = null;
                for (int i = 0; i < doors.Count; i++)
                {
                    doors[i].Update(deltaTime);

                    if (LDtkPlayer.collider.Contains(doors[i].collider))
                    {
                        LDtkPlayer.door = doors[i];

                        if (LDtkPlayer.animator.EnteredDoor())
                        {
                            doors[i].opening = true;
                        }
                        break;
                    }
                }

                for (int i = 0; i < crates.Count; i++)
                {
                    crates[i].Update(deltaTime);

                    if (LDtkPlayer.attack.Contains(crates[i].collider) && LDtkPlayer.attacking)
                    {
                        crates[i].Damage();
                    }
                }

                for (int i = diamonds.Count - 1; i >= 0; i--)
                {
                    diamonds[i].Update(deltaTime, (float)gameTime.TotalGameTime.TotalSeconds);

                    if (diamonds[i].delete)
                    {
                        _ = diamonds.Remove(diamonds[i]);
                    }
                    else if (diamonds[i].collected == false)
                    {
                        if (diamonds[i].collider.Contains(LDtkPlayer.collider))
                        {
                            diamondsCollected++;
                            diamonds[i].collected = true;
                        }
                    }
                }

                // Follow Player
                if (freeCam == false)
                {
                    cameraPosition = -new Vector2(LDtkPlayer.Position.X, LDtkPlayer.Position.Y - 30);
                }

                oldKeyboard = keyboard;
                oldMouse = mouse;

                light.Position = new Vector2(400, 240);

                
            }
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = 0;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];
            MouseState mouse = Mouse.GetState();

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            PlayerIndex player;
            if (pauseAction.Evaluate(input, ControllingPlayer, out player) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
            else
            {

                MousePosition = mouse.Position;
                // Otherwise move the player position.
                Vector2 movement = Vector2.Zero;

                if (keyboardState.IsKeyDown(Keys.Left))
                    movement.X--;

                if (keyboardState.IsKeyDown(Keys.Right))
                    movement.X++;

                if (keyboardState.IsKeyDown(Keys.Up))
                    movement.Y--;

                if (keyboardState.IsKeyDown(Keys.Down))
                    movement.Y++;

                playerPosition = (playerPosition + movement);

                if (keyboardState.IsKeyDown(Keys.Tab) && oldKeyboard.IsKeyDown(Keys.Tab) == false)
                {
                    freeCam = !freeCam;
                }

                if (freeCam)
                {
                    if (mouse.MiddleButton == ButtonState.Pressed)
                    {
                        Point pos = mouse.Position - oldMouse.Position;
                        cameraPosition += new Vector2(pos.X, pos.Y) * 30 / (pixelScale * 0.5f) * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                }

                // Follow Player
                if (freeCam == false)
                {
                    cameraPosition = -new Vector2(LDtkPlayer.Position.X - 400, LDtkPlayer.Position.Y - 240);
                }

                Vector2 thumbstick = gamePadState.ThumbSticks.Left;

                movement.X += thumbstick.X;
                movement.Y -= thumbstick.Y;

                //if (input.TouchState.Count > 0)
                //{
                //    Vector2 touchPosition = input.TouchState[0].Position;
                //    Vector2 direction = touchPosition - playerPosition;
                //    direction.Normalize();
                //    movement += direction;
                //}

                //if (movement.Length() > 1)
                //    movement.Normalize();

                //playerPosition += movement * 8f;

                oldKeyboard = input.CurrentKeyboardStates[playerIndex];
                oldMouse = mouse;
            }
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            camera = Matrix.CreateTranslation(cameraPosition.X, cameraPosition.Y, 0) * Matrix.CreateScale(pixelScale) * Matrix.CreateTranslation(cameraOrigin.X, cameraOrigin.Y, 0f);
            penumbra.BeginDraw();
            graphicsDevice.Clear(ClearOptions.Target, Color.White, 0, 0);
            levelManager.Clear(graphicsDevice);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera);
            {
                levelManager.Draw(spriteBatch);
                EntityRendering(spriteBatch);
                DebugRendering(spriteBatch);
            }
            spriteBatch.End();

            // UI
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            {
                spriteBatch.Draw(diamondTexture,
                    Vector2.One * pixelScale * 2,
                    new Rectangle(0, 0, 12, 12),
                    Color.White,
                    0,
                    Vector2.Zero,
                    pixelScale * 2,
                    SpriteEffects.None,
                    0);

                // Digit hundreds

                int units = diamondsCollected % 10;
                int tens = diamondsCollected / 10 % 10;
                int hundreds = diamondsCollected / 100 % 10;

                spriteBatch.Draw(fontTexture,
                    new Vector2(12, 1) * pixelScale * 2,
                    new Rectangle(7 * hundreds, 0, 7, 9),
                    new Color(97, 152, 204, 255),
                    0,
                    Vector2.Zero,
                    pixelScale * 2,
                    SpriteEffects.None,
                    0);

                // Digit tens
                spriteBatch.Draw(fontTexture,
                    new Vector2(12 + 7, 1) * pixelScale * 2,
                    new Rectangle(7 * tens, 0, 7, 9),
                    new Color(97, 152, 204, 255),
                    0,
                    Vector2.Zero,
                    pixelScale * 2,
                    SpriteEffects.None,
                    0);

                // Digit units
                spriteBatch.Draw(fontTexture,
                    new Vector2(12 + 7 + 7, 1) * pixelScale * 2,
                    new Rectangle(7 * units, 0, 7, 9),
                    new Color(97, 152, 204, 255),
                    0,
                    Vector2.Zero,
                    pixelScale * 2,
                    SpriteEffects.None,
                    0);
            }
            spriteBatch.End();
            penumbra.Draw(gameTime);

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

        ScreenManager.FadeBackBufferToBlack(alpha);
            }
}

        private void EntityRendering(SpriteBatch spritebatch)
        {
            //spritebatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera);
            for (int i = 0; i < doors.Count; i++)
            {
                spritebatch.Draw(doors[i].Texture, doors[i].Position, doors[i].Tile, Color.White, 0, doors[i].Pivot * doors[i].Size, 1, SpriteEffects.None, 0);
            }

            for (int i = 0; i < crates.Count; i++)
            {
                spritebatch.Draw(crates[i].Texture, crates[i].Position, crates[i].Tile, Color.White, 0, crates[i].Pivot * crates[i].Size, 1, SpriteEffects.None, 0);
            }

            spritebatch.Draw(LDtkPlayer.Texture, LDtkPlayer.Position, LDtkPlayer.Tile, Color.White, 0, (LDtkPlayer.Pivot * LDtkPlayer.Size) + new Vector2(LDtkPlayer.fliped ? -8 : 8, -14),
                1, LDtkPlayer.fliped ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.1f);

            for (int i = 0; i < diamonds.Count; i++)
            {
                spritebatch.Draw(diamonds[i].Texture, diamonds[i].Position, diamonds[i].Tile, Color.White, 0, diamonds[i].Pivot * diamonds[i].Size, 1, SpriteEffects.None, 0);
            }
        }

        private void DebugRendering(SpriteBatch spriteBatch)
        {
            // Debugging
            if (showTileColliders)
            {
                for (int i = 0; i < LDtkPlayer.tiles.Count; i++)
                {
                    spriteBatch.DrawRect(LDtkPlayer.tiles[i].rect, new Color(128, 255, 0, 128));
                }
            }

#if DEBUG
            if (showEntityColliders)
            {
                for (int i = 0; i < doors.Count; i++)
                {
                    spriteBatch.DrawRect(doors[i].collider, doors[i].EditorVisualColor);
                }

                for (int i = 0; i < crates.Count; i++)
                {
                    spriteBatch.DrawRect(crates[i].collider, crates[i].EditorVisualColor);
                }

                for (int i = 0; i < diamonds.Count; i++)
                {
                    spriteBatch.DrawRect(diamonds[i].collider, diamonds[i].EditorVisualColor);
                }

                spriteBatch.DrawRect(LDtkPlayer.collider, LDtkPlayer.EditorVisualColor);
                spriteBatch.DrawRect(LDtkPlayer.attack, LDtkPlayer.EditorVisualColor);
            }
        }
#endif

        public void OnWindowResized(GraphicsDevice graphicsdevice)
        {
            cameraOrigin = new Vector2(graphicsdevice.Viewport.Width / 2f, graphicsdevice.Viewport.Height / 2f);
            pixelScale = Math.Max(graphicsdevice.Viewport.Height / 250, 1);
        }
    }
}
