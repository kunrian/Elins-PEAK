using UnityEngine;

namespace PEAKUsageSkills.UI
{
    internal sealed class DebugOverlay : MonoBehaviour
    {
        private GUIStyle? labelStyle;

        private void OnGUI()
        {
            if (!Plugin.Settings.EnableMod.Value || !Plugin.Settings.ShowDebugOverlay.Value || Plugin.Diagnostics == null)
            {
                return;
            }

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    wordWrap = false,
                    richText = false,
                    alignment = TextAnchor.UpperLeft
                };
                labelStyle.normal.textColor = Color.white;
            }

            Rect area = new Rect(18f, 18f, 820f, 420f);
            GUI.Box(area, GUIContent.none);
            GUILayout.BeginArea(new Rect(area.x + 12f, area.y + 10f, area.width - 24f, area.height - 20f));
            GUILayout.Label(Plugin.Diagnostics.BuildOverlayText(), labelStyle);
            GUILayout.EndArea();
        }
    }
}
