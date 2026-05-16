public class NPCReactionGenerator
{
    public string GetFallbackOrderHint(NPCProfile profile, OrderContext order)
    {
        if (profile == null || order == null)
            return "Сделайте мне что-нибудь интересное.";

        string moodPart = profile.Mood switch
        {
            "tired" => "Я сегодня вымотан.",
            "cheerful" => "Хочется чего-то приятного.",
            "irritated" => "Не хочу долго объяснять.",
            _ => "Нужно что-нибудь подходящее."
        };

        string tastePart = order.TasteHint switch
        {
            "bitter" => "Люблю что-то с горчинкой.",
            "sweet" => "Хочется чего-то помягче и послаще.",
            "fresh" => "Нужно что-то освежающее.",
            _ => "На ваш вкус, но без крайностей."
        };

        string strengthPart = order.StrengthHint switch
        {
            "strong" => "Покрепче.",
            "light" => "Только не слишком крепкое.",
            _ => "Средней крепости."
        };

        return $"{moodPart} {tastePart} {strengthPart}";
    }

    public string GetFallbackReaction(float score, NPCProfile profile)
    {
        if (score >= 0.9f) return "Вот это уже похоже на то, что я хотел.";
        if (score >= 0.75f) return "Неплохо. Почти угадали.";
        if (score >= 0.5f) return "Что-то в этом есть, но не совсем то.";
        return "Нет, это совсем мимо.";
    }
}