using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using Newtonsoft.Json;
using Microsoft.Azure.Documents;

namespace KishFirstFun
{
    public class Rating
    {
        public string id { get; set; }
        public string userId { get; set; }
        public string productId { get; set; }
        public DateTime timestamp { get; set; }
        public string locationName { get; set; }
        public int rating { get; set; }
        public string userNotes { get; set; }
    }

    public static class CreateRatings
    {
        
        [FunctionName("CreateRatings")]
        public static async Task<HttpResponseMessage> CreateRating([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, [DocumentDB("openhackdb", "openhackcollection", CreateIfNotExists = true, ConnectionStringSetting = "CosmosDB")] IAsyncCollector<Rating> documents, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string userId = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "userId", true) == 0)
                .Value;
            string productId = req.GetQueryNameValuePairs()
               .FirstOrDefault(q => string.Compare(q.Key, "productId", true) == 0)
               .Value;
            string locationName = req.GetQueryNameValuePairs()
               .FirstOrDefault(q => string.Compare(q.Key, "locationName", true) == 0)
               .Value;
            string rating = req.GetQueryNameValuePairs()
               .FirstOrDefault(q => string.Compare(q.Key, "rating", true) == 0)
               .Value;
            string userNotes = req.GetQueryNameValuePairs()
              .FirstOrDefault(q => string.Compare(q.Key, "userNotes", true) == 0)
              .Value;


            bool blnValid = true;
            string strResponse = "";
            if (userId == null)
            {
                blnValid = false;
                strResponse = "Please pass a userId on the query string or in the request body";
            }
            if (productId == null)
            {
                blnValid = false;
                strResponse = "Please pass a productId on the query string or in the request body";
            }
            if (locationName == null)
            {
                blnValid = false;
                strResponse = "Please pass a locationName on the query string or in the request body";
            }
            if (rating == null)
            {
                blnValid = false;
                strResponse = "Please pass a rating on the query string or in the request body";
            }
            else if ((Convert.ToInt32(rating) < 0) && (Convert.ToInt32(rating) > 5))
            {
                blnValid = false;
                strResponse = "rating should be between 0 and 5";
            }
            if (rating == null)
            {
                blnValid = false;
                strResponse = "Please pass a rating on the query string or in the request body";
            }

            if (blnValid == false)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, strResponse);
            }


            string id = Guid.NewGuid().ToString();
            DateTime timestamp = DateTime.UtcNow;

            var web_response = new WebClient().DownloadString("https://serverlessohuser.trafficmanager.net/api/GetUser?userId=" + userId);

            if (web_response == "Please pass a valid userId on the query string")
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "userId not found");
            }
            if (web_response == "User does not exist")
            {
                return req.CreateResponse(HttpStatusCode.NotFound, "User does not exist");
            }

            /*
            User objUser = JsonConvert.DeserializeObject<User>(web_response);
            userId = objUser.userId;
            objUser = null;
            */
            web_response = new WebClient().DownloadString("https://serverlessohproduct.trafficmanager.net/api/GetProduct?productId=" + productId);

            if (web_response == "Please pass a valid productId on the query string")
            {
                return req.CreateResponse(HttpStatusCode.NotFound, "productId not found");
            }



            /*
            Product objProduct = JsonConvert.DeserializeObject<Product>(web_response);
            productId = objProduct.productId;
            objUser = null;
            */

            Rating objRating = new Rating();
            objRating.id = id;
            objRating.userId = userId;
            objRating.productId = productId;
            objRating.timestamp = timestamp;
            objRating.locationName = locationName;
            objRating.rating = Convert.ToInt32(rating);
            objRating.userNotes = userNotes;

            string JSONResponse = JsonConvert.SerializeObject(objRating);
            dynamic data = JsonConvert.DeserializeObject(JSONResponse);
            await documents.AddAsync(objRating);

            return req.CreateResponse(HttpStatusCode.OK, JSONResponse);



        }

    }
}
