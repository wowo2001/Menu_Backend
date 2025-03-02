using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Menu.Models;
using System.Xml.Linq;

namespace Menu.Data
{
    public interface ILocationData
    {
        Task<string> GetLocationData(string name, string username);

        Task<string> AddLocationData(NameLocation nameLocation, string username);

        Task<string> UpdateLocationData(NameLocation nameLocation, string username);

        Task<string> DeleteLocationData(string name, string username);

    }
    public class LocationData : ILocationData
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly string _tableName = "Name_Location";

        public LocationData()
        {
            var awsCredentials = new BasicAWSCredentials("AKIA4T4OCILUI2NQVOMT", "iLmmKfz4PEVIsDx7lR0e54ZsS2LjvAkCrWOu2C+2");
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.APSoutheast2  // Set your AWS region
            };

            _dynamoDbClient = new AmazonDynamoDBClient(awsCredentials, config);
        }

        public async Task<string> GetLocationData(string name, string username)
        {
            var request = new ScanRequest
            {
                TableName = _tableName,
                IndexName = "Username-Name-index",
                FilterExpression = "#name = :name and Username = :username",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#name", "Name" }  // Alias the reserved keyword "Name" to "#name"
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":name", new AttributeValue { S = name } },
                    { ":username", new AttributeValue { S = username } }
                }
            };
            string location = "";
            try
            {
                var response = await _dynamoDbClient.ScanAsync(request);
                if (response.Items != null && response.Items.Count > 0)
                {
                    // Assuming you are looking for a single item in the response
                    var item = response.Items.FirstOrDefault();  // Getting the first (or only) item from the list

                    if (item != null && item.ContainsKey("Location"))
                    {
                        // Access the "Location" value
                        location = item["Location"].S;
                    }
                    
                }
                return location;  // Return the "Location" value

            }
            catch (Exception ex)
            {
                throw;  
            }
        }

        public async Task<string> AddLocationData(NameLocation nameLocation, string username)
        {

            var putRequest = new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue { S = Guid.NewGuid().ToString() } },
                { "Name", new AttributeValue { S = nameLocation.Name } }, // Partition Key (Name)
                { "Location", new AttributeValue { S = nameLocation.Location } },
                { "Username", new AttributeValue { S = username } }

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

        public async Task<string> UpdateLocationData(NameLocation nameLocation, string username)
        {
            var id = await FindId(nameLocation.Name, username);
            if (id == null)
            {
                return "Item not found";
            }
            else
            {
                var updateRequest = new UpdateItemRequest
                {
                    TableName = _tableName,
                    Key = new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue { S = id } },  // Partition Key (Id)
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
        }

        

        public async Task<string> DeleteLocationData(string name, string username)
        {
            var id = await FindId(name, username);
            if (id == null)
            {
                return "Item not found";
            }
            else
            {
                var deleteRequest = new DeleteItemRequest
                {
                    TableName = _tableName,
                    Key = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = id } },  // Assuming "Name" is the partition key
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

        private async Task<string> FindId(string name, string username)
        {
            var request = new ScanRequest
            {
                TableName = _tableName,
                IndexName = "Username-Name-index",
                FilterExpression = "#name = :name and Username = :username",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#name", "Name" }  // Alias the reserved keyword "Name" to "#name"
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":name", new AttributeValue { S = name } },
                    { ":username", new AttributeValue { S = username } }
                }
            };

            var response = await _dynamoDbClient.ScanAsync(request);
            if (response.Items != null && response.Items.Count > 0)
            {
                var item = response.Items.FirstOrDefault();
                return item["Id"].S;

            }
            return null;
        }
    }
}
