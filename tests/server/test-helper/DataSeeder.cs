using System.Collections.Concurrent;
using MongoDB.Bson.Serialization;

namespace SilverKinetics.w80.TestHelper;

public static class DataSeeder
{
    public static async Task SeedTestDataAsync(IServiceProvider serviceProvider, string databaseName)
    {
        var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
        var database = mongoClient.GetDatabase(databaseName);

        string[] files;
        // The path for the seed files is relative to currently executing assembly (which
        // is different depending on which test are running), this is why this bandaid fix
        // will try a second path if directory was not found.
        // TODO: Fix this
        try
        {
            var f1 = Directory.GetFiles(@"../../../../../../../db/init-data", "*.json", SearchOption.TopDirectoryOnly);
            var f2 = Directory.GetFiles(@"../../../../../../../db/test-data", "*.json", SearchOption.TopDirectoryOnly);
            files = [..f1, ..f2];
        }
        catch (DirectoryNotFoundException)
        {
            var f1 = Directory.GetFiles(@"../../../../../../db/init-data", "*.json", SearchOption.TopDirectoryOnly);
            var f2 = Directory.GetFiles(@"../../../../../../db/test-data", "*.json", SearchOption.TopDirectoryOnly);
            files = [..f1, ..f2];
        }

        await Parallel.ForEachAsync(files, async (file, cancellationToken) =>
        {
            if (!_fileContents.TryGetValue(file, out BsonDocument[]? documents))
            {
                var json = await File.ReadAllTextAsync(file, cancellationToken);
                documents = BsonSerializer.Deserialize<BsonDocument[]>(json);
                _fileContents.Add(file, documents);
            }

            var collectionName = Path.GetFileNameWithoutExtension(file);
            var collection = database.GetCollection<BsonDocument>(collectionName);
            await collection.InsertManyAsync(documents, cancellationToken: cancellationToken);
        });
    }

    private readonly static IDictionary<string,BsonDocument[]> _fileContents = new ConcurrentDictionary<string,BsonDocument[]>();

}