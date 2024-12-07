using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Menu.Models;

namespace Menu.Data
{
    public interface IPurchaseListData
    {
        Task<AggregateList> GetPurchaseList(string name);

        Task<string> AddPurchaseList(AggregateList aggregateList);

        Task<string> UpdatePurchaseList(AggregateList aggregateList);

        Task<List<string>> GetAllPurchaseList();

    }
    public class PurchaseListData : IPurchaseListData
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly string _tableName = "PurchaseList";

        public PurchaseListData()
        {
            var awsCredentials = new BasicAWSCredentials("AKIA4T4OCILUI2NQVOMT", "iLmmKfz4PEVIsDx7lR0e54ZsS2LjvAkCrWOu2C+2");
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.APSoutheast2  // Set your AWS region
            };

            _dynamoDbClient = new AmazonDynamoDBClient(awsCredentials, config);
        }

        public async Task<AggregateList> GetPurchaseList(string Id)
        {
            var request = new GetItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = Id } }
                }
            };
            try
            {
                AggregateList aggregateList = new AggregateList();

                var response = await _dynamoDbClient.GetItemAsync(request);
                if (response.Item != null && response.Item.Count > 0)
                {
                    aggregateList.Id = Id;
                    var ingredientList = response.Item["allIngredientList"].L;
                    foreach (var ingredient in ingredientList)
                    {
                        var ingredientMap = ingredient.M;
                        IngredientPurchase ingredient_from_database = new IngredientPurchase();
                        ingredient_from_database.Unit = ingredientMap["unit"].S;
                        ingredient_from_database.Name = ingredientMap["name"].S;
                        ingredient_from_database.Amount = float.Parse(ingredientMap["amount"].S);
                        ingredient_from_database.purchased = bool.Parse(ingredientMap["purchased"].S);
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

        public async Task<string> AddPurchaseList(AggregateList aggregateList)
        {

            var putRequest = new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>
        {
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
                            { "purchased", new AttributeValue {S = i.purchased.ToString() } }
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

        public async Task<string> UpdatePurchaseList(AggregateList aggregateList)
        {
            var updateRequest = new UpdateItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue { S = aggregateList.Id } }  // Partition Key (Id)
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
                            { "purchased", new AttributeValue {S = i.purchased.ToString() } }
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

        public async Task<List<string>> GetAllPurchaseList()
        {
            var projectionExpression = "Id";
            var scanRequest = new ScanRequest
            {
                TableName = _tableName,
                ProjectionExpression = projectionExpression
            };
            var ids = new List<string>();
            try
            {
                // Perform the scan operation
                var result = await _dynamoDbClient.ScanAsync(scanRequest);

                // Iterate over the scan results and extract the 'Id' values
                foreach (var item in result.Items)
                {
                    if (item.ContainsKey("Id"))
                    {
                        ids.Add(item["Id"].S);  // Add the 'Id' value to the list
                    }
                }

                // If there are more items, use LastEvaluatedKey to paginate
                while (result.LastEvaluatedKey != null && result.LastEvaluatedKey.Count > 0)
                {
                    // Set the exclusive start key to continue from the last evaluated key
                    scanRequest.ExclusiveStartKey = result.LastEvaluatedKey;
                    result = await _dynamoDbClient.ScanAsync(scanRequest);

                    // Add more 'Id' values to the list
                    foreach (var item in result.Items)
                    {
                        if (item.ContainsKey("Id"))
                        {
                            ids.Add(item["Id"].S);
                        }
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
    }
}
