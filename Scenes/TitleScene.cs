using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonogameLibrary;
using MonogameLibrary.Scenes;

namespace DungeonSlime.Scenes;

public class TitleScene : Scene
{
private const string DUNGEON_TEXT = "Dungeon";
private const string SLIME_TEXT = "Slime";
private const string PRESS_ENTER_TEXT = "Press Enter To Start";

// normal text font
private SpriteFont _font;

// title text font
private SpriteFont _font5x;

// dungeon text position
private Vector2 _dungeonTextPos;

// dungeon text origin
private Vector2 _dungeonTextOrigin;

// slime text position
private Vector2 _slimeTextPos;

// slime text origin
private Vector2 _slimeTextOrigin;

// 'press enter' text position
private Vector2 _pressEnterPos;

// 'press enter' text origin
private Vector2 _pressEnterOrigin;


public override void Initialize()
{
    //LoadContent is called during base.Initialize()
    base.Initialize();

    //While on the title screen, we can enable exit on escape so the player
    //can close the game by pressing the escape key.
    Core.ExitOnEscape = true;

    //Set the position and origin for the Dungeon text.
    Vector2  size = _font5x.MeasureString(DUNGEON_TEXT);
    _dungeonTextPos = new Vector2(640, 100);
    _dungeonTextOrigin = size * 0.5f;

    //Set the position and origin for the Slime text.
    size = _font5x.MeasureString(SLIME_TEXT);
    _slimeTextPos = new Vector2(757, 207);
    _slimeTextOrigin = size * 0.5f;

    //Set the position and origin for the Press Enter text.
    size = _font.MeasureString(PRESS_ENTER_TEXT);
    _pressEnterPos = new Vector2(640, 620);
    _pressEnterOrigin = size * 0.5f;
}

public override void LoadContent()
{
    // Load the font for the standard text
    _font = Core.Content.Load<SpriteFont>("fonts/04B_30");
    // Load the font for the large text
    _font5x = Core.Content.Load<SpriteFont>("fonts/04B_30_5x");
}

public override void Update(GameTime gameTime)
{
    // If the user presses enter, switch to game scene.
    // Escape to exit is handled in the core library and enabled at initialize.
    if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Enter))
        Core.ChangeScene(new GameScene());
}

public override void Draw(GameTime gameTime)
{
    Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

    //begin the sprite batch to prepare for rendering.
    Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

    //drop shadow text color
    Color dropShadowColor = Color.Black * 0.5f;

    //Make a shadow on the Dungeon Text by rendering a slightly offset version of the text
    //with a lot of transparency to give it a drop shadow.
    Core.SpriteBatch.DrawString(_font5x,
            DUNGEON_TEXT,
            _dungeonTextPos + new Vector2(10,10),
            dropShadowColor,
            0.0f,
            _dungeonTextOrigin,
            1.0f,
            SpriteEffects.None,
            1.0f);

    //Now rendor the main text
    Core.SpriteBatch.DrawString(_font5x,
            DUNGEON_TEXT,
            _dungeonTextPos,
            Color.White,
            0.0f,
            _dungeonTextOrigin,
            1.0f,
            SpriteEffects.None,
            1.0f);

    //Do the same thing with the Slime text:
    //
    Core.SpriteBatch.DrawString(_font5x,
            SLIME_TEXT,
            _slimeTextPos + new Vector2(10,10),
            dropShadowColor,
            0.0f,
            _slimeTextOrigin,
            1.0f,
            SpriteEffects.None,
            1.0f);

    //Now rendor the main text
    Core.SpriteBatch.DrawString(_font5x,
            SLIME_TEXT,
            _slimeTextPos,
            Color.White,
            0.0f,
            _slimeTextOrigin,
            1.0f,
            SpriteEffects.None,
            1.0f);

    //Press enter text doesn't get a drop shadow
    Core.SpriteBatch.DrawString(_font,
            PRESS_ENTER_TEXT,
            _pressEnterPos,
            Color.White,
            0.0f,
            _pressEnterOrigin,
            1.0f,
            SpriteEffects.None,
            0.0f);

    //Always end the sprite batch when finished.
    Core.SpriteBatch.End();
    }

}
