using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonogameLibrary;
using MonogameLibrary.Graphics;
using MonogameLibrary.Input;
using MonogameLibrary.Scenes;

namespace DungeonSlime.Scenes;

public class GameScene : Scene
{
    // defines the slime animated sprite
    private AnimatedSprite _slime;

    // defines the bat animated sprite
    private AnimatedSprite _bat;

    // Tracks the position of the slime.
    private Vector2 _slimePosition;

    // Speed multiplier when moving.
    private const float MOVEMENT_SPEED = 5.0f;

    // Tracks the position of the bat.
    private Vector2 _batPosition;

    // Tracks the velocity of the bat.
    private Vector2 _batVelocity;

    // Defines the tilemap to draw (1 level only)
    private Tilemap _tilemap;

    // Defines the bounds of the room that the slime and bat are contained within:
    private Rectangle _roomBounds;

    // The sound effect to play when the bat bounces off the edge of the screen.
    private SoundEffect _bounceSoundEffect;

    // The sound effect to play when the slime eats the bat
    private SoundEffect _collectSoundEffect;

    // The SpriteFont Description used to draw text.
    private SpriteFont _font;

    // Tracks the players score.
    private int _score;

    // Defines the position to draw the score text at
    private Vector2 _scoreTextPosition;

    // Defines the origin used when drawing the score text.
    private Vector2 _scoreTextOrigin;

    public override void Initialize()
    {
        base.Initialize();

        Core.ExitOnEscape = false;

        Rectangle screenBounds = Core.GraphicsDevice.PresentationParameters.Bounds;

        _roomBounds = new Rectangle(
                (int)_tilemap.TileWidth,
                (int)_tilemap.TileHeight,
                screenBounds.Width - (int)_tilemap.TileWidth * 2,
                screenBounds.Height - (int)_tilemap.TileHeight * 2
                );

        // place the slime in the center of the game:
        int centerRow = _tilemap.Rows / 2;
        int centerColumn = _tilemap.Columns/ 2;
        _slimePosition = new Vector2(centerColumn * _tilemap.TileWidth, centerRow * _tilemap.TileHeight);

        // Set the inital position of the bat to be top left
        // to the right of the slime.
        _batPosition = new Vector2(_roomBounds.Left, _roomBounds.Top);

        // Assign the random velocity to the bat.
        AssignRandomBatVelocity();

        // Set the position of the score text to align to the left edge of the
        // room bounds, and to vertically be at the center of the first tile.
        _scoreTextPosition = new Vector2(_roomBounds.Left, _tilemap.TileHeight * 0.5f);

        // Set the origin of the text so it is left-centered.
        float scoreTextYOrigin = _font.MeasureString("Score").Y * 0.5f;
        _scoreTextOrigin = new Vector2(0, scoreTextYOrigin);
    }

    public override void LoadContent()
    {
        // Create the texture atlas from the XML configuration file
        TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images\\atlas-definition.xml");

        // Create the slime animated sprite from the atlas.
        _slime = atlas.CreateAnimatedSprite("slime-animation");
        _slime.Scale = new Vector2(4.0f, 4.0f);
        _slime.Color = Color.Orange;

        // Create the bat region from the atlas.
        _bat = atlas.CreateAnimatedSprite("bat-animation");
        _bat.Scale = new Vector2(4.0f, 4.0f);

        // Create the tileMap from the XML configuration file.
        _tilemap = Tilemap.FromFile(Content, "images\\tilemap-definition.xml");
        _tilemap.Scale = new Vector2(4.0f, 4.0f);

        // Load the sound effects
        _bounceSoundEffect = Content.Load<SoundEffect>("audio/bounce");
        _collectSoundEffect = Content.Load<SoundEffect>("audio/collect");

        // Load the font
        _font = Core.Content.Load<SpriteFont>("fonts/04B_30");

    }

    public override void Update(GameTime  gameTime)
    {
        //update the slime animated sprite:
        _slime.Update(gameTime);

        //update the bat animated sprite:
        _bat.Update(gameTime);

        // Check for keyboard input and handle it.
        CheckKeyboardInput();

        // Check for gamepad input and handle it.
        CheckGamePadInput();

        //Create a bounding circle for the slime:
        Circle slimeBounds = new Circle(
                (int)(_slimePosition.X + (_slime.Width * 0.5f)),
                (int)(_slimePosition.Y + (_slime.Height * 0.5f)),
                (int)(_slime.Width * 0.5f)
                );

        // use distance based checks to determine if the slime is within the
        // bounds of the game screen, and if it is outside that screen edge,
        // move it back.
        if (slimeBounds.Left < _roomBounds.Left)
            _slimePosition.X = _roomBounds.Left;
        else if (slimeBounds.Right > _roomBounds.Right)
            _slimePosition.X = _roomBounds.Right - _slime.Width;

        if (slimeBounds.Top < _roomBounds.Top)
            _slimePosition.Y = _roomBounds.Top;
        else if (slimeBounds.Bottom > _roomBounds.Bottom)
            _slimePosition.Y = _roomBounds.Bottom - _slime.Height;

        // Calculae the new position of the bat based on the velocity.
        Vector2 newBatPosition = _batPosition + _batVelocity;

        // Create a new bounding circle for the bat.
        Circle batBounds = new Circle(
            (int)(newBatPosition.X + (_bat.Width * 0.5f)),
            (int)(newBatPosition.Y + (_bat.Height * 0.5f)),
            (int)(_bat.Width * 0.5f)
            );

        Vector2 normal = Vector2.Zero;

        // use distance based checks to determin if bat is within the bounds
        // of the game screen, and if it is outside that screen edge,
        // reflect it about the screen edge normal.
        if (batBounds.Left < _roomBounds.Left)
        {
            normal.X = Vector2.UnitX.X; //it's just 1.0f...
            newBatPosition.X = _roomBounds.Left;
        }
        else if (batBounds.Right > _roomBounds.Right)
        {
            normal.X = -Vector2.UnitX.X; //it's just -1.0f
            newBatPosition.X = _roomBounds.Right - _bat.Width;
        }

        if (batBounds.Top < _roomBounds.Top)
        {
            normal.Y = Vector2.UnitY.Y; //it's just 1.0f
            newBatPosition.Y = _roomBounds.Top;
        }
        else if (batBounds.Bottom > _roomBounds.Bottom)
        {
            normal.Y = - Vector2.UnitY.Y; //it's just -1.0f
            newBatPosition.Y = _roomBounds.Bottom - _bat.Height;
        }

        // if the normal is anything but Vector2.Zero, this means the bat
        // had moved outside the screen edge so we should reflect it
        if (normal != Vector2.Zero)
        {
            normal.Normalize();
            _batVelocity = Vector2.Reflect(_batVelocity, normal);

            // Play the bounce sound effect
            Core.Audio.PlaySoundEffect(_bounceSoundEffect, 0.2f, 0.0f, 0.0f, false);
        }

        _batPosition = newBatPosition;

       if (slimeBounds.Intersects(batBounds))
        {
            // Choose a random row and column based on the total number of each
            int column = Random.Shared.Next(1, _tilemap.Columns - 1);
            int row    = Random.Shared.Next(1, _tilemap.Rows - 1);

            // change the bat position by setting the x and y values equal to
            // the column and row multiplied by the width and height.
            _batPosition = new Vector2(column * _bat.Width, row * _bat.Height);

            // Assign a new random velocity to the bat
            AssignRandomBatVelocity();

            Core.Audio.PlaySoundEffect(_collectSoundEffect, 0.2f, 0.0f, 0.0f, false);

            // Increase the player's score:
            _score += 100;
        }

    }

    private void AssignRandomBatVelocity()
    {
        // Generate a random angle.
        float angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);
        float x = (float)Math.Cos(angle);
        float y = (float)Math.Sin(angle);

        Vector2 direction = new Vector2(x, y);

        _batVelocity = direction * MOVEMENT_SPEED;
    }

    private void CheckKeyboardInput()
    {
        //If the escape key is pressed, return to the title screen.
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Escape))
            Core.ChangeScene(new TitleScene());

        // If the space key is held down, the movement speed increases by 1.5.
        float speed = MOVEMENT_SPEED;
        if (Core.Input.Keyboard.IsKeyDown(Keys.Space))
            speed *= 1.5f;

        if (Core.Input.Keyboard.IsKeyDown(Keys.W) || Core.Input.Keyboard.IsKeyDown(Keys.Up))
            _slimePosition.Y -= speed;

        if (Core.Input.Keyboard.IsKeyDown(Keys.S) || Core.Input.Keyboard.IsKeyDown(Keys.Down))
            _slimePosition.Y += speed;

        if (Core.Input.Keyboard.IsKeyDown(Keys.A) || Core.Input.Keyboard.IsKeyDown(Keys.Left))
            _slimePosition.X -= speed;

        if (Core.Input.Keyboard.IsKeyDown(Keys.D) || Core.Input.Keyboard.IsKeyDown(Keys.Right))
            _slimePosition.X += speed;

        //Sound inputs
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.M))
            Core.Audio.ToggleMute();

        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.OemPlus))
        {
            Core.Audio.SongVolume += 0.1f;
            Core.Audio.SoundEffectVolume += 0.1f;
        }

        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.OemMinus))
        {
            Core.Audio.SongVolume -= 0.1f;
            Core.Audio.SoundEffectVolume -= 0.1f;
        }

    }

    private Vector2 Middle() {
        return new Vector2(_roomBounds.Width/2, _roomBounds.Height/2);
    }

    private void CheckGamePadInput()
    {
            GamePadInfo gamePadOne = Core.Input.GamePads[(int)PlayerIndex.One];

                float speed = MOVEMENT_SPEED;
                if (gamePadOne.IsButtonDown(Buttons.A))
                {
                   speed *= 1.5f;
                   gamePadOne.SetVibration(1.0f, TimeSpan.FromSeconds(1));
                }
                else
                {
                    gamePadOne.StopVibration();
                }

                // The Alex test state:
                // Move to top left
                if (gamePadOne.LeftTrigger == 1.0f)
                    _slimePosition = Vector2.Zero;
                // Move to bottom right
                else if (gamePadOne.RightTrigger == 1.0f)
                    _slimePosition = Middle() * 2 - new Vector2(_slime.Width, _slime.Height);
                // Move to bottom left
                else if (gamePadOne.IsButtonDown(Buttons.LeftShoulder))
                    _slimePosition = new Vector2( 0, Middle().Y) * 2 - new Vector2(0.0f, _slime.Height);
                // Move to top right
                else if (gamePadOne.IsButtonDown(Buttons.RightShoulder))
                    _slimePosition = new Vector2(Middle().X, 0) * 2 - new Vector2(_slime.Width, 0);

                // check thumbstick first since it has priority over which gamepad input is movement.
                if (gamePadOne.LeftThumbstick != Vector2.Zero)
                {
                    _slimePosition.X += gamePadOne.LeftThumbstick.X * speed;
                    _slimePosition.Y -= gamePadOne.LeftThumbstick.Y * speed;
                }
                else
                {
                    //if Dpadup is down, move the slime up the screen.
                    if (gamePadOne.IsButtonDown(Buttons.DPadUp))
                        _slimePosition.Y -= speed;

                    if (gamePadOne.IsButtonDown(Buttons.DPadDown))
                        _slimePosition.Y += speed;

                    if (gamePadOne.IsButtonDown(Buttons.DPadLeft))
                        _slimePosition.X -= speed;

                    if (gamePadOne.IsButtonDown(Buttons.DPadRight))
                        _slimePosition.X += speed;
                }
    }

    public override void Draw(GameTime gameTime)
    {
        // Clear the back buffer.
        Core.GraphicsDevice.Clear(Color.CornflowerBlue);

        // Begin the sprite batch to prepare for rendering.
        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw the tilemap.
        _tilemap.Draw(Core.SpriteBatch);

        // Draw the slime texture region at a scale of 4.0
        _slime.Draw(Core.SpriteBatch, _slimePosition);

        // Draw the bat texture region 10px to the right of the slime at a scale of 4.0
        _bat.Draw(Core.SpriteBatch, _batPosition);

        // Draw the score
        Core.SpriteBatch.DrawString(
                _font,
                $"Score: {_score}",
                _scoreTextPosition,
                Color.White,
                0.0f,
                _scoreTextOrigin,
                1.0f,
                SpriteEffects.None,
                0.0f
                );

        // Always end the sprite batch when finished.
        Core.SpriteBatch.End(); 
    }
}
