/*
 * User class.
 * Container for user information such as usernames, passwords, and friends lists.
 * 
 * Author: Stephen Schmith
 * Created in Microsoft Visual Studio Code
 */

using System.Collections;
using System.Net.Sockets;

namespace IMServer
{
    class User
    {
        private readonly string username;
        private readonly string password;
        private readonly ArrayList friends;      // Friends are stored in memory since the server is assumed to be up 24/7. 
                                                 // Friends are stored as User objects.
        private readonly ArrayList messageLog;    // messageLog stores messages delivered to this user when they're offline.
        private TcpClient tcpClient;
        private bool isOnline;          // True if this user is online. Otherwise, false.

        public User(string u, string p)
        {
            username = u;
            password = p;
            isOnline = false;
            tcpClient = null;           // TcpClient set to null until it's initialized.

            friends = new ArrayList();
            messageLog = new ArrayList();
        }

        public bool Online
        // Property for accessing the value of isOnline.
        {
            get { return isOnline; }
            set { isOnline = value; }
        }

        public TcpClient Client
        // Property for accessing this User's tcpClient.
        {
            get { return tcpClient; }
            set { tcpClient = value; }
        }

        public string Username
        // Read-only property for getting the value of this.username.
        {
            get { return username; }
        }

        public string Password
        // Read-only property for getting the value of this.password.
        {
            get { return password; }
        }

        public ArrayList FriendsList
        // Read-only property for return this friends list.
        // Note: ArrayList is not type-safe. Returned objects must be cast to the User type, or an error will be thrown.
        {
            get { return friends; }
        }

        public void AddFriend(string un)
        // Friends list file is only created if a User adds a friend.
        {
            User newFriend = new(un, null);

            if (friends.Contains(newFriend))
            {
                // Do not allow duplicate additions to the friends list.
                return;
            }

            // Add friend
            friends.Add(newFriend);
        }

        public bool RemoveFriend(string un)
        // Remove a friend from the friends list.
        {
            User u = new(un, null);

            if (friends.Contains(u))
            {
                friends.Remove(u);
                return true;
            }

            return false;
        }

        public void InsertMessage(string source, string content)
        // Insert a message into this user's messageLog.
        // Sources are stored on even indexes, and their associated messages are stored on the next (odd) index.
        {
            messageLog.Add(source);
            messageLog.Add(content);
        }

        public ArrayList GetMessageLog()
        // Return the messageLog.
        {
            return messageLog;
        }

        public void ClearMessageLog()
        // Erase the contents of the message log.
        {
            messageLog.Clear();
        }

        public override bool Equals(object obj)
        // Override the built-in Equals method to allow User objects to be compared based on the username field.
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is not User u)
            {
                return false;
            }

            return username == u.Username;
        }

        public bool Equals(User u)
        // Specific Equals method for User objects.
        {
            if (u == null)
            {
                return false;
            }

            return username == u.Username;
        }

        public override int GetHashCode()
        // Create an arbitrary hash code for this class (required for Equals to work properly).
        {
            return (username.GetHashCode() + password.GetHashCode()) / 3;
        }
    }
}
