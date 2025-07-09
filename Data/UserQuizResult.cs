namespace QuizSite.Data;

public class UserQuizResult
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public int QuizId { get; set; }
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public int BlankCount { get; set; }
    public DateTime TakenAt { get; set; }
}