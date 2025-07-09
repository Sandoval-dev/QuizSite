namespace QuizSite.Data;

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public string CorrectAnswer { get; set; } = null!;
    public List<AnswerOption> Options { get; set; } = new();
    public int CorrectOptionIndex { get; set; }

    // Kategori ilişkisi
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public List<QuizQuestion> QuizQuestions { get; set; } = new();

}