using Amazon.Lambda.Core;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3;
using Amazon.S3.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lab4
{
    public class StepFunctionTasks
    {
        private readonly AmazonRekognitionClient rekognitionClient;
        private readonly AmazonDynamoDBClient dynamoDBClient;
        private readonly AmazonS3Client s3Client;

        public StepFunctionTasks()
        {
            rekognitionClient = new AmazonRekognitionClient();
            dynamoDBClient = new AmazonDynamoDBClient();
            s3Client = new AmazonS3Client();
            InitializeResources().Wait(); // Wait for initialization to complete before proceeding

        }

        private async Task InitializeResources()
        {
            
        }


        public async Task<State> DetectLabelsAndUpdateMetadata(State state, ILambdaContext context)
        {
            string bucket = state.Bucket;
            string key = state.Key;

            try
            {
                // Use Rekognition to detect labels
                DetectLabelsRequest request = new DetectLabelsRequest
                {
                    Image = new Amazon.Rekognition.Model.Image
                    {
                        S3Object = new Amazon.Rekognition.Model.S3Object
                        {
                            Bucket = bucket,
                            Name = key
                        }
                    },
                    MinConfidence = 90
                };

                DetectLabelsResponse response = await rekognitionClient.DetectLabelsAsync(request);

                // Extract labels with confidence greater than 90
                List<string> labels = response.Labels
                    .Where(label => label.Confidence > 90)
                    .Select(label => label.Name)
                    .ToList();

                // Update DynamoDB table with the detected labels
                Table table = Table.LoadTable(dynamoDBClient, "ImageMetadataTable");
                Document document = new Document();
                document["ImageKey"] = key;
                document["Labels"] = string.Join(",", labels);
                await table.PutItemAsync(document);

                return state;
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Error occurred: {ex.Message}");
                throw; 
            }
        }


        // Method to generate thumbnail and store in S3     
        public async Task<State> GenerateThumbnailAndStoreInS3(State state, ILambdaContext context)
        {
            try
            {
                string bucket = state.Bucket;
                string key = state.Key;

                // Download the image from S3
                GetObjectRequest getObjectRequest = new GetObjectRequest
                {
                    BucketName = bucket,
                    Key = key
                };

                using (GetObjectResponse getObjectResponse = await s3Client.GetObjectAsync(getObjectRequest))
                using (Stream inputStream = getObjectResponse.ResponseStream)
                {
                    // Load the image using ImageSharp
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(inputStream))
                    {
                        // Generate thumbnail
                        image.Mutate(x => x.Resize(100, 100));

                        // Upload thumbnail to S3
                        using (MemoryStream outputStream = new MemoryStream())
                        {
                            image.SaveAsJpeg(outputStream);

                            PutObjectRequest putObjectRequest = new PutObjectRequest
                            {
                                BucketName = "thumbnailbucketmaha",
                                Key = $"thumbnails/{key}",
                                InputStream = outputStream,
                                ContentType = "image/jpeg"
                            };

                            await s3Client.PutObjectAsync(putObjectRequest);
                        }
                    }
                }

                return state;
            }
            catch (Exception ex)
            {

                context.Logger.LogLine($"Error occurred: {ex.Message}");
                throw; 
            }
        }
    }
}
