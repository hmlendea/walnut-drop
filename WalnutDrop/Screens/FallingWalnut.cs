using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuciXNA.Graphics.Drawing;
using NuciXNA.Graphics.SpriteEffects;
using NuciXNA.Primitives;

namespace WalnutDrop.Screens
{
    internal sealed class FallingWalnut
    {
        private readonly Queue<Point2D> pendingWaypoints;
        private readonly TextureSprite sprite;
        private readonly MovementEffect effect;

        public bool IsComplete => !effect.IsActive && pendingWaypoints.Count.Equals(0);

        public FallingWalnut(IEnumerable<Point2D> screenWaypoints, int displaySize, float speed)
        {
            pendingWaypoints = new Queue<Point2D>(screenWaypoints);

            Point2D startLocation = pendingWaypoints.Dequeue();

            effect = new MovementEffect
            {
                Speed = speed
            };

            sprite = new TextureSprite
            {
                ContentFile = "board/walnut",
                Location = startLocation,
                Opacity = 1f,
                Tint = Colour.White,
                MovementEffect = effect
            };

            sprite.LoadContent();

            Size2D textureSize = sprite.TextureSize;
            float scaleRatio = (float)displaySize / textureSize.Width;
            sprite.Scale = new Scale2D(scaleRatio, scaleRatio);

            if (pendingWaypoints.Count > 0)
            {
                effect.TargetLocation = pendingWaypoints.Dequeue();
                effect.Activate();
            }
        }

        public void Update(GameTime gameTime)
        {
            sprite.Update(gameTime);

            if (!effect.IsActive && pendingWaypoints.Count > 0)
            {
                sprite.Location = effect.TargetLocation;
                effect.TargetLocation = pendingWaypoints.Dequeue();
                effect.Activate();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            sprite.Draw(spriteBatch);
        }

        public void Unload()
        {
            sprite.UnloadContent();
        }
    }
}
