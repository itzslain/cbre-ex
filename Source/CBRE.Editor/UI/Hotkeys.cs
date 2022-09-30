using CBRE.Common.Mediator;
using CBRE.UI;
using System.Windows.Forms;

namespace CBRE.Editor.UI
{
    public static class Hotkeys
    {
        public static bool HotkeyDown(Keys keyData)
        {
            string keyCombination = KeyboardState.KeysToString(keyData);
            CBRE.Settings.HotkeyImplementation hotkeyImplementation = CBRE.Settings.Hotkeys.GetHotkeyFor(keyCombination);
            if (hotkeyImplementation != null)
            {
                CBRE.Settings.HotkeyDefinition def = hotkeyImplementation.Definition;
                Mediator.Publish(def.Action, def.Parameter);
                return true;
            }
            return false;
        }

        public static readonly object SuppressHotkeysTag = new object();
    }
}
