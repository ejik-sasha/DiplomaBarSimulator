using System.Text;
using System.Threading.Tasks;

public class PromptBuilder
{
    public string BuildOrderPrompt(NPCProfile profile, OrderContext order)
    {
        StringBuilder sb = new();

        sb.AppendLine("Ты NPC-клиент бара в игре-симуляторе.");
        sb.AppendLine("Твоя задача — сделать заказ, но не называть коктейль напрямую.");
        sb.AppendLine("Сгенерируй 5 разных вариантов реплики клиента.");
        sb.AppendLine("Ответ должен быть на русском языке.");
        sb.AppendLine("Каждый вариант: 1-2 коротких предложения.");
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
        sb.AppendLine("- добавь небольшой жизненный контекст;");
        sb.AppendLine("- избегай длинных объяснений;");
        sb.AppendLine("- не называй своего имени;");
        sb.AppendLine("- не добавляй комментарии от себя;");
        sb.AppendLine("- верни только список из 5 вариантов.");
        sb.AppendLine();

        sb.AppendLine("Формат ответа:");
        sb.AppendLine("1. <реплика>");
        sb.AppendLine("2. <реплика>");
        sb.AppendLine("3. <реплика>");
        sb.AppendLine("4. <реплика>");
        sb.AppendLine("5. <реплика>");

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
        sb.AppendLine("Сгенерируй 3 разных варианта короткой реакции.");
        sb.AppendLine("Ответь естественно и в соответствии со своим характером.");
        sb.AppendLine();

        sb.AppendLine($"Архетип: {profile.Archetype}");
        sb.AppendLine($"Характер: {profile.Temper}");
        sb.AppendLine($"Настроение: {profile.Mood}");
        sb.AppendLine($"Ты хотел напиток: {order.RecipeName}");
        sb.AppendLine($"Оценка совпадения: {(result.FinalScore * 100f):F0}%");
        sb.AppendLine($"Комментарий системы: {result.Comment}");
        sb.AppendLine();

        sb.AppendLine("Правила:");
        sb.AppendLine("- если оценка высокая — похвали бармена;");
        sb.AppendLine("- если средняя — скажи, что напиток неплохой, но не совсем то;");
        sb.AppendLine("- если низкая — вырази недовольство;");
        sb.AppendLine("- не пиши пояснения;");
        sb.AppendLine("- верни только список из 3 вариантов.");
        sb.AppendLine();

        sb.AppendLine("Формат ответа:");
        sb.AppendLine("1. <реплика>");
        sb.AppendLine("2. <реплика>");
        sb.AppendLine("3. <реплика>");

        return sb.ToString();
    }
    
}