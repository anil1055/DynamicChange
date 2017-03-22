using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRP_APPHELPER_DLL
{
    public class clsQueryInfo
    {
        public string strName{ get; set; }
        public string strDescription { get; set; }
        public string strQuery { get; set; }
        public bool blnSetup { get; set; }
        public List<clsParametre> lstParameters { get; set; }

        public clsQueryInfo()
        {
            lstParameters = new List<clsParametre>();
        }

        public clsQueryInfo(string Name, string Desc, string Query, bool Setup, List<clsParametre> Parameters)
        {
            this.strName = Name;
            this.strDescription = Desc;
            this.strQuery = Query;
            this.lstParameters = Parameters;
            this.blnSetup = Setup;
        }        
    }

    public class clsParametre
	{
        public string strName { get; set; }
        public string strValue { get; set; }

        public clsParametre(string Name, string Value)
        {
            this.strName = Name;
            this.strValue = Value;
        }
        
	}
}
