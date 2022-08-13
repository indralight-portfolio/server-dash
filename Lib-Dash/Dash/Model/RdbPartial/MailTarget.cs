using Dash.Types;

namespace Dash.Model.Rdb
{
    public partial class MailTarget
    {
        public MailTarget(ulong oidAccount, uint templateId, MailTargetStatus status)
        {
            OidAccount = oidAccount;
            TemplateId = templateId;
            Status = (sbyte)status;
        }
    }
}
