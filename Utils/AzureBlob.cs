using System.IO;
using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SimpleEchoBot.Utils
{
    public class AzureBlob
    {
        public static async Task<string> UploadPhoto(string contentUrl,string contentType)
        {
            var contentStream = await ImageStream.GetImageStream(contentUrl);

            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=carsuggestionbotphoto;AccountKey=u/NEb5/lpWQ0zFfdl9+WCXWtonNLKGO3578pBOhxISCtXbu1Sq0vwMoxYG3oozmUAyxCxFHcsia83RCP6wNFPg==;EndpointSuffix=core.windows.net");

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference("userphoto");

            // Retrieve reference to a blob named "myblob".
            var blobName = Guid.NewGuid().ToString();
            if(contentType== "image/png")
            {
                blobName += ".png";
            }
            else
            {
                blobName += ".jpg";
            }
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            // Create or overwrite the "myblob" blob with contents from a local file.
            blockBlob.UploadFromStream(contentStream);
            var url = blockBlob.Uri.AbsoluteUri;

            return url;
        }
    }
}