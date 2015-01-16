using System;
using System.Collections.Generic;
using System.Text;

using Org.Zoolu.Sip.Address;
using Org.Zoolu.Sip.Authentication;
using Org.Zoolu.Sip.Dialog;
using Org.Zoolu.Sip.Header;
using Org.Zoolu.Sip.Message;
using Org.Zoolu.Sip.Provider;
using Org.Zoolu.Sip.Transaction;
using Org.Zoolu.Tools;

using Org.Zoolu.Net;

using Android.Preference;


namespace dot42SipTester1
{
    static class clsSIPTest
    {
        public static bool RegisterExt(string sExt)
        {
            try
            {
                IpAddress ip;
                int iPort = 5060;
                String sRealm = "10.0.0.8";
                String sFromUrl = "<sip:" + sExt + "@" + sRealm + ":" + iPort.ToString() + ">";    // FROM
                String toUrl = "<sip:" + "1000@" + sRealm + ":" + iPort.ToString() + ">";          // TO

                if (!SipStack.IsInit())
                {
                    SipStack.Init();
                }

                ip = IpAddress.GetLocalHostAddress();

                // Having the following line enabled creates the compiler error:
                SipProvider sipProvider = new SipProvider(ip.ToString(), iPort);

                /*
                Error	11	Error while compiling onReceivedMessage 
                * (Lorg/zoolu/sip/provider/Transport;Lorg/zoolu/sip/message/Message;)V 
                * in
                * org/zoolu/sip/provider/SipProvider: 
                * Unsupported type org/zoolu/sip/header/ViaHeader	
                * dot42SipTester1
                */

                return true;
            }
            catch
            {
                return false;
            }

           

        }

  
    }
}
