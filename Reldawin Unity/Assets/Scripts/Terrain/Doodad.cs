﻿using cakeslice;
using UnityEngine;

namespace LowCloud.Reldawin
{
    public class Doodad : MonoBehaviour
    {
        [System.Serializable]
        public struct Data
        {
            public Data( int type, Vector2Int tilePos, Vector2Int chunkIndex )
            {
                this.tilePositionInWorld = tilePos;
                this.tilePositionInChunk = new Vector2Int( tilePos.x % Chunk.Size, tilePos.y % Chunk.Size );
                this.chunkIndex = chunkIndex;
                this.type = type;
            }

            public Vector2Int tilePositionInChunk;
            public Vector2Int tilePositionInWorld;
            public Vector2Int chunkIndex;
            public int type;
        }

        [SerializeField] private new SpriteRenderer renderer;
        [SerializeField] private BoxCollider2D boxCollider2D;
        public Data data;

        public void Setup( Data d )
        {
            data = d;

            // setup sorting order so entities can walk behind and around
            renderer.sortingOrder = data.tilePositionInWorld.x * Chunk.Size + data.tilePositionInWorld.y;

            // setup world position
            transform.position = MyMath.CellToIsometric( d.tilePositionInWorld );

            // setup sprite
            renderer.sprite = SpriteLoader.GetDoodad(XMLLoader.GetDoodad(data.type).name);

            // setup collision size
            Vector2 colliderSize = renderer.sprite.bounds.size;
            boxCollider2D.size = colliderSize;

            // setup collision offset
            Vector2 halfSpriteSize = renderer.sprite.rect.size / 2;
            Vector2 offset = -( renderer.sprite.pivot - halfSpriteSize ) / 100;
            boxCollider2D.offset = offset;

            // enable the game object
            gameObject.SetActive( true );
        }

        private void OnMouseEnter()
        {
            string color = string.Empty;
            string action = string.Empty;

            DEDoodad d = XMLLoader.GetDoodad( data.type );

            switch ( d.interact )
            {
                case DEDoodad.Interact.NONE:
                    break;
                case DEDoodad.Interact.WOODCUTTING:
                    color = "<color=#FFFF00>";
                    action = "Cut down";
                    break;
                case DEDoodad.Interact.MINING:
                    color = "<color=#FFFF00>";
                    action = "Mine";
                    break;
                case DEDoodad.Interact.HANDS:
                    color = "<color=#FFFF00>";
                    action = "Gather";
                    break;
                case DEDoodad.Interact.DOOR:
                    color = "<color=#07F8E0>";
                    action = "Enter";
                    break;
            }
            Tooltip.p.MouseOver( action, d.name, color );
            Tooltip.p.gameObject.SetActive( true );
            //GetComponent<Outline>().enabled = true;
        }

        private void OnMouseExit()
        {
            Tooltip.p.gameObject.SetActive( false );
            //GetComponent<Outline>().enabled = false;
        }
    }
}