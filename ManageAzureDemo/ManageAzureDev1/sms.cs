using System;
using System.Collections.Generic;
using Twilio;
using System.Configuration;

namespace ManageAzureVmDemo
{
    public class Sms
    {
        public string SmsFunction { get; set; }
        public string SmsServerName { get; set; }
        public string SmsHelp { get; set; }
        public string SmsStatus { get; set; }

        static string accountSid = ConfigurationManager.AppSettings["SMSaccountSid"];
        static string authToken = ConfigurationManager.AppSettings["SMSauthToken"];
        static string twilioPhoneNumber = ConfigurationManager.AppSettings["SMSTwilioNumber"];
        static string smsRecipient = ConfigurationManager.AppSettings["SMSRecipient"];

        //Method that validates the input
        static bool ValidateSmsInput(string input)
        {
            string splitValue = ",";
            if (input.Contains(splitValue))
            {
                return true;
            }

            return false;
        }


        //method to split the input
        static string[] SplitSmsInput(string input)
        {

            string[] sInput = input.Split(',');

            return sInput;
        }

        //method to send help
        static string SmsHelpMsg()
            {
                string help = "Starting servers:\r\n(sms:1,servername)\r\nStopping servers:\r\n(sms:0,servername)\r\nGet status of Vm:\r\n(sms:status)";

                return help;     
            }

        //method to send sms using Twilio
        public static void SendSMS(string msg)
        {
            
            var twilio = new TwilioRestClient(accountSid, authToken);
            var message = twilio.SendMessage(
                from: twilioPhoneNumber,
                to: smsRecipient,
                body: msg);
          
        }

        //method to get sms from Twilio
        public static string GetSms()
        {

             var twilio = new TwilioRestClient(accountSid, authToken);

            // Build the parameters 
            var options = new MessageListRequest();
            List<string> msg = new List<string>();

            var messages = twilio.ListMessages(options);
            foreach (var message in messages.Messages)
            {
               
                if (message.To == twilioPhoneNumber)
                {
                    msg.Add(message.Body);
                }
            }

            //Get the last sms
            string smsInput = msg[0];
         
            return smsInput;
        }

        //method to pupulate properties used by the functions class
        public void GetInputToAzure()
        {

            //Create input to stop and start methods

            if (GetSms().StartsWith("0") || GetSms().StartsWith("1"))
            {
                //validate that the input can be splitted
                if (ValidateSmsInput(GetSms()))
                {
                    string[] smsInput = SplitSmsInput(GetSms());
                    SmsFunction = smsInput[0];
                    SmsServerName = smsInput[1];

                    Console.WriteLine("sms method {0}", SmsServerName);

                }
                else
                {
                    Console.WriteLine("incorrect input - does not contain ','. Input value is: {0}, need some help? SMS helpme", GetSms());
                    return;
                }
            }

            //create input to information methods
            else if (GetSms().ToLower() == "status")
            {
                if (ValidateSmsInput(GetSms()) == false)
                {
                    SmsStatus = GetSms().ToLower();
                    Console.WriteLine(GetSms());
                }
            }
               
            else if (GetSms().ToLower().StartsWith("helpme"))
            {
                SmsHelp = SmsHelpMsg();
                SendSMS(SmsHelp);
                return;
            }
                
            else
            {
                Console.WriteLine("incorrect input: {0}, need some help? SMS helpme", GetSms());
                return;
            }


        }



       

        
    }
}
