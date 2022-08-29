/*
 * IMServer class.
 * Implements all server functionality, as well as the Main method.
 * 
 * Author: Stephen Schmith
 * Last Modified: 3/18/2013
 * Created in Microsoft Visual Studio Express 2012
 */

using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections;
using System.IO;
using System.Net;

namespace IMServer
{
    // Main class for the Instant Messenger Server. Listens for and communicates with the Instant Messenger Client.
    class IMServer
    {
        private TcpListener tcpListener;    // Listens for incoming connections on the port specified in port.txt.
        private Thread listenThread;        // This thread spawns new client threads every time a client connects to the server.
        private UserList userList;          // Maintains a list of all users. Constructed from users.ul
        private const int defaultIncomingPort = 52434;

        public IMServer()
        // Instantiates the IMServer by initalizing a UserList. 
        // It then starts a new thread which listens for incoming connections.
        {
            // Get the user list.
            int incomingPort = defaultIncomingPort;
            userList = new UserList();

            if (File.Exists("port.txt"))
            {
                // Set the server port number from port.txt.
                try
                {
                    incomingPort = Convert.ToInt32(File.ReadAllText("port.txt"));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            } 
            else
            {
                Console.WriteLine("Listening on default port...");
            }

            try
            {
                // Listen for incoming connections on all network interfaces.
                this.tcpListener = new TcpListener(IPAddress.Any, incomingPort);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }

        private void ListenForClients()
        // Spawns new threads for each new client connection.
        {
            // Start listening for incoming connections
            this.tcpListener.Start();
            System.Console.WriteLine("Server started.\n\n Accepting connections...");

            while (true)
            {
                TcpClient client = this.tcpListener.AcceptTcpClient();

                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        private void HandleClientComm(object client)
        // This method handles communication with individual clients.
        // It will continue listening to a client until it receives an EXIT REQUEST or until the connection is severed.
        {
            TcpClient tcpClient = (TcpClient)client;

            // Get the input stream from the client
            NetworkStream clientStream = tcpClient.GetStream();

            // Create a StreamWriter for talking to the client
            StreamWriter writer = new StreamWriter(clientStream);
            writer.AutoFlush = true;

            // Store client data in an array of bytes
            byte[] message = new byte[4096];

            // Count the number of bytes read
            int bytesRead; 

            // Run this thread while listening to the client.
            bool listening = true;

            while (listening)
            {
                bytesRead = 0;

                try
                {
                    // Read data from the client.
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                // Convert the client data to a string and print it to the console.
                ASCIIEncoding encoder = new ASCIIEncoding();
                string clientMessage = encoder.GetString(message, 0, bytesRead).Trim();

                // Parse for the message prefix and content.
                int prefixEnd = clientMessage.IndexOf("=") + 1;
                String msgPrefix = clientMessage.Substring(0, prefixEnd);
                String msgContent = clientMessage.Substring(prefixEnd, clientMessage.Length - (prefixEnd));

                System.Console.WriteLine("Prefix: " + msgPrefix);
                System.Console.WriteLine("Content: " + msgContent);

                // When listening is set to false by delegateMessage, this thread will close some resources and then exit.
                listening = delegateMessage(msgPrefix, msgContent, tcpClient, writer);
            }

            writer.Close();
            tcpClient.Close();
        }

        private bool delegateMessage(string msgPrefix, string msgContent, TcpClient tcpClient, StreamWriter writer)
        /* delegateMessage delegates messages from the client to other methods based on the message prefix.
         * 
         * Postcondition: Returns false if it receives an EXIT REQUEST. Otherwise, it returns true.
         */
        {
                switch(msgPrefix)
                {
                    case "LOGIN REQUEST=":
                        HandleLoginRequest(msgContent, writer, tcpClient);

                        break;

                    case "FRIENDS LIST REQUEST=":
                        HandleFriendsListRequest(msgContent, writer, tcpClient);

                        break;

                    case "SEND MESSAGE REQUEST=":
                        HandleSendMessageRequest(msgContent, writer);

                        break;

                    case "ADD FRIEND REQUEST=":
                        // Content format: clientUsername.friendToBeAdded
                        string[] s = msgContent.Split('.');

                        // The user adding a friend must exist to send an Add Friend Request.
                        User x = userList.GetUser(s[0]);

                        if (userList.GetUser(s[1]) == null)
                        // Friend does not exist
                        {
                            String toClient = "ADD FRIEND REQUEST=FAILED.";
                            toClient += s[1];

                            try
                            {
                                writer.WriteLine(toClient);
                            }
                            catch (Exception e)
                            {
                                System.Console.WriteLine(e);
                            }
                        }
                        
                        // Friend exists. Add them to this user's friends list.
                        else
                        {
                            x.AddFriend(s[1]);
                            String toClient = "ADD FRIEND REQUEST=SUCCESS";

                            try
                            {
                                writer.WriteLine(toClient);
                            }
                            catch (Exception e)
                            {
                                System.Console.WriteLine(e);
                            }
                        }

                        break;

                    case "REMOVE FRIEND REQUEST=":
                        // Content format: clientUsername.friendToBeRemoved
                        s = msgContent.Split('.');
                        x = userList.GetUser(s[0]);

                        string toC = "REMOVE FRIEND REQUEST=";

                        if (x.RemoveFriend(s[1])) 
                        {
                            toC += "SUCCESSFUL";
                        }
                        else
                        {
                            toC += "FAILED";
                        }

                        try
                        {
                            writer.WriteLine(toC);
                        }
                        catch (Exception e)
                        {
                            System.Console.WriteLine(e);
                        }

                        break;

                    case "EXIT REQUEST=":
                        // The message content in an EXIT REQUEST is just the username.
                        User quitting = userList.GetUser(msgContent);
                        quitting.Online = false;
                        
                        // Tell the client thread to exit.
                        return false;

                    default:
                        System.Console.WriteLine("Client did not send valid message.");
                        break;
                }
                
                // Tell the client thread to continue.
                return true;
        }

        
        private void HandleLoginRequest(string msgContent, StreamWriter writer, TcpClient tcpClient)
        /* Handle a login request from the client.
         * msgContent is parsed for a username and password, which are both checked against the user list.
         * If the username or password is invalid, the client is prompted to try again.
         * Otherwise, the client is connected to the server.
         * 
         * Precondition: None
         * Postcondition: If the password and username combination in msgContent is valid, the user calling this method is logged in.
         *                Otherwise, the server informs the client to prompt the user for more information.
         */
        {
            System.Console.WriteLine("Received LOGIN REQUEST from " + ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString());
            string clientMsg;

            // Pull out the username and password
            int separator = msgContent.IndexOf(".");
            string u = msgContent.Substring(0, separator);
            string p = msgContent.Substring(separator + 1, msgContent.Length - (separator + 1));

            string retrievedPass = userList.GetPassword(u);

            if (retrievedPass == null)
            {
                clientMsg = "LOGIN REQUEST=RETRY";
            }
            else if (p == retrievedPass)
            {
                clientMsg = "LOGIN REQUEST=SUCCESS.";

                // Get this user's User object (it must exist if this statement has been reached)
                User loggingIn = userList.GetUser(u);

                // Set the TcpClient
                loggingIn.Client = tcpClient;

                // Set the user to be online
                loggingIn.Online = true;

                // Attach messages delivered to this user while they were offline
                ArrayList temp = loggingIn.GetMessageLog();

                for (int i = 0; i < temp.Count; i++)
                {
                    clientMsg += ((string)temp[i]) + ".";
                }

                // Clear the messagelog
                loggingIn.ClearMessageLog();
            }
            else
            {
                clientMsg = "LOGIN REQUEST=RETRY";
            }

            try
            {
                writer.WriteLine(clientMsg);
                System.Console.WriteLine("Message sent.");
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }          
        }

 
        private void HandleFriendsListRequest(string msgContent, StreamWriter writer, TcpClient client)
        /*
         * Precondition: The user calling this method must exist, and must have logged in successfully.
         * Postcondition: A message containing the user's friends list, along with his or her friends' statuses, is sent to the client.
         * 
         * Handle a friends list request from the client.
         * msgContent is parsed for a username. The friends list is then retrieved from the associated User in the user list.
         * If the user in msgContent has no friends, the server sends a "null" message back to the client.
         */
         {
            User t = userList.GetUser(msgContent);
            ArrayList friends = t.FriendsList;

            // Tell the client to show this friends list to the user.
            string toClient = "SHOW FRIENDS LIST=";

            if (friends.Count == 0)
            {
                toClient += "null";

                try
                {
                    writer.WriteLine(toClient);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e);
                }
            }
            else
            {
                User temp;
                for (int i = 0; i < friends.Count; i++)
                {
                    // Build the friends list message
                    temp = userList.GetUser(((User)friends[i]).Username);

                    // Check online status of each friend. If one disconnected without calling /exit, change their status.
                    // Check to see if the client is null; this means the user has never signed on before (in which case their Online property will be false).
                    if (temp.Client != null)
                    {
                        if (temp.Client.Connected == false)
                        {
                            temp.Online = false;
                        }
                    }

                    toClient += temp.Username + "." + temp.Online + ".";
                }

                try
                {
                    writer.WriteLine(toClient);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e);
                }
            }
        }

        private void HandleSendMessageRequest(string msgContent, StreamWriter writer)
        /* This method delivers messages between two users. msgContent is parsed for destination, source, and content data.
         * If the destination exists, the server attempts to send the message.
         * 
         * Precondition: msgContent is in the form "destination.source.content" where destination and source are usernames,
         *               and content is a message from one user to another of arbitrary length.
         *               
         * Postcondition: If the destination is valid, the message stored in "content" is sent to it.
         *                If not, the source client is notified.
         */
        {
            string[] splitted = msgContent.Split(new Char[] { '.' }, 3);
            string destination = splitted[0];
            string source = splitted[1];
            string content = splitted[2];

            string toClient;

            User sendTo = userList.GetUser(destination);
            TcpClient destinationClient = null;     

            // If sendTo is null, the destination doesn't exist.
            if (sendTo == null)
            {
                toClient = "SEND MESSAGE REQUEST FAILED=" + destination;

                try
                {
                    writer.WriteLine(toClient);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e);
                }
            }
            else
            {
                try
                {
                    destinationClient = sendTo.Client;
                }

                // If the destination client has been disposed, that user is disconnected.
                catch (ObjectDisposedException)
                {
                    System.Console.WriteLine("User disconnected without calling /exit.");
                    sendTo.Online = false;

                    // Make sure destinationClient is null so the rest of the method handles it appropriately.
                    destinationClient = null;
                }

                // If destinationClient == null, the destination user is offline. 
                // The message is then stored in their messageLog until they log in again.
                if (destinationClient == null || !(sendTo.Online) || !(destinationClient.Connected))
                {
                    sendTo.InsertMessage(source, content);
                    sendTo.Online = false;

                    toClient = "SEND UMESSAGE FAILED=" + destination;

                    try
                    {
                        writer.WriteLine(toClient);
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine(e);
                    }
                }
                
                // If the user is online, send them the message.
                else
                {
                    NetworkStream newStream = null;

                    try
                    {
                        newStream = destinationClient.GetStream();
                    }
                    catch (ObjectDisposedException e)
                    {
                        // In the event a user disconnects during the time between the previous block and this one,
                        // an error will be produced. Leave this method; the message will be lost.
                        System.Console.WriteLine(e);

                        // Tell the client about this error.
                        writer.WriteLine("SEND UMESSAGE DISCONNECT=" + destination);

                        return;
                    }

                    StreamWriter writeMessage = new StreamWriter(newStream);
                    writeMessage.AutoFlush = true;

                    String destMessage = "INCOMING UMESSAGE=" + source + "." + content;

                    // This stream writer must stay open. 
                    // Closing it will sever the client's output stream; this behavior is built in to the StreamWriter class.
                    // The garbage collector will eventually finalize writeMessage anyway.
                    writeMessage.WriteLine(destMessage);
                }
            }
        }

        /// <summary>
        /// The main method instantiates one instance of an IMServer.
        /// </summary>
        /// <param name="args">Unused.</param>
        static void Main(string[] args)
        {
            System.Console.WriteLine("Starting the IM Server...");
            IMServer server = new IMServer();
        }
    }
}
