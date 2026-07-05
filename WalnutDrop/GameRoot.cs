using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuciXNA.DataAccess.Content;
using NuciXNA.Graphics;
using NuciXNA.Gui.Screens;
using NuciXNA.Input;
using WalnutDrop.Screens;

namespace WalnutDrop
{
    internal sealed class GameRoot : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public GameRoot()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
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

            ScreenManager.Instance.StartingScreenType = typeof(GameplayScreen);
            ScreenManager.Instance.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Instance.Update(Window);
            ScreenManager.Instance.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(samplerState: SamplerState.AnisotropicClamp);
            ScreenManager.Instance.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
