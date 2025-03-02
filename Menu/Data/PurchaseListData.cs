using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Menu.Models;

namespace Menu.Data
{
    public interface IPurchaseListData
    {
        Task<AggregateList> GetPurchaseList(string name, string username);

        Task<string> AddPurchaseList(AggregateList aggregateList, string username);

        Task<string> UpdatePurchaseList(AggregateList aggregateList, string username);

        Task<List<string>> GetAllPurchaseList(string username);

        Task<string> DeletePurchaseList(string Id, string username);

    }
    public class PurchaseListData : IPurchaseListData
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly string _tableName = "PurchaseList_New";

        public PurchaseListData()
        {
            var awsCredentials = new BasicAWSCredentials("AKIA4T4OCILUI2NQVOMT", "iLmmKfz4PEVIsDx7lR0e54ZsS2LjvAkCrWOu2C+2");
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.APSoutheast2  // Set your AWS region
            };

            _dynamoDbClient = new AmazonDynamoDBClient(awsCredentials, config);
        }

        public async Task<AggregateList> GetPurchaseList(string Id, string username)
        {
            var request = new ScanRequest
            {
                TableName = _tableName,
                IndexName = "Username-Id-index",
                FilterExpression = "Id = :id and Username = :username",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":id", new AttributeValue { S = Id } },
                    { ":username", new AttributeValue { S = username } }
                }
            };

            try
            {
                AggregateList aggregateList = new AggregateList();

                var response = await _dynamoDbClient.ScanAsync(request);
                if (response.Items != null && response.Items.Count > 0)
                {
                    var item = response.Items.FirstOrDefault();
                    aggregateList.Id = Id;
                    var ingredientList = item["allIngredientList"].L;
                    foreach (var ingredient in ingredientList)
                    {
                        var ingredientMap = ingredient.M;
                        IngredientPurchase ingredient_from_database = new IngredientPurchase();
                        ingredient_from_database.Unit = ingredientMap["unit"].S;
                        ingredient_from_database.Name = ingredientMap["name"].S;
                        ingredient_from_database.Amount = float.Parse(ingredientMap["amount"].S);
                        ingredient_from_database.purchased = bool.Parse(ingredientMap["purchased"].S);
                        ingredient_from_database.location = ingredientMap["location"].S;
                        ingredient_from_database.source = ingredientMap["source"].S;
                        aggregateList.AllIngredientList.Add(ingredient_from_database);

                    }
                    return aggregateList;
                }
                else
                {
                    return new AggregateList();
                }

            }
            catch (Exception ex)
            {
                // Log the error and return an empty list or handle as needed
                return new AggregateList();  // Handle error gracefully
            }
        }

        public async Task<string> AddPurchaseList(AggregateList aggregateList, string username)
        {

            var putRequest = new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>
        {
                    {"Guid", new AttributeValue { S = Guid.NewGuid().ToString() }  },
                    {"Username", new AttributeValue { S = username } },
            { "Id", new AttributeValue { S = aggregateList.Id } }, // Partition Key (Name)
            { "allIngredientList", new AttributeValue
                {
                    L = aggregateList.AllIngredientList.Select(i => new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {
                            { "unit", new AttributeValue { S = i.Unit } },
                            { "name", new AttributeValue { S = i.Name} },
                            { "amount", new AttributeValue {S = i.Amount.ToString() } },
                            { "purchased", new AttributeValue {S = i.purchased.ToString() } },
                            { "location", new AttributeValue {S = i.location.ToString() } },
                            { "source", new AttributeValue {S = i.source.ToString() } }
                        }
                    }).ToList() // Convert the list of ingredients to a DynamoDB list of maps
                }
            }
        }
            };

            try
            {
                // Perform the DynamoDB PutItemAsync operation
                await _dynamoDbClient.PutItemAsync(putRequest);
                return "Item added"; // Return the choice object after adding it to DynamoDB
            }
            catch (Exception ex)
            {
                // Log the exception and handle any errors
                Console.WriteLine($"Error adding item to DynamoDB: {ex.Message}");
                throw; // Rethrow the exception to handle it at a higher level if needed
            }
        }

        public async Task<string> UpdatePurchaseList(AggregateList aggregateList, string username)
        {
            var guid = await FindId(aggregateList.Id, username);
            var updateRequest = new UpdateItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
            {
                { "Guid", new AttributeValue { S = guid } }  // Partition Key (Id)
            },
                UpdateExpression = "SET allIngredientList = :allIngredientList", // Update the 'Choice' attribute
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":allIngredientList", new AttributeValue
                    {
                        L = aggregateList.AllIngredientList.Select(i => new AttributeValue
                        {
                            M = new Dictionary<string, AttributeValue>
                        {
                            { "unit", new AttributeValue { S = i.Unit } },
                            { "name", new AttributeValue { S = i.Name} },
                            { "amount", new AttributeValue {S = i.Amount.ToString() } },
                            { "purchased", new AttributeValue {S = i.purchased.ToString() } },
                            { "location", new AttributeValue {S = i.location.ToString() } },
                            { "source", new AttributeValue {S = i.source.ToString()!=null? i.source.ToString():"extra"} }
                        }
                    }).ToList()
                    }
                }
            }
            };

            try
            {
                // Perform the DynamoDB UpdateItemAsync operation
                await _dynamoDbClient.UpdateItemAsync(updateRequest);
                return "Item updated"; // Return success message after updating the item
            }
            catch (Exception ex)
            {
                // Log the exception and handle any errors
                Console.WriteLine($"Error updating item in DynamoDB: {ex.Message}");
                throw; // Rethrow the exception to handle it at a higher level if needed
            }
        }

        public async Task<List<string>> GetAllPurchaseList(string username)
        {

            var request = new ScanRequest
            {
                TableName = _tableName,
                FilterExpression = " Username = :username",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":username", new AttributeValue { S = username } }
                    }
            };
            var ids = new List<string>();
            try
            {
                // Perform the scan operation
                var result = await _dynamoDbClient.ScanAsync(request);

                // Iterate over the scan results and extract the 'Id' values
                foreach (var item in result.Items)
                {
                    if (item.ContainsKey("Id"))
                    {
                        ids.Add(item["Id"].S);  // Add the 'Id' value to the list
                    }
                }

                return ids;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning DynamoDB table: {ex.Message}");
                return null;
            }

        }

        public async Task<string> DeletePurchaseList(string Id, string username)
        {
            var guid = await FindId(Id, username);
            var deleteRequest = new DeleteItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
        {
            { "Guid", new AttributeValue { S = guid } }  // Assuming "Name" is the partition key
        }
            };

            try
            {
                // Perform the delete operation
                var response = await _dynamoDbClient.DeleteItemAsync(deleteRequest);

                // Check if the delete operation was successful
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return "Purchase list successfully deleted.";  // Return success message
                }
                else
                {
                    return "Failed to delete the purchase list.";  // If something goes wrong with the delete
                }
            }
            catch (Exception ex)
            {
                // Handle any errors during the delete operation
                return $"Error deleting menu: {ex.Message}";
            }

        }

        private async Task<string> FindId(string id, string username)
        {
            var request = new ScanRequest
            {
                TableName = _tableName,
                IndexName = "Username-Id-index",
                FilterExpression = "Id = :id and Username = :username",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":id", new AttributeValue { S = id } },
                    { ":username", new AttributeValue { S = username } }
                }
            };

            var response = await _dynamoDbClient.ScanAsync(request);
            if (response.Items != null && response.Items.Count > 0)
            {
                var item = response.Items.FirstOrDefault();
                return item["Guid"].S;

            }
            return null;
        }
    }
}
