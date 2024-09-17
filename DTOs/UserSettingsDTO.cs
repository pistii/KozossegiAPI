namespace KozossegiAPI.DTOs
{
    /// <summary>
    /// TODO/NOTE -> This dto can be used in the future if will be implemented a server-client interval communication
    /// </summary>
    public class UserSettingsDTO
    {
        public int Days { get; set; }
        public bool? RemindUserOfUnfulfilledReg { get; set; } = false;
        public bool isOnlineEnabled = true;
    }
}
