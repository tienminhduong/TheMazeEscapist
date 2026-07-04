using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using CandyCoded.HapticFeedback;

namespace KingCat.Base
{
    public class VibrationController : MonoSingleton<VibrationController>
    {
        public enum HapticType
        {
            Light,
            Medium,
            Heavy
        }

        [HideInInspector] public bool IsMuteVibration;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = false;

        [Header("Config")]
        [SerializeField] private float minIntervalBetweenVibrates = 0.04f;
        [SerializeField] private bool fallbackToHandheldVibrateOnAndroid = true;

        private const string SETTING_VIBRATION_KEY = "settingMuteVibration";

        private bool? _cachedHasVibrator;
        private float _lastVibrateTime = -999f;

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(1f);

            LoadSetting();

#if UNITY_ANDROID && !UNITY_EDITOR
            CacheAndroidVibratorSupport();
#endif
        }

        public void LoadSetting()
        {
            // IsMuteVibration = UserProfileController.Instance.GetParam<bool>(SETTING_VIBRATION_KEY);
            Log($"LoadSetting -> IsMuteVibration = {IsMuteVibration}");
        }

        public void ToggleVibration()
        {
            bool newValue = !IsMuteVibration;
            // UserProfileController.Instance.SetParam(SETTING_VIBRATION_KEY, newValue);
            IsMuteVibration = newValue;
            Log($"ToggleVibration -> {IsMuteVibration}");
        }

        public void SetMuteVibration(bool isMute)
        {
            // UserProfileController.Instance.SetParam(SETTING_VIBRATION_KEY, isMute);
            IsMuteVibration = isMute;
            Log($"SetMuteVibration -> {IsMuteVibration}");
        }

        public void VibratePop(float delayTime = 0f) => Play(HapticType.Light, delayTime);
        public void VibratePeek(float delayTime = 0f) => Play(HapticType.Medium, delayTime);
        public void VibrateNope(float delayTime = 0f) => Play(HapticType.Heavy, delayTime);

        public void PlayLight(float delayTime = 0f) => Play(HapticType.Light, delayTime);
        public void PlayMedium(float delayTime = 0f) => Play(HapticType.Medium, delayTime);
        public void PlayHeavy(float delayTime = 0f) => Play(HapticType.Heavy, delayTime);

        public void Play(HapticType type, float delayTime = 0f)
        {
            PlayInternal(type, delayTime).Forget();
        }

        private async UniTaskVoid PlayInternal(HapticType type, float delayTime)
        {
            if (delayTime > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(delayTime));

            if (!CanVibrate())
                return;

            if (Time.unscaledTime - _lastVibrateTime < minIntervalBetweenVibrates)
            {
                Log($"Skip vibrate because of cooldown -> {type}");
                return;
            }

            _lastVibrateTime = Time.unscaledTime;

            PlayHaptic(type);

#if UNITY_ANDROID
            PlayAndroid(type);
#elif UNITY_IOS
            Log($"iOS -> {type}");
#else
            Log($"Unsupported platform -> {type}");
#endif
        }

        private void PlayHaptic(HapticType type)
        {
            try
            {
                switch (type)
                {
                    case HapticType.Light:
                        HapticFeedback.LightFeedback();
                        break;

                    case HapticType.Medium:
                        HapticFeedback.MediumFeedback();
                        break;

                    case HapticType.Heavy:
                        HapticFeedback.HeavyFeedback();
                        break;
                }

                Log($"CandyCoded Haptic -> {type}");
            }
            catch (Exception e)
            {
                Log($"PlayHaptic exception -> {e.Message}");
            }
        }

#if UNITY_ANDROID
        private void PlayAndroid(HapticType type)
        {
            if (!HasVibratorOnAndroid())
            {
                Log($"Android -> No vibrator, skip {type}");
                return;
            }

            int feedbackConstant = GetAndroidHapticConstant(type);

            bool hapticSuccess = PerformAndroidHapticFeedback(feedbackConstant);
            Log($"Android performHapticFeedback -> {(hapticSuccess ? "success" : "failed")} | {type} ({feedbackConstant})");

            bool vibratorSuccess = PerformAndroidVibration(type);
            Log($"Android vibrator -> {(vibratorSuccess ? "success" : "failed")} | {type}");

            if (!hapticSuccess && !vibratorSuccess && fallbackToHandheldVibrateOnAndroid)
            {
                Handheld.Vibrate();
                Log($"Android fallback Handheld.Vibrate() -> {type}");
            }
        }

        private int GetAndroidHapticConstant(HapticType type)
        {
            switch (type)
            {
                case HapticType.Light: return 1; // VIRTUAL_KEY
                case HapticType.Medium: return 3; // KEYBOARD_TAP
                case HapticType.Heavy: return 0; // LONG_PRESS
                default: return 1;
            }
        }

        private bool PerformAndroidHapticFeedback(int feedbackConstant)
        {
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var window = currentActivity.Call<AndroidJavaObject>("getWindow");
                using var decorView = window?.Call<AndroidJavaObject>("getDecorView");

                if (decorView == null)
                {
                    Log("DecorView is null");
                    return false;
                }

                return decorView.Call<bool>("performHapticFeedback", feedbackConstant);
            }
            catch (Exception e)
            {
                Log($"PerformAndroidHapticFeedback exception -> {e.Message}");
                return false;
            }
        }

        private bool PerformAndroidVibration(HapticType type)
        {
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

                if (vibrator == null)
                {
                    Log("Vibrator is null");
                    return false;
                }

                int apiLevel = GetAndroidApiLevel();

                if (apiLevel >= 26)
                {
                    using var vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");

                    long duration = GetAndroidVibrationDuration(type);
                    int amplitude = GetAndroidVibrationAmplitude(type);

                    using var effect = vibrationEffect.CallStatic<AndroidJavaObject>(
                        "createOneShot",
                        duration,
                        amplitude
                    );

                    vibrator.Call("vibrate", effect);
                }
                else
                {
                    vibrator.Call("vibrate", GetAndroidVibrationDuration(type));
                }

                return true;
            }
            catch (Exception e)
            {
                Log($"PerformAndroidVibration exception -> {e.Message}");
                return false;
            }
        }

        private long GetAndroidVibrationDuration(HapticType type)
        {
            switch (type)
            {
                case HapticType.Light: return 20;
                case HapticType.Medium: return 35;
                case HapticType.Heavy: return 60;
                default: return 30;
            }
        }

        private int GetAndroidVibrationAmplitude(HapticType type)
        {
            switch (type)
            {
                case HapticType.Light: return 60;
                case HapticType.Medium: return 120;
                case HapticType.Heavy: return 255;
                default: return 120;
            }
        }

        private int GetAndroidApiLevel()
        {
            try
            {
                using var version = new AndroidJavaClass("android.os.Build$VERSION");
                return version.GetStatic<int>("SDK_INT");
            }
            catch
            {
                return 0;
            }
        }
#endif

        public bool CanVibrate()
        {
            if (IsMuteVibration)
                return false;

#if UNITY_ANDROID
            return HasVibratorOnAndroid();
#elif UNITY_IOS
            return true;
#else
            return false;
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void CacheAndroidVibratorSupport()
        {
            if (_cachedHasVibrator.HasValue)
                return;

            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

                _cachedHasVibrator = vibrator != null && vibrator.Call<bool>("hasVibrator");
                Log($"CacheAndroidVibratorSupport -> {_cachedHasVibrator.Value}");
            }
            catch (Exception e)
            {
                _cachedHasVibrator = false;
                Log($"CacheAndroidVibratorSupport exception -> {e.Message}");
            }
        }

        private bool HasVibratorOnAndroid()
        {
            if (!_cachedHasVibrator.HasValue)
                CacheAndroidVibratorSupport();

            return _cachedHasVibrator.Value;
        }
#else
        private void CacheAndroidVibratorSupport()
        {
            _cachedHasVibrator = true;
        }

        private bool HasVibratorOnAndroid()
        {
            return true;
        }
#endif

        private void Log(string message)
        {
            if (!enableDebugLog) return;
            Debug.Log($"[VibrationController] {message}");
        }
    }
}