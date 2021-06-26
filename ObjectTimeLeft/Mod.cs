using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ObjectTimeLeft.Framework;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SObject = StardewValley.Object;

namespace ObjectTimeLeft
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static Configuration Config;

        private bool Showing;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            Mod.Config = helper.ReadConfig<Configuration>();
            this.Showing = Mod.Config.ShowOnStart;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.RenderingHud += this.OnRenderingHud;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var capi = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (capi != null)
            {
                capi.RegisterModConfig(this.ModManifest, () => Mod.Config = new Configuration(), () => this.Helper.WriteConfig(Mod.Config));
                capi.RegisterSimpleOption(this.ModManifest, "Show on Start", "Whether to start the game with time left already showing.", () => Mod.Config.ShowOnStart, val => Mod.Config.ShowOnStart = val);
                capi.RegisterSimpleOption(this.ModManifest, "Key: Toggle Display", "The key to toggle the display on objects.", () => Mod.Config.ToggleKey, val => Mod.Config.ToggleKey = val);
                capi.RegisterSimpleOption(this.ModManifest, "Text Scale", "Scale of text that will superimpose the objects.", () => Mod.Config.TextScale, val => Mod.Config.TextScale = val);
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == Mod.Config.ToggleKey)
                this.Showing = !this.Showing;
        }

        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderingHud(object sender, RenderingHudEventArgs e)
        {
            if (!this.Showing || !Context.IsPlayerFree)
                return;

            Color shadowColor = Color.Black * 0.5f;
            float zoom = this.GetOptions().zoomLevel;
            float scale = zoom * Mod.Config.TextScale;

            void DrawString(string str, Vector2 position, Color color)
            {
                e.SpriteBatch.DrawString(
                    spriteFont: Game1.dialogueFont,
                    text: str,
                    position: position,
                    color: color,
                    rotation: 0.0f,
                    origin: Vector2.Zero,
                    scale: scale,
                    effects: SpriteEffects.None,
                    layerDepth: 0.0f
                );
            }

            foreach (var pair in Game1.currentLocation.Objects.Pairs)
            {
                SObject obj = pair.Value;
                if (obj.MinutesUntilReady is <= 0 or 999999 || obj.Name == "Stone")
                    continue;

                string text = (obj.MinutesUntilReady / 10).ToString();
                Vector2 pos = this.GetTimeLeftPosition(pair.Key, text, zoom);

                // draw text outline for contrast
                void DrawOutline(int offsetX, int offsetY)
                {
                    Vector2 shadowPos = new(pos.X + (offsetX * zoom), pos.Y + (offsetY * zoom));
                    DrawString(text, shadowPos, shadowColor);
                }
                DrawOutline(0, 3);
                DrawOutline(3, 0);
                DrawOutline(0, -3);
                DrawOutline(-3, 0);

                // draw text
                DrawString(text, pos, Color.White);
            }
        }

        /// <summary>Get the position at which to draw the given text for a machine.</summary>
        /// <param name="tile">The tile position containing the machine.</param>
        /// <param name="text">The text to draw over the machine.</param>
        /// <param name="zoom">The UI zoom to apply.</param>
        private Vector2 GetTimeLeftPosition(Vector2 tile, string text, float zoom)
        {
            // get screen pixel position
            Vector2 pos = Game1.GlobalToLocal(
                Game1.viewport,
                new Vector2(x: tile.X * Game1.tileSize, y: tile.Y * Game1.tileSize)
            );

            // center text over tile
            float textWidth = Game1.dialogueFont.MeasureString(text).X;
            pos.X += (Game1.tileSize - textWidth) / 2;

            // apply zoom level
            return pos * zoom;
        }

        /// <summary>Get the <see cref="Game1.options"/> property.</summary>
        private Options GetOptions()
        {
            return Constants.TargetPlatform == GamePlatform.Android
                ? this.Helper.Reflection.GetField<Options>(typeof(Game1), "options").GetValue()
                : Game1.options;
        }
    }
}
