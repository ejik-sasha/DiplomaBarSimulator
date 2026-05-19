public class CocktailAction
{
    public CocktailActionType Type;
    public string TargetId;
    public float Amount;

    public CocktailAction(CocktailActionType type, string targetId, float amount)
    {
        Type = type;
        TargetId = targetId;
        Amount = amount;
    }
}