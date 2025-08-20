using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace IsometricActionGame.UI
{
    public interface IUIElement
    {
        bool IsVisible { get; set; }
        bool IsEnabled { get; set; }
        
        void LoadContent(ContentManager content, GraphicsDevice graphicsDevice);
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch);
    }
} 