namespace Qwizzi.Domain.Entities;

public class Answer
{
    public int AnswerId { get; set; }

    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public string? Text { get; set; } = string.Empty;
    public Guid? ImageId { get; set; }

    public ImageAsset? Image { get; set; }
    public bool IsCorrect { get; set; }
}