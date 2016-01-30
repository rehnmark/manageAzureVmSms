using System;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace ManageAzureVmDemo
{
    public class AzureQueue
    {

        //method used in the program class to do som cleanup. 
        //Removes the old queue and recreates it with the same name
        public static void ManageQueue()
        {

            Console.WriteLine("Starting some queue management");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.ConnectionStrings["AzureStorageConn"].ConnectionString);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue
            CloudQueue queue = queueClient.GetQueueReference("smsqueue");
            Console.WriteLine(queue.Uri);
            Console.WriteLine(" Queue exist -remove it to get rid of junk!!");
            //remove the Queue if it exists

            queue.DeleteIfExists();
            System.Threading.Thread.Sleep(60000);


            queue.CreateIfNotExists();
            Console.WriteLine("Queue created!!");


        }

    }
}

