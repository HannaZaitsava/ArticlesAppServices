namespace IntegrationTests.Common
{

    /*
     * Без коллекции: Если у нас 10 файлов с тестами, Docker-контейнер запустится 10 раз.
     * С коллекцией: Docker запустится всего 1 раз в начале прогона.
     */
    [CollectionDefinition(TestCollections.IntegrationTest)]
    public sealed class IntegrationTestCollection : ICollectionFixture<CollectionFixtureSharedTestContext> { }
}
