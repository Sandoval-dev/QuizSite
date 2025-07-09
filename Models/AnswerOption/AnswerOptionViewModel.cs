public class AnswerOptionViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; } // Quiz çözüm ekranında gerekmiyorsa kullanmazsın
}
