using Common.Utility;
using Dash.Model.Rdb;
using System.Collections.Generic;

namespace Dash.Model.Service
{
    [MessagePack.MessagePackObject()]
    public class ChangeModel
    {
        [MessagePack.Key(0)]
        public Account NewAccount;
        [MessagePack.Key(1)]
        public List<Money> NewMonies;
        [MessagePack.Key(2)]
        public List<Character> NewCharacters;
        [MessagePack.Key(3)]
        public List<Equipment> NewEquipments;
        [MessagePack.Key(4)]
        public List<Equipment> RemoveEquipments;
        [MessagePack.Key(5)]
        public List<Consume> NewConsumes;
        [MessagePack.Key(6)]
        public List<DailyReward> NewDailyRewards;
        [MessagePack.Key(7)]
        public List<SeasonPass> NewSeasonPasses;
        [MessagePack.Key(8)]
        public List<Collection> NewCollections;
        [MessagePack.Key(9)]
        public List<CollectionHistory> NewCollectionHistories;

        public ChangeModel Merge(Account account)
        {
            NewAccount = account;
            return this;
        }

        public ChangeModel Merge(Money money)
        {
            if (money == null) return this;
            NewMonies ??= new List<Money>();
            NewMonies.AddOrUpdate(money, (e => e.Type == money.Type));
            return this;
        }
        public ChangeModel Merge(List<Money> monies)
        {
            monies?.ForEach(e => Merge(e));
            return this;
        }

        public ChangeModel Merge(Character character)
        {
            if (character == null) return this;
            NewCharacters ??= new List<Character>();
            NewCharacters.AddOrUpdate(character, (e => e.Id == character.Id));
            return this;
        }
        public ChangeModel Merge(List<Character> characters)
        {
            characters?.ForEach(e => Merge(e));
            return this;
        }

        public ChangeModel Merge(Equipment equipment, bool remove = false)
        {
            if (equipment == null) return this;
            if (remove == true)
            {
                RemoveEquipments ??= new List<Equipment>();
                RemoveEquipments.Add(equipment);
            }
            else
            {
                NewEquipments ??= new List<Equipment>();
                NewEquipments.Add(equipment);
            }
            return this;
        }
        public ChangeModel Merge(List<Equipment> equipments, bool remove = false)
        {
            equipments?.ForEach(e => Merge(e, remove));
            return this;
        }

        public ChangeModel Merge(Consume consume)
        {
            if (consume == null) return this;
            NewConsumes ??= new List<Consume>();
            NewConsumes.AddOrUpdate(consume, (e => e.Id == consume.Id));
            return this;
        }
        public ChangeModel Merge(List<Consume> consumes)
        {
            consumes?.ForEach(e => Merge(e));
            return this;
        }

        public ChangeModel Merge(SeasonPass seasonPass)
        {
            if (seasonPass == null) return this;
            NewSeasonPasses ??= new List<SeasonPass>();
            NewSeasonPasses.AddOrUpdate(seasonPass, (e => e.Id == seasonPass.Id));
            return this;
        }
        public ChangeModel Merge(List<SeasonPass> seasonPasses)
        {
            seasonPasses?.ForEach(e => Merge(e));
            return this;
        }

        public ChangeModel Merge(DailyReward dailyReward)
        {
            if (dailyReward == null) return this;
            NewDailyRewards ??= new List<DailyReward>();
            NewDailyRewards.AddOrUpdate(dailyReward, (e => e.Id == dailyReward.Id));
            return this;
        }
        public ChangeModel Merge(List<DailyReward> dailyRewards)
        {
            dailyRewards?.ForEach(e => Merge(e));
            return this;
        }

        public ChangeModel Merge(Collection collection)
        {
            if (collection == null) return this;
            NewCollections ??= new List<Collection>();
            NewCollections.AddOrUpdate(collection, (e => e.Id == collection.Id));
            return this;
        }
        public ChangeModel Merge(List<Collection> collections)
        {
            collections?.ForEach(e => Merge(e));
            return this;
        }

        public ChangeModel Merge(CollectionHistory collectionHistory)
        {
            if (collectionHistory == null) return this;
            NewCollectionHistories ??= new List<CollectionHistory>();
            NewCollectionHistories.AddOrUpdate(collectionHistory, (e => e.Id == collectionHistory.Id));
            return this;
        }
        public ChangeModel Merge(List<CollectionHistory> collectionHistories)
        {
            collectionHistories?.ForEach(e => Merge(e));
            return this;
        }

        public ChangeModel Merge(ChangeModel other)
        {
            if (other == null) return this;
            Merge(other.NewAccount);
            Merge(other.NewMonies);
            Merge(other.NewCharacters);
            Merge(other.NewEquipments);
            Merge(other.RemoveEquipments);
            Merge(other.NewConsumes);
            Merge(other.NewDailyRewards);
            Merge(other.NewSeasonPasses);
            Merge(other.NewCollections);
            Merge(other.NewCollectionHistories);
            return this;
        }
    }
}