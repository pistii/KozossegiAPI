using System.Collections.Concurrent;

namespace KozossegiAPI.Realtime.Connection
{
    public class ConnectionMapping : IMapConnections
    {
        public ConcurrentDictionary<string, int> Connections;
        public ConnectionMapping()
        {
            Connections = new ConcurrentDictionary<string, int>();
        }

        public ConcurrentDictionary<string, int> keyValuePairs {
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
                return Connections.Any(p => p.Key == connectionId && p.Value == userId);
            }
            return Connections.Any(e => e.Value == userId);
        }

        public void Remove(string connectionId)
        {
            var connectionToRemove = Connections.FirstOrDefault(p => p.Key == connectionId);
            Connections.TryRemove(connectionToRemove);
        }

        public void AddConnection(string connectionId, int userId)
        {
            if (!string.IsNullOrEmpty(connectionId) || userId != 0)
            {
                try
                {
                    Connections.TryAdd(connectionId, userId);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Console.Error.WriteLine("Failed to add user to connections: " + ex.Message);
                }
                catch (InvalidOperationException ioex)
                {
                    Console.Error.WriteLine("Failed to add user to connections: " + ioex.Message);
                }
            }
        }


        public string[] GetConnectionsById(int userId)
        { //TODO: send the request all the opened connections
            var connection = Connections.Where(u => u.Value == userId).Select(i => i.Key).ToArray();
            return connection ?? null;
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
        public ConcurrentDictionary<string, int> keyValuePairs { get; }
        public bool ContainsUser(int userId, string connectionId = null);
        public void AddConnection(string connectionId, int userId);
        int GetUserById(string connectionId);
        string[] GetConnectionsById(int userId);
    }
}
