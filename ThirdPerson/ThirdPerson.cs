using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ABI_RC.Core;
using ABI_RC.Core.InteractionSystem;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(ThirdPerson.ThirdPerson), "ThirdPerson", "0.0.2", "elian")]
[assembly: MelonGame("ChilloutVR")]
[assembly: MelonColor(ConsoleColor.DarkYellow)]

namespace ThirdPerson
{
    public class ThirdPerson : MelonMod
    {
        internal static MelonLogger.Instance Logger;
        private static bool _previousState;
        private static bool State {
            get => _state;
            set {
                _previousState = _state;
                _state = value;
                _ourCam.SetActive(_state);
            } 
        }
        public override void OnApplicationStart() 
        {
            Logger = LoggerInstance; 
            MelonCoroutines.Start(SetupCameras());
            HarmonyInstance.Patch(
                typeof(ViewManager).GetMethods().FirstOrDefault(x => x.Name == nameof(ViewManager.UiStateToggle) && x.GetParameters().Length > 0),
                typeof(ThirdPerson).GetMethod(nameof(ToggleMainMenu), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod()
            );
            HarmonyInstance.Patch(
                typeof(CVR_MenuManager).GetMethod(nameof(CVR_MenuManager.ToggleQuickMenu)),
                typeof(ThirdPerson).GetMethod(nameof(ToggleQuickMenu), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod()
            );
        }
        public override void OnUpdate()
        {
            if(State)
            {
                if (Input.GetAxis("Mouse ScrollWheel") > 0f) IncrementDist();
                else if (Input.GetAxis("Mouse ScrollWheel") < 0f) DecrementDist();
            }

            if (!Input.GetKey(KeyCode.LeftControl)) return;
            if (Input.GetKeyDown(KeyCode.T)) State = !State;
            if (!State) return;
            if (!Input.GetKeyDown(KeyCode.Y)) return;
            RelocateCam((CameraLocation)(((int)_currentLocation + 1) % Enum.GetValues(typeof(CameraLocation)).Length), true);
        }
        #region Menu Patches
        private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly FieldInfo MainMenuOpen =
            typeof(ViewManager).GetField("_gameMenuOpen", Flags);
        private static readonly FieldInfo QuickMenuOpen =
            typeof(CVR_MenuManager).GetField("_quickMenuOpen", Flags);
        private static bool IsMmOpen => (bool)MainMenuOpen.GetValue(ViewManager.Instance);
        private static bool IsQmOpen => (bool)QuickMenuOpen.GetValue(CVR_MenuManager.Instance);
        private static void ToggleMainMenu(bool __0) => ToggleMenus(__0, true);
        private static void ToggleQuickMenu(bool __0) => ToggleMenus(__0, false);
        private static void ToggleMenus(bool isOpen, bool isMain)
        {
            if ((IsMmOpen && !isMain) || (IsQmOpen && isMain)) return;
            State = State switch
            {
                false when !isOpen && _previousState => true,
                true when isOpen => false,
                _ => State
            };
        }
        #endregion
        private static IEnumerator SetupCameras()
        {
            while (RootLogic.Instance == null) yield return null;
            while (RootLogic.Instance.activeCamera == null) yield return null;
            _defaultCam = RootLogic.Instance.activeCamera.gameObject;
            _ourCam = new GameObject { gameObject = { name = "ThirdPersonCameraObj" } };
            _ourCam.transform.SetParent(_defaultCam.transform);
            RelocateCam(CameraLocation.Default);
            _ourCam.gameObject.SetActive(false);
            _ourCam.AddComponent<Camera>();
            Logger.Msg("Finished setting up third person camera.");
        }
        private static void RelocateCam(CameraLocation loc, bool resetDist = false)
        {
            _ourCam.transform.rotation = _defaultCam.transform.rotation;
            if(resetDist) ResetDist();
            switch (loc)
            {
                case CameraLocation.Default:
                    {
                        _ourCam.transform.localPosition = new Vector3(0, 0.015f, -0.55f + _dist);
                        _currentLocation = CameraLocation.Default;
                        _ourCam.transform.localRotation = new Quaternion(0, 0, 0, 0);
                    }
                    break;
                case CameraLocation.FrontView:
                    {
                        _ourCam.transform.localPosition = new Vector3(0, 0.015f, 0.55f + _dist);
                        _ourCam.transform.localRotation = new Quaternion(0, 180, 0, 0);
                        _currentLocation = CameraLocation.FrontView;
                    }
                    break;
                case CameraLocation.RightSide:
                    {
                        _ourCam.transform.localPosition =  new Vector3(0.3f, 0.015f, -0.55f + _dist);
                        _currentLocation = CameraLocation.RightSide;
                        _ourCam.transform.localRotation = new Quaternion(0, 0, 0, 0);
                    }
                    break;
                case CameraLocation.LeftSide:
                    {
                        _ourCam.transform.localPosition = new Vector3(-0.3f, 0.015f, -0.55f + _dist);
                        _currentLocation = CameraLocation.LeftSide;
                        _ourCam.transform.localRotation = new Quaternion(0, 0, 0, 0);
                    }
                    break;
            }
        }
        private enum CameraLocation
        {
            Default,
            FrontView,
            RightSide,
            LeftSide
        }
        private static void ResetDist() => _dist = 0;
        private static void IncrementDist() { _dist += 0.25f; RelocateCam(_currentLocation); }
        private static void DecrementDist() { _dist -= 0.25f; RelocateCam(_currentLocation); }
        private static float _dist;
        private static bool _state;
        private static GameObject _ourCam, _defaultCam;
        private static CameraLocation _currentLocation = CameraLocation.Default;
    }
}
