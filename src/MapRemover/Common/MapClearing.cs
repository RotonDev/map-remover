using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace MapRemover.Common;

public class MapClearing : ModSystem
{
    public override void Load()
    {
        MethodInfo mDrawToMap = typeof(Main).GetMethod("DrawToMap", BindingFlags.Instance | BindingFlags.NonPublic);
        MethodInfo mDrawToMap_Section = typeof(Main).GetMethod("DrawToMap_Section", BindingFlags.Instance | BindingFlags.NonPublic);
        MonoModHooks.Modify(mDrawToMap, StopMapRendering);
        //MonoModHooks.Modify(mDrawToMap_Section, StopSectionRendering);
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
        ILLabel skipClearLabel = c.DefineLabel();
        c.EmitCall(configGetInstance);
        c.EmitLdfld(dontRenderTerrain);
        c.EmitBrfalse(skipClearLabel);
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
        c.MarkLabel(skipClearLabel);
        // match to the last instance of Main.clearMap being referenced
        // conveniently it's right after the map has been cleared!
        while (c.TryGotoNext(MoveType.After, i => i.MatchStsfld<Main>("clearMap")))
        {

        }
        c.EmitCall(configGetInstance);
        c.EmitLdfld(dontRenderTerrain);
        ILLabel skipRenderLabel = c.DefineLabel();
        c.EmitBrfalse(skipRenderLabel);
        c.EmitRet();
        c.MarkLabel(skipRenderLabel);
    }
    private static void StopSectionRendering(ILContext il)
    {
        Type configType = typeof(MapConfig);
        MethodInfo configGetInstance = configType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
        FieldInfo dontRenderTerrain = configType.GetField("DontRenderTerrain", BindingFlags.Public | BindingFlags.Instance);
        ILCursor c = new ILCursor(il);
        c.EmitCall(configGetInstance);
        c.EmitLdfld(dontRenderTerrain);
        ILLabel skipRenderLabel = c.DefineLabel();
        c.EmitBrfalse(skipRenderLabel);
        c.EmitRet();
        c.MarkLabel(skipRenderLabel);
    }
}