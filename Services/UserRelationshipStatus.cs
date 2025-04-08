using KozossegiAPI.Models;

namespace KozossegiAPI.Services
{
    public class UserRelationContext
    {
        public UserRelationshipStatus Status { get; set; }
        public bool CanPost { get; set; }
        public bool CanMessage { get; set; }
        public bool IsBlocked { get; set; }
        public bool ShouldShowAddFriend { get; set; }
        public bool ShouldShowFriendRequestReceived { get; set; }
        public bool ShouldShowFriendRequestSent { get; set; }
        public bool ShouldShowRemoveFriend { get; set; }
        public bool ShowSettings { get; set; }
        public string? Avatar { get; set; } = null;
        public PublicUserInfo UserInfo { get; set; }
    }

    public enum UserRelationshipStatus
    {
        Self,
        Friend,
        FriendRequestRejected,
        FriendRequestSent,
        FriendRequestReceived,
        Blocked,
        Stranger
    }

    public class PublicUserInfo
    {
        public PublicUserInfo(Personal personal)
        {
            this.UserId = personal.users!.PublicId;
            this.FirstName = personal.firstName;
            this.MiddleName = personal.middleName;
            this.LastName = personal.lastName;
            this.BirthOfPlace = personal.PlaceOfBirth;
            this.PlaceOfResidence = personal.PlaceOfResidence;
        }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public string? BirthOfPlace { get; set; }
        public string? PlaceOfResidence { get; set; }
    }

}
