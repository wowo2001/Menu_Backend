﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Menu.Models;
using System.Xml.Linq;

namespace Menu.Data
{
    public interface IShopListData
    {
        Task<WeeklyChoice> GetShopList(string name);
        Task<string> AddShopList(WeeklyChoice choice);

        Task<string> UpdateShopList(WeeklyChoice choice);

        Task<string> DeleteShopList(string Id);
    }
    public class ShopListData : IShopListData
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly string _tableName = "ShopList";
        public ShopListData()
        {
            var awsCredentials = new BasicAWSCredentials("AKIA4T4OCILUI2NQVOMT", "iLmmKfz4PEVIsDx7lR0e54ZsS2LjvAkCrWOu2C+2");
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.APSoutheast2  // Set your AWS region
            };

            _dynamoDbClient = new AmazonDynamoDBClient(awsCredentials, config);
        }

        public async Task<WeeklyChoice> GetShopList(string Id)
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
                var choice = new WeeklyChoice();
               
                var response = await _dynamoDbClient.GetItemAsync(request);
                if (response.Item != null && response.Item.Count > 0)
                {
                    choice.Id = Id;
                    var dailyChoiceList = response.Item["Choice"].L;
                    foreach (var dailyChoice in dailyChoiceList)
                    {
                        var dailyChoiceMap = dailyChoice.M;
                        var existingChoice = new DailyChoice
                        {
                            Day = dailyChoiceMap["Day"].S, // Get the day (e.g., "Monday")
                        };
                        var dishList = dailyChoiceMap["Dish"].L;
                        foreach (var dish in dishList)
                        {
                            string dishName = dish.S;
                            existingChoice.Dish.Add(dishName);
                        }
                        choice.MyChoice.Add(existingChoice);

                    }
                    return choice;
                }
                else
                {
                    return new WeeklyChoice();
                }

            }
            catch (Exception ex)
            {
                // Log the error and return an empty list or handle as needed
                return new WeeklyChoice();  // Handle error gracefully
            }
        }

        public async Task<string> AddShopList(WeeklyChoice choice)
        {
                var putRequest = new PutItemRequest
                {
                    TableName = _tableName,
                    Item = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = choice.Id } }, // Partition Key (Name)
            { "Choice", new AttributeValue
                {
                    L = choice.MyChoice.Select(i => new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {
                            { "Day", new AttributeValue { S = i.Day } },
                            { "Dish", new AttributeValue
                                    {
                                        L = i.Dish.Select(d => new AttributeValue { S = d }).ToList()
                                    }
                            }

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

        public async Task<string> UpdateShopList(WeeklyChoice choice)
        {
            int emptyIndex = -1;
            for (int i = 0; i < choice.MyChoice.Count; i++)
            {
                if (choice.MyChoice[i].Dish.Count == 0)
                {
                    emptyIndex = i;
                }
            }
            if (emptyIndex != -1) //If that day shoplist is empty, remove that day from weekly choice list
            {
                choice.MyChoice.RemoveAt(emptyIndex);
            }
            if (choice.MyChoice.Count == 0) //If weekly choice is empty is empty after remove that day shoplist, then remove the whole list
            {
                await DeleteShopList(choice.Id);
                return "No shoplist in this menu id is available, menu id is deleted";
            }
            else
            {
                var updateRequest = new UpdateItemRequest
                {
                    TableName = _tableName,
                    Key = new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue { S = choice.Id } }  // Partition Key (Id)
            },
                    UpdateExpression = "SET Choice = :choice", // Update the 'Choice' attribute
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":choice", new AttributeValue
                    {
                        L = choice.MyChoice.Select(i => new AttributeValue
                        {
                            M = new Dictionary<string, AttributeValue>
                            {
                                { "Day", new AttributeValue { S = i.Day } },
                                { "Dish", new AttributeValue
                                    {
                                        L = i.Dish.Select(d => new AttributeValue { S = d }).ToList()
                                    }
                                }
                            }
                        }).ToList() // Convert list of choices to DynamoDB list of maps
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
        }

        public async Task<string> DeleteShopList(string Id)
        {
            var deleteRequest = new DeleteItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = Id } }  // Assuming "Name" is the partition key
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
