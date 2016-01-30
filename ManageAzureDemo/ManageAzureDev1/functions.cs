using System.Configuration;
using Microsoft.Azure.WebJobs;

namespace ManageAzureVmDemo
{
    
    public class functions
    {
        
        
        //method that the actual webjob uses to monitor the azure storage queue
        public static void QueueTrigger([QueueTrigger("smsqueue")]string message)
        {
            
            Sms s = new Sms();
            s.GetInputToAzure();

            string ArmServer = s.SmsServerName;


            if (s.SmsFunction == "1")
            {
                string StartorStopVm = "start";
                ArmFunctions.AzureMainArmRequest(StartorStopVm, ArmServer).Wait();
            }

            if (s.SmsFunction == "0")
            {
                string StartorStopVm = "deallocate";
                ArmFunctions.AzureMainArmRequest(StartorStopVm, ArmServer).Wait();
            }

            if (s.SmsStatus == "status")
            {
                string getStatus = "status";
                ArmFunctions.AzureMainArmRequest(getStatus, "").Wait();
            }


        }


    }
}
