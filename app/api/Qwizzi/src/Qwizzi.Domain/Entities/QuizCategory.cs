namespace Qwizzi.Domain.Entities;

public class QuizCategory
{
    public int QuizCategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}