using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PoliticalPaths.Application.Imports;
using PoliticalPaths.Application.Services;
using PoliticalPaths.Infrastructure.Persistence;
using Xunit;

namespace PoliticalPaths.Tests;

public sealed class EntityResolverTests
{
    [Fact]
    public async Task Politician_resolution_is_case_insensitive_before_save_changes()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new AppDbContext(options);
        var cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var resolver = new EntityResolver(db, cache);

        var first = await resolver.GetOrCreatePolitykAsync(new NamesSurnameDto("Jan", "", "KOWALSKI"));
        var second = await resolver.GetOrCreatePolitykAsync(new NamesSurnameDto("jan", "", "kowalski"));

        Assert.Same(first, second);
        Assert.Single(db.Politycy.Local);
    }
}
