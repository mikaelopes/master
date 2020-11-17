using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp2
{
    class deviceId
    {

        public string serial;
        public DateTime deviceClock;
        public DateTime systemClockLastRequest;
        public bool statusRequest;

        public deviceId(string serialNumber)
        {

            this.serial = serialNumber;
            


        }

        public void refresh() {
            this.systemClockLastRequest = new DateTime();
            this.deviceClock = new DateTime();
            this.statusRequest = false;
        }

    }
}
