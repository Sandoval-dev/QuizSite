public class QuestionSolveViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public List<AnswerOptionViewModel> Options { get; set; } = new();
    public int? SelectedOptionId { get; set; } // Seçili cevabı tutacak

}