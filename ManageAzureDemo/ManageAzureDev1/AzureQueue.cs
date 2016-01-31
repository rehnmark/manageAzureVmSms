using System;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace ManageAzureVmDemo
{
    public class AzureQueue
    {

        //method used in the program class to do som cleanup. 
        //Removes the old queue and recreates it with the same name. 
        //We will wait 60 seconds before moving on the the next step.
        public static void ManageQueue()
        {

            Console.WriteLine("Starting some queue management");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            ConfigurationManager.ConnectionStrings["AzureStorageConn"].ConnectionString);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue
            CloudQueue queue = queueClient.GetQueueReference("smsqueue");
            Console.WriteLine(queue.Uri);
            Console.WriteLine("If the queue exist -remove it to get rid of junk!!, then wait 60 sec before continue");
            //remove the Queue if it exists

            queue.DeleteIfExists();
            System.Threading.Thread.Sleep(60000);


            queue.CreateIfNotExists();
            Console.WriteLine("Queue created!!");


        }

    }
}

