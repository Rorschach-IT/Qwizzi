using Qwizzi.Domain.Enums;

namespace Qwizzi.Domain.Entities;

public class Quiz
{
    public Guid QuizId { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public int CategoryId { get; set; }

    public QuizCategory Category { get; set; } = null!;
    public string? Description { get; set; } = string.Empty;
    public Guid? CoverImageId { get; set; }

    public ImageAsset? CoverImage { get; set; }
    public QuizStatus Status { get; set; } = QuizStatus.Draft;
    
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    // Verification by admin
    public Guid? VerifiedById { get; set; }
    public User? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Relationship
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}