using Examples.GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Examples.Screens
{
    class SplashScreenOne : GameScreen
    {
        #region Fields

        ContentManager content;
        Texture2D backgroundTexture;

        float screenTime;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public SplashScreenOne()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the Game class, the content
        /// would remain loaded forever.
        /// </summary>
        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services, "Content");

                backgroundTexture = content.Load<Texture2D>("MenuTextures/TEG Image");
            }
        }

        /// <summary>
        /// Deactivates the screen. Called when the game is being deactivated due to pausing or tombstoning.
        /// </summary>
        public override void Deactivate()
        {
            base.Deactivate();
        }

        /// <summary>
        /// Unload content for the screen. Called when the screen is removed from the screen manager.
        /// </summary>
        public override void Unload()
        {
            // The next screen is added immediately before unloading resources of current screen to allow a smooth visual transition
            //ScreenManager.AddScreen(new CustomMainMenuScreen(), null);
            content.Unload();
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Updates the background screen. Unlike most screens, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the
        /// coveredByOtherScreen parameter to false in order to stop the base
        /// Update method wanting to transition off.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            screenTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (screenTime > 3)
            {
                LoadingScreen.Load(ScreenManager, true, 0, new MainMenuScreenImage());
                this.IsExiting = true;
            }

            base.Update(gameTime, otherScreenHasFocus, false);
        }


        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            Viewport viewport = graphicsDevice.Viewport;

            spriteBatch.Begin();

            spriteBatch.Draw(backgroundTexture, ScreenManager.GraphicsDevice.Viewport.Bounds, new Color(TransitionAlpha, TransitionAlpha, TransitionAlpha));

            spriteBatch.End();
        }


        #endregion
    }
}

