using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TRP_APPHELPER_DLL
{
    public class clsHelpInfo
    {
        public string strName { get; set; }
        public string strDesc { get; set; }

        public clsHelpInfo(string Name, string Desc)
        {
            this.strName = Name;
            this.strDesc = Desc;
        }   
    }
}
