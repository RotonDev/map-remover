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
        MonoModHooks.Modify(mDrawToMap, StopMapRendering);
    }
    private static void StopMapRendering(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        // match to the last instance of Main.clearMap being referenced
        // conveniently it's right after the map has been cleared!
        while (c.TryGotoNext(MoveType.After, i => i.MatchStsfld<Main>("clearMap")))
        {

        }
        Type configType = typeof(MapConfig);
        MethodInfo configGetInstance = configType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
        FieldInfo dontRenderTerrain = configType.GetField("DontRenderTerrain", BindingFlags.Public | BindingFlags.Instance);
        c.EmitCallvirt(configGetInstance);
        c.EmitLdfld(dontRenderTerrain);
        ILLabel skipRenderLabel = c.DefineLabel();
        c.EmitBrfalse(skipRenderLabel);
        c.EmitRet();
        c.MarkLabel(skipRenderLabel);
    }
}