using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

[assembly: Parallelize(Workers = 10, Scope = ExecutionScope.MethodLevel)]
namespace Homework2_1
{
    [TestClass]
    public class APIHomeworkTest
    {
        private static HttpClient httpClient;

        private static readonly string BaseURL = "https://petstore.swagger.io/v2";

        private static readonly string PetsEndpoint = "/pet";

        private static string GetURL(string endpoint) => $"{BaseURL}{endpoint}";

        private static Uri GetURI(string endpoint) => new Uri(GetURL(endpoint));

        private readonly List<PetModel> cleanUpList = new List<PetModel>();

        [TestInitialize]
        public void TestInitialize()
        {
            httpClient = new HttpClient();
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            foreach (var data in cleanUpList)
            {
                var httpResponse = httpClient.DeleteAsync(GetURL($"{PetsEndpoint}/{data.Id}"));
            }
        }

        [TestMethod]
        public async Task PutMethod()
        {
            #region CREATE DATA AND SEND POST REQUEST

            // Create Json Object
            List<Category> tags = new List<Category>();
            tags.Add(new Category
            {
                Id = 1,
                Name = "Test Tag"
            });

            // Create Json Object
            PetModel petData = new PetModel()
            {
                Id = 2521,
                Category = new Category()
                {
                    Id = 2521,
                    Name = "Test Category",
                },
                Name = "Zoe.put",
                PhotoUrls = new string[]
                {
                    "string"
                },
                Tags = tags,
                Status = "available"
            };

            // Serialize Content
            var request = JsonConvert.SerializeObject(petData);
            var postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Request
            await httpClient.PostAsync(GetURL(PetsEndpoint), postRequest);

            #endregion

            #region GET ID OF THE CREATED DATA

            // Get Request
            var getResponse = await httpClient.GetAsync(GetURI($"{PetsEndpoint}/{petData.Id}"));

            // Deserialize Content
            var listPetData = JsonConvert.DeserializeObject<PetModel>(getResponse.Content.ReadAsStringAsync().Result);

            // Filter created data
            var createdPetData = listPetData.Id;

            #endregion

            #region SEND PUT REQUEST TO UPDATE DATA

            // Update value of petData
            petData = new PetModel()
            {
                Id = petData.Id,
                Category = new Category()
                {
                    Id = petData.Category.Id,
                    Name = petData.Category.Name,
                },
                Name = "Zoe.put.updated",
                PhotoUrls = new string[]
                {
                    petData.PhotoUrls[0]
                },
                Tags = tags,
                Status = "pending"
            };

            // Serialize Content
            request = JsonConvert.SerializeObject(petData);
            postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Put Request
            var httpResponse = await httpClient.PutAsync(GetURL($"{PetsEndpoint}"), postRequest);
            
            // Get Status Code
            var statusCode = httpResponse.StatusCode;

            #endregion

            #region GET UPDATED DATA

            // Get Request
            getResponse = await httpClient.GetAsync(GetURI($"{PetsEndpoint}/{petData.Id}"));

            // Deserialize Content
            listPetData = JsonConvert.DeserializeObject<PetModel>(getResponse.Content.ReadAsStringAsync().Result);

            // Filter created data
            createdPetData = listPetData.Id;
            string updatedPetName = listPetData.Name;
            string updatedPetStatus = listPetData.Status;

            string createdPetPhotoUrls = listPetData.PhotoUrls[0];

            long createdPetCategoryId = listPetData.Category.Id;
            string createdPetCategoryName = listPetData.Category.Name;
            
            long createdPetTagsId = listPetData.Tags[0].Id;
            string createdPetTagsName = listPetData.Tags[0].Name;


            #endregion

            #region CLEANUP DATA

            // Add data to cleanup list
            cleanUpList.Add(listPetData);

            #endregion

            #region ASSERTION

            // Assertion
            Assert.AreEqual(HttpStatusCode.OK, statusCode, "Status code is not equal to 200");
            Assert.AreEqual(petData.Id, createdPetData, "ID does not match");
            Assert.AreEqual(petData.Name, updatedPetName, "Name does not match");
            Assert.AreEqual(petData.Status, updatedPetStatus, "Status does not match");

            Assert.AreEqual(petData.PhotoUrls[0], createdPetPhotoUrls, "PhotoUrls does not match");

            Assert.AreEqual(petData.Category.Id, createdPetCategoryId, "Category Id does not match");
            Assert.AreEqual(petData.Category.Name, createdPetCategoryName, "Category Name does not match");

            Assert.AreEqual(petData.Tags[0].Id, createdPetTagsId, "Tags Id does not match");
            Assert.AreEqual(petData.Tags[0].Name, createdPetTagsName, "Tags Name does not match");


            #endregion
        }
    }
}
