
using Microsoft.Azure.Cosmos;
using cosmosdbdemo.Models;
namespace cosmosdbdemo.Data
{

    public class CosmosDbService
    {
        private Container _container;

        public CosmosDbService(CosmosClient cosmosClient,
            string databaseName, string containerName)
        {
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task AddItemAsync(ItemModel item)
        {
            await _container.CreateItemAsync(item, new PartitionKey(item.Id));
        }


        public async Task<ItemModel> GetItemAsync(string id)
        {
            ItemResponse<ItemModel> response = await
                _container.ReadItemAsync<ItemModel>(id, new PartitionKey(id));

            return response.Resource;
        }


        public async Task<IEnumerable<ItemModel>> GetItemsAsync(string queryString)
        {
            var query = _container.GetItemQueryIterator<ItemModel>(new QueryDefinition(queryString));
            List<ItemModel> results = new List<ItemModel>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }


        public async Task UpdateItemAsync(string id, ItemModel item)
        {
            await _container.UpsertItemAsync(item, new PartitionKey(id));
        }

        public async Task DeleteItemAsync(string id)
        {
            await _container.DeleteItemAsync<ItemModel>(id, new PartitionKey(id));
        }



    }

}
