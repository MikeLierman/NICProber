/*
 
  This Source Code is subject to the terms of the APACHE
  LICENSE 2.0. You can obtain a copy of the terms at
  https://www.apache.org/licenses/LICENSE-2.0
  Copyright (C) 2018 Invise Labs

  Learn more about Invise Labs and our projects by visiting: http://invi.se/labs

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace NICProber
{
    class Program
    {
        static void Main(string[] args)
        {
            /* KEEP Main() CLEAN */
            DoStuff();
        }

        /* VARIBLES */
        private static IPAddress defaultGateway = null;
        private static NetworkInterface NIC = null;
        private static String connectionType = "";
        private static bool checkFailed = false;
        private static bool fatalError = false;

        private static void DoStuff()
        {
            /* INITAL SETUP */
            GetDefaultGateway();

            /* LET'S TEST FOR SOME THINGS NOW */
            StartTests();

            /* Return the exit code. */
            if (checkFailed) { Console.WriteLine("CHECK FAILED;"); Environment.Exit(-3); }
            else { Console.WriteLine("CHECK PASSED;"); Environment.Exit(0); }
        }

        private static void StartTests()
        {
            if (NIC == null)
            {
                checkFailed = false;
                fatalError = true;
                return;
            }
            else
            { 
                /* DETERMINE SPEED NIC IS REPORTING */
                long cardSpeedMbps = Convert.ToInt32(NIC.Speed / 1000000); //- Speed from bits to megabits
                string linkSpeed = "";
                if (cardSpeedMbps > 9800 && cardSpeedMbps < 11100) { linkSpeed = "10.0 Gbps"; }
                else if (cardSpeedMbps > 999 && cardSpeedMbps < 1100) { linkSpeed = "1.0 Gbps"; }
                else { linkSpeed = cardSpeedMbps + " Mbps"; }

                /* DETERMINE TYPE OF CONNECTION */
                if(NIC.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                { connectionType = "WiFi"; }
                else
                { connectionType = "Wired"; }

                /* WIRED ETHERNET NOT RUNNING AT GIGABIT SPEED IS NOT OKAY IN 2018 */
                if(connectionType=="Wired" && cardSpeedMbps<950)
                { checkFailed = true; }

                Console.WriteLine(connectionType + " @ " + linkSpeed + "; ");
            }
        }

        private static void GetDefaultGateway()
        {
            var cards = NetworkInterface.GetAllNetworkInterfaces().ToList();

            if (cards.Any())
            {
                foreach (var card in cards)
                {
                    if (card == null)
                    { continue; }

                    if (card.Name.ToLower().Contains("virtual"))
                    { continue; }
                    if (card.Description.ToLower().Contains("virtual"))
                    { continue; }
                    if (card.Name.ToLower().Contains("hamachi"))
                    { continue; }
                    if (card.Description.ToLower().Contains("hamachi"))
                    { continue; }
                    if (card.Name.ToLower().Contains("virtualbox"))
                    { continue; }
                    if (card.Description.ToLower().Contains("virtualbox"))
                    { continue; }
                    if (card.Name.ToLower().Contains("vmware"))
                    { continue; }
                    if (card.Description.ToLower().Contains("vmware"))
                    { continue; }

                    if (card.OperationalStatus.HasFlag(OperationalStatus.Down))
                    { continue; }

                    var props = card.GetIPProperties();
                    if (props == null)
                    { continue; }

                    var gateways = props.GatewayAddresses;
                    if (!gateways.Any())
                    { continue; }

                    var gateway =
                        gateways.FirstOrDefault(g => g.Address.AddressFamily.ToString() == "InterNetwork");
                    if (gateway == null)
                    { continue; }

                    defaultGateway = gateway.Address;
                    NIC = card;
                    break;
                };
            }
        }
    }
}
