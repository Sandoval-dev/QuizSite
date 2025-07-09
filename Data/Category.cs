namespace QuizSite.Data;


public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<Quiz> Quizzes { get; set; } = new();
     public ICollection<Question> Questions { get; set; } = new List<Question>(); // Ekle

}