using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using ABI_RC.Core;
using ABI_RC.Core.InteractionSystem;
using MelonLoader;
using UnityEngine;
using System.Reflection;

[assembly: MelonInfo(typeof(elianel.ThirdPerson), "ThirdPerson", "0.0.1", "elian")]
[assembly: MelonGame("ChilloutVR")]
[assembly: MelonColor(ConsoleColor.DarkYellow)]

namespace elianel
{
    public class ThirdPerson : MelonMod
    {
        internal static MelonLogger.Instance Logger;
        public static bool State { 
            get => _state;
            set { 
                _state = value;
                _ourCam.SetActive(_state);
                Logger.Msg("Set state to: " + _state);
            } 
        }
        public override void OnApplicationStart() 
        {
            Logger = LoggerInstance; 
            MelonCoroutines.Start(SetupCameras());
            HarmonyInstance.Patch(
                typeof(ViewManager).GetMethods().FirstOrDefault(x => x.Name == nameof(ViewManager.UiStateToggle) && x.GetParameters().Length > 0),
                typeof(ThirdPerson).GetMethod(nameof(ThirdPerson.ToggleBigMenu), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod()
            );
            HarmonyInstance.Patch(
                typeof(CVR_MenuManager).GetMethod(nameof(CVR_MenuManager.ToggleQuickMenu)),
                typeof(ThirdPerson).GetMethod(nameof(ThirdPerson.ToggleQuickmenu), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod()
            );
        }
        public override void OnUpdate()
        {
            if(State)
            {
                if (Input.GetAxis("Mouse ScrollWheel") > 0f) IncrementDist();
                else if (Input.GetAxis("Mouse ScrollWheel") < 0f) DecrementDist();
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.T)) State = !State;
                if (!State) return;
                if (!Input.GetKeyDown(KeyCode.Y)) return;
                RelocateCam((CameraLocation)((((int)_currentLocation) + 1) % Enum.GetValues(typeof(CameraLocation)).Length), true);
            }

        }
        private static void ToggleBigMenu(bool __0)
        {
            if (!State && !_antiStateBig) return;
            else if(!State && _antiStateBig && !__0)
            {
                State = true;
                _antiStateBig = false;
            }
            else if (State && __0)
            {
                State = false;
                _antiStateBig = true;
            }
            else _antiStateBig = false;

        }
        private static void ToggleQuickmenu(bool __0)
        {
            if (!State && !_antiStateSmol) return;
            else if (!State && _antiStateSmol && !__0)
            {
                State = true;
                _antiStateSmol = false;
            }
            else if (State && __0)
            {
                State = false;
                _antiStateSmol = true;
            }
            else _antiStateSmol = false;
        }
        private static IEnumerator SetupCameras()
        {
            while (RootLogic.Instance == null) yield return null;
            while (RootLogic.Instance.activeCamera == null) yield return null;
            _defaultCam = RootLogic.Instance.activeCamera.gameObject;
            _ourCam = new GameObject();
            _ourCam.gameObject.name = "ThirdPersonCameraObj";
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ResetDist() => _dist = 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void IncrementDist() { _dist += 0.25f; RelocateCam(_currentLocation); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DecrementDist() { _dist -= 0.25f; RelocateCam(_currentLocation); }
        private static float _dist = 0;
        private static GameObject _ourCam;
        private static GameObject _defaultCam;
        private static bool _state = false;
        private static bool _antiStateBig = false;
        private static bool _antiStateSmol = false;
        private static CameraLocation _currentLocation = CameraLocation.Default;
    }
}
