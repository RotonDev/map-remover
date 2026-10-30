using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace MapRemover.Common;

public class MapClearing : ModSystem
{
    // npc name renderign
    // player name rendering
    // spawn icon
    // pylon
    // bed icon
    // player heads
    // pings
    // revenge
    // death marker text
    // death marker minimap
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
        MonoModHooks.Modify(mDrawPlayerHead, StopPlayerHeadRendering);
        MonoModHooks.Modify(mDrawToMap, StopMapRendering);
        MonoModHooks.Modify(mDrawMapIcons_PotionOfReturn1, StopPotionOfReturnHomeDrawing);
        MonoModHooks.Modify(mDrawMapIcons_PotionOfReturn2, StopPotionOfReturnLastPosDrawing);
        MonoModHooks.Modify(mDrawMapIcons_LastGolfballHit, StopGolfballHitDrawing);
        MonoModHooks.Modify(mDrawNpcHeadFriendly, StopTownNpcHeadDrawing);
        MonoModHooks.Modify(mDrawNpcHeadBoss, StopBossHeadDrawing);
        MonoModHooks.Modify(mDrawPlayerDeathMarker, StopDeathMarkerDrawing);
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