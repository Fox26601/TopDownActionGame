using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IsometricActionGame.Core.Graphics
{
    public class AnimatedSprite
    {
        private Texture2D _texture;
        private Rectangle[] _frames;
        private float _frameTime;
        private float _currentTime;
        private int _currentFrame;
        private bool _isLooping;
        private bool _isFinished;

        public int CurrentFrame => _currentFrame;
        public bool IsFinished => _isFinished;
        public int FrameCount => _frames.Length;
        public int CurrentFrameWidth => _currentFrame < _frames.Length ? _frames[_currentFrame].Width : 0;
        public int CurrentFrameHeight => _currentFrame < _frames.Length ? _frames[_currentFrame].Height : 0;
        public int TextureWidth => _texture?.Width ?? 0;
        public int TextureHeight => _texture?.Height ?? 0;

        public AnimatedSprite(Texture2D texture, int frameCount, float frameTime, bool isLooping = true)
        {
            _texture = texture;
            _frameTime = frameTime;
            _isLooping = isLooping;
            _currentFrame = 0;
            _currentTime = 0;
            _isFinished = false;

            // Create frames (assume they are arranged horizontally)
            int frameWidth = texture.Width / frameCount;
            int frameHeight = texture.Height;
            _frames = new Rectangle[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                _frames[i] = new Rectangle(i * frameWidth, 0, frameWidth, frameHeight);
            }
        }

        public AnimatedSprite(Texture2D texture, int columns, int rows, float frameTime, bool isLooping = true)
        {
            _texture = texture;
            _frameTime = frameTime;
            _isLooping = isLooping;
            _currentFrame = 0;
            _currentTime = 0;
            _isFinished = false;

            // Create frames for grid
            int frameWidth = texture.Width / columns;
            int frameHeight = texture.Height / rows;
            _frames = new Rectangle[columns * rows];

            int frameIndex = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    _frames[frameIndex] = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
                    frameIndex++;
                }
            }
        }

        public AnimatedSprite(Texture2D texture, int columns, int rows, int totalFrames, float frameTime, bool isLooping = true)
        {
            _texture = texture;
            _frameTime = frameTime;
            _isLooping = isLooping;
            _currentFrame = 0;
            _currentTime = 0;
            _isFinished = false;

            // Create frames for grid with frame count limitation
            int frameWidth = texture.Width / columns;
            int frameHeight = texture.Height / rows;
            _frames = new Rectangle[totalFrames]; // Use only the required number of frames

            int frameIndex = 0;
            for (int row = 0; row < rows && frameIndex < totalFrames; row++)
            {
                for (int col = 0; col < columns && frameIndex < totalFrames; col++)
                {
                    _frames[frameIndex] = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
                    frameIndex++;
                }
            }
        }

        public AnimatedSprite(Texture2D texture, int columns, int rows, int totalFrames, int startRow, float frameTime, bool isLooping = true)
        {
            _texture = texture;
            _frameTime = frameTime;
            _isLooping = isLooping;
            _currentFrame = 0;
            _currentTime = 0;
            _isFinished = false;

            // Create frames for grid with frame count limitation and start row
            int frameWidth = texture.Width / columns;
            int frameHeight = texture.Height / rows;
            _frames = new Rectangle[totalFrames]; // Use only the required number of frames

            int frameIndex = 0;
            for (int row = startRow; row < rows && frameIndex < totalFrames; row++)
            {
                for (int col = 0; col < columns && frameIndex < totalFrames; col++)
                {
                    _frames[frameIndex] = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
                    frameIndex++;
                }
            }
        }

        public AnimatedSprite(Texture2D texture, int columns, int rows, int totalFrames, int startColumn, int startRow, float frameTime, bool isLooping = true)
        {
            _texture = texture;
            _frameTime = frameTime;
            _isLooping = isLooping;
            _currentFrame = 0;
            _currentTime = 0;
            _isFinished = false;

            // Create frames for grid with frame count limitation and start position
            int frameWidth = texture.Width / columns;
            int frameHeight = texture.Height / rows;
            _frames = new Rectangle[totalFrames]; // Use only the required number of frames

            int frameIndex = 0;
            for (int row = startRow; row < rows && frameIndex < totalFrames; row++)
            {
                for (int col = startColumn; col < columns && frameIndex < totalFrames; col++)
                {
                    _frames[frameIndex] = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
                    frameIndex++;
                }
            }
        }

        public AnimatedSprite(Texture2D texture, int columns, int rows, int totalFrames, int startColumn, int startRow, int startFrame, float frameTime, bool isLooping = true)
        {
            _texture = texture;
            _frameTime = frameTime;
            _isLooping = isLooping;
            _currentFrame = 0;
            _currentTime = 0;
            _isFinished = false;

            // Create frames for grid with frame count limitation, start position, and start frame
            int frameWidth = texture.Width / columns;
            int frameHeight = texture.Height / rows;
            _frames = new Rectangle[totalFrames]; // Use only the required number of frames

            int frameIndex = 0;
            for (int row = startRow; row < rows && frameIndex < totalFrames; row++)
            {
                for (int col = startColumn; col < columns && frameIndex < totalFrames; col++)
                {
                    _frames[frameIndex] = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
                    frameIndex++;
                }
            }

            // Set the starting frame
            if (startFrame >= 0 && startFrame < totalFrames)
            {
                _currentFrame = startFrame;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (_isFinished) return;

            _currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_currentTime >= _frameTime)
            {
                _currentFrame++;
                _currentTime = 0;

                if (_currentFrame >= _frames.Length)
                {
                    if (_isLooping)
                    {
                        _currentFrame = 0;
                    }
                    else
                    {
                        _currentFrame = _frames.Length - 1;
                        _isFinished = true;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation = 0f, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0.5f, float scale = 1.0f)
        {
            if (_currentFrame < _frames.Length)
            {
                // Calculate origin to center the sprite both horizontally and vertically
                var origin = new Vector2(_frames[_currentFrame].Width / 2, _frames[_currentFrame].Height / 2);
                spriteBatch.Draw(_texture, position, _frames[_currentFrame], color, rotation, origin, scale, effects, layerDepth);
            }
        }

        public void Reset()
        {
            _currentFrame = 0;
            _currentTime = 0;
            _isFinished = false;
        }

        public void SetFrame(int frame)
        {
            if (frame >= 0 && frame < _frames.Length)
            {
                _currentFrame = frame;
                _currentTime = 0;
            }
        }

        public void SetFrameRange(int startFrame, int endFrame)
        {
            if (startFrame >= 0 && endFrame < _frames.Length && startFrame <= endFrame)
            {
                // Create a new array with only the specified frames
                int frameCount = endFrame - startFrame + 1;
                Rectangle[] newFrames = new Rectangle[frameCount];
                
                for (int i = 0; i < frameCount; i++)
                {
                    newFrames[i] = _frames[startFrame + i];
                }
                
                _frames = newFrames;
                _currentFrame = 0;
                _currentTime = 0;
                _isFinished = false;
            }
        }
    }
}
