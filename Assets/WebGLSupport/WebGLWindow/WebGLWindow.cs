﻿using System;
using AOT;
using UnityEngine;
// for DllImport

namespace WebGLSupport.WebGLWindow
{
    static class WebGLWindowPlugin
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern void WebGLWindowInit();
        [DllImport("__Internal")]
        public static extern void WebGLWindowOnFocus(Action cb);

        [DllImport("__Internal")]
        public static extern void WebGLWindowOnBlur(Action cb);

        [DllImport("__Internal")]
        public static extern void WebGLWindowOnResize(Action cb);

        [DllImport("__Internal")]
        public static extern void WebGLWindowInjectFullscreen();
#else
        public static void WebGLWindowInit() { }
        public static void WebGLWindowOnFocus(Action cb) { }
        public static void WebGLWindowOnBlur(Action cb) { }
        public static void WebGLWindowOnResize(Action cb) { }
        public static void WebGLWindowInjectFullscreen() { }
#endif

    }

    public static class WebGLWindow
    {
        static WebGLWindow()
        {
            WebGLWindowPlugin.WebGLWindowInit();
        }
        public static bool Focus { get; private set; }
        public static event Action OnFocusEvent = () => { };
        public static event Action OnBlurEvent = () => { };
        public static event Action OnResizeEvent = () => { };

        static string ViewportContent;
        static void Init()
        {
            Focus = true;
            WebGLWindowPlugin.WebGLWindowOnFocus(OnWindowFocus);
            WebGLWindowPlugin.WebGLWindowOnBlur(OnWindowBlur);
            WebGLWindowPlugin.WebGLWindowOnResize(OnWindowResize);
            WebGLWindowPlugin.WebGLWindowInjectFullscreen();
        }

        [MonoPInvokeCallback(typeof(Action))]
        static void OnWindowFocus()
        {
            Focus = true;
            OnFocusEvent();
        }

        [MonoPInvokeCallback(typeof(Action))]
        static void OnWindowBlur()
        {
            Focus = false;
            OnBlurEvent();
        }

        [MonoPInvokeCallback(typeof(Action))]
        static void OnWindowResize()
        {
            OnResizeEvent();
        }

        [RuntimeInitializeOnLoadMethod]
        static void RuntimeInitializeOnLoadMethod()
        {
            Init();
        }
    }
}
