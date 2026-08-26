using DungeonSlime.Scenes;
using Gum.Forms;
using Gum.Forms.Controls;
using MonoGameGum;
using Microsoft.Xna.Framework.Media;
using MonogameLibrary;

namespace DungeonSlime;

public class GameRoot : Core
{
    // the background theme song
    private Song _themeSong;

    public GameRoot() : base("Dungeon Slime", 1280, 720, false)
    {

    }

    protected override void Initialize()
    {

        base.Initialize();

        //Start playing thebackground music.
        Audio.PlaySong(_themeSong);

        //Initialize the Gum UI service.
        InitializeGum();

        //Start the game with the title scene.
        ChangeScene(new TitleScene());
    }

    protected override void LoadContent()
    {
        // load the background music
        _themeSong = Content.Load<Song>("audio/theme");


        // Ensure media player is not already plaing on device if so, stop it.
        if (MediaPlayer.State == MediaState.Playing)
        {
            MediaPlayer.Stop();
        }

        MediaPlayer.Play(_themeSong);

        MediaPlayer.IsRepeating = true;

    }

    // Boilerplate.
    private void InitializeGum()
    {
        // Initialize the Gum service. The second parameter specifies
        // the version of the default visuals to use. V3 is the latest
        // version.
        GumService.Default.Initialize(this, DefaultVisualsVersion.V3);

        // Tell the Gum service which content manager to use. We will tell it to
        // use the global content manager from our Core class.
        GumService.Default.ContentLoader.XnaContentManager = Core.Content;

        // Register keyboard input for UI control.
        FrameworkElement.KeyboardsForUiControl.Add(GumService.Default.Keyboard);

        // Register gamepad input for UI control.
        FrameworkElement.GamePadsForUiControl.Add(GumService.Default.Gamepads[0]);

        // Customize the tab reverse UI navigation to also trigger when the keyboard
        // Up arrow key is pushed.
        FrameworkElement.TabReverseKeyCombos.Add(
                new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Up });

        // Customize the tab UI navigation to also trigger when the keyboard
        // Down arrow key is pushed.
        FrameworkElement.TabKeyCombos.Add(
                new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Down });

        // The assets created for the UI were done so at 1/4th the size to keep the size of the
        // texture atlas small. So we will set the default canvas size to be 1/4th the size
        // of the game's resolution then tell gum to zoom in by a factor of 4.
        GumService.Default.CanvasWidth = GraphicsDevice.PresentationParameters.BackBufferWidth / 4.0f;
        GumService.Default.CanvasHeight = GraphicsDevice.PresentationParameters.BackBufferHeight / 4.0f;
        GumService.Default.Renderer.Camera.Zoom = 4.0f;
    }

}

