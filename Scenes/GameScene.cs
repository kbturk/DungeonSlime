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
        _roomBounds.Inflate(-tilemap.TileWidth, -_tilemap.TileHeight);

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

    }

    public override void Update(GameTime gametime)
    {

    }


    public override void Draw()
    {

    }

}
