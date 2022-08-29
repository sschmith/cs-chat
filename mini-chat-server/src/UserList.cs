/*
 * UserList class.
 * Aggregates User objects and provides methods to access them.
 * 
 * Author: Stephen Schmith
 * Last Modified: 3/14/2013
 * Created in Microsoft Visual Studio Express 2012
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMServer
{
    // This class maintains a current list of all username and passsword combinations.
    // A permanent list is maintained in the local directory. This list must be named users.ul
    class UserList
    {
        /*  
         *  Users are stored inside a Map-like object called Dictionary.
         *  ConcurrentDictionary is a thread-safe Dictionary.
         *  Usernames map to User objects.
         */

        private ConcurrentDictionary<string, User> users;
        
        public UserList()
        /*
         * Initialize the user Dictionary and fill it with data from users.ul.
         * If users.ul doesn't exist, it is created and the line "admin.password" is appended to add a default user to the system.
         */
        {
            users = new ConcurrentDictionary<string, User>();

            // Check the status of users.ul. If it exists, fill the user dictionary with its data.
            if (File.Exists("users.ul"))
            {
                // Usernames are listed first in users.ul, and are followed by a period and then the password associated with that username.
                StreamReader reader = new StreamReader("users.ul");
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] splitted = line.Split('.');
                    string un = splitted[0];
                    string pass = splitted[1];

                    User u = new User(un, pass);

                    // Add the username and User object to the dictionary
                    users.TryAdd(un, u);
                }

                reader.Close();
            }

            // If users.ul doesn't exist, create it and add a default user named "admin".
            else
            {
                try
                {
                    StreamWriter writer = new StreamWriter("users.ul");
                    writer.WriteLine("admin.password");
                    writer.Close();

                    User u = new User("admin", "password");

                    users.TryAdd("admin", u);
                }
                catch(IOException e)
                {
                    System.Console.WriteLine(e);
                }
            }
        }

        public void Add(string u, User user)
        // Add a user to the user list.
        // If the user is already in the list, TryAdd won't do anything.
        {
            users.TryAdd(u, user);
        }

        public User GetUser(string uname)
        // Returns the user object associated with the username "uname".
        // Returns null if this user doesn't exist.
        {
            User temp;
            

            if (users.TryGetValue(uname, out temp))
                return temp;

            return null;
        }

        public string GetPassword(string uname)
        // Returns the password associated with the user named "uname".
        // Returns null if the requested username isn't stored in the users Dictionary.
        {
            User temp;
            users.TryGetValue(uname, out temp);

            if (temp == null)
            {
                return null;
            }
            else
            {
                return temp.Password;
            }
        }

        public void Print()
        // Print the userList for debugging purposes.
        {
            foreach (var entry in users)
            {
                System.Console.WriteLine("[{0} {1}]", entry.Key, entry.Value.Password);
            }
        }
    }
}
