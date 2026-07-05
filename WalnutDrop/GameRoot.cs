using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuciXNA.DataAccess.Content;
using NuciXNA.Graphics;
using NuciXNA.Input;
using WalnutDrop.Screens;

namespace WalnutDrop
{
    internal sealed class GameRoot : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private readonly GameplayScreen _screen;

        public GameRoot()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _screen = new GameplayScreen();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            NuciContentManager.Instance.LoadContent(Content, GraphicsDevice);
            GraphicsManager.Instance.Graphics = _graphics;
            GraphicsManager.Instance.SpriteBatch = _spriteBatch;

            _screen.LoadContent(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Instance.Update(Window);
            _screen.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(15, 15, 35));

            _spriteBatch.Begin();
            _screen.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
