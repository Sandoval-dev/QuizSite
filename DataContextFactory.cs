using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using QuizSite.Data;

public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

        var connectionString = "Host=vzgnxgkoivvnaagdnzlb.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=KPjb3!&N!E5Z-gW;SSL Mode=Require;Trust Server Certificate=true";

        optionsBuilder.UseNpgsql(connectionString);

        return new DataContext(optionsBuilder.Options);
    }
}
