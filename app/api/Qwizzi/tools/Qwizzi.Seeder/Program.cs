using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qwizzi.Domain.Entities;
using Qwizzi.Domain.Enums;
using Qwizzi.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;

namespace Qwizzi.Seeder;

internal static class Program
{
    private static readonly string[] QuizTitles = ["Windows PowerShell", "Logic Gates", "East of Eden", "Silent Hill"];

    private static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(typeof(Program).Assembly)
            .Build();
        
        var root = FindBackendRoot();
        var connectionString = configuration.GetConnectionString("Database")
                               ?? throw new InvalidOperationException(
                                   "Missing ConnectionStrings:Database user secret.");

        var sourceDirectory = Path.Combine(root, SeedSettings.SourceImagesRelativePath);
        var storageDirectory = Path.Combine(root, SeedSettings.StorageRelativePath);
        ValidateImages(sourceDirectory);

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var db = new AppDbContext(options);

        if (db.Database.HasPendingModelChanges())
        {
            throw new InvalidOperationException(
                "The EF model has changes that are not captured in a migration. " +
                "Create and apply the migration before running the seeder.");
        }

        var pendingMigrations =
            await db.Database.GetPendingMigrationsAsync();

        if (pendingMigrations.Any())
        {
            throw new InvalidOperationException(
                "The database has unapplied migrations. " +
                "Run dotnet ef database update before running the seeder.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync();
        await RemovePreviousSeedAsync(db);
        var users = CreateUsers();
        var hasher = new PasswordHasher<User>();
        foreach (var user in users.Values)
            user.PasswordHash = hasher.HashPassword(user, "QwizziDemo!2026");
        db.Users.AddRange(users.Values);
        await db.SaveChangesAsync();

        var categories = await GetCategoriesAsync(db);
        var images = await CreateImagesAsync(db, users, sourceDirectory, storageDirectory);
        await db.SaveChangesAsync();
        var quizzes = CreateQuizzes(users, categories, images);
        db.Quizzes.AddRange(quizzes);
        await db.SaveChangesAsync();
        db.QuizAttempts.Add(CreateAttempt(users["GamerBoy14"], quizzes.Single(q => q.Title == "Logic Gates")));
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        Console.WriteLine("Seed complete: 5 users, 4 quizzes, 25 questions, 94 answers, 7 images and 1 completed attempt.");
        Console.WriteLine("Demo password for all seeded users: QwizziDemo!2026");
    }

    private static Dictionary<string, User> CreateUsers()
    {
        var rows = new (string Name, string Email, UserRole Role)[]
        {
            ("ITGuy12", "itguy12@example.test", UserRole.User),
            ("East_Eden12", "east.eden12@example.test", UserRole.User),
            ("GamerBoy14", "gamerboy14@example.test", UserRole.User),
            ("Admin_main", "admin.main@example.test", UserRole.Administrator),
            ("Admin_second", "admin.second@example.test", UserRole.Administrator)
        };
        return rows.ToDictionary(x => x.Name, x => new User { Username = x.Name, Email = x.Email, Role = x.Role, Status = true });
    }

    private static async Task<Dictionary<string, int>> GetCategoriesAsync(AppDbContext db)
    {
        var names = new[] { "IT Technology", "Movies", "Games" };
        foreach (var name in names)
            if (!await db.QuizCategories.AnyAsync(c => c.Name == name)) db.QuizCategories.Add(new QuizCategory { Name = name });
        await db.SaveChangesAsync();
        return await db.QuizCategories.Where(c => names.Contains(c.Name)).ToDictionaryAsync(c => c.Name, c => c.QuizCategoryId);
    }

    private static async Task<Dictionary<string, ImageAsset>> CreateImagesAsync(AppDbContext db, Dictionary<string, User> users, string source, string storage)
    {
        Directory.CreateDirectory(storage);
        var result = new Dictionary<string, ImageAsset>();
        for (var i = 1; i <= 7; i++)
        {
            var fileName = $"image-{i:00}.webp";
            var bytes = await File.ReadAllBytesAsync(Path.Combine(source, fileName));
            var (width, height) = ReadWebpDimensions(bytes);
            var owner = i <= 4 ? "ITGuy12" : i == 5 ? "East_Eden12" : "Admin_main";
            var key = $"{SeedSettings.StorageKeyPrefix}/{fileName}";
            var destination = Path.Combine(storage, key.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            await File.WriteAllBytesAsync(destination, bytes);
            var asset = new ImageAsset
            {
                StorageKey = key, OriginalFileName = fileName, ContentType = "image/webp",
                SizeBytes = bytes.LongLength, Width = width, Height = height,
                UploadedById = users[owner].UserId
            };
            db.ImageAssets.Add(asset);
            result.Add(fileName, asset);
        }
        return result;
    }

    private static List<Quiz> CreateQuizzes(Dictionary<string, User> users, Dictionary<string, int> categories, Dictionary<string, ImageAsset> images)
    {
        var result = new List<Quiz>
        {
            BuildQuiz(users, categories, "Windows PowerShell", "IT Technology", "ITGuy12", QuizStatus.Approved, images["image-01.webp"], StandardQuestions("PowerShell")),
            BuildQuiz(users, categories, "Logic Gates", "IT Technology", "ITGuy12", QuizStatus.Approved, images["image-02.webp"], LogicQuestions(images)),
            BuildQuiz(users, categories, "East of Eden", "Movies", "East_Eden12", QuizStatus.Rejected, images["image-05.webp"], StandardQuestions("East of Eden", 10), users["Admin_second"], SeedSettings.RejectionReason),
            BuildQuiz(users, categories, "Silent Hill", "Games", "Admin_main", QuizStatus.Approved, images["image-06.webp"], SilentHillQuestions(images))
        };
        return result;
    }

    private static Quiz BuildQuiz(Dictionary<string, User> users, Dictionary<string, int> categories, string title, string category, string author, QuizStatus status, ImageAsset cover, List<Question> questions, User? moderator = null, string? reason = null) => new()
    {
        Title = title, Description = $"Seed quiz: {title}", CategoryId = categories[category],
        AuthorId = users[author].UserId, Author = users[author], Status = status, CoverImage = cover, Questions = questions,
        VerifiedBy = moderator, VerifiedById = moderator?.UserId, VerifiedAt = DateTime.UtcNow, RejectionReason = reason
    };

    private static List<Question> StandardQuestions(string topic, int count = 5) => Enumerable.Range(1, count)
        .Select(i => QuestionWithText($"{topic}: sample question {i}?", $"Answer A{i}", $"Answer B{i}", $"Answer C{i}", $"Answer D{i}")).ToList();

    private static List<Question> LogicQuestions(Dictionary<string, ImageAsset> images)
    {
        var questions = Enumerable.Range(1, 3).Select(i => QuestionWithText($"Logic gates question {i}: which statement is correct?", "AND gate outputs true only when both inputs are true.", "OR gate outputs true only when both inputs are false.", "NOT gate has two outputs.", correct: 0)).ToList();
        questions.Add(new Question { Text = "Which image shows a logic gate diagram?", Answers = [new Answer { Image = images["image-03.webp"], IsCorrect = true }, new Answer { Image = images["image-04.webp"], IsCorrect = false }] });
        return questions;
    }

    private static List<Question> SilentHillQuestions(Dictionary<string, ImageAsset> images)
    {
        var questions = Enumerable.Range(1, 5).Select(i => QuestionWithText($"Silent Hill question {i}: choose the correct answer.", "Silent Hill", "Resident Evil", "Fatal Frame", "Alone in the Dark", correct: 0)).ToList();
        questions.Add(new Question
        {
            Text = "Which location is shown in the image?", Image = images["image-07.webp"],
            Answers = [new Answer { Text = "Hospital", IsCorrect = true }, new Answer { Text = "Woodside Apartments" }, new Answer { Text = "Lakeview Hotel" }]
        });
        return questions;
    }

    private static Question QuestionWithText(string text, params string[] answers) => QuestionWithText(text, answers, 0);
    private static Question QuestionWithText(string text, string a, string b, string c, int correct = 0) => QuestionWithText(text, [a, b, c], correct);
    private static Question QuestionWithText(string text, string a, string b, string c, string d, int correct = 0) => QuestionWithText(text, [a, b, c, d], correct);
    private static Question QuestionWithText(string text, string[] answers, int correct = 0) => new()
    {
        Text = text, Answers = answers.Select((answer, index) => new Answer { Text = answer, IsCorrect = index == correct }).ToList()
    };

    private static QuizAttempt CreateAttempt(User user, Quiz quiz)
    {
        var chosen = quiz.Questions.Select((question, index) => new { question, index })
            .Select(x => new { x.index, Correct = x.question.Answers.Single(a => a.IsCorrect), Wrong = x.question.Answers.First(a => !a.IsCorrect) }).ToArray();
        var answers = chosen.Select((x, i) => new UserAnswer { SelectedAnswer = i == 2 ? x.Wrong : x.Correct, IsCorrect = i != 2 }).ToList();
        return new QuizAttempt { UserId = user.UserId, QuizId = quiz.QuizId, StartedAt = DateTime.UtcNow.AddMinutes(-4), CompletedAt = DateTime.UtcNow, Score = 3, UserAnswers = answers };
    }

    private static async Task RemovePreviousSeedAsync(AppDbContext db)
    {
        var oldQuizzes = await db.Quizzes.Where(q => QuizTitles.Contains(q.Title)).ToListAsync();
        db.Quizzes.RemoveRange(oldQuizzes);
        await db.SaveChangesAsync();
        var oldImages = await db.ImageAssets.Where(i => i.StorageKey.StartsWith(SeedSettings.StorageKeyPrefix + "/")).ToListAsync();
        db.ImageAssets.RemoveRange(oldImages);
        var oldUsers = await db.Users.Where(u => new[] { "ITGuy12", "East_Eden12", "GamerBoy14", "Admin_main", "Admin_second" }.Contains(u.Username)).ToListAsync();
        db.Users.RemoveRange(oldUsers);
        await db.SaveChangesAsync();
    }

    private static void ValidateImages(string directory)
    {
        foreach (var file in SeedSettings.RequiredImageFileNames)
            if (!File.Exists(Path.Combine(directory, file))) throw new FileNotFoundException($"Required seed image was not found: {Path.Combine(directory, file)}");
    }

    private static string FindBackendRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Qwizzi.slnx"))) return directory.FullName;
        throw new DirectoryNotFoundException("Could not locate backend root (Qwizzi.slnx).");
    }

    private static (int Width, int Height) ReadWebpDimensions(byte[] data)
    {
        if (data.Length < 30 || System.Text.Encoding.ASCII.GetString(data, 0, 4) != "RIFF" || System.Text.Encoding.ASCII.GetString(data, 8, 4) != "WEBP") throw new InvalidDataException("Invalid WebP image.");
        var kind = System.Text.Encoding.ASCII.GetString(data, 12, 4);
        if (kind == "VP8X") return (1 + Read24(data, 24), 1 + Read24(data, 27));
        if (kind == "VP8L") return (1 + data[21] + ((data[22] & 0x3f) << 8), 1 + (data[22] >> 6) + (data[23] << 2) + ((data[24] & 0x0f) << 10));
        if (kind == "VP8 ")
            for (var i = 20; i + 7 < data.Length; i++)
                if (data[i] == 0x9d && data[i + 1] == 0x01 && data[i + 2] == 0x2a)
                    return ((data[i + 3] | data[i + 4] << 8) & 0x3fff, (data[i + 5] | data[i + 6] << 8) & 0x3fff);
        throw new InvalidDataException("Unsupported or malformed WebP image.");
    }

    private static int Read24(byte[] data, int offset) => data[offset] | data[offset + 1] << 8 | data[offset + 2] << 16;
}
