namespace Common.Net.WWW
{
    [System.Serializable]
    public abstract class CommonHttpResponseModel<TErrorCode>
    {
        protected abstract TErrorCode GetErrorCode();
        protected abstract void SetErrorCode(TErrorCode errorCode);
        protected abstract string GetErrorText();
        protected abstract void SetErrorText(string errorText);

        public void SetResult(TErrorCode errorCode, string errorText = null)
        {
            SetErrorCode(errorCode);
            SetErrorText(errorText);
        }

        public void SetResult(CommonHttpResponseModel<TErrorCode> model)
        {
            SetErrorCode(model.GetErrorCode());
            SetErrorText(model.GetErrorText());
        }

        public virtual void LogResponse()
        {
#if Common_Unity
            UnityEngine.Debug.Log($"[HttpResponse]{GetErrorCode()}:{GetErrorText()}");
#endif
        }
    }
}