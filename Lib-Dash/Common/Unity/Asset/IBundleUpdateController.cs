#if Common_Unity

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common.Unity.Asset
{
    public enum BundleMessageType
    {
        Undefined,
        AdditionalAssets,
        NetworkError,
    }

    public enum ConfirmType
    {
        Undefined,
        Ok,
        Retry,
    }

    public interface IBundleUpdateController
    {
        void Show();

        void Hide();

        void ShowAdditionalMessage(BundleMessageType state, float sizeMB);

        IEnumerator WaitForConfirm(ConfirmType state);

        void HideAdditionalMessage();
        
        void SetProgressInfo(BundleUpdateProgressInfo progressInfo);
    }
}
#endif