﻿using KozoskodoAPI.Controllers.Cloud;
using KozoskodoAPI.Models;

namespace KozoskodoAPI.DTOs
{
    public class RegisterForm : user
    {
        public string Password { get; set; }
    }

    /// <summary>
    /// Dto to complete the registration. This could be used also with the settings
    /// </summary>
    public class ModifyUserInfoDTO : AvatarUpload
    {
        public string? firstName { get; set; }
        public string? middleName { get; set; }
        public string? lastName { get; set; }
        public string? PlaceOfResidence { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PlaceOfBirth { get; set; }
        public string? EmailAddress { get; set; }
        public string? SecondaryEmailAddress { get; set; }
        public string? Profession { get; set; }
        public string? Workplace { get; set; }
        public string? SchoolName { get; set; }
        public string? Class { get; set; }
        public string? StartYear { get; set; }
        public string? EndYear { get; set; }
        public bool isOnline { get; set; }
        public string? Pass1 { get; set; }
        public string? Pass2 { get; set; }


    }

}
