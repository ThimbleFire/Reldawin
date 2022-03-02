﻿using ReldawinServerMaster;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ReldawinServerMaster
{
    public class Tile
    {
        public static byte GetTypeByHeight( float height )
        {
            for ( byte i = 0; i < XMLDevice.tileinfo.Count; i++ )
            {
                TETile tile = XMLDevice.tileinfo[i];

                if ( tile == null )
                    continue;

                if ( height > tile.minHeight && height <= tile.maxHeight )
                    return tile.id;
            }

            return 0;
        }
    }

    public class Doodad
    {
        public int tileX, tileY;
        public int type;

        public Doodad( int type, int tileX, int tileY )
        {
            this.tileX = tileX;
            this.tileY = tileY;
            this.type = type;
        }

        public Doodad( int type )
        {
            this.type = type;
        }
    }
    
    class World
    {
        public static int Width = 64;
        public static int Height = 64;
        public static int Seed = 1596423667;
        public static double Scale = 0.5d;
        public static int RNGSeed = int.MinValue;
        public static byte[,] tiles;
        public static Random random { get; set; }
        public const byte DoodadsPerChunk = 25;
        public const byte DaysInGamePerRealDays = 8;

        public delegate void TickEventHandler(  );
        public static event TickEventHandler OnTick;
        
        private static byte TickInterval = 4;
        private static byte TickCount = 0;

        public static bool Setup()
        {            
            object[] data = CommonSQL.GetMapData();

            if ( data == null )
            {
                return false;
            }

            Seed = Convert.ToInt32( data[0] );
            Width = Convert.ToInt32( data[1] );
            Height = Convert.ToInt32( data[2] );
            Scale = Convert.ToDouble( data[3] );
            RNGSeed = Convert.ToInt32( data[4] );
            tiles = MapGen.GenerateTiles( Width, Height, Scale, Seed );

            random = new Random( RNGSeed );

            Thread t = new Thread( new ThreadStart( ServerTick ) );
            t.Start();

            return true;
        }

        public static void ServerTick()
        {
            while ( true )
            {
                Thread.Sleep( 2400 );

                OnTick?.Invoke();
                
                if( TickCount++ >= TickInterval)
                {
                    TickCount = 0;
                }
            }
        }

        public static void LoadNewMap()
        {
            tiles = MapGen.GenerateTiles( Width, Height, Scale, Seed );
        }

        public static string GetChunkData( int chunkX, int chunkY )
        {
            // get tile range of tiles
            int xStart = chunkX * 15;
            int yStart = chunkY * 15;
            int xLim = xStart + 17;
            int yLim = yStart + 17;

            string data = string.Empty;

            for ( int y = yStart; y < yLim; y++ )
            {
                for ( int x = xStart; x < xLim; x++ )
                {
                    data += (byte)tiles[x, y];
                }
            }

            return data;
        }

        public static List<Doodad> GetDoodads( int chunkX, int chunkY )
        {
            List<Doodad> doodads = new List<Doodad>();
            Random rand = new Random( RNGSeed );

            // Search SQL database for existing doodads within {chunkX, chunkY}
            // If no doodads exist, generate them.
            // If doodads do exist, fetch and return them.
            List<object> doodadsInChunk = CommonSQL.GetChunkDoodads( chunkX, chunkY );

            if ( doodadsInChunk == null )
            {
                // get tile range of tiles
                int xStart = chunkX * 15;
                int yStart = chunkY * 15;
                int xLim = xStart + 15;
                int yLim = yStart + 15;

                int doodadCountInThisChunk = rand.Next( 0, World.DoodadsPerChunk );

                for ( int i = 0; i < doodadCountInThisChunk; i++ )
                {
                    int x = World.random.Next( xStart, xLim );
                    int y = World.random.Next( yStart, yLim );

                    while ( doodads.Find( z => z.tileX == x && z.tileY == y ) != null )
                    {
                        x = World.random.Next( xStart, xLim );
                        y = World.random.Next( yStart, yLim );
                    }

                    Doodad doodad = (Doodad)XMLDevice.tileinfo[tiles[x, y]].GetDoodad;

                    if ( doodad == null )
                        continue;

                    doodad.tileX = x;
                    doodad.tileY = y;
                    doodads.Add( doodad );
                    CommonSQL.InsertDoodad( doodad );
                }
            }
            else
            {
                for ( int i = 0; i < doodadsInChunk.Count; i += 3 )
                {
                    int type = Convert.ToInt32(doodadsInChunk[i]);
                    int x = Convert.ToInt32( doodadsInChunk[i + 1] );
                    int y = Convert.ToInt32( doodadsInChunk[i + 2] );
                    doodads.Add( new Doodad( type, x, y ) );
                }
            }

            return doodads;
        }
    }
}
