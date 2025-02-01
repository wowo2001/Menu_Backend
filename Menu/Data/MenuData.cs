using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Menu.Models;
namespace Menu.Data
{
    public interface IMenuData
    {
        Task<List<Ingredient>> GetIngredient(string name, string username);
        Task<MenuList> GetMenu(string category, string username);

        Task<string> EditMenu(MenuDetails menu, string username);

        Task<string> AddMenu(MenuDetails menu, string username);

        Task<string> DeleteMenu(string name, string username);
    }
    public class MenuData : IMenuData
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly string _tableName = "Menu_New";
        public MenuData()
        {
            var awsCredentials = new BasicAWSCredentials("AKIA4T4OCILUI2NQVOMT", "iLmmKfz4PEVIsDx7lR0e54ZsS2LjvAkCrWOu2C+2");
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.APSoutheast2  // Set your AWS region
            };

            _dynamoDbClient = new AmazonDynamoDBClient(awsCredentials, config);
        }

        public async Task<List<Ingredient>> GetIngredient(string name, string username)
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

            try
            {
                var response = await _dynamoDbClient.ScanAsync(request);

                if (response.Items != null && response.Items.Count > 0)
                {
                    var item = response.Items.FirstOrDefault();
                    // Get the 'Ingredient' list from the response
                    var ingredientList = item["Ingredient"].L;


                    // Prepare a list to hold the ingredient details
                    var ingredients = new List<Ingredient>();

                    foreach (var ingredient in ingredientList)
                    {
                        // Each ingredient is a map (M) containing details like Unit, Name, and Amount
                        var ingredientMap = ingredient.M;
                        float amount;
                        float.TryParse(ingredientMap["Amount"].N, out amount);
                        // Create a dictionary for each ingredient
                        var ingredientDetails = new Ingredient()
                        {
                            Unit = ingredientMap["Unit"].S,
                            Name = ingredientMap["Name"].S,
                            Amount = amount
                        };

                        // Add the ingredient dictionary to the list
                        ingredients.Add(ingredientDetails);
                    }


                    // Return the list of ingredient dictionaries
                    return ingredients;
                }
                else
                {
                    return new List<Ingredient>();  // Return empty list if item not found
                }
            }
            catch (Exception ex)
            {
                // Log the error and return an empty list or handle as needed
                return new List<Ingredient>();  // Handle error gracefully
            }

        }

        public async Task<MenuList> GetMenu(string category, string username)
        {
            ScanRequest request;
            if (category != "All")
            {
                request = new ScanRequest
                {
                    TableName = _tableName,
                    IndexName = "Username-Category-index",
                    FilterExpression = "Category = :category  and Username = :username",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":category", new AttributeValue { S = category } },
                        { ":username", new AttributeValue { S = username } }
                    }
                };
            }
            else {
                request = new ScanRequest
                {
                    TableName = _tableName,
                    FilterExpression = " Username = :username",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":username", new AttributeValue { S = username } }
                    }
                };
            }
            try
            {
                var response = await _dynamoDbClient.ScanAsync(request);
                if (response.Items != null && response.Items.Count > 0)
                {
                    var menuList = new MenuList();

                    // Iterate through each item in the response
                    foreach (var item in response.Items)
                    {
                        // Extract the "Name" field from each item
                        if (item.ContainsKey("Name") && item["Name"].S != null)
                        {
                            // Add the name to the list
                            menuList.Name.Add(item["Name"].S);
                       
                        }
                    }

                    return menuList; // Return the list of names
                }
                else
                {
                    return new MenuList();  // Return empty list if no matching items are found
                }
            }
            catch (Exception ex)
            {
                // Log the error (e.g., with a logger)
                return new MenuList();  // Return empty list or handle as needed
            }
        }

        public async Task<string> EditMenu(MenuDetails menu, string username)
        {
            var id = await FindId(menu.Name, username);
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
            { "Id", new AttributeValue { S = id } }  // Use the 'Name' as the key
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
                        { "Amount", new AttributeValue { N = i.Amount.ToString() } }
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
        }

        public async Task<string> AddMenu(MenuDetails menu, string username)
        {
            var putRequest = new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>
        {
            {"Id", new AttributeValue { S = Guid.NewGuid().ToString() }  },
            {"Username", new AttributeValue { S = username }  },
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
                            { "Amount", new AttributeValue { N = i.Amount.ToString() } }
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

        public async Task<string> DeleteMenu(string name, string username)
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
            { "Id", new AttributeValue { S = id } }  // Assuming "Name" is the partition key
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
