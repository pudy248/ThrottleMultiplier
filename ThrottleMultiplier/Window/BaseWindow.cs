using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using KSP.IO;
using System.IO;
using ClickThroughFix;

namespace ThrottleMultiplier.Window
{

    public class PersistentWindow
    {
        [Persistent]
        public float left;
        [Persistent]
        public float top;
        [Persistent]
        public float width;
        [Persistent]
        public float height;

        public PersistentWindow(float left,float top,float width,float height)
        {
            this.left=left;
            this.top=top;
            this.width=width;
            this.height=height;
        }
        public PersistentWindow()
        {
            this.left = 0;
            this.top = 0;
            this.width = 0;
            this.height = 0;
        }
        public static implicit operator Rect(PersistentWindow rect)
        {
            return new Rect(rect.left, rect.top, rect.width, rect.height);
        }
        public static implicit operator PersistentWindow(Rect rect)
        {
            return new PersistentWindow(rect.xMin, rect.yMin, rect.width, rect.height);
        }
    }

    public class BaseWindow
    {
        int WindowID;
        protected ThrottleMultiplierer turner;
        public bool WindowVisible = false; 
        public string WindowTitle = "GravityTurn";
        public static bool ShowGUI = true;

        [Persistent]
        public PersistentWindow windowPos = new PersistentWindow();

        // Get width of a displayed string
        public float TxtWidth(string txt)
        {
            float txtWidth;
            GUIContent content = new GUIContent(txt);
            txtWidth = GUI.skin.textField.CalcSize(content).x;

            return txtWidth;
        }

        protected void ItemLabel(string labelText)
        {
            //GUILayout.Label(labelText, GUILayout.ExpandWidth(false), GUILayout.Width(windowPos.width / 2));
            float itemLabelWidth = 150;

            GUILayout.Label(labelText, GUILayout.ExpandWidth(false), GUILayout.Width(itemLabelWidth));
        }

        public BaseWindow(ThrottleMultiplierer turner, int inWindowID)
        {
            this.turner = turner;
            turner.windowManager.Register(this);
            WindowID = inWindowID;
            if (windowPos.left + windowPos.width > Screen.width)
            {
                windowPos.left = Screen.width - windowPos.width;
            }
            if (windowPos.top + windowPos.height > Screen.height )
            {
                windowPos.top = Screen.height - windowPos.height;
            }
            if (windowPos.top < 0)
                windowPos.top = 0;
        }

        public virtual void WindowGUI(int windowID)
        {
            if (!ShowGUI)
                return;
            if (GUI.Button(new Rect(windowPos.width - 18, 2, 16, 16), "X"))
            {
                WindowVisible = false;
            }
            //GUI.DragWindow();
        }
        public void drawGUI()
        {
            if (WindowVisible && ShowGUI)
            {
                GuiUtils.LoadSkin(true);
                GUI.skin = GuiUtils.skin;
                windowPos = ClickThruBlocker.GUILayoutWindow(WindowID, windowPos, WindowGUI, WindowTitle, GUILayout.MinWidth(275));
                //windowPos = GUILayout.Window(WindowID, windowPos, WindowGUI, WindowTitle, GUILayout.MinWidth(300));
            }
        }
    }
}
