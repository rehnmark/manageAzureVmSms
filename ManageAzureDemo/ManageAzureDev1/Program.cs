using Microsoft.Azure.WebJobs;
using System.Configuration;

namespace ManageAzureVmDemo
{
    class Program
    {

       
        static void Main(string[] args)
        {
            
            
            //run this first to remove the old queue if it exists and create a new clean one.
            AzureQueue.ManageQueue();

            JobHostConfiguration config = new JobHostConfiguration();
            config.Queues.MaxDequeueCount = 1;

            var host = new JobHost();

            host.RunAndBlock();

        }
    }
}
