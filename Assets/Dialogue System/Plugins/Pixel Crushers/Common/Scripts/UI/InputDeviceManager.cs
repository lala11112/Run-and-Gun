﻿// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

#if USE_NEW_INPUT
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
#endif

namespace PixelCrushers
{

    public enum InputDevice { Joystick, Keyboard, Mouse, Touch }

    /// <summary>
    /// This script checks for joystick and keyboard input. If the player uses a joystick,
    /// it enables autofocus. If the player uses the mouse or keyboard, it disables autofocus.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class InputDeviceManager : MonoBehaviour
    {

        [Tooltip("Current input mode.")]
        public InputDevice inputDevice = InputDevice.Joystick;

        [Tooltip("If any of these keycodes are pressed, current device is joystick.")]
        public KeyCode[] joystickKeyCodesToCheck = new KeyCode[] { KeyCode.JoystickButton0, KeyCode.JoystickButton1, KeyCode.JoystickButton2, KeyCode.JoystickButton7 };

        [Tooltip("If any of these buttons are pressed, current device is joystick. Must be defined in Input Manager.")]
        public string[] joystickButtonsToCheck = new string[0];

        [Tooltip("If any of these axes are greater than Joystick Axis Threshold, current device is joystick. Must be defined in Input Manager.")]
        public string[] joystickAxesToCheck = new string[0];

        [Tooltip("Joystick axis values must be above this threshold to switch to joystick mode.")]
        public float joystickAxisThreshold = 0.5f;

        [Tooltip("If any of these buttons are pressed, current device is keyboard (unless device is currently mouse).")]
        public string[] keyButtonsToCheck = new string[0];

        [Tooltip("If any of these keys are pressed, current device is keyboard (unless device is currently mouse).")]
        public KeyCode[] keyCodesToCheck = new KeyCode[] { KeyCode.Escape };

        public enum KeyInputSwitchesModeTo { Keyboard, Mouse }

        [Tooltip("Which mode to switch to if user presses Key Buttons/Codes To Check.")]
        public KeyInputSwitchesModeTo keyInputSwitchesModeTo = KeyInputSwitchesModeTo.Mouse;

        [Tooltip("Always enable joystick/keyboard navigation even in Mouse mode.")]
        public bool alwaysAutoFocus = false;

        [Tooltip("Switch to mouse control if player clicks mouse buttons or moves mouse.")]
        public bool detectMouseControl = true;

        [Tooltip("If mouse moves more than this, current device is mouse.")]
        public float mouseMoveThreshold = 0.1f;

        [Tooltip("Hide cursor in joystick/key mode, show in mouse mode.")]
        public bool controlCursorState = true;

        [Tooltip("When paused and device is mouse, make sure cursor is visible.")]
        public bool enforceCursorOnPause = false;

        [Tooltip("Enable GraphicRaycasters (which detect cursor clicks on UI elements) only when device is mouse.")]
        public bool controlGraphicRaycasters = false;

        [Tooltip("If any of these keycodes are pressed, go back to the previous menu.")]
        public KeyCode[] backKeyCodes = new KeyCode[] { KeyCode.JoystickButton1 };

        [Tooltip("If any of these buttons are pressed, go back to the previous menu.")]
        public string[] backButtons = new string[] { "Cancel" };

        [Tooltip("'Submit' input button defined on Event System.")]
        public string submitButton = "Submit";

        [Tooltip("Survive scene changes and only allow one instance.")]
        public bool singleton = true;

        public UnityEvent onUseKeyboard = new UnityEvent();
        public UnityEvent onUseJoystick = new UnityEvent();
        public UnityEvent onUseMouse = new UnityEvent();
        public UnityEvent onUseTouch = new UnityEvent();

        public delegate bool GetButtonDownDelegate(string buttonName);
        public delegate float GetAxisDelegate(string axisName);

        public GetButtonDownDelegate GetButtonDown = null;
        public GetButtonDownDelegate GetButtonUp = null;
        public GetAxisDelegate GetInputAxis = null;

        private Vector3 m_lastMousePosition;
        private bool m_ignoreMouse = false;
        private CursorLockMode m_cursorLockMode = CursorLockMode.Locked;
        private bool m_inputAllowed = true;

        private static InputDeviceManager m_instance = null;
        public static InputDeviceManager instance
        {
            get { return m_instance; }
            set { m_instance = value; }
        }

        /// <summary>
        /// Current input device detected by InputDeviceManager. May changed based on
        /// input from other devices.
        /// </summary>
        public static InputDevice currentInputDevice
        {
            get
            {
                return (m_instance != null) ? m_instance.inputDevice : InputDevice.Joystick;
            }
        }

        /// <summary>
        /// Returns true if current input device uses mouse cursor.
        /// </summary>
        public static bool deviceUsesCursor
        {
            get { return currentInputDevice == InputDevice.Mouse; }
        }

        /// <summary>
        /// Lock mode to use when locking cursor.
        /// </summary>
        public static CursorLockMode cursorLockMode
        {
            get { return (m_instance != null) ? m_instance.m_cursorLockMode : CursorLockMode.Locked; }
            set { if (m_instance != null) m_instance.m_cursorLockMode = value; }
        }

        /// <summary>
        /// Automatically select (and keep selected) a selectable on the current UIPanel.
        /// </summary>
        public static bool autoFocus
        {
            get { return (m_instance != null && instance.alwaysAutoFocus) || currentInputDevice == InputDevice.Joystick || currentInputDevice == InputDevice.Keyboard; }
        }

        public static bool isBackButtonDown
        {
            get { return (m_instance != null) ? m_instance.IsBackButtonDown() : false; }
        }

        /// <summary>
        /// Allow user input?
        /// </summary>
        public static bool isInputAllowed
        {
            get { return (m_instance != null) ? m_instance.m_inputAllowed : true; }
            set { if (m_instance != null) m_instance.m_inputAllowed = value; }
        }

        public static bool IsButtonDown(string buttonName)
        {
            if (!isInputAllowed) return false;
            return (m_instance != null && m_instance.GetButtonDown != null) ? m_instance.GetButtonDown(buttonName) : DefaultGetButtonDown(buttonName);
        }

        public static bool IsButtonUp(string buttonName)
        {
            if (!isInputAllowed) return false;
            return (m_instance != null && m_instance.GetButtonUp != null) ? m_instance.GetButtonUp(buttonName) : DefaultGetButtonUp(buttonName);
        }

        public static bool IsKeyDown(KeyCode keyCode)
        {
            if (!isInputAllowed) return false;
            return DefaultGetKeyDown(keyCode);
        }

        public static bool IsAnyKeyDown()
        {
            if (!isInputAllowed) return false;
            return DefaultGetAnyKeyDown();
        }

        public static float GetAxis(string axisName)
        {
            if (!isInputAllowed) return 0;
            return (m_instance != null && m_instance.GetInputAxis != null) ? m_instance.GetInputAxis(axisName) : DefaultGetAxis(axisName);
        }

        public static Vector3 GetMousePosition()
        {
            if (!isInputAllowed) return Vector3.zero;
            return DefaultGetMousePosition();
        }

#if UNITY_2019_3_OR_NEWER && UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            m_instance = null;
#if USE_NEW_INPUT
            inputActionDict = new Dictionary<string, InputAction>();
            m_specialKeyCodeDict = null;
#endif
        }
#endif

        public void Awake()
        {
            if (m_instance != null && singleton)
            {
                Destroy(gameObject);
            }
            else
            {
                m_instance = this;
                GetButtonDown = DefaultGetButtonDown;
                GetButtonUp = DefaultGetButtonUp;
                GetInputAxis = DefaultGetAxis;
                if (singleton)
                {
#if UNITY_EDITOR
                    if (Application.isPlaying)
                    { // If GameObject is hidden in Scene view, DontDestroyOnLoad will report (harmless) error.
                        UnityEditor.SceneVisibilityManager.instance.Show(gameObject, true);
                    }
#endif
                    transform.SetParent(null);
                    DontDestroyOnLoad(gameObject);
                }
            }
        }

        public void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
#if USE_NEW_INPUT
            InputSystem.onDeviceChange -= OnInputSystemDeviceChange;
            InputSystem.onEvent -= OnInputSystemEvent;
#endif
        }

        public void Start()
        {
            m_lastMousePosition = GetMousePosition();
            SetInputDevice(inputDevice);
            BrieflyIgnoreMouseMovement();
            SceneManager.sceneLoaded += OnSceneLoaded;
#if USE_NEW_INPUT
            InputSystem.onDeviceChange += OnInputSystemDeviceChange;
            InputSystem.onEvent += OnInputSystemEvent;
#endif
        }

#if USE_NEW_INPUT
        private void OnInputSystemDeviceChange(UnityEngine.InputSystem.InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added ||
               (change == InputDeviceChange.UsageChanged && device.lastUpdateTime >= Time.time - 1))
            {
                if (device is Joystick || device is Gamepad)
                {
                    SetInputDevice(InputDevice.Joystick);
                }
                else if (device is Keyboard)
                {
                    SetInputDevice((keyInputSwitchesModeTo == KeyInputSwitchesModeTo.Mouse) ? InputDevice.Mouse : InputDevice.Keyboard);
                }
                else
                {
                    SetInputDevice(InputDevice.Mouse);
                }
            }
        }
#endif

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            BrieflyIgnoreMouseMovement();
        }

        public void SetInputDevice(InputDevice newDevice)
        {
            inputDevice = newDevice;
            m_lastMousePosition = GetMousePosition();
            SetCursor(deviceUsesCursor);
            SetGraphicRaycasters(deviceUsesCursor);
            switch (inputDevice)
            {
                case InputDevice.Joystick:
                    onUseJoystick.Invoke();
                    break;
                case InputDevice.Keyboard:
                    onUseKeyboard.Invoke();
                    break;
                case InputDevice.Mouse:
                    var eventSystem = UnityEngine.EventSystems.EventSystem.current;
                    var currentSelectable = (eventSystem != null && eventSystem.currentSelectedGameObject != null) ? eventSystem.currentSelectedGameObject.GetComponent<UnityEngine.UI.Selectable>() : null;
                    if (currentSelectable != null && !autoFocus) currentSelectable.OnDeselect(null);
                    onUseMouse.Invoke();
                    break;
                case InputDevice.Touch:
                    onUseTouch.Invoke();
                    break;
            }
        }

        private void SetGraphicRaycasters(bool deviceUsesCursor)
        {
            if (!controlGraphicRaycasters) return;
            var raycasters = GameObjectUtility.FindObjectsByType<UnityEngine.UI.GraphicRaycaster>();
            for (int i = 0; i < raycasters.Length; i++)
            {
                raycasters[i].enabled = deviceUsesCursor;
            }
        }

#if USE_NEW_INPUT
        private float prevInputSystemCheckTime = 0f;
        private const float InputSystemCheckFrequencyInSeconds = 0.5f;
        private List<UnityEngine.InputSystem.InputDevice> reportedUnknownDevices = new List<UnityEngine.InputSystem.InputDevice>();

        private void OnInputSystemEvent(InputEventPtr eventPtr, UnityEngine.InputSystem.InputDevice device)
        {
            if (Time.time < prevInputSystemCheckTime) return;
            prevInputSystemCheckTime = Time.time + InputSystemCheckFrequencyInSeconds;

            if (!(eventPtr.IsA<StateEvent>() || eventPtr.IsA<DeltaStateEvent>())) return;

            PixelCrushers.InputDevice newDevice = PixelCrushers.InputDevice.Joystick;
            if (device is UnityEngine.InputSystem.Joystick || device is UnityEngine.InputSystem.Gamepad)
                newDevice = PixelCrushers.InputDevice.Joystick;
            else if (device is UnityEngine.InputSystem.Keyboard)
                newDevice = keyInputSwitchesModeTo == KeyInputSwitchesModeTo.Keyboard ? PixelCrushers.InputDevice.Keyboard : PixelCrushers.InputDevice.Mouse;
            else if (device is UnityEngine.InputSystem.Mouse)
                newDevice = PixelCrushers.InputDevice.Mouse;
            else if (device is UnityEngine.InputSystem.Touchscreen)
                newDevice = PixelCrushers.InputDevice.Touch;
            else
            {
                if (!reportedUnknownDevices.Contains(device))
                {
                    reportedUnknownDevices.Add(device);
                    Debug.LogWarning($"Pixel Crushers Input Device Manager: Detected an unknown device type: {device.displayName}.");
                }
                return;
            }

            if (inputDevice == newDevice) return;

            if (newDevice == PixelCrushers.InputDevice.Joystick)
            {
                if (!eventPtr.HasButtonPress())
                    if (device is UnityEngine.InputSystem.Joystick)
                    {
                        var joystick = device as UnityEngine.InputSystem.Joystick;
                        var x = joystick.stick.x.ReadValue();
                        var y = joystick.stick.y.ReadValue();
                        if (!(Mathf.Abs(x) > joystickAxisThreshold || Mathf.Abs(y) > joystickAxisThreshold)) return;
                    }
                    else
                    {
                        var gamePad = device as UnityEngine.InputSystem.Gamepad;
                        var xL = gamePad.leftStick.x.ReadValue();
                        var yL = gamePad.leftStick.y.ReadValue();
                        if (!(Mathf.Abs(xL) > joystickAxisThreshold || Mathf.Abs(yL) > joystickAxisThreshold))
                        {
                            var xR = gamePad.rightStick.x.ReadValue();
                            var yR = gamePad.rightStick.y.ReadValue();
                            if (!(Mathf.Abs(xR) > joystickAxisThreshold || Mathf.Abs(yR) > joystickAxisThreshold)) return;
                        }
                    }
            }
            else if (newDevice == PixelCrushers.InputDevice.Keyboard)
            {
                if (!isInputAllowed) return;
            }
            else if (newDevice == PixelCrushers.InputDevice.Mouse)
            {
                if (!detectMouseControl) return;
                var mousePosition = DefaultGetMousePosition();
                var didMouseMove = !m_ignoreMouse && (Mathf.Abs(mousePosition.x - m_lastMousePosition.x) > mouseMoveThreshold || 
                    Mathf.Abs(mousePosition.y - m_lastMousePosition.y) > mouseMoveThreshold);
                m_lastMousePosition = mousePosition;
            }

            SetInputDevice(newDevice);
        }
#else
        // With input manager/Rewired, poll inputs for device changes:
        public void Update()
        {
            switch (inputDevice)
            {
                case InputDevice.Joystick:
                    if (IsUsingMouse()) SetInputDevice(InputDevice.Mouse);
                    else if (IsUsingKeyboard()) SetInputDevice((keyInputSwitchesModeTo == KeyInputSwitchesModeTo.Keyboard) ? InputDevice.Keyboard : InputDevice.Mouse);
                    break;
                case InputDevice.Keyboard:
                    if (IsUsingMouse()) SetInputDevice(InputDevice.Mouse);
                    else if (IsUsingJoystick()) SetInputDevice(InputDevice.Joystick);
                    break;
                case InputDevice.Mouse:
                    if (IsUsingJoystick()) SetInputDevice(InputDevice.Joystick);
                    else if (keyInputSwitchesModeTo == KeyInputSwitchesModeTo.Keyboard && IsUsingKeyboard()) SetInputDevice(InputDevice.Keyboard);
                    break;
                case InputDevice.Touch:
                    if (IsUsingMouse()) SetInputDevice(InputDevice.Mouse);
                    else if (IsUsingKeyboard()) SetInputDevice(InputDevice.Mouse);
                    break;
            }
        }
#endif

        public bool IsUsingJoystick()
        {
            try
            {
                for (int i = 0; i < joystickKeyCodesToCheck.Length; i++)
                {
                    if (IsKeyDown(joystickKeyCodesToCheck[i]))
                    {
                        return true;
                    }
                }
                for (int i = 0; i < joystickButtonsToCheck.Length; i++)
                {
                    if (GetButtonDown(joystickButtonsToCheck[i]))
                    {
                        return true;
                    }
                }
                for (int i = 0; i < joystickAxesToCheck.Length; i++)
                {
                    if (Mathf.Abs(DefaultGetAxis(joystickAxesToCheck[i])) > joystickAxisThreshold)
                    {
                        return true;
                    }
                }
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError("Some input settings listed on the Input Device Manager component are missing from Unity's Input Manager. To automatically add them, inspect the Input Device Manager component on the GameObject '" + name + "' and click the 'Add Input Definitions' button at the bottom.\n" + e.Message, this);
            }
            return false;
        }

        public bool IsUsingMouse()
        {
            if (!detectMouseControl) return false;
            if (DefaultGetMouseButtonDown(0) || DefaultGetMouseButtonDown(1)) return true;
            var mousePosition = DefaultGetMousePosition();
            var didMouseMove = !m_ignoreMouse && (Mathf.Abs(mousePosition.x - m_lastMousePosition.x) > mouseMoveThreshold || Mathf.Abs(mousePosition.y - m_lastMousePosition.y) > mouseMoveThreshold);
            m_lastMousePosition = mousePosition;
            return didMouseMove;
        }

        public void BrieflyIgnoreMouseMovement()
        {
            StartCoroutine(BrieflyIgnoreMouseMovementCoroutine());
        }

        IEnumerator BrieflyIgnoreMouseMovementCoroutine()
        {
            m_ignoreMouse = true;
            yield return new WaitForSeconds(0.5f);
            m_ignoreMouse = false;
            m_lastMousePosition = DefaultGetMousePosition();
            if (deviceUsesCursor)
            {
                SetCursor(true);
            }
        }

        public bool IsUsingKeyboard()
        {
            try
            {
                for (int i = 0; i < keyCodesToCheck.Length; i++)
                {
                    if (DefaultGetKeyDown(keyCodesToCheck[i]))
                    {
                        return true;
                    }
                }
                for (int i = 0; i < keyButtonsToCheck.Length; i++)
                {
                    if (GetButtonDown(keyButtonsToCheck[i]))
                    {
                        return true;
                    }
                }
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError("Some input settings listed on the Input Device Manager component are missing from Unity's Input Manager. To automatically add them, inspect the Input Device Manager component and click the 'Add Input Definitions' button at the bottom.\n" + e.Message, this);
            }
            return false;
        }

        public bool IsBackButtonDown()
        {
            try
            {
                for (int i = 0; i < backKeyCodes.Length; i++)
                {
                    if (DefaultGetKeyDown(backKeyCodes[i]))
                    {
                        return true;
                    }
                }
                for (int i = 0; i < backButtons.Length; i++)
                {
                    if (GetButtonDown(backButtons[i]))
                    {
                        return true;
                    }
                }
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError("Some input settings listed on the Input Device Manager component are missing from Unity's Input Manager. To automatically add them, inspect the Input Device Manager component and click the 'Add Input Definitions' button at the bottom.\n" + e.Message, this);
            }
            return false;
        }

        public void SetCursor(bool visible)
        {
            if (!controlCursorState) return;
            ForceCursor(visible);
        }

        public void ForceCursor(bool visible)
        {
            Cursor.visible = visible;
            Cursor.lockState = visible ? CursorLockMode.None : cursorLockMode;
            m_lastMousePosition = GetMousePosition();
            StartCoroutine(ForceCursorAfterOneFrameCoroutine(visible));
        }

        private IEnumerator ForceCursorAfterOneFrameCoroutine(bool visible)
        {
            yield return CoroutineUtility.endOfFrame;
            Cursor.visible = visible;
            Cursor.lockState = visible ? CursorLockMode.None : cursorLockMode;
        }

#if USE_NEW_INPUT
        public static Dictionary<string, InputAction> inputActionDict = new Dictionary<string, InputAction>();

        public static void RegisterInputAction(string name, InputAction inputAction)
        {
            inputActionDict[name] = inputAction;
        }

        public static void UnregisterInputAction(string name)
        {
            if (inputActionDict.ContainsKey(name)) inputActionDict.Remove(name);
        }

        // Cache key to translations to avoid GC:
        private static Dictionary<KeyCode, string> m_keyCodeStrings = new Dictionary<KeyCode, string>();

        // Number keys translate differently in Input System, so create a quick lookup dictionary:
        protected static Dictionary<KeyCode, KeyControl> m_specialKeyCodeDict = null;
        protected static Dictionary<KeyCode, KeyControl> specialKeyCodeDict
        {
            get
            {
                if (m_specialKeyCodeDict == null)
                {
                    m_specialKeyCodeDict = new Dictionary<KeyCode, KeyControl>();
                    for (int i = (int)KeyCode.Alpha0; i <= (int)KeyCode.Alpha9; i++)
                    {
                        try
                        {
                            m_specialKeyCodeDict.Add((KeyCode)i, Keyboard.current[(i - (int)KeyCode.Alpha0).ToString()] as KeyControl);
                        }
                        catch (KeyNotFoundException) { }
                    }
                    for (int i = (int)KeyCode.Keypad0; i <= (int)KeyCode.Keypad9; i++)
                    {
                        try
                        {
                            m_specialKeyCodeDict.Add((KeyCode)i, Keyboard.current["numpad" + (i - (int)KeyCode.Keypad0).ToString()] as KeyControl);
                        }
                        catch (KeyNotFoundException) { }
                    }
                }
                return m_specialKeyCodeDict;
            }
        }
#endif

        public static bool DefaultGetKeyDown(KeyCode keyCode)
        {
#if USE_NEW_INPUT
            if (Keyboard.current == null || keyCode == KeyCode.None) return false;
            if (keyCode == KeyCode.Return) return (Keyboard.current["enter"] as KeyControl).wasPressedThisFrame;
            if (!m_keyCodeStrings.TryGetValue(keyCode, out var s))
            {
                // Store the Input System's lowercase string equivalent for the keycode.
                // This is done only once per keycode and stored in a dictionary.
                s = keyCode.ToString().ToLower();
                m_keyCodeStrings.Add(keyCode, s);

            }
            if (s.StartsWith("mouse"))
            {
                if (s == "mouse0") return Mouse.current.leftButton.wasPressedThisFrame;
                else if (s == "mouse1") return Mouse.current.rightButton.wasPressedThisFrame;
                else if (s == "mouse2") return Mouse.current.middleButton.wasPressedThisFrame;
            }
            if (s.StartsWith("joystick") || s.StartsWith("mouse")) return false;
            if ((KeyCode.Alpha0 <= keyCode && keyCode <= KeyCode.Alpha9) || 
                (KeyCode.Keypad0 <= keyCode && keyCode <= KeyCode.Keypad9))
            {
                KeyControl numKeyControl;
                return specialKeyCodeDict.TryGetValue(keyCode, out numKeyControl) ? numKeyControl.wasPressedThisFrame : false;
            }
            var keyControl = Keyboard.current[s] as KeyControl;
            return (keyControl != null) ? keyControl.wasPressedThisFrame : false;
#else
            return Input.GetKeyDown(keyCode);
#endif
        }

        public static bool DefaultGetAnyKeyDown()
        {
#if USE_NEW_INPUT
            return Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
#else
            return Input.anyKeyDown;
#endif
        }

        public static bool DefaultGetButtonDown(string buttonName)
        {
            try
            {
#if USE_NEW_INPUT
                InputAction inputAction;
                if (inputActionDict.TryGetValue(buttonName, out inputAction))
                {
                    foreach (var control in inputAction.controls)
                    {
                        if (((control is ButtonControl) && (control as ButtonControl).wasPressedThisFrame) ||
                            ((control is KeyControl) && (control as KeyControl).wasPressedThisFrame))
                        {
                            return true;
                        }
                    }
                }
                return false;
#else
                return string.IsNullOrEmpty(buttonName) ? false : Input.GetButtonDown(buttonName);
#endif
            }
            catch (System.ArgumentException) // Input button not in setup.
            {
                return false;
            }
        }

        public static bool DefaultGetButtonUp(string buttonName)
        {
            try
            {
#if USE_NEW_INPUT
                InputAction inputAction;
                if (inputActionDict.TryGetValue(buttonName, out inputAction))
                {
                    foreach (var control in inputAction.controls)
                    {
                        if (((control is ButtonControl) && (control as ButtonControl).wasReleasedThisFrame) ||
                            ((control is KeyControl) && (control as KeyControl).wasReleasedThisFrame))
                        {
                            return true;
                        }
                    }
                }
                return false;
#else
                return string.IsNullOrEmpty(buttonName) ? false : Input.GetButtonUp(buttonName);
#endif
            }
            catch (System.ArgumentException) // Input button not in setup.
            {
                return false;
            }
        }

        public static float DefaultGetAxis(string axisName)
        {
            try
            {
#if USE_NEW_INPUT
                InputAction inputAction;
                if (inputActionDict.TryGetValue(axisName, out inputAction))
                {
                    return inputAction.ReadValue<float>();
                }
                return 0;
#else
                return string.IsNullOrEmpty(axisName) ? 0 : Input.GetAxis(axisName);
#endif
            }
            catch (System.ArgumentException) // Input axis not in setup.
            {
                return 0;
            }
        }

        public static Vector3 DefaultGetMousePosition()
        {
#if USE_NEW_INPUT
            if (Mouse.current == null) return Vector3.zero;
            var pos = Mouse.current.position.ReadValue();
            return new Vector3(pos.x, pos.y, 0);
#else
            return Input.mousePosition;
#endif
        }

        public static bool DefaultGetMouseButtonDown(int buttonNumber)
        {
#if USE_NEW_INPUT
            if (Mouse.current == null) return false;
            switch (buttonNumber)
            {
                case 0: return Mouse.current.leftButton.wasPressedThisFrame;
                case 1: return Mouse.current.rightButton.wasPressedThisFrame;
                case 2: return Mouse.current.middleButton.wasPressedThisFrame;
                default: return false;
            }
#else
            return Input.GetMouseButtonDown(buttonNumber);
#endif
        }

    }
}
