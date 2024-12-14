using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Menu.Models;
using System.Xml.Linq;

namespace Menu.Data
{
    public interface ILocationData
    {
        Task<string> GetLocationData(string name);

        Task<string> AddLocationData(NameLocation nameLocation);

        Task<string> UpdateLocationData(NameLocation nameLocation);

        Task<List<string>> GetAllLocationData();

        Task<string> DeleteLocationData(string name);

    }
    public class LocationData : ILocationData
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly string _tableName = "NameLocation";

        public LocationData()
        {
            var awsCredentials = new BasicAWSCredentials("AKIA4T4OCILUI2NQVOMT", "iLmmKfz4PEVIsDx7lR0e54ZsS2LjvAkCrWOu2C+2");
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.APSoutheast2  // Set your AWS region
            };

            _dynamoDbClient = new AmazonDynamoDBClient(awsCredentials, config);
        }

        public async Task<string> GetLocationData(string name)
        {
            var request = new GetItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Name", new AttributeValue { S = name } }
                }
            };
            string location = "";
            try
            {
                var response = await _dynamoDbClient.GetItemAsync(request);
                if (response.Item != null && response.Item.Count > 0)
                {
                    location = response.Item["Location"].S;
                    return location;
                }
                else
                {
                    return location;
                }

            }
            catch (Exception ex)
            {
                throw;  
            }
        }

        public async Task<string> AddLocationData(NameLocation nameLocation)
        {

            var putRequest = new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>
            {
                { "Name", new AttributeValue { S = nameLocation.Name } }, // Partition Key (Name)
                { "Location", new AttributeValue { S = nameLocation.Location } }

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

        public async Task<string> UpdateLocationData(NameLocation nameLocation)
        {
            var updateRequest = new UpdateItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
            {
                { "Name", new AttributeValue { S = nameLocation.Name } }  // Partition Key (Id)
            },
                UpdateExpression = "SET #Location = :Location", // Update the 'Choice' attribute
                ExpressionAttributeNames = new Dictionary<string, string>
        {
            { "#Location", "Location" } // Map the reserved 'Location' to '#Location'
        },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            { ":Location", new AttributeValue { S = nameLocation.Location } } // Use NameLocation.Location
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

        public async Task<List<string>> GetAllLocationData()
        {
            var projectionExpression = "Name";
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
                    if (item.ContainsKey("Name"))
                    {
                        ids.Add(item["Name"].S);  // Add the 'Id' value to the list
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
                        if (item.ContainsKey("Name"))
                        {
                            ids.Add(item["Name"].S);
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

        public async Task<string> DeleteLocationData(string name)
        {
            var deleteRequest = new DeleteItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
        {
            { "Name", new AttributeValue { S = name } }  // Assuming "Name" is the partition key
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
    }
}
