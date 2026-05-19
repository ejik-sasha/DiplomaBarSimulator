public class DialogueCandidate
{
    public string Text;
    public float Score;
    public string RejectReason;
    public bool IsValid => string.IsNullOrWhiteSpace(RejectReason);
}