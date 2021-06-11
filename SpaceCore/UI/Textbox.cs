using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;

namespace SpaceCore.UI
{
    public class Textbox : Element, IKeyboardSubscriber
    {
        private Texture2D tex;
        private SpriteFont font;

        public virtual string String { get; set; }

        private bool selected;
        public bool Selected
        {
            get { return this.selected; }
            set
            {
                if (this.selected == value)
                    return;

                this.selected = value;
                if (this.selected)
                    Game1.keyboardDispatcher.Subscriber = this;
                else
                {
                    if (Game1.keyboardDispatcher.Subscriber == this)
                        Game1.keyboardDispatcher.Subscriber = null;
                }
            }
        }

        public Action<Element> Callback { get; set; }

        public Textbox()
        {
            this.tex = Game1.content.Load<Texture2D>("LooseSprites\\textBox");
            this.font = Game1.smallFont;
        }

        public override int Width => 192;
        public override int Height => 48;

        public override void Update(bool hidden = false)
        {
            base.Update(hidden);

            if (this.ClickGestured && this.Callback != null)
            {
                if (this.Hover)
                    this.Selected = true;
                else
                    this.Selected = false;
            }
        }

        public override void Draw(SpriteBatch b)
        {
            b.Draw(this.tex, this.Position, Color.White);

            // Copied from game code - caret
            string text = this.String;
            Vector2 vector2;
            for (vector2 = this.font.MeasureString(text); (double)vector2.X > (double)192; vector2 = this.font.MeasureString(text))
                text = text.Substring(1);
            if (DateTime.UtcNow.Millisecond % 1000 >= 500 && this.Selected)
                b.Draw(Game1.staminaRect, new Rectangle((int)this.Position.X + 16 + (int)vector2.X + 2, (int)this.Position.Y + 8, 4, 32), Game1.textColor);

            b.DrawString(this.font, text, this.Position + new Vector2(16, 12), Game1.textColor);
        }

        protected virtual void receiveInput(string str)
        {
            this.String += str;
            if (this.Callback != null)
                this.Callback.Invoke(this);
        }

        public void RecieveTextInput(char inputChar)
        {
            this.receiveInput(inputChar.ToString());

            // Copied from game code
            switch (inputChar)
            {
                case '"':
                    return;
                case '$':
                    Game1.playSound("money");
                    break;
                case '*':
                    Game1.playSound("hammer");
                    break;
                case '+':
                    Game1.playSound("slimeHit");
                    break;
                case '<':
                    Game1.playSound("crystal");
                    break;
                case '=':
                    Game1.playSound("coin");
                    break;
                default:
                    Game1.playSound("cowboy_monsterhit");
                    break;
            }
        }

        public void RecieveTextInput(string text)
        {
            this.receiveInput(text);
        }

        public void RecieveCommandInput(char command)
        {
            if (command == '\b' && this.String.Length > 0)
            {
                Game1.playSound("tinyWhip");
                this.String = this.String.Substring(0, this.String.Length - 1);
                if (this.Callback != null)
                    this.Callback.Invoke(this);
            }
        }

        public void RecieveSpecialInput(Keys key)
        {
        }
    }
}
