#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Examples.GameStateManagement;
using Penumbra;
using Examples.Camera;
#endregion

namespace Examples.Screens
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        SpriteFont gameFont;

        Texture2D _backgroundTexture2D;
        Rectangle _fullScreen;

        Vector2 playerPosition = new Vector2(100, 100);
        Vector2 enemyPosition = new Vector2(100, 100);

        // Create sample light source and shadow hull.
        Light light = new PointLight
        {
            Scale = new Vector2(1000f), // Range of the light source (how far the light will travel)
            ShadowType = ShadowType.Solid // Will not lit hulls themselves
        };
        //Hull hull = new Hull(new Vector2(1.0f), new Vector2(-1.0f, 1.0f), new Vector2(-1.0f), new Vector2(1.0f, -1.0f))
        //{
        //    Position = new Vector2(400f, 240f),
        //    Scale = new Vector2(50f)
        //};

        Camera2D Camera;

        RenderTarget2D renderTarget;

        Random random = new Random();

        float pauseAlpha;

        // Declare an inputaction, in this case, pauseAction
        InputAction pauseAction;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            // Construct the pauseAction with required parameters and inputs.
            // InputAction(Buttons[] buttons, Keys[] keys, bool newPressOnly)
            pauseAction = new InputAction(new Buttons[] { Buttons.Start, Buttons.Back }, new Keys[] { Keys.Escape }, true);
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

                gameFont = content.Load<SpriteFont>("gamefont");

                _backgroundTexture2D = content.Load<Texture2D>("TestLevelOne");

                Camera = new Camera2D(ScreenManager.GraphicsDevice.Viewport, 800, 480, 1);

                Camera.Pos = new Vector2(400, 240);

                _fullScreen = new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height);

                renderTarget = new RenderTarget2D(ScreenManager.GraphicsDevice, 800, 480, false, ScreenManager.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

                ScreenManager._penumbra.Lights.Add(light);
                //ScreenManager._penumbra.Hulls.Add(hull);

                // A real game would probably have more content than this sample, so
                // it would take longer to load. We simulate that by delaying for a
                // while, giving you a chance to admire the beautiful loading screen.
                Thread.Sleep(1000);

                // once the load has finished, we use ResetElapsedTime to tell the game's
                // timing mechanism that we have just finished a very long frame, and that
                // it should not try to catch up.
                ScreenManager.Game.ResetElapsedTime();
            }
        }


        public override void Deactivate()
        {
            base.Deactivate();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void Unload()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


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
                // Apply some random jitter to make the enemy move around.
                const float randomization = 10;

                enemyPosition.X += (float)(random.NextDouble() - 0.5) * randomization;
                enemyPosition.Y += (float)(random.NextDouble() - 0.5) * randomization;

                // Apply a stabilizing force to stop the enemy moving off the screen.
                Vector2 targetPosition = new Vector2(
                    ScreenManager.GraphicsDevice.Viewport.Width / 2 - gameFont.MeasureString("Insert Gameplay Here").X / 2, 
                    200);

                enemyPosition = Vector2.Lerp(enemyPosition, targetPosition, 0.05f);

                // Animate light position and hull rotation.
                light.Position =
                    new Vector2(400f, 240f) + // Offset origin
                    new Vector2( // Position around origin
                        (float)Math.Cos(gameTime.TotalGameTime.TotalSeconds),
                        (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds)) * 240f;
                //hull.Rotation = MathHelper.WrapAngle(-(float)gameTime.TotalGameTime.TotalSeconds);

                // TODO: this game isn't very fun! You could probably improve
                // it by inserting something more interesting in this space :-)
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
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            MouseState mousestate = Mouse.GetState();

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            PlayerIndex player;
            // Evaluats the pauseAction for conditions defined in the constructor, back and start buttons, Esc key, or gamepadDisconnected
            if (pauseAction.Evaluate(input, ControllingPlayer, out player) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), player);
            }
            else
            {
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

                if (keyboardState.IsKeyDown(Keys.R))
                    Camera.Zoom += 0.15f;

                if (keyboardState.IsKeyDown(Keys.T))
                    Camera.Zoom -= 0.15f;

                if (keyboardState.IsKeyDown(Keys.F))
                    Camera.Rotation += 0.05f;

                if (keyboardState.IsKeyDown(Keys.G))
                    Camera.Rotation -= 0.05f;

                Camera.Pos = new Vector2(mousestate.X, mousestate.Y);

                Vector2 thumbstick = gamePadState.ThumbSticks.Left;

                movement.X += thumbstick.X;
                movement.Y -= thumbstick.Y;

                if (input.TouchState.Count > 0)
                {
                    Vector2 touchPosition = input.TouchState[0].Position;
                    Vector2 direction = touchPosition - playerPosition;
                    direction.Normalize();
                    movement += direction;
                }

                if (movement.Length() > 1)
                    movement.Normalize();

                playerPosition += movement * 8f;
            }
        }

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            ScreenManager._penumbra.AmbientColor = Color.Transparent;
            // This game has a blue background. Why? Because!
            graphics.Clear(ClearOptions.Target, Color.Black, 0, 0);
            
            // Use SamplerState.PointClamp when using small texture2D to avoid blur
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.GetTransformation());

            spriteBatch.Draw(_backgroundTexture2D, ScreenManager.GraphicsDevice.Viewport.Bounds, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha, 1f));

            spriteBatch.DrawString(gameFont, "Insert Gameplay Here", enemyPosition, Color.DarkRed);

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }
        #endregion
    }
}
