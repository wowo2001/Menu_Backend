using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Xml.Linq;
namespace Menu.Data
{
    public interface IMenuData
    {
        Task<List<Dictionary<string, string>>> GetIngredient(string name);
        Task<List<string>> GetMenu(string category);

        Task<string> EditMenu(EditMenuRequest menu);

        Task<string> AddMenu(EditMenuRequest menu);

        Task<string> DeleteMenu(DeleteMenuRequest menu);
    }
    public class MenuData : IMenuData
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly string _tableName = "Menu";
        public MenuData()
        {
            var awsCredentials = new BasicAWSCredentials("AKIA4T4OCILUI2NQVOMT", "iLmmKfz4PEVIsDx7lR0e54ZsS2LjvAkCrWOu2C+2");
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.APSoutheast2  // Set your AWS region
            };

            _dynamoDbClient = new AmazonDynamoDBClient(awsCredentials, config);
        }

        public async Task<List<Dictionary<string, string>>> GetIngredient(string name)
        {
            {
                var request = new GetItemRequest
                {
                    TableName = _tableName,
                    Key = new Dictionary<string, AttributeValue>
                {
                    { "Name", new AttributeValue { S = name } }
                }
                };

                try
                {
                    var response = await _dynamoDbClient.GetItemAsync(request);
                    if (response.Item != null && response.Item.Count > 0)
                    {
                        // Get the 'Ingredient' list from the response
                        var ingredientList = response.Item["Ingredient"].L;

                        // Prepare a list to hold the ingredient details
                        var ingredients = new List<Dictionary<string, string>>();

                        foreach (var ingredient in ingredientList)
                        {
                            // Each ingredient is a map (M) containing details like Unit, Name, and Amount
                            var ingredientMap = ingredient.M;

                            // Create a dictionary for each ingredient
                            var ingredientDetails = new Dictionary<string, string>
                        {
                            { "Unit", ingredientMap["Unit"].S },
                            { "Name", ingredientMap["Name"].S },
                            { "Amount", ingredientMap["Amount"].S }
                        };

                            // Add the ingredient dictionary to the list
                            ingredients.Add(ingredientDetails);
                        }

                        // Return the list of ingredient dictionaries
                        return ingredients;
                    }
                    else
                    {
                        return new List<Dictionary<string, string>>();  // Return empty list if item not found
                    }
                }
                catch (Exception ex)
                {
                    // Log the error and return an empty list or handle as needed
                    return new List<Dictionary<string, string>>();  // Handle error gracefully
                }
            }
        }

        public async Task<List<string>> GetMenu(string category)
        {
            ScanRequest request;
            if (category != "All")
            {
                request = new ScanRequest
                {
                    TableName = _tableName,
                    FilterExpression = "Category = :category",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":category", new AttributeValue { S = category } }
            }
                };
            }
            else {
                request = new ScanRequest
                {
                    TableName = _tableName
                };
            }
            try
            {
                var response = await _dynamoDbClient.ScanAsync(request);
                if (response.Items != null && response.Items.Count > 0)
                {
                    var menuList = new List<string>();

                    // Iterate through each item in the response
                    foreach (var item in response.Items)
                    {
                        // Extract the "Name" field from each item
                        if (item.ContainsKey("Name") && item["Name"].S != null)
                        {
                            // Add the name to the list
                            menuList.Add(item["Name"].S);
                        }
                    }

                    return menuList; // Return the list of names
                }
                else
                {
                    return new List<string>();  // Return empty list if no matching items are found
                }
            }
            catch (Exception ex)
            {
                // Log the error (e.g., with a logger)
                return new List<string>();  // Return empty list or handle as needed
            }
        }

        public async Task<string> EditMenu(EditMenuRequest menu)
        {
            var updateRequest = new UpdateItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
        {
            { "Name", new AttributeValue { S = menu.Name } }  // Use the 'Name' as the key
        },
                UpdateExpression = "SET Category = :category, Ingredient = :ingredient",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            { ":category", new AttributeValue { S = menu.Category } },
            { ":ingredient", new AttributeValue { L = menu.Ingredient.Select(i => new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {
                        { "Unit", new AttributeValue { S = i.Unit } },
                        { "Name", new AttributeValue { S = i.Name } },
                        { "Amount", new AttributeValue { S = i.Amount } }
                    }
                }).ToList() } }
        }
            };

            try
            {
                var response = await _dynamoDbClient.UpdateItemAsync(updateRequest);
                return "Item successfully updated.";
            }
            catch (Exception ex)
            {
                // Log and handle exception
                return "Item Failed to updated.";
            }
        }

        public async Task<string> AddMenu(EditMenuRequest menu)
        {
            var putRequest = new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>
        {
            { "Name", new AttributeValue { S = menu.Name } }, // Partition Key (Name)
            { "Category", new AttributeValue { S = menu.Category } }, // Category field
            { "Ingredient", new AttributeValue
                {
                    L = menu.Ingredient.Select(i => new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {
                            { "Unit", new AttributeValue { S = i.Unit } },
                            { "Name", new AttributeValue { S = i.Name } },
                            { "Amount", new AttributeValue { S = i.Amount } }
                        }
                    }).ToList() // Convert the list of ingredients to a DynamoDB list of maps
                }
            }
        }
            };

            try
            {
                // Execute the PutItem request
                var response = await _dynamoDbClient.PutItemAsync(putRequest);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return "Item successfully added.";
                }
                else
                {
                    return "Failed to add item.";
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                Console.WriteLine("Error adding item: " + ex.Message);
                return "Error adding item.";
            }
        }

        public async Task<string> DeleteMenu(DeleteMenuRequest menu)
        {
            var deleteRequest = new DeleteItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
        {
            { "Name", new AttributeValue { S = menu.Name } }  // Assuming "Name" is the partition key
        }
            };

            try
            {
                // Perform the delete operation
                var response = await _dynamoDbClient.DeleteItemAsync(deleteRequest);

                // Check if the delete operation was successful
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return "Menu item successfully deleted.";  // Return success message
                }
                else
                {
                    return "Failed to delete the menu item.";  // If something goes wrong with the delete
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
