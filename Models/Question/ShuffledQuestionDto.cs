public class ShuffledQuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public List<ShuffledOptionDto> Options { get; set; } = new();
}