public class QuizStatsViewModel
{
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public DateTime? LastTakenAt { get; set; }
    public int Correct { get; set; }
    public int Wrong { get; set; }
    public int TotalQuestions { get; set; }
    public int Blank { get; set; }
    public string UserName { get; set; } = string.Empty;

}
