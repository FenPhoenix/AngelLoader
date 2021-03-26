using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    [PublicAPI]
    public interface IDarkable
    {
        bool DarkModeEnabled { get; set; }
    }
}
