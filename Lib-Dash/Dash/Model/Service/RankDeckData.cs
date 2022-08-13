using Dash.Model.Rdb;
using Dash.Types;
using System.Collections.Generic;


namespace Dash.Model.Service
{
    public class PlayerData
    {
        public string NickName;
        public List<PlayerDeckData> DeckDatas;
    }
    public class PlayerDeckData
    {
        public DeckSlotType SlotType;
        public Character Character;
        public EquipRune EquipRune;
        public List<Equipment> Equipments;

        public static PlayerDeckData Make(PlayerModel playerModel, int slotIndex)
        {
            PlayerDeckData data = new PlayerDeckData();
            if (playerModel.Type == PlayerType.User)
            {
                data.SlotType = DeckSlotType.Player;
            }
            else
            {
                data.SlotType = (DeckSlotType)slotIndex;
            }
            data.Character = playerModel.Character;
            data.EquipRune = playerModel.EquipRune;
            data.Equipments = playerModel.Equipments;
            return data;
        }
    }
}
