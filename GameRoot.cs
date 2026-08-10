using DungeonSlime.Scenes;
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

}

