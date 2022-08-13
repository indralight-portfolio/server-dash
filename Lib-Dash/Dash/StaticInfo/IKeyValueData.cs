using Common.StaticInfo;
using Dash.Types;
using Newtonsoft.Json;
using System;

namespace Dash.StaticInfo
{
    [Serializable]
    public class StringKeyData : IKeyValueData<string>
    {
        public string StrKey;

        [JsonIgnore]
        public string Key => StrKey;
    }
    [Serializable]
    public class MenuKeyData : IKeyValueData<Menu>
    {
        public Menu Menu;

        [JsonIgnore]
        public Menu Key => Menu;
    }
    public class MenuButtonTypeData : IKeyValueData<MenuButtonType>
    {
        public MenuButtonType ButtonType;

        [JsonIgnore]
        public MenuButtonType Key => ButtonType;
    }

    [Serializable]
    public class RarityKeyData : IKeyValueData<Rarity>
    {
        public Rarity Rarity;

        [JsonIgnore]
        public Rarity Key => Rarity;
    }

    [Serializable]
    public class RarityIntKeyData : IKeyValueData<KeyType<Rarity, int>>
    {
        public Rarity Rarity;
        [JsonIgnore]
        public int IntKey;

        [JsonIgnore]
        public KeyType<Rarity, int> Key => new KeyType<Rarity, int>(Rarity, IntKey);
    }

    [Serializable]
    public class RaritySlotIntKeyData : IKeyValueData<KeyType<Rarity, EquipmentSlotType, int>>
    {
        public Rarity Rarity;
        public EquipmentSlotType SlotType;
        [JsonIgnore]
        public int IntKey;

        [JsonIgnore]
        public KeyType<Rarity, EquipmentSlotType, int> Key => new KeyType<Rarity, EquipmentSlotType, int>(Rarity, SlotType, IntKey);
    }

    [Serializable]
    public class ElementalTypeIntKeyData : IKeyValueData<KeyType<ElementalType, int>>
    {
        public ElementalType ElementalType;
        [JsonIgnore]
        public int IntKey;

        [JsonIgnore]
        public KeyType<ElementalType, int> Key => new KeyType<ElementalType, int>(ElementalType, IntKey);
    }

    [Serializable]
    public class RarityElementalIntKeyData : IKeyValueData<KeyType<Rarity, ElementalType, int>>
    {
        public Rarity Rarity;
        public ElementalType ElementalType;
        [JsonIgnore]
        public int IntKey;

        [JsonIgnore]
        public KeyType<Rarity, ElementalType, int> Key => new KeyType<Rarity, ElementalType, int>(Rarity, ElementalType, IntKey);
    }

    [Serializable]
    public class RarityWeaponTypeIntKeyData : IKeyValueData<KeyType<Rarity, WeaponType, int>>
    {
        public Rarity Rarity;
        public WeaponType WeaponType;
        [JsonIgnore]
        public int IntKey;

        [JsonIgnore]
        public KeyType<Rarity, WeaponType, int> Key => new KeyType<Rarity, WeaponType, int>(Rarity, WeaponType, IntKey);
    }


    [Serializable]
    public class ItemTypeRarityKeyData : IKeyValueData<KeyType<ItemType, Rarity>>
    {
        public ItemType ItemType;
        public Rarity Rarity;

        [JsonIgnore]
        public KeyType<ItemType, Rarity> Key => new KeyType<ItemType, Rarity>(ItemType, Rarity);
    }

}
