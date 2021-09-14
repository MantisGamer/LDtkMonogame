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

        // LDtk stuff
        private LDtkWorld world;
        private LevelManager levelManager;

        // Entities
        private readonly List<Door> doors = new List<Door>();
        private readonly List<Crate> crates = new List<Crate>();
        private readonly List<Diamond> diamonds = new List<Diamond>();

        // UI

        float pauseAlpha;

        InputAction pauseAction;

        Vector2 enemyPosition = new Vector2(100, 100);
        Vector2 playerPosition = new Vector2(64, 64);

        Random random = new Random();

        Point MousePosition = new Point();
        private KeyboardState oldKeyboard;
        private MouseState oldMouse;

        // Store reference to lighting system.
        PenumbraComponent penumbra;

        // Create sample light source and shadow hull.
        Light light = new PointLight
        {
            Scale = new Vector2(1000f), // Range of the light source (how far the light will travel)
            ShadowType = ShadowType.Solid // Will not lit hulls themselves
        };
        Hull hull = new Hull(new Vector2(1.0f), new Vector2(-1.0f, 1.0f), new Vector2(-1.0f), new Vector2(1.0f, -1.0f))
        {
            Position = new Vector2(400f, 240f),
            Scale = new Vector2(50f)
        };

        #endregion

        public PlayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

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
                penumbra.Hulls.Add(hull);
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

                // Initialize the lighting system.
                penumbra.Initialize();


                #region LDTK Code
                ScreenManager.Game.Window.ClientSizeChanged += (o, e) => OnWindowResized();
                OnWindowResized();

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

                // Apply some random jitter to make the enemy move around.
                const float randomization = 10;

                enemyPosition.X += (float)(random.NextDouble() - 0.5) * randomization;
                enemyPosition.Y += (float)(random.NextDouble() - 0.5) * randomization;
                enemyPosition = Vector2.Lerp(enemyPosition, new Vector2((int)MousePosition.X, (int)MousePosition.Y), 0.05f);

                // Animate light position and hull rotation.
                light.Position =
                    new Vector2(400f, 240f) + // Offset origin
                    new Vector2( // Position around origin
                        (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds),
                        (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds)) * 240f;
                hull.Rotation = MathHelper.WrapAngle(-(float)gameTime.TotalGameTime.TotalSeconds);

                levelManager.SetCenterPoint(playerPosition);
                levelManager.Update();
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

                playerPosition = playerPosition + movement;

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
                    cameraPosition = playerPosition;
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
        public override void Draw(GameTime gameTime)
        {

            Matrix camera = Matrix.CreateTranslation(cameraPosition.X, cameraPosition.Y, 0) * Matrix.CreateScale(pixelScale) * Matrix.CreateTranslation(cameraOrigin.X, cameraOrigin.Y, 0);
            // Everything between penumbra.BeginDraw and penumbra.Draw will be
            // lit by the lighting system.
            penumbra.BeginDraw();

            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.White, 0, 0);
            levelManager.Clear(ScreenManager.GraphicsDevice);

            // Use SamplerState.PointClamp when using small texture2D to avoid blur
            //ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.GetTransformation());
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, transformMatrix: camera);

            {
                levelManager.Draw(ScreenManager.SpriteBatch);
                EntityRendering();
            }

            ScreenManager.SpriteBatch.DrawString(gameFont, "Insert Gameplay Here", enemyPosition, Color.DarkRed);

            ScreenManager.SpriteBatch.End();
            penumbra.Draw(gameTime);

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

        ScreenManager.FadeBackBufferToBlack(alpha);
            }
}

        private void EntityRendering()
        {
            for (int i = 0; i < doors.Count; i++)
            {
                ScreenManager.SpriteBatch.Draw(doors[i].Texture, doors[i].Position, doors[i].Tile, Color.White, 0, doors[i].Pivot * doors[i].Size, 1, SpriteEffects.None, 0);
            }

            for (int i = 0; i < crates.Count; i++)
            {
                ScreenManager.SpriteBatch.Draw(crates[i].Texture, crates[i].Position, crates[i].Tile, Color.White, 0, crates[i].Pivot * crates[i].Size, 1, SpriteEffects.None, 0);
            }

            for (int i = 0; i < diamonds.Count; i++)
            {
                ScreenManager.SpriteBatch.Draw(diamonds[i].Texture, diamonds[i].Position, diamonds[i].Tile, Color.White, 0, diamonds[i].Pivot * diamonds[i].Size, 1, SpriteEffects.None, 0);
            }
        }

        private void DebugRendering()
        {

        }

        public virtual void OnWindowResized()
        {
            cameraOrigin = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2f, ScreenManager.GraphicsDevice.Viewport.Height / 2f);
            pixelScale = Math.Max(ScreenManager.GraphicsDevice.Viewport.Height / 250, 1);
        }
    }
}
