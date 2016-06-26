using ICities;

namespace MapEditorTunnels
{
    public class Mod : IUserMod
    {
        public static float UNDERGROUND_OFFSET = 2.0f;

        public string Name => "Map Editor Tunnels Enabler";

        public string Description => "Allows to place tunnels in Map Editor";
    }
}
