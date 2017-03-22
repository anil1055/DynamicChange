using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TRP_APPHELPER_DLL
{
    public class clsOptionInfo
    {
        public string strConnection { get; set; }
        public string strDesc { get; set; }
        public string strFirm { get; set; }
        public string strPeriod { get; set; }

        public clsOptionInfo(string Desc, string Conn, string Firm, string Period)
        {
            this.strConnection = Conn;
            this.strDesc = Desc;
            this.strFirm = Firm;
            this.strPeriod = Period;
        }
    }
}
