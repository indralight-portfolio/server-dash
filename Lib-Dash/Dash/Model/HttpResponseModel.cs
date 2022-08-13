using Dash.Types;

namespace Dash.Model
{
    [MessagePack.MessagePackObject()]
    public class HttpResponseModel : Common.Net.WWW.CommonHttpResponseModel<ErrorCode>
    {
        [MessagePack.Key(0)]
        public ErrorCode ErrorCode { get; set; }
        [MessagePack.Key(1)]
        public string ErrorText
        {
            get
            {
                return errorText ?? ErrorCode.ToString();
            }
            set
            {
                errorText = value;
            }
        }

        [MessagePack.Key(5)]
        private string errorText;

        public HttpResponseModel()
        {
        }

        public HttpResponseModel(HttpResponseModel model)
        {
            SetResult(model.ErrorCode, model.ErrorText);
        }

        public HttpResponseModel(ErrorCode errorCode, string errorText = null)
        {
            SetResult(errorCode, errorText);
        }

        protected override ErrorCode GetErrorCode()
        {
            return ErrorCode;
        }

        protected override void SetErrorCode(ErrorCode errorCode)
        {
            ErrorCode = errorCode;
        }

        protected override string GetErrorText()
        {
            return ErrorText;
        }

        protected override void SetErrorText(string errorText)
        {
            ErrorText = errorText;
        }
    }
}