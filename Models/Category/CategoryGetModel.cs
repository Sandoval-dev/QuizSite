public class CategoryGetModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int QuizCount { get; set; }
    public int QuestionCount { get; set; }
}