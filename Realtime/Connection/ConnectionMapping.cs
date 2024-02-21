using KozoskodoAPI.Models;

namespace KozoskodoAPI.Realtime.Connection
{
    public class ConnectionMapping : IMapConnections
    {
        public Dictionary<string, int> Connections;
        public ConnectionMapping()
        {
            Connections = new Dictionary<string, int>();
        }

        public Dictionary<string, int> keyValuePairs {
            get { return Connections; }
        }

        /// <summary>
        /// Returns true if the map contains the userId and the connectionId
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool ContainsUser(int userId, string connectionId = null)
        {
            if (connectionId != null)
            {
                if (Connections.ContainsKey(connectionId) && Connections.ContainsValue(userId))
                {
                    return true;
                }
            }
            if (Connections.ContainsValue(userId))
            {
                return true;
            }
            return false;
        }

        public void Remove(string connectionId)
        {
            Connections.Remove(connectionId);
        }

        public void AddConnection(string connectionId, int userId)
        {
            Connections.TryAdd(connectionId, userId);
        }


        public string GetConnectionById(int userId)
        { //TODO: send the request all the opened connections
            var connection = Connections.FirstOrDefault(_ => _.Value == userId);
            return connection.Key ?? "Not found";
        }

        public int GetUserById(string connectionId)
        {
            var userId = Connections.FirstOrDefault(_ => _.Key == connectionId);
            return userId.Value;
        }

    }

    public interface IMapConnections
    {
        public void Remove(string connectionId);
        public Dictionary<string, int> keyValuePairs { get; }
        public bool ContainsUser(int userId, string connectionId = null);
        public void AddConnection(string connectionId, int userId);
        int GetUserById(string connectionId);
        string GetConnectionById(int userId);
    }
}
