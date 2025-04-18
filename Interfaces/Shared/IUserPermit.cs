namespace KozossegiAPI.Interfaces.Shared
{
    public interface IUserPermit
    {
        public bool IsActivated { get; set; }
        public bool IsRestricted { get; set; }
        public bool IsOnlineEnabled { get; set; }
    }
}
