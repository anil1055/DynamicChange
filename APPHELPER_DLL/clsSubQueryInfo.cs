using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TRP_APPHELPER_DLL
{
    public class clsSubQueryInfo
    {
        public string strName { get; set; }
        public string strDesc { get; set; }
        public string strContent { get; set; }

        public clsSubQueryInfo(string Name, string Desc, string Content)
        {
            this.strName = Name;
            this.strDesc = Desc;
            this.strContent = Content;
        }

        public clsSubQueryInfo()
        {

        }   
    }
}
