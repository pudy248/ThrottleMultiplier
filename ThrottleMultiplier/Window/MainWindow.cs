using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace ThrottleMultiplier.Window
{
    public class MainWindow :  BaseWindow
    {

        bool initted = false;

        // To calculate space needed by toggle text
        public float mainWindowBiggerLineWidth;

        public MainWindow(ThrottleMultiplierer inTurner, int inWindowID)
            : base(inTurner,inWindowID)
        {
            turner = inTurner;

            windowPos.width = 250;
            windowPos.height = 100;
            windowPos.left = 63;
            windowPos.top = 65;
            Version v = typeof(ThrottleMultiplierer).Assembly.GetName().Version;
            WindowTitle = String.Format("ThrottleMultiplier");
        }

        private void UiMultiplier()
        {
            GUILayout.BeginHorizontal();
            ItemLabel(Localizer.Format("#autoLOC_TM_TheNameOfTheMod"));
            ThrottleMultiplierer._mult = Mathf.Exp(GUILayout.HorizontalSlider(Mathf.Log(ThrottleMultiplierer._mult), -10, 0, new[] {GUILayout.Width(200)}));
			ThrottleMultiplierer._mult = float.Parse(GUILayout.TextField(string.Format("{0:0.0#######}", ThrottleMultiplierer._mult), GUILayout.Width(80)));
            GUILayout.EndHorizontal();
        }

        public override void WindowGUI(int windowID)
        {
            base.WindowGUI(windowID);
            if (!WindowVisible && turner.toolbarControl.enabled)
            {
                turner.toolbarControl.SetFalse(false);
            }
            GUILayout.BeginVertical();
            UiMultiplier();
            GUILayout.EndVertical();
   
            Rect r = GUILayoutUtility.GetLastRect();
            float minHeight = r.height + r.yMin + 10;
            if (windowPos.height != minHeight && minHeight>20)
            {
                windowPos.height = minHeight;
            }
            GUI.DragWindow();
            initted = true;
        }
    }
}
