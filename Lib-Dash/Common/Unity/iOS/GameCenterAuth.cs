#if Common_Unity
using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Common.Unity.iOS
{
    public static class GameCenterAuth
    {
        public delegate void Callback(Model.GameCenterAuth gameCenterAuth);
        public static Callback _callback;
        public static void Verify(Callback callback)
        {
            Debug.Log("GameCenterAuth.Verify");
            _callback = callback;
#if UNITY_IOS && !UNITY_EDITOR
            generateIdentityVerificationSignature(identityVerificationSignatureCallback);
#else
            _callback.Invoke(null);
#endif
        }

        #region DllImport
#if UNITY_IOS && !UNITY_EDITOR
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void IdentityVerificationSignatureCallback(
        string publicKeyUrl,
        string signature,
        string salt,
        ulong timestamp,
        string playerId,
        string bundleId,
        string error);

        [DllImport("__Internal")]
        private static extern void generateIdentityVerificationSignature(
            [MarshalAs(UnmanagedType.FunctionPtr)]IdentityVerificationSignatureCallback callback);

        // Note: This callback has to be static because Unity's il2Cpp doesn't support marshalling instance methods.
        [AOT.MonoPInvokeCallback(typeof(IdentityVerificationSignatureCallback))]
        private static void identityVerificationSignatureCallback(
            string publicKeyUrl,
            string signature,
            string salt,
            ulong timestamp,
            string playerId,
            string bundleId,
            string error)
        {
            UnityEngine.Debug.Log($"publicKeyUrl: {publicKeyUrl}");
            UnityEngine.Debug.Log($"signature: {signature}");
            UnityEngine.Debug.Log($"salt: {salt}");
            UnityEngine.Debug.Log($"timestamp: {timestamp}");
            UnityEngine.Debug.Log($"error: {error}");
            Model.GameCenterAuth gameCenterAuth = new Model.GameCenterAuth
            {
                PlayerId = playerId,
                BundleId = bundleId,
                PublicKeyUrl = publicKeyUrl,
                Signature = signature,
                Salt = salt,
                Timestamp = timestamp
            };

            _callback.Invoke(gameCenterAuth);
        }
#endif
        #endregion // DllImport
    }
}
#endif