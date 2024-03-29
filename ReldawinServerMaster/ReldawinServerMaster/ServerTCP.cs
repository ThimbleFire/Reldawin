﻿using Bindings;
using System.Net;
using System.Net.Sockets;

namespace ReldawinServerMaster
{
    internal class ServerTCP
    {
        public static Client[] clients;
        public static Socket serverSocket;

        public static void AnnounceDisconnect( int index, int ID ) {
            List<Client> clientList = FetchOtherClients( index );

            if( clientList.Count == 0 )
                return;

            using( PacketBuffer buffer = new PacketBuffer( Packet.Disconnect ) ) {
                buffer.WriteInteger( ID );

                foreach( Client client in clientList )
                    SendDataTo( client.index, buffer.ToArray() );
            }
        }

        public static void ChangeClientPosition( int index, int x, int y ) {
            // set the coordinates on the server
            clients[index].MovePosition( x, y );

            // set the coordinates on the database
            SQLReader.SetEntityCoordinates( x, y, clients[index].properties.ID );
        }

        public static void ConfirmStartInteract( int index, int id ) {
            using( new DebugTimer( clients[index].properties.Username + " ConfirmStartInteract" ) ) {
                List<Client> clientList = FetchOtherClients( index );

                if( clientList.Count == 0 )
                    return;

                using( PacketBuffer buffer = new PacketBuffer( Packet.StartInteract ) ) {
                    buffer.WriteInteger( id );

                    foreach( Client client in clientList )
                        SendDataTo( client.index, buffer.ToArray() );
                }
            }
        }

        public static void Harvest( int index, int yieldItemID ) {
            using( new DebugTimer( clients[index].properties.Username + " Harvest" ) ) {
                using( PacketBuffer buffer = new PacketBuffer( Packet.YieldInteract ) ) {
                    buffer.WriteInteger( yieldItemID );

                    SendDataTo( index, buffer.ToArray() );
                }
            }
        }

        public static void InitializeClient( int index, Vector2Int coordinates, int id ) => clients[index].Setup( coordinates, id );

        public static void Interrupt( int index, bool includeSender ) {
            using( new DebugTimer( clients[index].properties.Username + " Interrupt" ) ) {
                // get other players
                List<Client> clientList = FetchOtherClients( index );

                if( includeSender ) {
                    // add the player character calling the event
                    clientList.Add( clients[index] );
                }

                using( PacketBuffer buffer = new PacketBuffer( Packet.StopInteract ) ) {
                    buffer.WriteInteger( clients[index].properties.ID );

                    foreach( Client client in clientList )
                        SendDataTo( client.index, buffer.ToArray() );
                }
            }
        }

        public static void ReturnDoesUserExist( int index, bool result ) {
            using( new DebugTimer( clients[index].properties.Username + " ReturnDoesUserExist" ) ) {
                using( PacketBuffer buffer = new PacketBuffer( Packet.DoesUserExist ) ) {
                    buffer.WriteBoolean( result );
                    SendDataTo( index, buffer.ToArray() );
                }
            }
        }

        public static void SendAccountCreateFail( int index ) {
            using( new DebugTimer( "? SendAccountCreateFail" ) ) {
                using( PacketBuffer buffer = new PacketBuffer( Packet.Account_Create_Fail ) ) {
                    SendDataTo( index, buffer.ToArray() );
                }
            }
        }

        public static void SendAccountCreateSuccess( int index ) {
            using( new DebugTimer( "? SendAccountCreateSuccess" ) ) {
                using( PacketBuffer buffer = new PacketBuffer( Packet.Account_Create_Success ) ) {
                    SendDataTo( index, buffer.ToArray() );
                }
            }
        }

        public static void SendChunkDataToPlayer( int index, int chunkX, int chunkY ) {
            using( new DebugTimer( clients[index].properties.Username + " SendChunkDataToPlayer" ) ) {

                string data = MapData.GetChunk(chunkX, chunkY);

                SceneObjects[] sceneObjects = SQLReader.GetSceneObjectsInChunk(chunkX, chunkY);

                using( PacketBuffer buffer = new PacketBuffer( Packet.Load_Chunk ) ) {
                    buffer.WriteInteger( chunkX );
                    buffer.WriteInteger( chunkY );
                    buffer.WriteString( data );
                        buffer.WriteInteger( sceneObjects != null ? sceneObjects.Length : 0 );
                        for( int i = 0; i < sceneObjects.Length; i++ ) {
                            buffer.WriteInteger( sceneObjects[i].Type );
                            buffer.WriteInteger(sceneObjects[i].CoordinateX);
                            buffer.WriteInteger(sceneObjects[i].CoordinateY);
                        }

                    SendDataTo( index, buffer.ToArray() );
                }
            }
            return;
        }

        public static void SendChunkDoodadsToPlayer( int index, int chunkX, int chunkY ) {
            using( new DebugTimer( clients[index].properties.Username + " SendChunkDoodadsToPlayer" ) ) {
                //List<Doodad> doodads = World.GetDoodads( chunkX, chunkY );

                using( PacketBuffer buffer = new PacketBuffer( Packet.Load_Doodads ) ) {
                    buffer.WriteInteger( chunkX );
                    buffer.WriteInteger( chunkY );
                    buffer.WriteInteger( /*doodads.Count*/ 0 );

                    //foreach( Doodad doodad in doodads ) {
                    //    buffer.WriteByte( ( byte )doodad.type );
                    //    buffer.WriteInteger( doodad.tileX );
                    //    buffer.WriteInteger( doodad.tileY );
                    //}

                    SendDataTo( index, buffer.ToArray() );
                }
            }
        }

        public static void SendConnectionOK( int index ) {
            using( new DebugTimer( "SendConnectionOK" ) ) {
                using( PacketBuffer buffer = new PacketBuffer( Packet.ConnectionOK ) ) {
                    SendDataTo( index, buffer.ToArray() );
                    Console.WriteLine( "[ServerTCP] " + "Send ConnectionOK" );
                }
            }
        }

        public static void SendLoginFail( int index, string args ) {
            using( new DebugTimer( "? SendLoginFail" ) ) {
                using( PacketBuffer buffer = new PacketBuffer( Packet.Account_Login_Fail ) ) {
                    buffer.WriteString( args );
                    SendDataTo( index, buffer.ToArray() );
                    Console.WriteLine( "[ServerTCP] " + args );
                }
            }
        }

        public static void SendLoginSuccess( int index, int id, string username ) {
            using( new DebugTimer( username + " SendLoginSuccess" ) ) {
                using( PacketBuffer buffer = new PacketBuffer( Packet.Account_Login_Success ) ) {
                    buffer.WriteInteger( id );
                    SendDataTo( index, buffer.ToArray() );
                }

                clients[index].properties.Username = username;

                // Alert other players of other players logging in

                using( PacketBuffer buffer = new PacketBuffer( Packet.OtherPlayerCharacterLoggedIn ) ) {
                    buffer.WriteString( username );

                    Vector2Int result = SQLReader.GetEntityCoordinates( id );

                    if( result == null ) {
                        Console.WriteLine( "[ServerTCP] [DbError] Could not get {0}'s coordinates.", username );
                        return;
                    }

                    // Send players the logging-in players coordinates so they can decide whether they're worth loading in their game
                    buffer.WriteInteger( result.x );
                    buffer.WriteInteger( result.y );
                    buffer.WriteInteger( id );

                    // Tell all the players another player has logged in.
                    for( int i = 0; i < clients.Length; i++ ) {
                        // Player clients are of course not null
                        if( clients[i].loggedIn != false ) {
                            // And we don't need to tell <index> since they're the player logging in
                            if( clients[i].index != index ) {
                                SendDataTo( clients[i].index, buffer.ToArray() );
                            }
                        }
                    }
                }
            }
        }

        public static void SendMoveQueryToAllPlayers( int index, int ID, float pointX, float pointY ) {
            using( new DebugTimer( clients[index].properties.Username + " SendMoveQueryToAllPlayers" ) ) {
                List<Client> clientList = FetchOtherClients( index );

                // If there are no other players to tell our position has changed, forget it.
                if( clientList.Count == 0 )
                    return;

                using( PacketBuffer buffer = new PacketBuffer( Packet.AnnounceMovementToNearbyPlayers ) ) {
                    buffer.WriteInteger( ID );
                    buffer.WriteFloat( pointX );
                    buffer.WriteFloat( pointY );
                    //temporarily removed hasInventorySpace on client and server since inventory mechanics have changed
                    //buffer.WriteBoolean( clients[index].properties.items.Count < 20 );

                    foreach( Client client in clientList )
                        SendDataTo( client.index, buffer.ToArray() );
                }
            }
        }

        public static void SendOtherPlayerCharacterListRequest( int index ) {
            using( new DebugTimer( clients[index].properties.Username + " SendOtherPlayerCharacterListRequest" ) ) {
                List<Client> clientList = FetchOtherClients( index );

                using( PacketBuffer buffer = new PacketBuffer( Packet.OtherPlayerCharacterListRequest ) ) {
                    buffer.WriteInteger( clientList.Count );

                    if( clientList.Count > 0 ) {
                        foreach( Client client in clientList ) {
                            buffer.WriteString( client.properties.Username );
                            buffer.WriteInteger( client.properties.Position.x );
                            buffer.WriteInteger( client.properties.Position.y );
                            buffer.WriteInteger( client.properties.ID );
                            buffer.WriteBoolean( client.properties.Running );
                        }
                    }
                    SendDataTo( index, buffer.ToArray() );
                }
            }
        }

        public static void SendPingTest( int index ) {
            using( PacketBuffer buffer = new PacketBuffer( Packet.PingTest ) ) {
                SendDataTo( index, buffer.ToArray() );
            }
        }

        public static void SendToggleRunningToAllPlayers( int index, int ID ) {
            using( new DebugTimer( clients[index].properties.Username + " SendToggleRunningToAllPlayers" ) ) {
                clients[index].properties.Running = !clients[index].properties.Running;

                List<Client> clientList = FetchOtherClients( index );

                if( clientList.Count == 0 )
                    return;

                using( PacketBuffer buffer = new PacketBuffer( Packet.ToggleRunning ) ) {
                    buffer.WriteInteger( ID );

                    foreach( Client client in clientList )
                        SendDataTo( client.index, buffer.ToArray() );
                }
            }
        }

        public static void SetupServer() {
            clients = new Client[Log.MAX_PLAYERS];
            serverSocket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

            serverSocket.Bind( new IPEndPoint( IPAddress.Any, 5555 ) );
            serverSocket.Listen( Log.BUFFER_PLAYERS );
            serverSocket.BeginAccept( new AsyncCallback( AcceptCallback ), null );
            for( int i = 0; i < Log.MAX_PLAYERS; i++ ) {
                clients[i] = new Client();
            }

        }

        public static void TickResult( int index, int yieldItemID ) => Harvest( index, yieldItemID );

        internal static void PlaceItemInInventory( byte index, List<byte> data ) {
            using( new DebugTimer( clients[index].properties.Username + "PlaceItemInInventory" ) ) {
                using( PacketBuffer buffer = new PacketBuffer( Packet.PlaceItemInInventory ) ) {
                    buffer.WriteBytes( data.ToArray() );
                    SendDataTo( index, buffer.ToArray() );
                }
            }
        }

        internal static void SendCoordinatesOnDatabase( int index, Vector2Int coordinates ) {
            using( PacketBuffer buffer = new PacketBuffer( Packet.RequestSpawn ) ) {
                buffer.WriteInteger( coordinates.x );
                buffer.WriteInteger( coordinates.y );
                SendDataTo( index, buffer.ToArray() );
            }
        }

        private static void AcceptCallback( IAsyncResult ar ) {
            // Could this line below be made better?
            Socket socket = serverSocket.EndAccept( ar );
            serverSocket.BeginAccept( new AsyncCallback( AcceptCallback ), null );

            for( int i = 0; i < Log.MAX_PLAYERS; i++ ) {
                if( clients[i].socket == null ) {
                    clients[i].socket = socket;
                    clients[i].index = i;
                    clients[i].ip = socket.RemoteEndPoint.ToString();
                    clients[i].StartClient();
                    Console.WriteLine( "[ServerTCP] " + Log.SERVER_LOBBY_JOIN, i );
                    SendConnectionOK( i );
                    return;
                }
            }
        }
        private static List<Client> FetchOtherClients( int index ) {
            List<Client> clientList = new List<Client>();

            // Get a list of all the clients

            foreach( Client client in clients )
                if( client.loggedIn != false )
                    if( client.index != index )
                        clientList.Add( client );

            return clientList;
        }

        private static void SendDataTo( int index, byte[] data ) {
            byte[] sizeInfo = new byte[4];
            sizeInfo[0] = ( byte )data.Length;
            sizeInfo[1] = ( byte )( data.Length >> 08 );
            sizeInfo[2] = ( byte )( data.Length >> 16 );
            sizeInfo[2] = ( byte )( data.Length >> 24 );

            clients[index].socket.Send( sizeInfo );
            clients[index].socket.Send( data );
        }
    }
}