using Amazon.DynamoDBv2;
using Amazon.S3;
using Amazon;
using System.IO;

namespace Lab4
{
    class Helper
    {
        internal static MemoryStream selectedBookStream;

        public static object help { get; internal set; }

        public AmazonDynamoDBClient Help()
        {
            string awsAccessKey = "AKIAU6GDZVUTLIRLL2O5";
            string awsSecretKey = "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";
            return new AmazonDynamoDBClient(awsAccessKey, awsSecretKey, RegionEndpoint.USEast1);
        }

        public AmazonS3Client HelpS3()
        {
            string awsAccessKey = "AKIAU6GDZVUTLIRLL2O5";
            string awsSecretKey = "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";
            return new AmazonS3Client(awsAccessKey, awsSecretKey, RegionEndpoint.USEast1);
        }
    }
}
