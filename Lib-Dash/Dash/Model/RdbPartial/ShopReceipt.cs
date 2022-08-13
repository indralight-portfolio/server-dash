using System.Collections.Generic;
#if Common_Server
#endif

namespace Dash.Model.Rdb
{
    public partial class ShopReceipt
    {
        #region Query
        public static string GetByProductIdQuery = $"SELECT * FROM {nameof(ShopReceipt)}" +
                                    $" WHERE {nameof(OidAccount)} = @{nameof(OidAccount)}" +
                                    $" AND {nameof(ProductId)} = @{nameof(ProductId)}" +
                                    $" ORDER BY Id DESC LIMIT 1";
        public static List<KeyValuePair<string, object>> GetByProductIdQueryParam(ulong oidAccount, int productId)
        {
            return new List<KeyValuePair<string, object>> {
                new KeyValuePair<string, object>($"@{nameof(OidAccount)}", oidAccount),
                new KeyValuePair<string, object>($"@{nameof(ProductId)}", productId),
            };
        }
        #endregion
    }
}
