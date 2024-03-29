using System.Collections.Generic;
using UnityEngine;
public class ItemStats : MonoBehaviour
{
    public const string hexMagic = "<color=#4850B8>";
    public const string hexRare = "<color=#FFFF00>";
    public const string hexGray = "<color=#8A8A8A>";
    public const string hexRed = "<color=#FF0000>";
    public string Name { get { return string.Format( "{0}{1}{2}", Rarity == 0 ? hexGray : Rarity <= 3 ? hexMagic : hexRare, gameObject.name, "</color>" ); } }
    public byte Rarity { get { return (byte)( prefixes.Count + suffixes.Count ); } }
    public Type type { get { return itemBasics.type; } }
    public int MinDamage {
        get {
            Suffix s = suffixes.Find( x => x.type == GearStats.Suffix.Dmg_Phys_Min );
            Prefix p = prefixes.Find( x => x.type == GearStats.Prefix.Dmg_Phys_Percent );
            if( s != null && p != null ) {
                return (int)( ( itemBasics.value + s.value ) * ( p.value / 100.0f + 1 ) );
            }
            if( s != null ) {
                return (int)( itemBasics.value + s.value );
            }
            if( p != null ) {
                return (int)( itemBasics.value * ( p.value / 100.0f + 1 ) );
            }
            return itemBasics.value;
        }
    }
    public int MaxDamage {
        get {
            Suffix s = suffixes.Find( x => x.type == GearStats.Suffix.Dmg_Phys_Max );
            Prefix p = prefixes.Find( x => x.type == GearStats.Prefix.Dmg_Phys_Percent );
            if( s != null && p != null ) {
                return (int)( ( itemBasics.value2 + s.value ) * ( p.value / 100.0f + 1 ) );
            }
            if( s != null ) {
                return (int)( itemBasics.value2 + s.value );
            }
            if( p != null ) {
                return (int)( itemBasics.value2 * ( p.value / 100.0f + 1 ) );
            }
            return itemBasics.value2;
        }
    }
    public int Defense {
        get {
            Suffix s = suffixes.Find( x => x.type == GearStats.Suffix.Def_Phys_Flat );
            Prefix p = prefixes.Find( x => x.type == GearStats.Prefix.Def_Phys_Percent );
            if( s != null && p != null ) {
                return (int)( ( itemBasics.value + s.value ) * ( p.value / 100.0f + 1 ) );
            }
            if( s != null ) {
                return itemBasics.value + s.value;
            }
            if( p != null ) {
                return (int)( itemBasics.value * ( p.value / 100.0f + 1 ) );
            }
            return itemBasics.value;
        }
    }
    public ItemBasics itemBasics;
    public List<Implicit> implicits;
    public List<Prefix> prefixes;
    public List<Suffix> suffixes;
    public List<Stat> requirements;
    public string description;
    public AudioClip soundEndDrag;
    public string Tooltip {
        get {
            System.Text.StringBuilder t = new System.Text.StringBuilder( Name );
            t.Append( "\n" + Type_Text[(byte)type] );
            if( type == Type.PRIMARY || type == Type.SECONDARY )
                t.Append( "\n" + hexGray + "One-hand damage: </color>" + hexMagic + MinDamage + " to " + MaxDamage + "</color>" );
            else if( type != Type.RING && type != Type.NECK )
                t.Append( "\n" + hexGray + "Defense: </color>" + hexMagic + Defense + "</color>" );
            if( itemBasics.durability > 0 )
                t.Append( "\n" + hexGray + "Durability: </color>" + itemBasics.durability );
            foreach( Stat stat in requirements ) {
                switch( stat.stat ) {
                    case GearStats.Attributes.Level:
                        if( Character.Level < stat.value ) t.Append( hexRed );
                        else t.Append( hexGray );
                        break;
                    case GearStats.Attributes.Strength:
                        if( Character.Strength < stat.value ) t.Append( hexRed );
                        else t.Append( hexGray );
                        break;
                    case GearStats.Attributes.Dexterity:
                        if( Character.Dexterity < stat.value ) t.Append( hexRed );
                        else t.Append( hexGray );
                        break;
                    case GearStats.Attributes.Constitution:
                        if( Character.Constitution < stat.value ) t.Append( hexRed );
                        else t.Append( hexGray );
                        break;
                    case GearStats.Attributes.Intelligence:
                        if( Character.Intelligence < stat.value ) t.Append( hexRed );
                        else t.Append( hexGray );
                        break;
                }
                t.Append( string.Format( "\nRequired {0}: {1} </color>", stat.stat, stat.value ) );
            }
            foreach( var implicitMod in implicits ) {
                t.Append( "\n" + hexMagic + string.Format( GearStats.Affix_Text[(byte)implicitMod.type], implicitMod.value ) + "</color>" );
            }
            foreach( var prefix in prefixes ) {
                t.Append( "\n" + hexMagic + string.Format( GearStats.Affix_Text[(byte)prefix.type], prefix.value ) + "</color>" );
            }
            foreach( var suffix in suffixes ) {
                t.Append( "\n" + hexMagic + string.Format( GearStats.Affix_Text[(byte)suffix.type], suffix.value ) + "</color>" );
            }
            if( description != string.Empty )
                t.Append( "\n\n<i>" + description + "</i>" );
            return t.ToString();
        }
    }
    [System.Serializable]
    public class Stat
    {
        public GearStats.Attributes stat;
        public byte value;
    }
    [System.Serializable]
    public class ItemBasics
    {
        public Type type;
        public byte tier;
        public int value, value2;
        public byte durability;
        public string SpriteFilePath { get { return $"Sprites/Equipment/{type}/{tier}"; } }
    }
    [System.Serializable]
    public class Prefix
    {
        public GearStats.Prefix type;
        public int value;
    }
    [System.Serializable]
    public class Suffix
    {
        public GearStats.Suffix type;
        public int value;
    }
    [System.Serializable]
    public class Implicit
    {
        public GearStats.Implicit type;
        public int value;
    }
    public static string[] Type_Text = new string[15] { "Any", "Helmet", "Chest", "Gloves", "Legs", "Feet", "Weapon", "Offhand", "Ring", "Amulet", "Artifact", "Miscellaneous", "Consumable", "Quest Item", "Belt" };
    public enum Type { ANY, HEAD, CHEST, GLOVES, BELT, FEET, PRIMARY, SECONDARY, RING, NECK, ARTIFACT, MISC, CONSUMABLE, QUEST }
}
public class GearStats
{
    // The order of affix text must match the enum order on Character.StatID
    public static string[] Affix_Text = new string[37]
    {
        "+{0} to accuracy rating",
        "+{0} to minimum damage",
        "+{0} to maximum damage",
        "{0}% increased damage",
        "Adds {0} fire damage",
        "Adds {0} cold damage",
        "Adds {0} lightning damage",
        "Adds {0} poison damage",
        "+{0} to defense",
        "{0}% increased defence",
        "{0} physical damage reduction",
        "{0} magical damage reduction",
        "Damage reduced by {0}",
        "{0}% to fire resistance",
        "{0}% to cold resistance",
        "{0}% to lightning resistance",
        "{0}% to poison resistance",
        "{0}% to all resistances",
        "+{0} to life on hit",
        "+{0} to life after each kill",
        "+{0} to mana on hit",
        "+{0} to mana after each kill",
        "+{0} to life",
        "+{0} to mana",
        "+{0} to life regen per second",
        "+{0} to mana regen per second",
        "+{0} to strength",
        "+{0} to dexterity",
        "+{0} to constitution",
        "+{0} to intelligence",
        "+{0}% increase attack speed",
        "+{0}% faster cast rate",
        "+{0}% increased movement speed",
        "+{0}% faster block recovery",
        "+{0}% faster stagger recovery",
        "+{0}% magic find",
        "+{0} to maximum durability",
    };
    public enum Prefix
    {
        Plus_Accuracy = 0,
        Dmg_Phys_Percent = 3,
        Dmg_Ele_Fire = 4,
        Dmg_Ele_Cold = 5,
        Dmg_Ele_Lightning = 6,
        Dmg_Ele_Poison = 7,
        Def_Phys_Percent = 9,
        Def_Dmg_Reduction_Phys = 10,
        Def_Dmg_Reduction_Magic = 11,
        Def_Ele_Res_Fire = 13,
        Def_Ele_Res_Cold = 14,
        Def_Ele_Res_Lightning = 15,
        Def_Ele_Res_Poison = 16,
        Def_Ele_Res_All = 17,
        On_Hit_Life = 18,
        On_Kill_Life = 19,
        Plus_Durability = 20,
        Plus_Mana = 23,
        Plus_Regen_Mana = 25,
        Plus_Magic_Find = 35,
    }
    public enum Suffix
    {
        Dmg_Phys_Min = 1,
        Dmg_Phys_Max = 2,
        Dmg_Ele_Fire = 4,
        Dmg_Ele_Cold = 5,
        Dmg_Ele_Lightning = 6,
        Dmg_Ele_Poison = 7,
        Def_Phys_Flat = 8,
        Def_Dmg_Reduction_All = 12,
        On_Kill_Mana = 20,
        On_Hit_Mana = 21,
        Plus_Life = 22,
        Plus_Regen_Life = 24,
        Plus_Str = 26,
        Plus_Dex = 27,
        Plus_Con = 28,
        Plus_Int = 29,
        Plus_Speed_Phys = 30,
        Plus_Speed_Magic = 31,
        Plus_Speed_Movement = 32,
        Plus_Block_Recovery = 33,
        Plus_Stagger_Recovery = 34,
        Plus_Magic_Find = 35,
    }
    public enum Implicit
    {
        Def_Ele_Res_All = 17,
        Plus_Life = 22,
        Plus_Mana = 23,
        Def_Dmg_Reduction_All = 12,
        Plus_Regen_Life = 24,
        Plus_Regen_Mana = 25,
        Plus_Speed_Phys = 30,
        Plus_Speed_Magic = 31,
        Plus_Speed_Movement = 32,
        Plus_Block_Recovery = 33,
        Plus_Stagger_Recovery = 34,
        Plus_Magic_Find = 35,
    }
    public enum Attributes
    {
        Level, Strength, Dexterity, Constitution, Intelligence
    }
}