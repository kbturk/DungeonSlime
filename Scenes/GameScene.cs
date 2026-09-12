using System;
using DungeonSlime.GameObjects;
using DungeonSlime.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonogameLibrary;
using MonogameLibrary.Graphics;
using MonogameLibrary.Scenes;

namespace DungeonSlime.Scenes;

public class GameScene : Scene
{
    private enum GameState
    {
        Playing,
        Paused,
        GameOver
    }

    // Reference to the slime.
    private Slime _slime;

    // Reference to the bat.
    private Bat _bat;

    private Tilemap _tilemap;

    private Rectangle _roomBounds;

    private SoundEffect _collectSoundEffect;

    private int _score;

    private GameSceneUI _ui;

    private GameState _state;

    // TODO: evaluate putting this in the GameSceneUI
    private ScoreManager _scoreManager;

    public override void Initialize()
    {
        //LoadContent is called during base.Initialize()
        base.Initialize();

        //During the game scene, we want to disable exit on escape. Instead
        //the escape key will be used to return back to the title screen.
        Core.ExitOnEscape = false;

        //Create the room bounds by getting the bounds of the screen then
        //using the Inflate method to "Deflate" the bounds by the width and height
        //of a title so that the boudns only covers the inside room of the
        //dungeon tilemap.
        _roomBounds = Core.GraphicsDevice.PresentationParameters.Bounds;
        _roomBounds.Inflate(-_tilemap.TileWidth, -_tilemap.TileHeight);

        // Subscribe to the slime's BodyCollision event so that a game over
        // can be triggered when this event is raised.
        _slime.BodyCollision += OnSlimeBodyCollision;

        //Create any UI elements from the root element created in pervious
        //scenes.
        GumService.Default.Root.Children.Clear();

        //Initialize the user interface for the game scene.
        InitializeUI();

        // Initialize a new game to be played.
        InitializeNewGame();

    }

    private void InitializeUI()
    {
        // Clera out any previous UI element in case we came
        // here from a different scene.
        GumService.Default.Root.Children.Clear();

        // Create the game scene ui instance.
        _ui = new GameSceneUI();

        // Subscribe to the events from the game scene ui.
        _ui.ResumeButtonClick +=  OnResumeButtonClicked;
        _ui.RetryButtonClick +=  OnRetryButtonClicked;
        _ui.QuitButtonClick +=  OnQuitButtonClicked;
    }

    // game scene UI event handlers. there are event handlers with the same name in the GameSceneUI class.

    private void OnResumeButtonClicked(object sender, EventArgs args)
    {
        // Change the game state back to playing.
        _state = GameState.Playing;
    }

    private void OnRetryButtonClicked(object sender, EventArgs args)
    {
        //player has chosen to retry, so initialize a new game.
        InitializeNewGame();
    }

    private void OnQuitButtonClicked(object sender, EventArgs args)
    {
        //player has chosen to quit, so return to title screen.
        Core.ChangeScene(new TitleScene());
    }

    private void InitializeNewGame()
    {
        // Calculate the position for the slime, which will be at the
        // center tile of the tile map.
        Vector2 slimePos = new Vector2();
        slimePos.X = (_tilemap.Columns / 2) * _tilemap.TileWidth;
        slimePos.Y = (_tilemap.Rows / 2) * _tilemap.TileHeight;

        // Initialize the slime.
        _slime.Initialize(slimePos, _tilemap.TileWidth);

        // Initialize the bat.
        _bat.RandomizeVelocity();
        PositionBatAwayFromSlime();

        // Reset the score.
        _score = 0;

        // Set the game state to playing.
        _state = GameState.Playing;
    }

    public override void LoadContent()
    {
        // Create the texture atlas from the XML
        // configuration file.
        TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");

        // Create the tilemap from the XML configureation file.
        _tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");
        _tilemap.Scale = new Vector2(4.0f, 4.0f);

        // Create the animated sprite for the eslime from the atlas.
        AnimatedSprite slimeAnimation = atlas.CreateAnimatedSprite("slime-animation");
        slimeAnimation.Scale = new Vector2(4.0f, 4.0f);

        //create the slime.
        _slime = new Slime(slimeAnimation);

        // Create the animated sprite for the eslime from the atlas.
        AnimatedSprite batAnimation = atlas.CreateAnimatedSprite("bat-animation");
        batAnimation.Scale = new Vector2(4.0f, 4.0f);

        // Load the bounce sand effect for the bat.
        SoundEffect bounceSoundEffect = Content.Load<SoundEffect>("audio/bounce");

        //create the bat.
        _bat = new Bat(batAnimation, bounceSoundEffect);

        // Load the collect sound effect.
        _collectSoundEffect = Content.Load<SoundEffect>("audio/collect");

        _scoreManager = new ScoreManager();
        _scoreManager.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {
        // Ensure the UI is always updated.
        _ui.Update(gameTime);

        // If the game is in the game over state, immediately return.
        // here.
        if (_state == GameState.GameOver)
        {
            return;
        }

        // We don't process the user pressing pause
        // unless we're not in the game over screen.
        if (GameController.Pause())
            TogglePause();

        // like game over, if we're in the pause state no
        // more updating is required. just return.
        if (_state == GameState.Paused)
            return;

        // So we're in the game state, so let's do some
        // updating.

        // Update the slime.
        _slime.Update(gameTime);

        // Update the bat.
        _bat.Update(gameTime);

        // Perform the collision checks.
        CollisionChecks();
    }

    private void CollisionChecks()
    {
        // Capture the current bounds of the slime and bat.
        Circle slimeBounds = _slime.GetBounds();
        Circle batBounds = _bat.GetBounds();

        // First perform a collision check to see if the slime is
        // colliding with the bat, which means the slime eats the bat.
        if (slimeBounds.Intersects(batBounds))
        {
            // Move the bat to a new position away from the slime.
            PositionBatAwayFromSlime();

            // Randomize the velocity of the bat.
            _bat.RandomizeVelocity();

            // Grow that slime!
            _slime.Grow();

            // Give some points!
            _score += 100;

            // Update the score display on the UI.
            _ui.UpdateScoreText(_score);

            // Play the sound effect
            Core.Audio.PlaySoundEffect(_collectSoundEffect);
        }

        // Next check if the slime collided (game over)
        if (slimeBounds.Top < _roomBounds.Top ||
                slimeBounds.Bottom > _roomBounds.Bottom ||
                slimeBounds.Left < _roomBounds.Left ||
                slimeBounds.Right > _roomBounds.Right)
        {
            GameOver();
            return;
        }

        // finally: check if the bat is colliding with a wall
        // by validating it is within the bounds of the room.
        // If it's outside the room bounds, then it collided and
        // should bounce off the wall.
        if (batBounds.Top < _roomBounds.Top)
            _bat.Bounce(Vector2.UnitY);
        else if (batBounds.Bottom > _roomBounds.Bottom)
            _bat.Bounce(-Vector2.UnitY);
        if (batBounds.Left < _roomBounds.Left)
            _bat.Bounce(Vector2.UnitX);
        else if (batBounds.Right > _roomBounds.Right)
            _bat.Bounce(-Vector2.UnitX);
    }

    private void PositionBatAwayFromSlime()
    {
        // Calcute the position that is in the center of the bounds
        // of the room.
        float roomCenterX = _roomBounds.X + _roomBounds.Width * 0.5f;
        float roomCenterY = _roomBounds.Y + _roomBounds.Height * 0.5f;
        Vector2 roomCenter = new Vector2( roomCenterX, roomCenterY);

        // Get the bounds of the slime and calculate the center position.
        Circle slimeBounds = _slime.GetBounds();
        Vector2 slimeCenter = new Vector2(slimeBounds.X, slimeBounds.Y);

        // Calculate the distance vector from the center of the room to
        // the center of the slime.
        Vector2 centerToSlime = slimeCenter - roomCenter;

        // Get the bounds of the bat.
        Circle batBounds = _bat.GetBounds();

        // Calculate the amount ofpadding we will add to the new position
        // to ensure the bat doesn't stick to the walls.
        int padding = batBounds.Radius * 2;

        // Calculate the new position of the bat by finding which
        // component the center to the slime is larger and in which
        // direction.
        Vector2 newBatPosition = Vector2.Zero;
        if (Math.Abs(centerToSlime.X) > Math.Abs(centerToSlime.Y))
        {
            // the slime is closer to either the left or 
            // right wall instead of the top or bottom,
            // so we'll put the bat on the opposite
            // (left or right) side of the room. 
            // This means the Y position can be randomly
            // selected from the whole height of the room.
            // without intersecting the slime.
            newBatPosition.Y = Random.Shared.Next(
                    _roomBounds.Top + padding,
                    _roomBounds.Bottom - padding
                    );

            // figure out the X bat component:
            if (centerToSlime.X > 0)
            {
                // The slime is closer to the right side wall
                // so put the bat somewhere on the left.
                newBatPosition.X = _roomBounds.Left + padding;
            }
            else
            {
                // the slime is closer to the left wall,
                // so place the bat on the right side.
                newBatPosition.X = _roomBounds.Right - padding * 2;
            }
        }
        else
        {
            // the slime is closer to either the top or 
            // bottom wall so we'll put the bat on the opposite
            // (bottom/top) side of the room. 
            // This means the X position can be randomly
            // selected from the whole width of the room.
            // without intersecting the slime.
            newBatPosition.X = Random.Shared.Next(
                    _roomBounds.Left + padding,
                    _roomBounds.Right - padding
                    );

            // figure out the Y bat component:
            if (centerToSlime.Y > 0)
            {
                // The slime is closer to the top wall
                // so put the bat somewhere on the bottom.
                newBatPosition.Y = _roomBounds.Top + padding;
            }
            else
            {
                // the slime is closer to the bottom wall,
                // so place the bat on the top.
                newBatPosition.Y = _roomBounds.Bottom - padding * 2;
            }
        }

        _bat.Position = newBatPosition;
    }

    //Event Handlers:
    private void OnSlimeBodyCollision(object sender, EventArgs args)
    {
        GameOver();
    }

    private void TogglePause()
    {
        if (_state == GameState.Paused)
        {
            // we're unpausing the game, so hide the pause panel.
            _ui.HidePausePanel();

            // change the state to playing.
            _state = GameState.Playing;
        }
        else
        {
            // We're now pausing the game, so show the panel
            // and change the state.
            _ui.ShowPausePanel();

            _state = GameState.Paused;
        }
    }

    private void GameOver()
    {
        // Show the game over panel.
        _ui.ShowGameOverPanel();

        // Set the game state to game over.
        _state = GameState.GameOver;

        // Save scores:
        _scoreManager.AddNewScore("MOM", _score);
        _scoreManager.SaveScores();

    }

    public override void Draw(GameTime gameTime)
    {
        // Clear the back buffer.
        Core.GraphicsDevice.Clear(Color.CornflowerBlue);

        // Begin the sprite batch to prepare for rendering.
        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw the tilemap
        _tilemap.Draw(Core.SpriteBatch);

        // Draw the slime
        _slime.Draw();

        // Draw the bat.
        _bat.Draw();

        // Always end the sprite batch.
        Core.SpriteBatch.End();

        // Draw the UI. I believe it has its own
        // spritebatch.
        _ui.Draw();
    }

}
