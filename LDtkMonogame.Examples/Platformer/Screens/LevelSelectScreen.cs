#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
#endregion

namespace Examples.Screens
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class LevelSelectScreen : MenuScreenImage
    {

        ContentManager content;


        Texture2D levelOneTexture2D, levelTwoTexture2D, levelThreeTexture2D;

        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public LevelSelectScreen()
            : base("")
        {

        }

        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services, "Content");

                this.backgroundImage = content.Load<Texture2D>("MenuTextures/TitleScreenLowRes");
                this.blankImage = content.Load<Texture2D>("Art/blank");

                levelOneTexture2D = content.Load<Texture2D>("MenuTextures/one_bitwise");
                levelTwoTexture2D = content.Load<Texture2D>("MenuTextures/two_bitwise");
                levelThreeTexture2D = content.Load<Texture2D>("MenuTextures/three_bitwise");

                // Create our menu entries.
                MenuEntryImage levelOne = new MenuEntryImage(this.levelOneTexture2D);
                MenuEntryImage levelTwo = new MenuEntryImage(this.levelTwoTexture2D);
                MenuEntryImage levelThree = new MenuEntryImage(this.levelThreeTexture2D);

                // Hook up menu event handlers.
                levelOne.Selected += PlayLevelOne;
                levelTwo.Selected += PlayLevelTwo;
                levelThree.Selected += PlayLevelThree;

                // Add entries to the menu.
                MenuEntries.Add(levelOne);
                MenuEntries.Add(levelTwo);
                MenuEntries.Add(levelThree);

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

        #region Handle Input

        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void PlayLevelOne(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
                               new PlayScreen());
        }

        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void PlayLevelTwo(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
                               new GameplayScreen());
        }

        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void PlayLevelThree(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
                               new GameplayScreen());
        }

        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        //protected override void OnCancel(PlayerIndex playerIndex)
        //{
        //    const string message = "Are you sure you want to exit this sample?";

        //    MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(message);

        //    confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

        //    ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
        //}

        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        //void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        //{
        //    ScreenManager.Game.Exit();
        //}


        #endregion
    }
}
