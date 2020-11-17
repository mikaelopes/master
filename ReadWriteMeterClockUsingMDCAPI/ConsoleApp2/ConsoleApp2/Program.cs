using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Text.Json;
using System.Threading;
using System.Xml;
using doCommandRequest;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isread = false;
            //  string w = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<ResponseMessage xmlns=\"http://iec.ch/TC57/2011/schema/message\" xmlns:m=\"http://iec.ch/TC57/2011/MeterReadings#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://iec.ch/TC57/2011/schema/message Message.xsd\"><Header><Verb>reply</Verb><Noun>MeterReadings</Noun><Revision>2.0</Revision><Timestamp>2020-11-15T12:37:16+04:30</Timestamp><Source>HES</Source><MessageID>530DDA95-2EDD-47E4-B699-C49E1E960B76</MessageID><CorrelationID>1001</CorrelationID></Header><Reply><Result>OK</Result><Error><code>0.0</code></Error></Reply><Payload><m:MeterReadings><m:MeterReading><m:Meter><m:mRID>002009291302</m:mRID></m:Meter><m:Readings><m:reason>inquiry</m:reason><m:ReadingType ref=\"3.0.129.160\"/><m:timePeriod/><m:value>2020-11-15 12:34:31</m:value><m:timeStamp>2020-11-15T12:37:16+04:30</m:timeStamp></m:Readings></m:MeterReading><m:Reading/></m:MeterReadings></Payload></ResponseMessage>";
            string headerSuccess = "---------------SUCCESS READ/WRITE CLOCK DEVICES---------------";
            string headerFail = "---------------FAILED READ/WRITE CLOCK DEVICES---------------";
            string headerStatistic = "---------------STATISTICS";// ---------------";
            //populate the json
            List<deviceId> devices = populateDeviceIdFromJson();
            ///set/get the meters clock

            Console.WriteLine("Type 1 to READ METER CLOCK and 2 to SET METER CLOCK");
            if (Console.ReadLine() == "1") { isread = true;
            headerStatistic += " READING RTC-------------------";
            }
            else { isread = false;
                headerStatistic += " SETTING RTC---------------";
            }
            
            foreach (deviceId device in devices)
            {
                device.refresh();

                try {
                    
                    device.systemClockLastRequest = DateTime.Now;

                    if (isread)
                    {

                        Console.Write("Getting Meter Clock: " + device.serial);
                        device.deviceClock = readDeviceClock(device.serial);
                        device.statusRequest = true;
                    }
                    else {
                        Console.Write("Setting Meter Clock: " + device.serial);
                        device.statusRequest = writeDeviceClockCurrentSystemTime(device.serial);

                    }
              
                    
                    Console.WriteLine(" Result: SUCCESS");
                } catch (Exception e) {
                    // HAVE PROBLEM
                    device.statusRequest = false;
                    Console.WriteLine(" Result: FAILED");
                    Thread.Sleep(6000);
                 //DO NOTHING PROBABLY TIMEOUT
                }
                

            }
            // log to file the result
            LogToFile(headerSuccess);
            var successDevices = 0;
            foreach (deviceId device in devices)
            {
                try
                {
                    // IF NOT TIMEOUT
                    if (device.statusRequest) {
                        LogToFile(device.serial + "," + device.deviceClock + "," + device.systemClockLastRequest);
                        successDevices++;
                    }
                    

                }
                catch (Exception e)
                {

                    //DO NOTHING PROBABLY TIMEOUT
                }

            }
            LogToFile(headerFail);
            var failedDevices = 0;
            foreach (deviceId device in devices)
            {
                try
                {
                    // IF  TIMEOUT
                    if (!device.statusRequest)
                    {
                        LogToFile(device.serial + "," + device.deviceClock + "," + device.systemClockLastRequest);
                        failedDevices++;
                    }


                }
                catch (Exception e)
                {

                    //DO NOTHING PROBABLY TIMEOUT
                }

            }

            LogToFile(headerStatistic);
            LogToFile("Failed Devices: " + failedDevices + " \t Success Devices: " + successDevices + " \t Failure Rate: " + (((double)failedDevices)/((double)failedDevices + (double)successDevices)).ToString("P")) ;
            LogToFile("\r\n\r\n");






        }

        static public List<deviceId> populateDeviceIdFromJson()
        {

            List<deviceId> deviceList = new List<deviceId>();

            using (JsonDocument document = JsonDocument.Parse(new StreamReader("DeviceJson.json").ReadToEnd()))
            {
                JsonElement root = document.RootElement;
                JsonElement devices = root.GetProperty("DeviceList");
                foreach (JsonElement device in devices.EnumerateArray())
                {
                    if (device.TryGetProperty("DeviceId", out JsonElement deviceIdElement))
                    {
                        deviceList.Add(new deviceId(deviceIdElement.GetString()));
                    }

                }
            }

            return deviceList;

        }
        static public DateTime readDeviceClock(string deviceId) {
            //EndPointAddres
            string endPointAddress = "http://192.168.6.171:8090/HES/services/DoCommandRequest";
            string deviceReplyTag = "m:value";
            string statusResultTag = "Result";
            string errorTag = "";
            string clock;
            //soapRequest INIT
            string soapRequest = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?> <RequestMessage xmlns=\"http://iec.ch/TC57/2011/schema/message\" xmlns:m=\"http://iec.ch/TC57/2011/GetMeterReadings#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://iec.ch/TC57/2011/schema/message Message.xsd\"> <Header> <Verb>get</Verb> <Noun>MeterReadings</Noun> <Revision>2.0</Revision> <Timestamp>2015-03-14T20:04:39+04:30</Timestamp> <Source>MDM</Source> <AsyncReplyFlag>false</AsyncReplyFlag> <ReplyAddress>http://ip:port/AmiWeb/services/Metering</ReplyAddress> <AckRequired>false</AckRequired> <MessageID>83c643e6-85c5-43c0-9e0a-fa1deb469b72</MessageID> <CorrelationID>1001</CorrelationID> </Header> <Request> <m:GetMeterReadings> <m:EndDevice> <m:mRID>"
                +deviceId+ //DEVICE ID 
                                 "</m:mRID> </m:EndDevice> <m:ReadingType> <m:Names> <m:name>3.0.129.160</m:name> <m:NameType> <m:name>ReadingType</m:name> </m:NameType> </m:Names> </m:ReadingType> </m:GetMeterReadings> </Request> </RequestMessage>";
            //soapRequest FINISH
            
            //Create Object
            DoCommandRequestClient service = new DoCommandRequestClient(new BasicHttpBinding(), new EndpointAddress(endPointAddress));
            
            //Trigger the request
            var reply =  service.doCommand(soapRequest);
            //convert to xml document
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(reply);

            var resultOfTask = xml.GetElementsByTagName(statusResultTag).Item(0).InnerText;
            clock =  xml.GetElementsByTagName(deviceReplyTag).Item(0).InnerText;

            //dispose service
            service = null;
            //disposing xml
            xml = null;
            //disposing reply
            reply = null;
            //GCDo
            GC.Collect();

           

            return DateTime.Parse(clock);
        }
        static public bool writeDeviceClockCurrentSystemTime(string deviceId)
        {
            //EndPointAddres
            string endPointAddress = "http://192.168.6.171:8090/HES/services/DoCommandRequest";
            string deviceReplyTag = "m:value";
            string statusResultTag = "Result";
            string errorTag = "";
            string clock;
            bool resultOfTask = false;
            //soapRequest INIT

            DoCommandRequestClient service = new DoCommandRequestClient(new BasicHttpBinding(), new EndpointAddress(endPointAddress));
            string soapRequest = "<?xml version=\"1.0\" encoding=\"utf-8\"?> <RequestMessage xmlns=\"http://iec.ch/TC57/2011/schema/message\" xmlns:m=\"http://iec.ch/TC57/2011/EndDeviceControls#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://iec.ch/TC57/2011/schema/message Message.xsd\"> <Header> <Verb>create</Verb> <Noun>EndDeviceControls</Noun> <Revision>2.0</Revision> <Timestamp>2016-01-01T00:00:00+04:30</Timestamp> <Source>MDM</Source> <AsyncReplyFlag>false</AsyncReplyFlag> <ReplyAddress>http://ip:port/AmiWeb/services/Metering</ReplyAddress> <AckRequired>false</AckRequired> <MessageID>83c643e6-85c5-43c0-9e0a-fa1deb469b72</MessageID> <CorrelationID>1001</CorrelationID> </Header> <Payload> <m:EndDeviceControls> <m:EndDeviceControl> <m:EndDeviceControlType ref=\"3.0.129.160\"/> <m:EndDeviceAction> <m:command>"
            
                 +DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+

                 "</m:command> </m:EndDeviceAction> <m:EndDevices> <m:mRID>"
                + deviceId +
                 "</m:mRID> </m:EndDevices> </m:EndDeviceControl> </m:EndDeviceControls> </Payload> </RequestMessage>";
            //soapRequest FINISH

            //Create Object
           
            //Trigger the request
            var reply = service.doCommand(soapRequest);
            //convert to xml document
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(reply);

             resultOfTask = (xml.GetElementsByTagName(statusResultTag).Item(0).InnerText == "OK");
            

            //dispose service
            service = null;
            //disposing xml
            xml = null;
            //disposing reply
            reply = null;
            //GCDo
            GC.Collect();



            return resultOfTask;
        }

        public static void LogToFile(string msg)
        {
            try
            {
                string path = @".\log" + DateTime.Now.ToString("yyyyMMdd") +".txt";
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(msg);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
