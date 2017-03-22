using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Xml;
using TRP_APPHELPER_DLL;

namespace TRP_APPHELPER_IDE
{
    public partial class ucNodeUser : UserControl
    {
        public clsAppHelper cAppHelper { get; set; }
        public frmMainView frmMainView { get; set; }

        public ucNodeUser(clsAppHelper cAppHelp, frmMainView frmMain)
        {
            frmMainView = frmMain;
            cAppHelper = cAppHelp;
            InitializeComponent();
        }

        private void NodeUser_Load(object sender, EventArgs e)
        {

        }

        private void buttonProcess_Click(object sender, EventArgs e)
        {
            nodesControl.Execute();

            if (clsNodeInfo.dtResult != null)
            {
                gcQueryResult.DataSource = clsNodeInfo.dtResult;
                gvQuery.OptionsView.ShowGroupPanel = false;
                gvQuery.OptionsView.ColumnAutoWidth = false;
                gvQuery.OptionsBehavior.Editable = false;
                gvQuery.PopulateColumns();
                gvQuery.BestFitColumns();
                gcQueryResult.ForceInitialize();

                clsNodeInfo.dtResult = new DataTable();
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (! txtNodeDosya.Text.Equals(""))
            {
                byte[] byVeri = nodesControl.Serialize();

                string strOut;
                cAppHelper.LoadProject(cAppHelper.strPath, out strOut);
                cAppHelper.TransactionWorkFlow(txtNodeDosya.Text, byVeri);

                ByteArrayToFile("_EXT/NodeEditor/" + txtNodeDosya.Text, byVeri);

                frmMainView.fncCreateTreeView();
            }                     
        }

        public bool ByteArrayToFile(string _FileName, byte[] _ByteArray)
        {
            try
            {
                // Open file for reading
                System.IO.FileStream _FileStream =
                   new System.IO.FileStream(_FileName, System.IO.FileMode.Create,
                                            System.IO.FileAccess.Write);

                _FileStream.Write(_ByteArray, 0, _ByteArray.Length);
                _FileStream.Close();

                return true;
            }
            catch (Exception _Exception)
            {
                Console.WriteLine("Exception caught in process: {0}",
                                  _Exception.ToString());
            }

            return false;
        }
    }
}
