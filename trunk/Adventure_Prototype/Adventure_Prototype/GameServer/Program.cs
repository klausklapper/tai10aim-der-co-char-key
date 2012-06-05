﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lidgren.Network;

/*
 * Server Application - Keep updated with the game itself!
 * 
 */





namespace GameServer
{

	enum ServerStatus
	{
		LOBBY,
		GAME
	}

	enum PacketTypes
	{
		LOGIN,
		ENV_INFO,
		PLAYER_INFO,
		BROADCAST,
		LOBBY,
		GAME_STATE_CHANGED
	}



    class Program
    {

        static NetServer Server;				//The Server Object itself
        static NetPeerConfiguration Config;		//Used to store the server config
		static ServerStatus serverStatus;		//Simpleton to check whether we're in game or lobby.




		static void print(String text)
		{
			Console.WriteLine(DateTime.Now.ToString() + ": " + text);
		}



        static void Main(string[] args)
        {
			//Boot in Lobby mode
			serverStatus = ServerStatus.LOBBY;

			//Setup Configuration
            Config = new NetPeerConfiguration("CoCharKey");	//Make sure the Application ID is the same on Server & Game!
            Config.Port = 14242;
            Config.MaximumConnections = 2;
            Config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
			Config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            

            // Create new Server, load in config
			try
			{
				Server = new NetServer(Config);
				Server.Start();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Print(ex.ToString());
				return;
			}
            

			print("Server started!");

			DateTime time = DateTime.Now;							//Used for time measurement
			TimeSpan interval = new TimeSpan(0, 0, 0, 0, 30);		//This is the interval for our network communication, currently 30ms
            NetIncomingMessage inc;									//This holds incoming messages
			List<Character> GameWorldState = new List<Character>();	//Collection of connected players

			print("Bouncing on port " + Server.Configuration.Port.ToString());
			print("Server is up and running, we are ready to roll...");


			NetOutgoingMessage ret = Server.CreateMessage();
			ret.Write(true);
			Server.SendDiscoveryResponse(ret, new System.Net.IPEndPoint(0000,Server.Configuration.Port));


            while (true)
             {
				 //System.Diagnostics.Debug.Print(DateTime.Now.ToString());

				 System.Threading.Thread.Sleep(50);
				

                if ((inc = Server.ReadMessage()) != null)
                {

                    switch (inc.MessageType)
                    {
						/*##################################################
						 * LOGIN CONNECTION APPROVAL
						 *##################################################
						 */
                        case NetIncomingMessageType.ConnectionApproval:
                            if (inc.ReadByte() == (byte)PacketTypes.LOGIN)
                            {
								print(inc.SenderEndpoint.Address.ToString() + " trying to connect...");

								bool alreadyConnected = false;

								foreach (Character c in GameWorldState)
								{
									if (c.Connection == inc.SenderConnection)
									{
										print(inc.SenderEndpoint.Address.ToString() + " is already connected!");
										alreadyConnected = true;
										break;
									}
								}

								if (alreadyConnected)
								{
									break;
								}



                                inc.SenderConnection.Approve();

								//Initialize Player here
								Character tmp = new Character(inc.ReadString(), inc.SenderConnection);
								tmp.ready = false;
								GameWorldState.Add(tmp);

                                NetOutgoingMessage outmsg = Server.CreateMessage();

								outmsg.Write((byte)PacketTypes.LOGIN);
								outmsg.Write(Server.Connections.Length);
								outmsg.Write(tmp.Name);

                                Server.SendMessage(outmsg, inc.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);
                            }

                            break;








						/*##################################################
						 * PING!
						 *##################################################
						 */
						case NetIncomingMessageType.DiscoveryRequest :

							System.Diagnostics.Debug.Print(inc.LengthBytes.ToString());
							print(inc.SenderEndpoint.Address.ToString() + " pinged me!");
							NetOutgoingMessage outmsg2 = Server.CreateMessage();
							outmsg2.Write("Speak friend and enter");
							Server.SendDiscoveryResponse(outmsg2, inc.SenderEndpoint);
							break;





						case NetIncomingMessageType.ErrorMessage:
							System.Diagnostics.Debug.Print("Error Message, Length :" + inc.LengthBytes.ToString() + " Bytes");
							System.Diagnostics.Debug.Print(inc.ReadString());
							System.Diagnostics.Debug.Print("------------------------------------------------------------------");
							break;


						case NetIncomingMessageType.DebugMessage:
							System.Diagnostics.Debug.Print("Debug Message, Length :" + inc.LengthBytes.ToString() + " Bytes");
							System.Diagnostics.Debug.Print(inc.ReadString());
							System.Diagnostics.Debug.Print("------------------------------------------------------------------");
							break;


						case NetIncomingMessageType.WarningMessage:
							System.Diagnostics.Debug.Print("Warning Message, Length :" + inc.LengthBytes.ToString() + " Bytes");
							System.Diagnostics.Debug.Print(inc.ReadString());
							System.Diagnostics.Debug.Print("------------------------------------------------------------------");
							break;







						//LOGOUT?!?! O____O
						case NetIncomingMessageType.UnconnectedData:
							inc.SenderConnection.Disconnect("Bye");
							break;





						/*##################################################
						 * MOTHERFLIPPIN STATUS CHANGES!
						 *##################################################
						 */
                        case NetIncomingMessageType.StatusChanged:

                            print(inc.SenderEndpoint.Address.ToString() + ": " + inc.SenderConnection.Status.ToString());

                            if (inc.SenderConnection.Status == NetConnectionStatus.Disconnected || inc.SenderConnection.Status == NetConnectionStatus.Disconnecting)
                            {
                                foreach (Character cha in GameWorldState)
                                {
                                    if (cha.Connection == inc.SenderConnection)
                                    {
                                        GameWorldState.Remove(cha);
                                        break;
                                    }
                                }
                            }
                            break;



						/*##################################################
						 * DATA!!!!!!!!!!!!!!!!
						 *##################################################
						 */
						case NetIncomingMessageType.Data:

							byte packageID = inc.ReadByte();

							if ((PacketTypes)packageID == PacketTypes.LOBBY)
							{
								String name = inc.ReadString();
								foreach (Character c in GameWorldState)
								{
									if (c.Connection == inc.SenderConnection && c.Name == name)
									{
										c.Name = name;
										c.ready = inc.ReadBoolean();
										print(c.Name + " is " + c.ready.ToString());
										break;
									}
								}
							}


							if ((PacketTypes)packageID == PacketTypes.GAME_STATE_CHANGED)
							{
								serverStatus = (ServerStatus)inc.ReadByte();

								NetOutgoingMessage msg = Server.CreateMessage();
								msg.Write((byte)PacketTypes.GAME_STATE_CHANGED);
								msg.Write((byte)serverStatus );

								Server.SendMessage(msg, Server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
							}

							break;




                        default:
                            //Nothing to do here *swoosh*
							//print("Found a " + inc.MessageType.ToString());
							//print("This aint the MessageType we're looking for. Move along...");
							System.Diagnostics.Debug.Print(inc.MessageType.ToString() + inc.LengthBytes.ToString() );
                            break;
                    }
                }






                /*##################################################
				* BROADCAST
				*##################################################
				*/
                if ((time + interval) < DateTime.Now)
                {
					if (serverStatus == ServerStatus.LOBBY)
					 {
						NetOutgoingMessage msg = Server.CreateMessage();
						Config.EnableMessageType(NetIncomingMessageType.Data);

						//Write byte
						msg.Write((byte)PacketTypes.LOBBY);

						//Number of players
						msg.Write(Server.Connections.Length);

						//Player Names
						foreach (Character c in GameWorldState)
						{
							msg.Write(c.Name);
							msg.Write(c.ready);
						}

						Server.SendMessage(msg, Server.Connections, NetDeliveryMethod.ReliableOrdered, 0);

						continue;
					}

                    if (Server.ConnectionsCount != 0)
                    {
                        NetOutgoingMessage outmsg = Server.CreateMessage();

                        // Write byte
                        outmsg.Write((byte)PacketTypes.BROADCAST);

                        // Write Int
                        outmsg.Write(GameWorldState.Count);

                        // Iterate throught all the players in game
                        foreach (Character ch2 in GameWorldState)
                        {

                            // Write all properties of character, to the message
                            outmsg.WriteAllProperties(ch2);
                        }

                        // Message contains
                        // byte = Type
                        // Int = Player count
                        // Character obj * Player count

                        // Send messsage to clients ( All connections, in reliable order, channel 0)
                        Server.SendMessage(outmsg, Server.Connections, NetDeliveryMethod.ReliableOrdered, 0);
                    }
                    // Update current time
                    time = DateTime.Now;
                }




                // Lets not overcompensate much...
            }
        }
    }
}
