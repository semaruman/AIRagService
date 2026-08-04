using Microsoft.EntityFrameworkCore;

namespace AIRagService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options);
