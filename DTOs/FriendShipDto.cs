using KozossegiAPI.Models;

namespace KozossegiAPI.DTOs
{
    public class FriendShipDto: Personal
    {
        public FriendshipStatus FriendshipStatus { get; set; }
        //public friendshipSince TODO
    }

    public enum FriendshipStatus
    {
        Friends,
        Friendship_sent,
        Friendship_received,
        Unfriend
    }
}
