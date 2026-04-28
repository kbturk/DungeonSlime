using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonogameLibrary;

namespace DungeonSlime;

public class GameRoot: Core
{
    private Texture2D _logo;
    private float LogoFade = 1;
    private float WordFade = 0;
    private int millsecondsPerFrame = 70;
    private int timeSinceLastFrame = 0;

    public GameRoot(): base("Dungeon Slime", 1280, 720, false)
    {

    }

    protected override void Initialize()
    {

        base.Initialize();
    }

    protected override void LoadContent()
    {

        _logo = Content.Load<Texture2D>("images/logo");

        //base.LoadContent();
    }

    private float FadeOut(GameTime gameTime, float fade)
    {
        timeSinceLastFrame +=(int)gameTime.ElapsedGameTime.Milliseconds;
        if (timeSinceLastFrame > millsecondsPerFrame) {
            if (fade > 0.0f)
            fade -= 0.05f;
            else fade = 0.0f;
            timeSinceLastFrame = 0;
        }
        return fade;
    }


    private float FadeIn(GameTime gameTime, float fade)
    {
        timeSinceLastFrame +=(int)gameTime.ElapsedGameTime.Milliseconds;
        if (timeSinceLastFrame > millsecondsPerFrame) {
            if (fade < 1.0f)
            fade += 0.05f;
            else fade = 1.0f;
            timeSinceLastFrame = 0;
        }
        return fade;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        if (LogoFade > 0.0f)
            LogoFade = FadeOut(gameTime, LogoFade);
        if (WordFade < 1.0f)
            WordFade = FadeIn(gameTime, WordFade);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        Rectangle iconSourceRect = new Rectangle(0, 0, 128, 128);
        Rectangle wordmarkSourceRect = new Rectangle(150, 34, 458, 58);

        SpriteBatch.Begin(sortMode: SpriteSortMode.FrontToBack);

        //Draw only the icon portion
        SpriteBatch.Draw(
                _logo,                   //texture
                base.Middle(),           //position
                iconSourceRect,          //source rectangle
                Color.White * LogoFade,  //color
                0.0f,                    //rotation
                new Vector2(             //origin
                    iconSourceRect.Width,
                    iconSourceRect.Height)*0.5f,
                1.5f,                    //scale
                SpriteEffects.None,      //effects
                1.0f                     //layerdepth
                    );

        //Draw only the word
        SpriteBatch.Draw(
                _logo,                    //texture
                base.Middle(),            //position
                wordmarkSourceRect,       //source rectangle
                Color.White * WordFade,   //color
                0.0f,                     //rotation
                new Vector2(              //origin
                    wordmarkSourceRect.Width,
                    wordmarkSourceRect.Height)*0.5f,
                1.5f,                     //scale
                SpriteEffects.None,       //effects
                0.0f                      //layerdepth
                    );
        SpriteBatch.End();

        base.Draw(gameTime);
    }
}
