using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace ManageAzureVmDemo
{
    public class ArmFunctions
    {

        static string SubscriptionId = ConfigurationManager.AppSettings["AzureSubscription"];
    
        static Dictionary<string, string> azureVmdictionary = new Dictionary<string, string>();
        static Dictionary<string, string> azureVmStatusdictionary = new Dictionary<string, string>();

        //method to create the Http client and authenticate with azure
        public static async Task AzureMainArmRequest(string inputToArmCall,string inputSrv)
        {
            string tenantId = ConfigurationManager.AppSettings["AzureTenantId"];
            string clientId = ConfigurationManager.AppSettings["AzureClientId"];
            string clientSecret = ConfigurationManager.AppSettings["AzureClientSecret"];

            string token = await AuthenticationHelpers.AcquireTokenBySPN(tenantId, clientId, clientSecret);

            using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.BaseAddress = new Uri("https://management.azure.com/");

                //get all the VM's from the subscription
                var response = await client.GetAsync(
                $"/subscriptions/{SubscriptionId}/providers/Microsoft.Compute/virtualMachines?api-version=2015-06-15");
                
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsAsync<dynamic>();
                    foreach (var item in json.value)
                    {

                    var t = Convert.ToString(item.id);
                    string[] splittedValues = t.Split('/');
                    string resourceGroupName = splittedValues[4];
                    string serverName = splittedValues[8];

                    azureVmdictionary.Add(serverName, resourceGroupName);
                    azureVmStatusdictionary.Add(serverName, Convert.ToString(await getVmStatus(client, resourceGroupName, serverName)));
                    
                    }


                //Call nested functions to do the actual job
     
                if (inputToArmCall == "start" || inputToArmCall == "deallocate")
                {
                    await StartOrStopAzureVm(client, inputSrv.ToLower(),inputToArmCall);
                }

                //send sms with current status of the servers
                if (inputToArmCall == "status")
                {
                    string statusMsg = string.Empty;
                    foreach (var item in azureVmStatusdictionary)
                    {
                        statusMsg += item.Key + "," + item.Value.Replace("VM", "") + "\r\n";

                    }
                    Sms.SendSMS(statusMsg);

                    Console.WriteLine(statusMsg);

                }

            }//End using state


        }//End Method AzureMainArmRequest


        //method to start or stop your virtual machines
        static async Task StartOrStopAzureVm(HttpClient client, string stringInput, string inputToArmCall)
        {

            string armServerName = null;
            string armResourceGroupName = null;
            

            //match the serverinput with the name in the dictionary
            foreach (var i in azureVmdictionary.Where(p => p.Key.Contains(stringInput)))
            {
                armServerName = i.Key.ToString();
                armResourceGroupName = i.Value.ToString();
            }

  
            if (!(armServerName == ""))
            {
                using (var startResponse = await client.PostAsync(
                $"/subscriptions/{SubscriptionId}/resourceGroups/{armResourceGroupName}/providers/Microsoft.Compute/virtualMachines/{armServerName}/{inputToArmCall}?api-version=2015-06-15", null))
                {
                    startResponse.EnsureSuccessStatusCode();
                    if (inputToArmCall == "start")
                    {
                        Console.WriteLine("starting server {0}", armServerName);
                    }
                    if (inputToArmCall == "deallocate")
                    {
                        Console.WriteLine("stopping server {0}", armServerName);
                    }
                    
                }
            }
            else
            {
                Console.WriteLine("blank input");
                return;
            }

        }//end StartOrStopAzureVm

        //method to get the current status of your virtual machines, are the running or not?
        static async Task<string> getVmStatus(HttpClient client, string armResourceGroupName, string armServerName)
        {

            using (var getStatus = await client.GetAsync(
                $"/subscriptions/{SubscriptionId}/resourceGroups/{armResourceGroupName}/providers/Microsoft.Compute/virtualMachines/{armServerName}/InstanceView?api-version=2015-05-01-preview"))
            {
                getStatus.EnsureSuccessStatusCode();

                var json = await getStatus.Content.ReadAsAsync<dynamic>();
               

                foreach (var item in json.statuses)
                {
                    string value = Convert.ToString(item.displayStatus);
                    string outPut = string.Empty;
                    if (value.StartsWith("VM"))
                    {
                        outPut = value;  
                        return outPut;
                    }
                    
                }
                return null;
            }
            
        }
    }//end class

}//End namespace
