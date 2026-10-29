using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace MapRemover.Common;

public class MapConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;
    public bool DontRenderTerrain;
}