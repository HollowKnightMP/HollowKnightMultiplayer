using System.Collections.Generic;
using UnityEngine;

namespace HollowKnightMP.Debugging
{
    public class HKMPDebugManager : MonoBehaviour
    {
        public readonly List<BaseDebugger> Debuggers;
        public readonly KeyCode EnableDebuggerHotkey = KeyCode.F7;
        private readonly HashSet<BaseDebugger> prevActiveDebuggers = new HashSet<BaseDebugger>();
        private bool isDebugging;
        private Rect windowRect;

        private HKMPDebugManager()
        {
            Debuggers = new List<BaseDebugger>(new BaseDebugger[] { new SceneDebugger() });
        }

        public void OnGUI()
        {
            if (!isDebugging)
            {
                return;
            }

            // Main window to display all available debuggers.
            windowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), windowRect, DoWindow, "debugging");

            // Render debugger windows if they are enabled.
            foreach (BaseDebugger debugger in Debuggers)
            {
                debugger.OnGUI();
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(EnableDebuggerHotkey))
            {
                ToggleDebugging();
            }

            if (isDebugging)
            {
                CheckDebuggerHotkeys();

                foreach (BaseDebugger debugger in Debuggers)
                {
                    if (debugger.Enabled)
                    {
                        debugger.Update();
                    }
                }
            }
        }

        public void ToggleDebugging()
        {
            isDebugging = !isDebugging;
            if (isDebugging)
            {
                ShowDebuggers();
            }
            else
            {
                HideDebuggers();
                foreach (BaseDebugger baseDebugger in Debuggers)
                {
                    baseDebugger.ResetWindowPosition();
                }
            }
        }

        private void DoWindow(int windowId)
        {
            using (new GUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
            {
                foreach (BaseDebugger debugger in Debuggers)
                {
                    string hotkeyString = debugger.GetHotkeyString();
                    debugger.Enabled = GUILayout.Toggle(debugger.Enabled, $"{debugger.DebuggerName} debugger{(!string.IsNullOrEmpty(hotkeyString) ? $" ({hotkeyString})" : "")}");
                }
            }
        }

        private void CheckDebuggerHotkeys()
        {
            foreach (BaseDebugger debugger in Debuggers)
            {
                if (Input.GetKeyDown(debugger.Hotkey) && Input.GetKey(KeyCode.LeftControl) == debugger.HotkeyControlRequired && Input.GetKey(KeyCode.LeftShift) == debugger.HotkeyShiftRequired && Input.GetKey(KeyCode.LeftAlt) == debugger.HotkeyAltRequired)
                {
                    debugger.Enabled = !debugger.Enabled;
                }
            }
        }

        private void HideDebuggers()
        {
            foreach (BaseDebugger debugger in GetComponents<BaseDebugger>())
            {
                if (debugger.Enabled)
                {
                    prevActiveDebuggers.Add(debugger);
                }

                debugger.Enabled = false;
            }
        }

        private void ShowDebuggers()
        {
            foreach (BaseDebugger debugger in prevActiveDebuggers)
            {
                debugger.Enabled = true;
            }

            prevActiveDebuggers.Clear();
        }
    }
}
