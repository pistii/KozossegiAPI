using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace KozoskodoAPI.DTOs
{
    public class ProfilePageDto
    {
        public Personal PersonalInfo { get; set; }
        public ContentDto<PostDto>? Posts { get; set; }
        public List<Personal_IsOnlineDto>? Friends { get; set; }
        public string? PublicityStatus { get; set; }
        //public bool RemindUserOfUnfulfilledReg { get; set; } = false;
        public UserSettingsDTO? settings { get; set; }
    }
}
