#region File Description
//-----------------------------------------------------------------------------
// MenuEntry.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Examples.GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Examples.Screens
{
    /// <summary>
    /// Helper class represents a single entry in a MenuScreen. By default this
    /// just draws the entry text string, but it can be customized to display menu
    /// entries in different ways. This also provides an event that will be raised
    /// when the menu entry is selected.
    /// </summary>
    class MenuEntryImage
    {

        #region Fields
        Color _color = Color.White;
        float _scale = 1;
        Vector2 _origin;

        /// <summary>
        /// The texture2D rendered for this entry.
        /// </summary>
        Texture2D _texture2D;

        /// <summary>
        /// Tracks a fading selection effect on the entry.
        /// </summary>
        /// <remarks>
        /// The entries transition out of the selection effect when they are deselected.
        /// </remarks>
        float _selectionFade;

        /// <summary>
        /// The position at which the entry is drawn. This is set by the MenuScreen
        /// each frame in Update.
        /// </summary>
        Vector2 _position;

        #endregion

        #region Properties


        /// <summary>
        /// Gets or sets the text of this menu entry.
        /// </summary>
        public Texture2D Texture2D
        {
            get { return _texture2D; }
        }


        /// <summary>
        /// Gets or sets the position at which to draw this menu entry.
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public int Width
        {
            get { return _texture2D.Width; }
        }

        public int Height
        {
            get { return _texture2D.Height; }
        }

        public Vector2 Origin
        {
            get { return new Vector2(_texture2D.Width / 2, _texture2D.Height / 2); }
            set { _origin = value; }
        }

        public float Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        #endregion

        #region Events


        /// <summary>
        /// Event raised when the menu entry is selected.
        /// </summary>
        public event EventHandler<PlayerIndexEventArgs> Selected;


        /// <summary>
        /// Method for raising the Selected event.
        /// </summary>
        protected internal virtual void OnSelectEntry(PlayerIndex playerIndex)
        {
            if (Selected != null)
                Selected(this, new PlayerIndexEventArgs(playerIndex));
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new menu entry with the specified texture.
        /// </summary>
        public MenuEntryImage(Texture2D texture2D)
        {
           this._texture2D = texture2D;
           this._origin = new Vector2(_texture2D.Width / 2, _texture2D.Height / 2);
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the menu entry.
        /// </summary>
        public virtual void Update(MenuScreenImage screen, bool isSelected, GameTime gameTime)
        {

            // When the menu selection changes, entries gradually fade between
            // their selected and deselected appearance, rather than instantly
            // popping to the new state.
            float fadeSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 4;

            if (isSelected)
                _selectionFade = Math.Min(_selectionFade + fadeSpeed, 1);
            else
                _selectionFade = Math.Max(_selectionFade - fadeSpeed, 0);
        }


        /// <summary>
        /// Draws the menu entry. This can be overridden to customize the appearance.
        /// </summary>
        public virtual void Draw(MenuScreenImage screen, bool isSelected, GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw the selected entry in yellow, otherwise white.
            _color = isSelected ? Color.OrangeRed : Color.DarkSlateGray;

            // Pulsate the size of the selected menu entry.
            double time = gameTime.TotalGameTime.TotalSeconds;
            
            float pulsate = (float)Math.Sin(time * 4) + 1;

            _scale = 1 + pulsate * 0.05f * _selectionFade;
            //float _scale = 1 + pulsate * 0.05f * _selectionFade;

            // Modify the alpha to fade text out during transitions.
            _color *= screen.TransitionAlpha;

            spriteBatch.Draw(_texture2D, _position, null, _color, 0, _origin, _scale, SpriteEffects.None, 0);
        }

        #endregion
    }
}
