using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonogameLibrary;
using MonogameLibrary.Graphics;
using System;

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

    public GameRoot() : base("Dungeon Slime", 1280, 720, false)
    {

    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
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

        base.Update(gameTime);
    }

    private void CheckKeyboardInput()
    {
        //Get the state of the keyboard input
        KeyboardState keyboardState = Keyboard.GetState();

        // If the space key is held down, the movement speed increases by 1.5.
        float speed = MOVEMENT_SPEED;
        if (keyboardState.IsKeyDown(Keys.Space))
            speed *= 1.5f;

        if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
            _slimePosition.Y -= speed;

        if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
            _slimePosition.Y += speed;

        if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
            _slimePosition.X -= speed;

        if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
            _slimePosition.X += speed;
    }

    private void CheckGamePadInput()
    {
                GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

                float speed = MOVEMENT_SPEED;
                if (gamePadState.IsButtonDown(Buttons.A))
                {
                   speed *= 1.5f;
                   GamePad.SetVibration(PlayerIndex.One, 1.0f, 1.0f);
                }
                else
                {
                    GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
                }

                // The Alex test state:
                // Move to top left
                if (gamePadState.Triggers.Left == 1.0f)
                    _slimePosition = Vector2.Zero;
                // Move to bottom right
                else if (gamePadState.Triggers.Right == 1.0f)
                    _slimePosition = Middle() * 2 - new Vector2(_slime.Width, _slime.Height);
                // Move to bottom left
                else if (gamePadState.IsButtonDown(Buttons.LeftShoulder))
                    _slimePosition = new Vector2( 0, Middle().Y) * 2 - new Vector2(0.0f, _slime.Height);
                // Move to top right
                else if (gamePadState.IsButtonDown(Buttons.RightShoulder))
                    _slimePosition = new Vector2(Middle().X, 0) * 2 - new Vector2(_slime.Width, 0);


                // check thumbstick first since it has priority over which gamepad input is movement.
                if (gamePadState.ThumbSticks.Left != Vector2.Zero)
                {
                    _slimePosition.X += gamePadState.ThumbSticks.Left.X * speed;
                    _slimePosition.Y -= gamePadState.ThumbSticks.Left.Y * speed;
                }
                else
                {
                    //if Dpadup is down, move the slime up the screen.
                    if (gamePadState.IsButtonDown(Buttons.DPadUp))
                        _slimePosition.Y -= speed;

                    if (gamePadState.IsButtonDown(Buttons.DPadDown))
                        _slimePosition.Y += speed;

                    if (gamePadState.IsButtonDown(Buttons.DPadLeft))
                        _slimePosition.X -= speed;

                    if (gamePadState.IsButtonDown(Buttons.DPadRight))
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
        _bat.Draw(SpriteBatch, new Vector2(_slime.Width + 10, 0));

        // Always end the sprite batch when finished.
        SpriteBatch.End();

        base.Draw(gameTime);
    }
}

