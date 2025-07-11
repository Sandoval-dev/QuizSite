using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using QuizSite.Data;

public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseNpgsql("Host=db.vzgnxgkoivvnaagdnzlb.supabase.co;Database=postgres;Username=postgres;Password=KPjb3!&N!E5Z-gW;SSL Mode=Require;Trust Server Certificate=true");

        return new DataContext(optionsBuilder.Options);
    }
}
