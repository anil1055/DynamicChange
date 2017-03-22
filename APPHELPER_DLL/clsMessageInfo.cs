using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TRP_APPHELPER_DLL
{
    public class clsMessageInfo
    {
        public string strID { get; set; }
        public string strContent { get; set; }

        public clsMessageInfo(string ID, string Content)
        {
            this.strID = ID;
            this.strContent = Content;
        }
    }
}
