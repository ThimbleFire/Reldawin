﻿using System.Collections.Generic;
using UnityEngine;

namespace LowCloud.Reldawin
{
    public class SpriteLoader
    {
        // remember to rename doodads after sprite names in atlas.
        public static Dictionary<string, Vector2[]> tileUVMap;
        public static Dictionary<string, Sprite> doodadDictionary;
        public static Dictionary<string, Sprite> itemDictionary;

        public static Vector2[] GetEmpty
        {
            get
            {
                return tileUVMap["Empty"];
            }
        }

        /// Width and Height specify the dimensions of Template2.png
        public static void Setup( int spriteMapWidth, int spriteMapHeight )
        {
            tileUVMap = new Dictionary<string, Vector2[]>();
            doodadDictionary = new Dictionary<string, Sprite>();
            itemDictionary = new Dictionary<string, Sprite>();

            Sprite[] sprites = Resources.LoadAll<Sprite>( "Sprites/Enviroment/Terrain/Tile" );

            foreach ( Sprite s in sprites )
            {
                float left = s.rect.x / spriteMapWidth;
                float right = ( s.rect.x + s.rect.width ) / spriteMapWidth;

                float top = s.rect.y / spriteMapHeight;
                float bot = ( s.rect.y + s.rect.height ) / spriteMapHeight;

                float middleX = ( s.rect.x + ( s.rect.width / 2 ) ) / spriteMapWidth;
                float middleY = ( s.rect.y + ( s.rect.height / 2 ) ) / spriteMapHeight;

                Vector2 topMiddle = new Vector2( middleX, top );
                Vector2 botMiddle = new Vector2( middleX, bot );

                Vector2 rightMiddle = new Vector2( right, middleY );
                Vector2 leftMiddle = new Vector2( left, middleY );

                Vector2[] uvs = new Vector2[]
                {
                    topMiddle,
                    rightMiddle,
                    leftMiddle,
                    botMiddle
                };

                tileUVMap.Add( s.name, uvs );
            }

            sprites = Resources.LoadAll<Sprite>( "Sprites/Enviroment/Terrain/terrainDetails" );

            foreach ( Sprite s in sprites )
            {
                doodadDictionary.Add( s.name, s );
            }

            sprites = Resources.LoadAll<Sprite>( "Sprites/Interface/Buttons/icon_selected" );

            foreach ( Sprite s in sprites )
            {
                doodadDictionary.Add( s.name, s );
            }

            sprites = Resources.LoadAll<Sprite>( "Sprites/Interface/Items/items_32x32" );

            foreach ( Sprite s in sprites )
            {
                itemDictionary.Add( s.name, s );
            }
        }

        public static Vector2[] GetTileUVs( int type, Tile[] neighbours = null )
        {
            if ( type == 0 )
                return GetEmpty;

            if ( neighbours == null )
                return GetTile( XMLLoader.GetTile( type ).name + "_" + Random.Range( 0, 16 ) );

            string key = XMLLoader.GetTile( type ).name;

            if ( !IsSameType( type, neighbours[0] )
           && !IsSameType( type, neighbours[2] )
           && IsSameType( type, neighbours[1] )
           && IsSameType( type, neighbours[3] ) )
            {
                key += "_18";
                return GetTile( key );
            }

            if ( !IsSameType( type, neighbours[0] )
           && !IsSameType( type, neighbours[1] )
           && !IsSameType( type, neighbours[2] )
           && IsSameType( type, neighbours[3] ) )
            {
                key += "_24";
                return GetTile( key );
            }
            if ( IsSameType( type, neighbours[0] )
           && !IsSameType( type, neighbours[1] )
           && !IsSameType( type, neighbours[2] )
           && !IsSameType( type, neighbours[3] ) )
            {
                key += "_25";
                return GetTile( key );
            }

            if ( IsSameType( type, neighbours[2] )
             && !IsSameType( type, neighbours[5] )
             && !IsSameType( type, neighbours[6] ) )
            {
                key += "_59";
                return GetTile( key );
            }
            if ( IsSameType( type, neighbours[0] )
             &&  IsSameType( type, neighbours[1] )
             &&  IsSameType( type, neighbours[2] )
             &&  IsSameType( type, neighbours[3] )
             && !IsSameType( type, neighbours[4] )
             && !IsSameType( type, neighbours[5] ) )
            {
                key += "_60";
                return GetTile( key );
            }

            if ( !IsSameType( type, neighbours[3] ) && !IsSameType( type, neighbours[0] ) )
            {
                key += "_19";
                return GetTile( key );
            }
            if ( !IsSameType( type, neighbours[2] ) && !IsSameType( type, neighbours[3] ) && !IsSameType( type, neighbours[6] ) )
            {
                key += "_20";
                return GetTile( key );
            }
            if ( !IsSameType( type, neighbours[1] ) && !IsSameType( type, neighbours[2] ) )
            {
                key += "_21";
                return GetTile( key );
            }
            if ( !IsSameType( type, neighbours[0] ) && !IsSameType( type, neighbours[1] ) )
            {
                key += "_22";
                return GetTile( key );
            }
            if ( IsSameType( type, neighbours[2] ) && IsSameType( type, neighbours[3] ) && !IsSameType( type, neighbours[6] ) )
            {
                key += "_57";
                return GetTile( key );
            }
            if ( IsSameType( type, neighbours[1] ) && IsSameType( type, neighbours[2] ) && !IsSameType( type, neighbours[5] ) )
            {
                key += "_56";
                return GetTile( key );
            }
            if ( IsSameType( type, neighbours[0] ) && IsSameType( type, neighbours[3] ) && !IsSameType( type, neighbours[7] ) )
            {
                key += "_54";
                return GetTile( key );
            }
            if ( IsSameType( type, neighbours[0] ) && IsSameType( type, neighbours[1] ) && !IsSameType( type, neighbours[4] ) )
            {
                key += "_55";
                return GetTile( key );
            }
            if ( !IsSameType( type, neighbours[3] ) )
            {
                key += "_31";
                return GetTile( key );
            }
            if ( !IsSameType( type, neighbours[0] ) )
            {
                key += "_32";
                return GetTile( key );
            }
            if ( !IsSameType( type, neighbours[1] ) )
            {
                key += "_33";
                return GetTile( key );
            }
            if ( !IsSameType( type, neighbours[2] ) )
            {
                key += "_34";
                return GetTile( key );
            }

            return GetTile( key );
        }

        private static bool IsSameType( int type, Tile neightbour )
        {
            if ( neightbour == null )
                return false;

            if ( neightbour.TileType == type )
                return true;

            return false;
        }

        private static Vector2[] GetTile( string key )
        {
            if ( tileUVMap.ContainsKey( key ) )
            {
                return tileUVMap[key];
            }
            else
            {
                Debug.LogError( key + " isn't in the tileUVMap dictionary" );
                return tileUVMap["Void"];
            }
        }

        public static Sprite GetDoodad( string key )
        {
            if ( doodadDictionary.ContainsKey( key ) )
            {
                return doodadDictionary[key];
            }
            else
            {
                Debug.LogError( key + " isn't in the SpriteLoader Doodad Dictionary" );
                return doodadDictionary["Empty"];
            }
        }

        public static Sprite GetItem( string key )
        {
            if ( itemDictionary.ContainsKey( key ) )
            {
                return itemDictionary[key];
            }
            else
            {
                Debug.LogError( key + " isn't in the SpriteLoader Item Dictionary" );
                return itemDictionary["FlintKnife"];
            }
        }
    }
}