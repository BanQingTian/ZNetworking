using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NRKernal
{
    [HelpURL("https://developer.nreal.ai/develop/unity/customize-phone-controller")]
    public class NRVirtualDisplayer : SingletonBehaviour<NRVirtualDisplayer>
    {
        [SerializeField]
        private Camera m_UICamera;
        [SerializeField]
        private MultiScreenController m_VirtualController;

        private Vector3 m_StartPos = Vector3.one * 1000f;
        private Vector2 m_ScreenResolution;

        // Not supported on runtime
        public const float ScaleFactor = 1f;
        public static int VirtualDisplayFPS = 30;
        private float m_CurrentTime;

#if !UNITY_EDITOR
        private static IntPtr m_RenderTexturePtr;
        private delegate void RenderEventDelegate(int eventID);
        private static RenderEventDelegate RenderThreadHandle = new RenderEventDelegate(RunOnRenderThread);
        private static IntPtr RenderThreadHandlePtr = Marshal.GetFunctionPointerForDelegate(RenderThreadHandle);
#else
        private static RenderTexture m_ControllerScreen;
#endif
        internal static NativeMultiDisplay NativeMultiDisplay { get; private set; }
        public event Action OnMultiDisplayInited;

        public static bool RunInBackground;
        private bool m_IsInit = false;
        public bool IsPlaying { get; private set; }

        private void OnApplicationPause(bool pause)
        {
            // If NRSessionBehaviour is exist, do not oprate.
            if (NRSessionManager.Instance.NRSessionBehaviour != null)
            {
                return;
            }

            if (pause)
            {
                if (RunInBackground)
                {
                    this.Pause();
                }
                else
                {
                    NRDevice.ForceKill();
                }
            }
            else
            {
                this.Resume();
            }
        }

        public void Pause()
        {
            if (!m_IsInit || !IsPlaying)
            {
                return;
            }
#if !UNITY_EDITOR
            NativeMultiDisplay.Pause();
#endif
            IsPlaying = false;
        }

        public void Resume()
        {
            if (!m_IsInit || IsPlaying)
            {
                return;
            }
#if !UNITY_EDITOR
            NativeMultiDisplay.Resume();
#endif
            IsPlaying = true;
        }

        new void Awake()
        {
            base.Awake();
            if (isDirty) return;

            NRDebugger.Log("[NRVirtualDisplayer] Awake");
            this.IsPlaying = false;
#if UNITY_EDITOR
            this.m_UICamera.enabled = false;
#endif
            this.Init();
        }

        private void Update()
        {
            if (!m_IsInit) return;

#if UNITY_EDITOR
            UpdateEmulator();
            if (m_VirtualController)
            {
                m_VirtualController.gameObject.SetActive(NRInput.EmulateVirtualDisplayInEditor);
            }

            if (IsPlaying)
            {
                m_CurrentTime += Time.deltaTime;
            }

            if (IsPlaying && m_CurrentTime > (1f / VirtualDisplayFPS))
            {
                m_CurrentTime = 0;
                m_UICamera.Render();
            }
#endif
        }

        public void Destory()
        {
#if !UNITY_EDITOR
            if (NativeMultiDisplay != null)
            {
                NativeMultiDisplay.Stop();
                NativeMultiDisplay.Destroy();
                NativeMultiDisplay = null;
            }
#else
            m_ControllerScreen?.Release();
            m_ControllerScreen = null;
#endif

        }

        new void OnDestroy()
        {
            if (isDirty) return;
            base.OnDestroy();
            this.Destory();
        }

        private void Init()
        {
            if (m_IsInit) return;

            try
            {
                NRDevice.Instance.Init();
            }
            catch (Exception e)
            {
                NRDebugger.LogError("[NRVirtualDisplayer] NRDevice init error:" + e.ToString());
                throw;
            }

            transform.position = m_StartPos;
            this.SetVirtualDisplayResolution();
            NRSessionManager.Instance.VirtualDisplayer = this;
            NRDebugger.Log("[NRVirtualDisplayer] Init");

#if !UNITY_EDITOR
            //m_RenderTexturePtr = m_ControllerScreen.GetNativeTexturePtr();
            NativeMultiDisplay = new NativeMultiDisplay();
            NativeMultiDisplay.Create();
            //NativeMultiDisplay.InitColorSpace();
            NativeMultiDisplay.ListenMainScrResolutionChanged(OnDisplayResolutionChanged);
            NativeMultiDisplay.Start();
            // Creat multiview controller..
            //GL.IssuePluginEvent(RenderThreadHandlePtr, 0);
            //LoadPhoneScreen();
#elif UNITY_EDITOR
            InitEmulator();
#endif
            m_VirtualController?.Init();
            m_IsInit = true;
            OnMultiDisplayInited?.Invoke();
            IsPlaying = true;
        }

        public void UpdateResolution(Vector2 size)
        {
            NRPhoneScreen.Resolution = size;
            this.SetVirtualDisplayResolution();

#if !UNITY_EDITOR
            //m_RenderTexturePtr = m_ControllerScreen.GetNativeTexturePtr();
            //GL.IssuePluginEvent(RenderThreadHandlePtr, 0);
#else
            this.UpdateEmulatorScreen(size * ScaleFactor);
#endif

            var m_PointRaycaster = gameObject.GetComponentInChildren<NRMultScrPointerRaycaster>();
            m_PointRaycaster.UpdateScreenSize(size * ScaleFactor);
        }

        private void SetVirtualDisplayResolution()
        {
            m_ScreenResolution = NRPhoneScreen.Resolution;

#if UNITY_EDITOR
            if (m_ControllerScreen != null)
            {
                m_ControllerScreen.Release();
            }

            m_ControllerScreen = new RenderTexture(
                (int)(m_ScreenResolution.x * ScaleFactor),
                (int)(m_ScreenResolution.y * ScaleFactor),
                24
            );
            m_UICamera.targetTexture = m_ControllerScreen;
#endif

            m_UICamera.aspect = m_ScreenResolution.x / m_ScreenResolution.y;
            m_UICamera.orthographicSize = 6;

            var canvas = transform.GetComponentInChildren<Canvas>(true);
            float realScale = ScaleFactor;
            var scaler = canvas.gameObject.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            }
            else if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                var referenceResolution = scaler.referenceResolution;
                var match = scaler.matchWidthOrHeight;
                float scale_width = (m_ScreenResolution.x * ScaleFactor) / referenceResolution.x;
                float scale_height = (m_ScreenResolution.y * ScaleFactor) / referenceResolution.y;

                realScale = Mathf.Lerp(scale_width, scale_height, match);
                cacheCanvasScalerInfo = new CanvasScalerInfo()
                {
                    referenceResolution = scaler.referenceResolution,
                    matchWidthOrHeight = scaler.matchWidthOrHeight
                };
            }
            // Here is for resolution changed in runtime.
            else if (cacheCanvasScalerInfo != null)
            {
                var referenceResolution = cacheCanvasScalerInfo.referenceResolution;
                var match = cacheCanvasScalerInfo.matchWidthOrHeight;
                float scale_width = (m_ScreenResolution.x * ScaleFactor) / referenceResolution.x;
                float scale_height = (m_ScreenResolution.y * ScaleFactor) / referenceResolution.y;

                realScale = Mathf.Lerp(scale_width, scale_height, match);
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = realScale;
        }

        // Only works for ScaleWithScreenSize mode.
        private CanvasScalerInfo cacheCanvasScalerInfo = null;
        private class CanvasScalerInfo
        {
            public Vector2 referenceResolution;
            public float matchWidthOrHeight;
        }

        [MonoPInvokeCallback(typeof(NativeMultiDisplay.NRDisplayResolutionCallback))]
        public static void OnDisplayResolutionChanged(int width, int height)
        {
            Debug.LogFormat("[NRVirtualDisplayer] Display resolution changed width:{0} height:{1}", width, height);
            MainThreadDispather.QueueOnMainThread(delegate ()
            {
                NRVirtualDisplayer.Instance.UpdateResolution(new Vector2(width, height));
            });
        }

#if !UNITY_EDITOR
        [MonoPInvokeCallback(typeof(RenderEventDelegate))]
        private static void RunOnRenderThread(int eventID)
        {
            //NativeMultiDisplay.UpdateHomeScreenTexture(m_RenderTexturePtr);
        }
#endif

#if UNITY_EDITOR
        private static Vector2 m_EmulatorTouch = Vector2.zero;
        private Vector2 m_EmulatorPhoneScreenAnchor;
        private float m_EmulatorRawImageWidth;
        private float m_EmulatorRawImageHeight;
        private RawImage emulatorPhoneRawImage;

        public static Vector2 GetEmulatorScreenTouch()
        {
            return m_EmulatorTouch;
        }

        private void InitEmulator()
        {
            GameObject emulatorVirtualController = new GameObject("NREmulatorVirtualController");
            DontDestroyOnLoad(emulatorVirtualController);
            Canvas controllerCanvas = emulatorVirtualController.AddComponent<Canvas>();
            controllerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            GameObject rawImageObj = new GameObject("RamImage");
            rawImageObj.transform.parent = controllerCanvas.transform;
            emulatorPhoneRawImage = rawImageObj.AddComponent<RawImage>();
            emulatorPhoneRawImage.raycastTarget = false;
            UpdateEmulatorScreen(m_ScreenResolution * ScaleFactor);
        }

        private void UpdateEmulatorScreen(Vector2 size)
        {
            float scaleRate = 0.18f;
            m_EmulatorRawImageWidth = size.x * scaleRate;
            m_EmulatorRawImageHeight = size.y * scaleRate;
            emulatorPhoneRawImage.rectTransform.pivot = Vector2.right;
            emulatorPhoneRawImage.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0f, 0f);
            emulatorPhoneRawImage.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0f, 0f);
            emulatorPhoneRawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_EmulatorRawImageWidth);
            emulatorPhoneRawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_EmulatorRawImageHeight);
            emulatorPhoneRawImage.texture = m_ControllerScreen;

            Vector2 gameViewSize = Handles.GetMainGameViewSize();
            m_EmulatorPhoneScreenAnchor = new Vector2(gameViewSize.x - m_EmulatorRawImageWidth, 0f);
        }

        private void UpdateEmulator()
        {
            if (NRInput.EmulateVirtualDisplayInEditor
                && Input.GetMouseButton(0)
                && Input.mousePosition.x > m_EmulatorPhoneScreenAnchor.x
                && Input.mousePosition.y < (m_EmulatorPhoneScreenAnchor.y + m_EmulatorRawImageHeight))
            {
                m_EmulatorTouch.x = (Input.mousePosition.x - m_EmulatorPhoneScreenAnchor.x) / m_EmulatorRawImageWidth * 2f - 1f;
                m_EmulatorTouch.y = (Input.mousePosition.y - m_EmulatorPhoneScreenAnchor.y) / m_EmulatorRawImageHeight * 2f - 1f;
            }
            else
            {
                m_EmulatorTouch = Vector2.zero;
            }
        }
#endif
    }
}
