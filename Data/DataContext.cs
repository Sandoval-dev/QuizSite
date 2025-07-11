using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizSite.Data;

public class DataContext : IdentityDbContext<IdentityUser>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<AnswerOption> AnswerOptions { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<Quiz> Quizzes { get; set; } = null!;
    public DbSet<UserQuizResult> UserQuizResults { get; set; } = null!;
    public DbSet<QuizQuestion> QuizQuestion { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Quiz -> Category (Many-to-One)
        builder.Entity<Quiz>()
        .HasOne(q => q.category)
        .WithMany(c => c.Quizzes)
        .HasForeignKey(q => q.CategoryId);

        //Question => AnswerOptions (One-to-Many)
        builder.Entity<Question>()
        .HasMany(q => q.Options)
        .WithOne()
        .OnDelete(DeleteBehavior.Cascade);

        // UserQuizResult -> Quiz (Many-to-One)
        builder.Entity<UserQuizResult>()
        .HasOne<Quiz>()
        .WithMany() //her userquizresult bir tane quiz ile ilişkilidir ama her quiz n tane userquizresult la ilişkili olabilir.
        .HasForeignKey(r => r.QuizId);

        // UserQuizResult -> IdentityUser (Many-to-One)
        builder.Entity<UserQuizResult>()
        .HasOne<IdentityUser>() //her userquizresult bir kullanıcıya aittir.
        .WithMany() //kullanıcının birden fazla userquizresult u olabilir.
        .HasForeignKey(r => r.UserId) //bunlar userId ile eşleşir
        .OnDelete(DeleteBehavior.Restrict);

        //Question -> Category İlişkisi (Many-to-One)
        builder.Entity<Question>()
        .HasOne(q => q.Category)
        .WithMany(c => c.Questions)
        .HasForeignKey(q => q.CategoryId)
        .OnDelete(DeleteBehavior.Cascade);

        //Quiz <--> Question (Many-to-Many) //aynı soru bir quiz içinde yalnızca bir kez bulunabilir.
        builder.Entity<QuizQuestion>()
        .HasKey(qq => new { qq.QuizId, qq.QuestionId }); //Composite key

        builder.Entity<QuizQuestion>()
        .HasOne(qq => qq.Quiz)
        .WithMany(q => q.QuizQuestions)
        .HasForeignKey(qq => qq.QuizId);

        builder.Entity<QuizQuestion>()
        .HasOne(qq => qq.Question)
        .WithMany(q => q.QuizQuestions)
        .HasForeignKey(qq => qq.QuestionId);



    }
}