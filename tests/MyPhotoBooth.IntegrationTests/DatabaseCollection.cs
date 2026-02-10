using MyPhotoBooth.IntegrationTests.Fixtures;

namespace MyPhotoBooth.IntegrationTests;

[CollectionDefinition("Database Collection")]
public class DatabaseCollection : ICollectionFixture<PostgreSqlFixture>
{
}
