using System;
using System.Collections.Generic;
using Examples.GameStateManagement;
using Examples.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Penumbra;

namespace LDtk.Examples.Platformer
{
    public class Platformer : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatchMain;
        ScreenManager screenManager;
        ScreenFactory screenFactory;
        PenumbraComponent penumbra;

        public Platformer()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;

            TargetElapsedTime = TimeSpan.FromTicks(333333);

            IsMouseVisible = true;

            // Create the screen factory and add it to the Services
            screenFactory = new ScreenFactory();
            Services.AddService(typeof(IScreenFactory), screenFactory);

            // Create the screen manager component.
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);

            penumbra = new PenumbraComponent(this);
            Components.Add(penumbra);
        }

        protected override void Initialize()
        {
            Content.RootDirectory = "Content";
            spriteBatchMain = new SpriteBatch(graphics.GraphicsDevice);

            // On Windows and Xbox we just add the initial screens
            AddInitialScreens();

            base.Initialize();
        }

        private void AddInitialScreens()
        {
            // Activate the first screens.
            screenManager.AddScreen(new SplashScreenOne(), null);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            penumbra.AmbientColor = Color.White;

            penumbra.BeginDraw();

            graphics.GraphicsDevice.Clear(Color.Transparent);

            screenManager.Draw(gameTime, GraphicsDevice, spriteBatchMain);

            base.Draw(gameTime);
        }
    }
}