using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=UserHierarchyDb;Integrated Security=True;");

        return new AppDbContext(optionsBuilder.Options);
    }
}