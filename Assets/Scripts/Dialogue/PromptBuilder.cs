using System.Text;
using System.Threading.Tasks;

public class PromptBuilder
{
    public string BuildOrderPrompt(NPCProfile profile, OrderContext order)
    {
        StringBuilder sb = new();

        sb.AppendLine("Ты NPC-клиент бара в игре-симуляторе.");
        sb.AppendLine("Твоя задача — сделать заказ, но не называть коктейль напрямую.");
        sb.AppendLine("Говори естественно, как живой человек.");
        sb.AppendLine("Ответ должен быть на русском языке.");
        sb.AppendLine("Длина ответа: 1-3 коротких предложения.");
        sb.AppendLine();

        sb.AppendLine($"Имя клиента: {profile.DisplayName}");
        sb.AppendLine($"Архетип клиента: {profile.Archetype}");
        sb.AppendLine($"Характер: {profile.Temper}");
        sb.AppendLine($"Настроение: {profile.Mood}");
        sb.AppendLine($"Предпочтение по вкусу: {profile.PreferredTaste}");
        sb.AppendLine($"Предпочтение по крепости: {profile.PreferredStrength}");
        sb.AppendLine($"Любит лед: {(profile.LikesIce ? "да" : "нет")}");
        sb.AppendLine();

        sb.AppendLine($"Скрытый коктейль: {order.RecipeName}");
        sb.AppendLine($"Вкусовая подсказка: {order.TasteHint}");
        sb.AppendLine($"Крепость: {order.StrengthHint}");
        sb.AppendLine($"Нужен лед: {(order.WantsIce ? "да" : "нет")}");
        sb.AppendLine();

        sb.AppendLine("Правила:");
        sb.AppendLine("- не называй скрытый коктейль напрямую;");
        sb.AppendLine("- не перечисляй ингредиенты напрямую;");
        sb.AppendLine("- говори в стиле своего архетипа;");
        sb.AppendLine("- обязательно оставляй намеки на то, каким человеком являешься, добавляй контекста о ситуации в своей жизни;");
        sb.AppendLine("- избегай длинных объяснений;");
        sb.AppendLine("- не называй своего имени, говори от первого лица;");
        sb.AppendLine("- выдай только реплику клиента, без кавычек и комментариев.");

        return sb.ToString();
    }
    public string BuildReactionPrompt(
        NPCProfile profile,
        OrderContext order,
        CocktailEvaluationResult result)
    {
        StringBuilder sb = new();

        sb.AppendLine("Ты клиент бара в игре-симуляторе.");
        sb.AppendLine("Ты только что попробовал напиток, который приготовил игрок.");
        sb.AppendLine("Ответь естественно, коротко и в соответствии со своим характером.");
        sb.AppendLine();

        sb.AppendLine($"Характер: {profile.Temper}");
        sb.AppendLine($"Настроение: {profile.Mood}");
        sb.AppendLine($"Ты хотел напиток: {order.RecipeName}");
        sb.AppendLine($"Оценка совпадения: {(result.FinalScore * 100f):F0}%");
        sb.AppendLine($"Комментарий системы: {result.Comment}");
        sb.AppendLine();

        sb.AppendLine("Если оценка высокая — похвали бармена.");
        sb.AppendLine("Если средняя — скажи, что напиток неплохой, но не совсем то.");
        sb.AppendLine("Если низкая — вырази недовольство.");
        sb.AppendLine("Сгенерируй одну реплику на русском языке.");

        return sb.ToString();
    }
    
}