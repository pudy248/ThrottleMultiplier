using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;
using KramaxReloadExtensions;
using ToolbarControl_NS;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace ThrottleMultiplier
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    internal class Patcher : MonoBehaviour {
        void Awake() {
			var harmony = new Harmony("ThrottleMultiplier");
			harmony.PatchAll(typeof(Patcher).Assembly);
		}
    }

    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(ThrottleMultiplierer.MODID, ThrottleMultiplierer.MODNAME);
        }
    }


    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ThrottleMultiplierer : ReloadableMonoBehaviour
    {
        public static Vessel getVessel { get { return FlightGlobals.ActiveVessel; } }

        #region GUI Variables

        public static float _mult = 1f;

        #endregion

        #region Window Stuff

        //public ApplicationLauncherButton button;
        internal ToolbarControl toolbarControl;

        internal static Window.MainWindow mainWindow = null;
        public Window.WindowManager windowManager = new Window.WindowManager();
        public string Message = "";

        static public string DebugMessage = "";
        static public bool DebugShow = false;

        #endregion

        public static void Log(
            string format,
            params object[] args
            )
        {

            //string method = "";
#if DEBUGfalse
            StackFrame stackFrame = new StackFrame(1, true);
            method = string.Format(" [{0}]|{1}", stackFrame.GetMethod().ToString(), stackFrame.GetFileLineNumber());
#endif
            string incomingMessage;
            if (args == null)
                incomingMessage = format;
            else
                incomingMessage = string.Format(format, args);
#if   false
            UnityEngine.Debug.Log("GravityTurn: " + incomingMessage);
#endif
        }

        private void OnGUI()
        {
            // hide UI if F2 was pressed
            if (!Window.BaseWindow.ShowGUI)
                return;
            if (Event.current.type == EventType.Repaint || Event.current.isMouse)
            {
                //myPreDrawQueue(); // Your current on preDrawQueue code
            }
            windowManager.DrawGuis(); // Your current on postDrawQueue code
        }

        private void ShowGUI()
        {
            Window.BaseWindow.ShowGUI = true;
        }
        private void HideGUI()
        {
            Window.BaseWindow.ShowGUI = false;
        }

        /*
         * Called after the scene is loaded.
         */
        public void Awake()
        {
            Log("ThrottleMultiplier: Awake {0}", this.GetInstanceID());
        }

        void Start()
        {
            Log("Starting");
            try
            {
                CreateButtonIcon();
                mainWindow = new Window.MainWindow(this, 6378070);
                GameEvents.onShowUI.Add(ShowGUI);
                GameEvents.onHideUI.Add(HideGUI);
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        private void SetWindowOpen()
        {
            mainWindow.WindowVisible = true;
        }

        internal const string MODID = "ThrottleMultiplier";
        internal const string MODNAME = "ThrottleMultiplier";
        private void CreateButtonIcon()
        {
			Log("CreateButtonIcon");
            toolbarControl = gameObject.AddComponent<ToolbarControl>();
            toolbarControl.AddToAllToolbars(SetWindowOpen,
                () => mainWindow.WindowVisible = false,
                ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.TRACKSTATION,
                MODID,
                "throttleMultiplierButton",
				"ThrottleMultiplier/PluginData/Textures/icon_38",
				"ThrottleMultiplier/PluginData/Textures/icon_24",
                MODNAME
            );
        }
        void OnDestroy()
        {
            //windowManager.OnDestroy();
            //ApplicationLauncher.Instance.RemoveModApplication(button);

            GameEvents.onShowUI.Remove(ShowGUI);
            GameEvents.onHideUI.Remove(HideGUI);

            if (toolbarControl != null)
            {
                toolbarControl.OnDestroy();
                Destroy(toolbarControl);
            }
        }

		[HarmonyPatch(typeof(FlightInputHandler), "FixedUpdate")]
		public class FlightInputHandler_ThrottlePatch {
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				var il = new List<CodeInstruction>(instructions);
				var throttle = AccessTools.Field(typeof(FlightInputHandler), "throttle");
				var deltaTime = AccessTools.PropertyGetter(typeof(Time), nameof(Time.deltaTime));
				var mult = AccessTools.Field(typeof(ThrottleMultiplierer), nameof(ThrottleMultiplierer._mult));

				for (int i = 0; i < il.Count - 2; i++) {
					// ldfld float32 FlightInputHandler::throttle
					// ldc.r4 1f
					// call float32 UnityEngine.Time::get_deltaTime()
					if (il[i].LoadsField(throttle) &&
						il[i + 1].Is(OpCodes.Ldc_R4, 1f) &&
						il[i + 2].Calls(deltaTime)) {
                        // ldc.r4 -> ldsfld
						il[i + 1].opcode = OpCodes.Ldsfld;
						il[i + 1].operand = mult;
					}
				}

				return il.AsEnumerable();
			}
		}
	}
}
