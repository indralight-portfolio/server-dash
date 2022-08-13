namespace server_dash.Statistics
{
    public class InvalidUndoneGamePayload : IPayload
    {
        public static InvalidUndoneGamePayload From()
        {
            var result = new InvalidUndoneGamePayload();
            return result;
        }
    }
}