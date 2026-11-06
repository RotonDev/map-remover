using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace MapRemover.Common;

public class MapConfig : ModConfig
{
    public static MapConfig Instance => ModContent.GetInstance<MapConfig>();
    public override ConfigScope Mode => ConfigScope.ServerSide;
    [Header("MapRenderingOptions")]
    [DefaultValue(false)]
    public bool DontRenderTerrain;
    [DefaultValue(true)]
    public bool RenderPotionOfReturnHome;
    [DefaultValue(true)]
    public bool RenderPotionOfReturnLastPos;
    [DefaultValue(true)]
    public bool RenderLastGolfBallHit;
    [DefaultValue(true)]
    public bool RenderNpcHeads;
    [DefaultValue(true)]
    public bool RenderBossHeads;
    [DefaultValue(true)]
    public bool RenderPlayerHeads;
    [DefaultValue(true)]
    public bool RenderDeathMarkers;
    [DefaultValue(true)]
    public bool RenderDroppedCoins;
    [DefaultValue(true)]
    public bool RenderPings;
    [DefaultValue(true)]
    public bool RenderWorldSpawnIcon;
    [DefaultValue(true)]
    public bool RenderSpawnpointIcon;
    [DefaultValue(true)]
    public bool RenderPylons;
}