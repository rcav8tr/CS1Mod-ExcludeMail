using Harmony;
using ICities;

namespace ExcludeMail
{
    public class ExcludeMail : IUserMod
    {
        // required name and description of this mod
        public string Name => "Exclude Mail";
        public string Description => "Optionally exclude mail from Outside Connections info view";

        // Harmony instance
        public static HarmonyInstance harmony;
    }
}
