namespace Dash.Model.Rdb
{
    public partial class IssueSerial
    {
        public bool Equals(IssueSerial other)
        {
            return Equipment.Equals(other.Equipment);
        }
    }
}
