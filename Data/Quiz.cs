namespace QuizSite.Data;

public class Quiz
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category category { get; set; } = null!;
    public List<QuizQuestion> QuizQuestions { get; set; } = new();
    public int DurationInMinutes { get; set; } // Quiz süresi (dakika cinsinden)
}