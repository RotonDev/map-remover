using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace MapRemover.Common;

public class MapClearing : ModSystem
{
    // spawn icon
    // bed icon
    public override void Load()
    {
        MethodInfo mDrawToMap = typeof(Main).GetMethod("DrawToMap", BindingFlags.Instance | BindingFlags.NonPublic);
        MethodInfo mDrawMapIcons_PotionOfReturn1 = typeof(Main).GetMethod("DrawMapIcons_PotionOfReturnHomePosition", BindingFlags.Instance | BindingFlags.NonPublic);
        MethodInfo mDrawMapIcons_PotionOfReturn2 = typeof(Main).GetMethod("DrawMapIcons_PotionOfReturnAppearAfterUsePosition", BindingFlags.Instance | BindingFlags.NonPublic);
        MethodInfo mDrawMapIcons_LastGolfballHit = typeof(Main).GetMethod("DrawMapIcons_LastGolfballHit", BindingFlags.Instance | BindingFlags.NonPublic);
        MethodInfo mDrawNpcHeadFriendly = typeof(Main).GetMethod("DrawNPCHeadFriendly", BindingFlags.Static | BindingFlags.NonPublic);
        MethodInfo mDrawNpcHeadBoss = typeof(Main).GetMethod("DrawNPCHeadBoss", BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo mDrawPlayerHead = typeof(MapHeadRenderer).GetMethod("DrawPlayerHead", BindingFlags.Instance | BindingFlags.Public);
        MethodInfo mDrawPlayerDeathMarker = typeof(Main).GetMethod("DrawPlayerDeathMarker", BindingFlags.Static | BindingFlags.NonPublic);
        MethodInfo mDrawMap = typeof(Main).GetMethod("DrawMap", BindingFlags.Instance | BindingFlags.NonPublic);
        MethodInfo mUseMouseOver = typeof(CoinLossRevengeSystem.RevengeMarker).GetMethod("UseMouseOver", BindingFlags.Public | BindingFlags.Instance);
        MethodInfo mDrawMapIcon = typeof(CoinLossRevengeSystem.RevengeMarker).GetMethod("DrawMapIcon", BindingFlags.Public | BindingFlags.Instance);
        MethodInfo mDrawPings = typeof(PingMapLayer).GetMethod("Draw", BindingFlags.Instance | BindingFlags.Public);
        MethodInfo mDrawSpawns = typeof(SpawnMapLayer).GetMethod("Draw", BindingFlags.Instance | BindingFlags.Public);
        MethodInfo mDrawPylons = typeof(TeleportPylonsMapLayer).GetMethod("Draw", BindingFlags.Instance | BindingFlags.Public);
        MonoModHooks.Modify(mDrawSpawns, StopSpawnpointRendering);
        MonoModHooks.Modify(mDrawPlayerHead, StopPlayerHeadRendering);
        MonoModHooks.Modify(mDrawToMap, StopMapRendering);
        MonoModHooks.Modify(mDrawMapIcons_PotionOfReturn1, StopPotionOfReturnHomeDrawing);
        MonoModHooks.Modify(mDrawMapIcons_PotionOfReturn2, StopPotionOfReturnLastPosDrawing);
        MonoModHooks.Modify(mDrawMapIcons_LastGolfballHit, StopGolfballHitDrawing);
        MonoModHooks.Modify(mDrawNpcHeadFriendly, StopTownNpcHeadDrawing);
        MonoModHooks.Modify(mDrawNpcHeadBoss, StopBossHeadDrawing);
        MonoModHooks.Modify(mDrawPlayerDeathMarker, StopDeathMarkerDrawing);
        MonoModHooks.Modify(mDrawMap, StopHoverTexts);
        MonoModHooks.Modify(mUseMouseOver, StopDroppedMoneyRendering);
        MonoModHooks.Modify(mDrawMapIcon, StopMoneyIconRendering);
        MonoModHooks.Modify(mDrawPings, StopPingRendering);
        MonoModHooks.Modify(mDrawPylons, StopPylonRendering);
    }
    private static void StopPylonRendering(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        ILLabel skip = EmitSkipForBoolConfigOption(il, c, "RenderPylons", true);
        c.EmitRet();
        c.MarkLabel(skip);
    }
    private static void StopSpawnpointRendering(ILContext il)
    {
        Type configType = typeof(MapConfig);
        MethodInfo configGetInstance = configType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
        FieldInfo worldSpawn = configType.GetField("RenderWorldSpawnIcon", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        FieldInfo playerSpawn = configType.GetField("RenderSpawnpointIcon", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

        ILCursor c = new ILCursor(il);
        ILLabel retLabel = c.DefineLabel();
        ILLabel skipFirstCheckLabel = c.DefineLabel();

        c.GotoNext(MoveType.After, i => i.MatchCall<Vector2>(".ctor"));
        c.EmitCall(configGetInstance);
        c.EmitLdfld(worldSpawn);
        c.EmitBrfalse(skipFirstCheckLabel);

        c.GotoNext(MoveType.After, i => i.MatchStindRef());
        c.MarkLabel(skipFirstCheckLabel);
        c.EmitCall(configGetInstance);
        c.EmitLdfld(playerSpawn);
        c.EmitBrfalse(retLabel);

        c.GotoNext(i => i.MatchRet());
        c.MarkLabel(retLabel);
        // move into condition :)
    }
    private static void StopPingRendering(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        ILLabel skip = EmitSkipForBoolConfigOption(il, c, "RenderPings", true);
        c.EmitRet();
        c.MarkLabel(skip);
    }
    private static void StopMoneyIconRendering(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        ILLabel skip = EmitSkipForBoolConfigOption(il, c, "RenderDroppedCoins", true);
        c.EmitLdcI4(0);
        c.EmitRet();
        c.MarkLabel(skip);
    }
    private static void StopDroppedMoneyRendering(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        ILLabel skip = EmitSkipForBoolConfigOption(il, c, "RenderDroppedCoins", true);
        c.EmitRet();
        c.MarkLabel(skip);
    }
    private static void StopHoverTexts(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        int textIndex = -1;
        c.GotoNext(i => i.MatchStloc(out textIndex));

        // do this twice for full map/minimap (genius!)
        for (int i = 0; i < 2; i++)
        {
            // npc names
            c.GotoNext(MoveType.After, i => i.MatchCallvirt<NPC>("get_FullName"), i => i.MatchStloc(textIndex));
            EmitStringSwapForCondition(il, c, "RenderNpcHeads", textIndex);
            // boss names
            c.GotoNext(MoveType.After, i => i.MatchCallvirt<NPC>("get_GivenOrTypeName"), i => i.MatchStloc(textIndex));
            EmitStringSwapForCondition(il, c, "RenderBossHeads", textIndex);
            // player names
            c.GotoNext(MoveType.After, i => i.MatchLdfld<Player>("name"), i => i.MatchStloc(textIndex));
            EmitStringSwapForCondition(il, c, "RenderPlayerHeads", textIndex);

            // death timestamp
            // even though this pattern occurs twice,
            // the second one is in a different order
            // so we can't use loops here! sad!
            if (i == 0)
            {
                c.GotoNext(i => i.MatchCallvirt<SpriteBatch>("Draw"));
                c.GotoPrev(i => i.MatchBgeUn(out _));
                // zero out value in check pre death marker rendering
                // if the user doesn't want them rendered
                ILLabel zeroSkip = EmitSkipForBoolConfigOption(il, c, "RenderDeathMarkers", branchIf: true);
                c.EmitPop();
                c.EmitLdcI4(0);
                c.MarkLabel(zeroSkip);

                c.GotoNext(MoveType.After, i => i.MatchLdstr("Game.PlayerDeathTime"));
                c.GotoNext(MoveType.After, i => i.MatchCall("Terraria.Localization.Language", "GetTextValue"), i => i.MatchStloc(textIndex));
                EmitStringSwapForCondition(il, c, "RenderDeathMarkers", textIndex);
            }
        }
        // move back to the beginning of the method
        while (c.TryGotoPrev(i => i.MatchStloc(textIndex)))
        {
           
        }
        //chest names
        for (int i = 0; i < 2; i++)
        {
            c.GotoNext(MoveType.After, i => i.MatchCall<Main>("DrawMap_FindChestName"), i => i.MatchStloc(textIndex));
            EmitStringSwapForCondition(il, c, "DontRenderTerrain", textIndex, branchIf: false);
        }
        for (int i = 0; i < 2; i++)
        {
            c.GotoNext(MoveType.After, i => i.MatchCallvirt<LocalizedText>("get_Value"), i => i.MatchStloc(textIndex));
            EmitStringSwapForCondition(il, c, "DontRenderTerrain", textIndex, branchIf: false);
        }

        //dresser names lol
        c.GotoNext(MoveType.After, i => i.MatchCall<Chest>("FindChest"));
        c.GotoNext(MoveType.After, i => i.MatchStloc(textIndex));
        EmitStringSwapForCondition(il, c, "DontRenderTerrain", textIndex, branchIf: false);

        //tile names
        c.GotoNext(MoveType.After, i => i.MatchCallvirt<MapLegend>("FromTile"), i => i.MatchStloc(textIndex));
        EmitStringSwapForCondition(il, c, "DontRenderTerrain", textIndex, branchIf: false);

        c.GotoNext(MoveType.After, i => i.MatchLdstr("Game.PlayerDeathTime"));
        c.GotoNext(MoveType.After, i => i.MatchCall("Terraria.Localization.Language", "GetTextValue"), i => i.MatchStloc(textIndex));
        EmitStringSwapForCondition(il, c, "RenderDeathMarkers", textIndex);



        static void EmitStringSwapForCondition(ILContext il, ILCursor c, string condition, int variableIndex, bool branchIf = true)
        {
            ILLabel skip = EmitSkipForBoolConfigOption(il, c, condition, branchIf);
            c.EmitLdstr("");
            c.EmitStloc(variableIndex);
            c.MarkLabel(skip);
        }
    }
    private static void StopDeathMarkerDrawing(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        ILLabel skip = EmitSkipForBoolConfigOption(il, c, "RenderDeathMarkers", branchIf: true);
        c.EmitLdcI4(0);
        c.EmitRet();
        c.MarkLabel(skip);
    }
    private static void StopPlayerHeadRendering(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        ILLabel skip = EmitSkipForBoolConfigOption(il, c, "RenderPlayerHeads", branchIf: true);
        c.EmitRet();
        c.MarkLabel(skip);
    }
    private static void StopTownNpcHeadDrawing(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        ILLabel skip = EmitSkipForBoolConfigOption(il, c, "RenderNpcHeads", branchIf: true);
        c.EmitRet();
        c.MarkLabel(skip);
    }
    private static void StopBossHeadDrawing(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        ILLabel skip = EmitSkipForBoolConfigOption(il, c, "RenderBossHeads", branchIf: true);
        c.EmitRet();
        c.MarkLabel(skip);
    }
    private static void StopGolfballHitDrawing(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        ILLabel skip = EmitSkipForBoolConfigOption(il, c, "RenderLastGolfBallHit", branchIf: true);
        c.EmitRet();
        c.MarkLabel(skip);
    }
    private static void StopPotionOfReturnLastPosDrawing(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        ILLabel skip = EmitSkipForBoolConfigOption(il, c, "RenderPotionOfReturnLastPos", branchIf: true);
        c.EmitRet();
        c.MarkLabel(skip);
    }
    private static void StopPotionOfReturnHomeDrawing(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        ILLabel skip = EmitSkipForBoolConfigOption(il, c, "RenderPotionOfReturnHome", branchIf: true);
        c.EmitRet();
        c.MarkLabel(skip);
    }

    private static void StopMapRendering(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        Type configType = typeof(MapConfig);
        MethodInfo configGetInstance = configType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
        FieldInfo dontRenderTerrain = configType.GetField("DontRenderTerrain", BindingFlags.Public | BindingFlags.Instance);
        // forcefully set clearMap to true to ensure the map gets cleared
        /*
         * Main.mapReady = true;
		Main.loadMapLastX = 0;
		Main.loadMap = false;
		Main.loadMapLock = false;
         */
        ILLabel skip = EmitSkipForBoolConfigOption(il, c, "DontRenderTerrain",branchIf:false);
        c.EmitLdcI4(1);
        c.EmitStsfld(typeof(Main).GetField("clearMap", BindingFlags.Static | BindingFlags.Public));
        c.EmitLdcI4(1);
        c.EmitStsfld(typeof(Main).GetField("mapReady", BindingFlags.Static | BindingFlags.Public));
        c.EmitLdcI4(0);
        c.EmitStsfld(typeof(Main).GetField("loadMap", BindingFlags.Static | BindingFlags.Public));
        c.EmitLdcI4(0);
        c.EmitStsfld(typeof(Main).GetField("loadMapLock", BindingFlags.Static | BindingFlags.Public));
        c.EmitLdcI4(0);
        c.EmitStsfld(typeof(Main).GetField("loadMapLastX", BindingFlags.Static | BindingFlags.Public));
        c.MarkLabel(skip);
        // match to the last instance of Main.clearMap being referenced
        // conveniently it's right after the map has been cleared!
        while (c.TryGotoNext(MoveType.After, i => i.MatchStsfld<Main>("clearMap")))
        {

        }
        ILLabel skipRender = EmitSkipForBoolConfigOption(il, c, "DontRenderTerrain", branchIf: false);
        c.EmitRet();
        c.MarkLabel(skipRender);
    }
    /// <summary>
    /// Emits a basic branching pattern for checking a boolean config option.
    /// You will have to mark the branch target yourself after emitting other instructions.
    /// </summary>
    /// <param name="il"></param>
    /// <param name="c"></param>
    /// <param name="optionName">The name of the <see cref="MapConfig"/> field you want to check</param>
    /// <param name="branchIf">The value for which instructions ahead should be jumped over.</param>
    /// <returns>The label to mark the branch target with.</returns>
    private static ILLabel EmitSkipForBoolConfigOption(ILContext il, ILCursor c, string optionName, bool branchIf = false)
    {
        ILLabel skipLabel = c.DefineLabel();
        Type configType = typeof(MapConfig);
        MethodInfo configGetInstance = configType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
        FieldInfo field = configType.GetField(optionName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        c.EmitCall(configGetInstance);
        c.EmitLdfld(field);
        if (branchIf)
            c.EmitBrtrue(skipLabel);
        else
            c.EmitBrfalse(skipLabel);
        return skipLabel;
    }
}