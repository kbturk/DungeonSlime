using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonogameLibrary;
using MonogameLibrary.Graphics;
using MonogameLibrary.Input;

namespace DungeonSlime;

public class GameRoot : Core
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

    public GameRoot() : base("Dungeon Slime", 1280, 720, false)
    {

    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();

        // Set the inital position of the bat to be 10px
        // to the right of the slime.
        _batPosition = new Vector2(_slime.Width + 10, 0);

        // Assign the random velocity to the bat.
        AssignRandomBatVelocity();
    }

    protected override void LoadContent()
    {
        // Create the texture atlas from the XML configuration file
        TextureAtlas atlas = TextureAtlas.FromFile(Content, "images\\atlas-definition.xml");

        // Create the slime animated sprite from the atlas.
        _slime = atlas.CreateAnimatedSprite("slime-animation");
        _slime.Scale = new Vector2(4.0f, 4.0f);
        _slime.Color = Color.Orange;

        // Create the bat region from the atlas.
        _bat = atlas.CreateAnimatedSprite("bat-animation");
        _bat.Scale = new Vector2(4.0f, 4.0f);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        //update the slime animated sprite:
        _slime.Update(gameTime);

        //update the bat animated sprite:
        _bat.Update(gameTime);

        // Check for keyboard input and handle it.
        CheckKeyboardInput();

        // Check for gamepad input and handle it.
        CheckGamePadInput();

        //Create a bounding box for the screen:
        Rectangle screenBounds = new Rectangle(
                0,
                0,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight
                );

        //Create a bounding circle for the slime:
        Circle slimeBounds = new Circle(
                (int)(_slimePosition.X + (_slime.Width * 0.5f)),
                (int)(_slimePosition.Y + (_slime.Height * 0.5f)),
                (int)(_slime.Width * 0.5f)
                );

        // use distance based checks to determine if the slime is within the
        // bounds of the game screen, and if it is outside that screen edge,
        // move it back.
        if (slimeBounds.Left < screenBounds.Left)
            _slimePosition.X = screenBounds.Left;
        else if (slimeBounds.Right > screenBounds.Right)
            _slimePosition.X = screenBounds.Right - _slime.Width;

        if (slimeBounds.Top < screenBounds.Top)
            _slimePosition.Y = screenBounds.Top;
        else if (slimeBounds.Bottom > screenBounds.Bottom)
            _slimePosition.Y = screenBounds.Bottom - _slime.Height;

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
        if (batBounds.Left < screenBounds.Left)
        {
            normal.X = Vector2.UnitX.X; //it's just 1.0f...
            newBatPosition.X = screenBounds.Left;
        }
        else if (batBounds.Right > screenBounds.Right)
        {
            normal.X = -Vector2.UnitX.X; //it's just -1.0f
            newBatPosition.X = screenBounds.Right - _bat.Width;
        }

        if (batBounds.Top < screenBounds.Top)
        {
            normal.Y = Vector2.UnitY.Y; //it's just 1.0f
            newBatPosition.Y = screenBounds.Top;
        }
        else if (batBounds.Bottom > screenBounds.Bottom)
        {
            normal.Y = - Vector2.UnitY.Y; //it's just -1.0f
            newBatPosition.Y = screenBounds.Bottom - _bat.Height;
        }

        // if the normal is anything but Vector2.Zero, this means the bat
        // had moved outside the screen edge so we should reflect it
        if (normal != Vector2.Zero)
        {
            normal.Normalize();
            _batVelocity = Vector2.Reflect(_batVelocity, normal);
        }

        _batPosition = newBatPosition;

        if (slimeBounds.Intersects(batBounds))
        {
            // divide the width and height of the screen into equal columns
            // and rows based on the width and height of the bat.
            int totalColumns = GraphicsDevice.PresentationParameters.BackBufferWidth / (int)_bat.Width;
            int totalRows =    GraphicsDevice.PresentationParameters.BackBufferHeight / (int)_bat.Height;

            // Choose a random row and column based on the total number of each
            int column = Random.Shared.Next(0, totalColumns);
            int row    = Random.Shared.Next(0, totalRows);

            // change the bat position by setting the x and y values equal to
            // the column and row multiplied by the width and height.
            _batPosition = new Vector2(column * _bat.Width, row * _bat.Height);

            // Assign a new random velocity to the bat
            AssignRandomBatVelocity();
        }

        base.Update(gameTime);
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
        // If the space key is held down, the movement speed increases by 1.5.
        float speed = MOVEMENT_SPEED;
        if (Input.Keyboard.IsKeyDown(Keys.Space))
            speed *= 1.5f;

        if (Input.Keyboard.IsKeyDown(Keys.W) || Input.Keyboard.IsKeyDown(Keys.Up))
            _slimePosition.Y -= speed;

        if (Input.Keyboard.IsKeyDown(Keys.S) || Input.Keyboard.IsKeyDown(Keys.Down))
            _slimePosition.Y += speed;

        if (Input.Keyboard.IsKeyDown(Keys.A) || Input.Keyboard.IsKeyDown(Keys.Left))
            _slimePosition.X -= speed;

        if (Input.Keyboard.IsKeyDown(Keys.D) || Input.Keyboard.IsKeyDown(Keys.Right))
            _slimePosition.X += speed;
    }

    private void CheckGamePadInput()
    {
            GamePadInfo gamePadOne = Input.GamePads[(int)PlayerIndex.One];

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

    protected override void Draw(GameTime gameTime)
    {
        // Clear the back buffer.
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // Begin the sprite batch to prepare for rendering.
        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw the slime texture region at a scale of 4.0
        _slime.Draw(SpriteBatch, _slimePosition);

        // Draw the bat texture region 10px to the right of the slime at a scale of 4.0
        _bat.Draw(SpriteBatch, _batPosition);

        // Always end the sprite batch when finished.
        SpriteBatch.End();

        base.Draw(gameTime);
    }
}

