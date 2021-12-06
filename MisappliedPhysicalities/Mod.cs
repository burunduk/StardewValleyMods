using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MisappliedPhysicalities.Game;
using MisappliedPhysicalities.VirtualProperties;
using Netcode;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Tools;

namespace MisappliedPhysicalities
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        internal static IDynamicGameAssetsApi dga;
        internal static ContentPack dgaPack;

        public override void Entry( IModHelper helper )
        {
            Log.Monitor = Monitor;
            instance = this;

            Assets.Load( helper.Content );

            Helper.ConsoleCommands.Add( "mp_items", "...", OnItemsCommand );

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.Display.MenuChanged += OnMenuChanged;

            var harmony = new Harmony( ModManifest.UniqueID );
            harmony.PatchAll();
        }

        private void OnItemsCommand( string cmd, string[] args )
        {
            Dictionary<ISalable, int[]> stock = new();
            {
                stock.Add( new DrillTool(), new int[] { 0, int.MaxValue } );
                stock.Add( new ConveyorBelt(), new int[] { 0, int.MaxValue } );
                stock.Add( new Unhopper( Vector2.Zero ), new int[] { 0, int.MaxValue } );
                foreach ( var data in dgaPack.GetItems() )
                {
                    var item = data.ToItem();
                    stock.Add( item, new int[] { 0, int.MaxValue } );
                }
            }
            Game1.activeClickableMenu = new ShopMenu( stock );
        }

        private void OnGameLaunched( object sender, GameLaunchedEventArgs e )
        {
            var sc = Helper.ModRegistry.GetApi< ISpaceCoreApi >( "spacechase0.SpaceCore" );
            sc.RegisterSerializerType( typeof( NullObject ) );
            sc.RegisterSerializerType( typeof( DrillTool ) );
            sc.RegisterSerializerType( typeof( ConveyorBelt ) );
            sc.RegisterSerializerType( typeof( Unhopper ) );
            sc.RegisterCustomProperty( typeof( GameLocation ), "BelowGroundObjects",
                                       typeof( NetVector2Dictionary<StardewValley.Object, NetRef<StardewValley.Object>> ),
                                       AccessTools.Method( typeof( GameLocation_BelowGroundObjects ), nameof( GameLocation_BelowGroundObjects.get_BelowGroundObjects ) ),
                                       AccessTools.Method( typeof( GameLocation_BelowGroundObjects ), nameof( GameLocation_BelowGroundObjects.set_BelowGroundObjects ) ) );
            sc.RegisterCustomProperty( typeof( GameLocation ), "ElevatedObjects",
                                       typeof( NetVector2Dictionary<StardewValley.Object, NetRef<StardewValley.Object>> ),
                                       AccessTools.Method( typeof( GameLocation_ElevatedObjects ), nameof( GameLocation_ElevatedObjects.get_ElevatedObjects ) ),
                                       AccessTools.Method( typeof( GameLocation_ElevatedObjects ), nameof( GameLocation_ElevatedObjects.set_ElevatedObjects ) ) );

            dga = Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>( "spacechase0.DynamicGameAssets" );
            dga.AddEmbeddedPack( this.ModManifest, Path.Combine( Helper.DirectoryPath, "assets", "dga" ) );
            dgaPack = DynamicGameAssets.Mod.GetPacks().First( cp => cp.GetManifest().UniqueID == ModManifest.UniqueID );

            var gmcm = Helper.ModRegistry.GetApi< IGenericModConfigMenuApi >( "spacechase0.GenericModConfigMenu" );
        }

        private void OnMenuChanged( object sender, MenuChangedEventArgs e )
        {
            if ( !( e.NewMenu is ShopMenu shop ) )
                return;

            if ( shop.storeContext != "ClintUpgrade" )
                return;

            Tool orig = Game1.player.getToolFromName( "Axe" );
            if ( orig != null && ( orig.UpgradeLevel == 4 || orig.UpgradeLevel == 5 ) )
            {
                Tool tool = new Axe() { UpgradeLevel = orig.UpgradeLevel + 1 };
                shop.forSale.Add( tool );
                shop.itemPriceAndStock.Add( tool, new[] { tool.UpgradeLevel == 5 ? 100000 : 250000 } );
            }

            orig = Game1.player.getToolFromName( "Watering Can" );
            if ( orig != null && ( orig.UpgradeLevel == 4 || orig.UpgradeLevel == 5 ) )
            {
                Tool tool = new WateringCan() { UpgradeLevel = orig.UpgradeLevel + 1 };
                shop.forSale.Add( tool );
                shop.itemPriceAndStock.Add( tool, new[] { tool.UpgradeLevel == 5 ? 100000 : 250000 } );
            }

            orig = Game1.player.getToolFromName( "Pickaxe" );
            if ( orig != null && ( orig.UpgradeLevel == 4 || orig.UpgradeLevel == 5 ) )
            {
                Tool tool = new Pickaxe() { UpgradeLevel = orig.UpgradeLevel + 1 };
                shop.forSale.Add( tool );
                shop.itemPriceAndStock.Add( tool, new[] { tool.UpgradeLevel == 5 ? 100000 : 250000 } );
            }

            orig = Game1.player.getToolFromName( "Hoe" );
            if ( orig != null && ( orig.UpgradeLevel == 4 || orig.UpgradeLevel == 5 ) )
            {
                Tool tool = new Hoe() { UpgradeLevel = orig.UpgradeLevel + 1 };
                shop.forSale.Add( tool );
                shop.itemPriceAndStock.Add( tool, new[] { tool.UpgradeLevel == 5 ? 100000 : 250000 } );
            }
        }
    }
}
