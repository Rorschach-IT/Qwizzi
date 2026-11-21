namespace Qwizzi.Domain.Entities;

public class Question
{
    public int QuestionId { get; set; }
    
    public Guid QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    public string Text { get; set; } = string.Empty;
    public Guid? ImageId { get; set; }

    public ImageAsset? Image { get; set; }
    
    // Explanation displayed in the summary for an incorrect answer
    public string? Explanation { get; set; } = string.Empty;

    // Relations
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}