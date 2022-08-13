namespace Dash.Types
{
    //중간에 끼어들것을 고려해서 10 단위로 증가
    public enum DeckType : byte
    {
        Undefined = 0,
        Multi = 1,
        Solo = 10,
        Duo = 20,
        Trio = 30,
        PathOfGlory,
        Tower = 40,
    }
    
    public static class DeckTypeExtension
    {
        public static CodeLocale ToCodeLocaleType(this DeckType deckType)
        {
            switch(deckType)
            {
                case DeckType.Duo:
                    return CodeLocale.Dash_Types_DeckType_Duo;
                case DeckType.Trio:
                    return CodeLocale.Dash_Types_DeckType_Trio;
                case DeckType.Multi:
                    return CodeLocale.Dash_Types_DeckType_Multi;
                case DeckType.PathOfGlory:
                    return CodeLocale.Dash_Types_DeckType_PathOfGlory;
            }
            return CodeLocale.Undefined;
        }
        public static bool IsMultipleDeckType(this DeckType deckType)
        {
            switch(deckType)
            {
                case DeckType.Tower:
                    return true;
            }
            return false;
        }
    }
}
