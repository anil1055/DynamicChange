using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TRP_APPHELPER_DLL;

namespace TRP_APPHELPER_IDE
{
    public partial class frmNodeEditorView : Form
    {
        private string strFile { get; set; }
        public clsNodeInfo context { get; set; }
        public clsAppHelper cAppHelper { get; set; }
        public frmMainView frmMainView { get; set; }

        public frmNodeEditorView()
        {
            InitializeComponent();
        }

        public frmNodeEditorView(clsAppHelper cAppHelp, frmMainView frmMain)
        {
            frmMainView = frmMain;
            cAppHelper = cAppHelp;                
            InitializeComponent();
            controlNodeEditor.txtNodeDosya.Text = cAppHelper.strNodeFileName;
            controlNodeEditor.gcQueryResult.DataSource = null;
            context = new clsNodeInfo(cAppHelper);  
        }

        
        private void NodeEditor_Load(object sender, EventArgs e)
        {
            controlNodeEditor.nodesControl.Context = context;
            controlNodeEditor.nodesControl.OnNodeContextSelected += NodesControlOnOnNodeContextSelected;

            try
            {
                string strOut;
                cAppHelper.LoadProject(cAppHelper.strPath, out strOut);

                byte[] bytes = cAppHelper.GetWorkFlow(cAppHelper.strNodeFileName);
                controlNodeEditor.nodesControl.Deserialize(bytes);
                //Deserialize(File.ReadAllBytes("_EXT/NodeEditor/" + strFile))
            }
            catch (System.Exception exp)
            {
                System.Console.WriteLine("{0}", exp.Message);
                return;
            }

        }

        private void NodesControlOnOnNodeContextSelected(object o)
        {
            controlNodeEditor.propertyGrid.SelectedObject = o;
        }
    }
}
