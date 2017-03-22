using CloneControl;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using ScintillaNET;
using ScintillaNET.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using TRP_APPHELPER_DLL;

namespace TRP_APPHELPER_IDE
{
    public partial class frmMainView : Form
    {
        clsAppHelper cAppHelper;
        List<KeyValuePair<string, List<Control>>> lstTabPageClone;
        List<string> lstOpenTabQuery;
        List<Control> lstControl;
        DataTable dtTable;
        ScintillaNET.Scintilla scTxtQuery, scTxtSubQuery;
        enum eScintillaSecim { TabloIsmi, BoslukBirak };
        eScintillaSecim enSecim;
        string strTable = "";

        public void fncCreateTreeView()
        {
            #region Treeview'in List<clsSorguInfo> ya göre oluşturulması
            tvMainMenu.Nodes.Clear();

            tvMainMenu.Nodes.Add("Ayarlar");
            tvMainMenu.Nodes.Add("Mesajlar");
            tvMainMenu.Nodes.Add("Tablolar");
            tvMainMenu.Nodes.Add("Sorgular");

            //Sorgu verilerini Xml'den doldurma
            cAppHelper.GetWorkingQueryInfo();
            if (cAppHelper.lstWorkingQuery != null)
            {
                int index = 0, icIndex = 0;
                //Treeviewin sorgu classına göre oluşturulması
                foreach (var item in cAppHelper.lstWorkingQuery)
                {
                    icIndex = 2;
                    tvMainMenu.Nodes[3].Nodes.Add(new TreeNode(item.strName));
                    tvMainMenu.Nodes[3].Nodes[index].Nodes.Add(new TreeNode("Aciklama"));
                    tvMainMenu.Nodes[3].Nodes[index].Nodes[0].Nodes.Add(new TreeNode(item.strDescription));
                    tvMainMenu.Nodes[3].Nodes[index].Nodes.Add(new TreeNode("Sorgu"));
                    tvMainMenu.Nodes[3].Nodes[index].Nodes[1].Nodes.Add(new TreeNode(item.strQuery));
                    foreach (var icerik in item.lstParameters)
                    {
                        tvMainMenu.Nodes[3].Nodes[index].Nodes.Add(new TreeNode(icerik.strName));
                        tvMainMenu.Nodes[3].Nodes[index].Nodes[icIndex].Nodes.Add(new TreeNode(icerik.strValue));
                        icIndex++;
                    }
                    index++;                    
                }
            }

            tvMainMenu.Nodes.Add("Kurulum Sorguları");
            //Sorgu verilerini Xml'den doldurma
            cAppHelper.GetInstallQueryInfo();
            if (cAppHelper.lstInstallQuery != null)
            {
                int index = 0, icIndex = 0;
                //Treeviewin sorgu classına göre oluşturulması
                foreach (var item in cAppHelper.lstInstallQuery)
                {
                    icIndex = 2;
                    tvMainMenu.Nodes[4].Nodes.Add(new TreeNode(item.strName));
                    tvMainMenu.Nodes[4].Nodes[index].Nodes.Add(new TreeNode("Aciklama"));
                    tvMainMenu.Nodes[4].Nodes[index].Nodes[0].Nodes.Add(new TreeNode(item.strDescription));
                    tvMainMenu.Nodes[4].Nodes[index].Nodes.Add(new TreeNode("Sorgu"));
                    tvMainMenu.Nodes[4].Nodes[index].Nodes[1].Nodes.Add(new TreeNode(item.strQuery));
                    foreach (var icerik in item.lstParameters)
                    {
                        tvMainMenu.Nodes[4].Nodes[index].Nodes.Add(new TreeNode(icerik.strName));
                        tvMainMenu.Nodes[4].Nodes[index].Nodes[icIndex].Nodes.Add(new TreeNode(icerik.strValue));
                        icIndex++;
                    }
                    index++;
                }
            }

            tvMainMenu.Nodes.Add("Yardım Mesajları");
            cAppHelper.ReadAllHelpMessage();
            if (cAppHelper.lstHelpMessage.Count != 0)
            {
                foreach (var item in cAppHelper.lstHelpMessage)
                {
                    tvMainMenu.Nodes[5].Nodes.Add(new TreeNode(item.strName));
                }
            }

            tvMainMenu.Nodes.Add("Alt Sorgular");
            cAppHelper.ReadSubQuery();
            foreach (var item in cAppHelper.lstSubQuery)
            {
                tvMainMenu.Nodes[6].Nodes.Add(new TreeNode(item.strName));
            }

            tvMainMenu.Nodes.Add("İş Akışları");
            foreach (var item in cAppHelper.ReadWorkFlowName())
            {
                tvMainMenu.Nodes[7].Nodes.Add(new TreeNode(item.ToString()));
            }
            #endregion
        }

        private void fncUpdateQuery(string strTabPageName)
        {
            #region Sorgunun güncellenmesi durumunda yapılan işlemler
            dtTable = new DataTable();
            foreach (var page in lstTabPageClone)
            {
                //Aktif tabPagein bulunmasına göre güncelleme
                if (page.Key.Equals(strTabPageName))
                {
                    bool blnUpdate = true;
                    foreach (var item in cAppHelper.lstQueryInfo)
                    {
                        if (page.Value.ElementAt(2).Text.Equals(item.strName) && strTabPageName.Equals("Yeni Sorgu"))
                        {
                            blnUpdate = false;
                            break;
                        }
                    }

                    if (blnUpdate)
                    {
                        //Regexpression kontrolü
                        Regex regName = new Regex(@"^[A-Za-z0-9]*$");
                        if (regName.IsMatch(page.Value.ElementAt(2).Text))
                        {
                            DataGridView grid = (DataGridView)page.Value.ElementAt(8);
                            dtTable = (DataTable)(grid.DataSource);
                            //Verilerin class'a aktarılması
                            cAppHelper.lstQueryParameters = new List<KeyValuePair<string, string>>();

                            foreach (DataRow item in dtTable.Rows)
                            {
                                if (item != null)
                                {
                                    cAppHelper.lstQueryParameters.Add(new KeyValuePair<string, string>(item[0].ToString(), item[1].ToString()));
                                }
                            }

                            cAppHelper.strQueryName = page.Value.ElementAt(2).Text;
                            cAppHelper.strQueryDesc = page.Value.ElementAt(4).Text;
                            cAppHelper.strQueryCode = page.Value.ElementAt(6).Controls[0].Text;
                            CheckBox chkAcilis = (CheckBox)page.Value.ElementAt(12);
                            //Güncelleme işlemi için metoda yollanması
                            cAppHelper.TransactionQuery(page.Key.ToString(), chkAcilis.Checked);
                            fncCreateTreeView();

                            #region Güncelleme ile acikTab ve clonelanan listin yeniden güncellenmesi
                            int dIndex;
                            dIndex = lstOpenTabQuery.IndexOf(strTabPageName);
                            lstOpenTabQuery.RemoveAt(dIndex);
                            lstOpenTabQuery.Insert(dIndex, page.Value.ElementAt(2).Text);

                            //Tabpagelisti sorguyu tekrar güncellerken yenileme
                            for (int i = 0; i < lstTabPageClone.Count; i++)
                            {
                                if (lstTabPageClone.ElementAt(i).Key.Equals(strTabPageName))
                                {
                                    lstTabPageClone.RemoveAt(i);
                                    lstTabPageClone.Insert(i, new KeyValuePair<string, List<Control>>(page.Value.ElementAt(2).Text, page.Value));
                                    break;
                                }
                            }

                            #endregion

                            foreach (TabPage item in tpage.TabPages)
                            {
                                if (item.Name.Equals(strTabPageName))
                                {
                                    item.Text = page.Value.ElementAt(2).Text;
                                    item.Name = page.Value.ElementAt(2).Text;
                                    break;
                                }
                            }

                            toolStripStatusKayıt.Text = "Sorgu kaydedildi: " + DateTime.Now;
                            break;
                        }
                        else
                        {
                            MessageBox.Show("Sorgu adını yanlış girdiniz!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Geçerli sorgu adı zaten var!");
                    }
                }
            }

            #endregion
        }

        private void fncTabPageAssign(TabPage tpNew)
        {
            #region Tabpage elemanlarını atama
            lstControl = new List<Control>();

            List<Control> lstPageIcerik = new List<Control> { splitCntSorgu };
            fncTabPageClone(lstPageIcerik, 0);
            SplitContainer split = (SplitContainer)lstControl.ElementAt(0);

            lstPageIcerik = new List<Control> { pnlQueryData, txtQueryDesc, lblSorguAd, txtQueryName, lblSorguAciklama, txtQuery };
            fncTabPageClone(lstPageIcerik, 1);
            split.Panel1.Controls.Add(lstControl.ElementAt(1));

            lstPageIcerik = new List<Control> { pnlQueryParam, dGvQueryParam };
            fncTabPageClone(lstPageIcerik, 7);
            split.Panel1.Controls.Add(lstControl.ElementAt(7));

            lstPageIcerik = new List<Control> { pnlQueryResult, grdQueryResult, lblSorguSonuc , chbxAcilis};
            fncTabPageClone(lstPageIcerik, 9);
            split.Panel2.Controls.Add(lstControl.ElementAt(9));
            tpNew.Controls.Add(split);
            #endregion
        }

        private void fncTabPageClone(List<Control> lstPageContent, int dIndex)
        {
            #region TpDuzenle içindekilerin kopyalandığı ve aktarıldığı bölüm
            foreach (var item in lstPageContent)
            {
                clsControl.CopyCtrl2ClipBoard(item);
                lstControl.Add(clsControl.CloneCtrl(item));
            }

            for (int i = dIndex; i < lstControl.Count; i++)
            {
                if (!lstControl.ElementAt(i).Name.ToLower().Contains("pnl") && !lstControl.ElementAt(i).Name.ToLower().Contains("split"))
                {
                    lstControl.ElementAt(dIndex).Controls.Add(lstControl.ElementAt(i));
                }
                if (lstControl.ElementAt(i).Name.ToLower().Equals("txtquery"))
                {
                    lstControl.ElementAt(i).BringToFront();
                }
                lstControl.ElementAt(i).Show();
            }
            #endregion
        }

        private void fncTabPageReset()
        {
            tpage.Visible = false;
            //Proje açılınca tabPage ve değerleri tutulan listelerin sıfırlanması
            foreach (TabPage item in tpage.TabPages)
            {
                tpage.TabPages.Remove(item);
            }
            lstTabPageClone = new List<KeyValuePair<string, List<Control>>>();
            lstControl = new List<Control>();
            lstOpenTabQuery = new List<string>();

            newSubQueryToolStripMenuItem.Enabled = true;
            newQueryToolStripMenuItem.Enabled = true;
            newHelpToolStripMenuItem.Enabled = true;
            saveAllToolStripMenuItem.Enabled = true;
        }

        private void fncOptions()
        {
            clsOptionInfo cOption = cAppHelper.GetOptions();
            if (cOption != null)
            {
                txtOptionDesc.Text = cOption.strDesc;
                txtOptionsConnect.Text = cOption.strConnection;
                txtOptionFirm.Text = cOption.strFirm;
                txtOptionPeriod.Text = cOption.strPeriod;
            }
            else
            {
                txtOptionDesc.Clear();
                txtOptionsConnect.Text = "Server=LOCALHOST;Database=GO3;User Id=SA;Password=;";
                txtOptionFirm.Clear();
                txtOptionPeriod.Clear();
            }
            toolStripStatusAdi.Text = "Adı:" + cAppHelper.strProjectName;
            toolStripStatusFirma.Text = " Firma:" + txtOptionFirm.Text;
            toolStripStatusDonem.Text = " Dönem:" + txtOptionPeriod.Text;
            toolStripStatusBaglanti.Text = " Bağlantı:" + txtOptionsConnect.Text;
        }

        private void fncSaveTabs(TabPage tpTabSecim)
        {
            if (tpTabSecim.Equals(tpOptions))
            {
                List<string> lstAyarlar = new List<string>() { txtOptionDesc.Text, txtOptionsConnect.Text, txtOptionFirm.Text, txtOptionPeriod.Text };
                cAppHelper.TransactionOptions(lstAyarlar);
                fncOptions();
                toolStripStatusKayıt.Text = "Ayar kaydedildi: " + DateTime.Now;
            }
            if (tpTabSecim.Equals(tpMessage))
            {
                dtTable = (DataTable)(dGvMessage.DataSource);
                bool blKontrol = cAppHelper.TransactionMessage(dtTable);
                if (blKontrol)
                {
                    MessageBox.Show("Aynı mesaj numarası bulunmaktadır.");
                }
                else
                {
                    toolStripStatusKayıt.Text = "Mesaj kaydedildi: " + DateTime.Now;
                }
            }
            if (tpTabSecim.Equals(tpTable))
            {
                dtTable = (DataTable)(dgvTable.DataSource);
                bool blKontrol = cAppHelper.TransactionTable(dtTable);
                if (blKontrol)
                {
                    MessageBox.Show("Aynı tablo numarası bulunmaktadır.");
                }
                else
                {
                    toolStripStatusKayıt.Text = "Tablo kaydedildi: " + DateTime.Now;
                }
            }
            if (tpTabSecim.Equals(tpHelpMessage))
            {
                cAppHelper.TransactionHelpMessage(txtFormName.Text, rtxtFormDesc.Text);
                toolStripStatusKayıt.Text = "Yardım mesajı kaydedildi: " + DateTime.Now;
            }
            if (tpTabSecim.Equals(tpSubQuery))
            {
                cAppHelper.strSubQueryDesc = txtSubQueryDesc.Text;
                cAppHelper.strSubQueryCode = txtSubQuery.Controls[txtSubQuery.Controls.Count - 1].Text;
                cAppHelper.strSubQueryName = txtSubQueryName.Text;
                string strSQName = tpSubQuery.Text.Equals("Alt Sorgu") ? "" : tpSubQuery.Text;
                cAppHelper.TransactionSubQuery(strSQName);
                tpSubQuery.Text = txtSubQueryName.Text;
                toolStripStatusKayıt.Text = "Alt sorgu kaydedildi: " + DateTime.Now;
            }
        }

        private void fncSaveTabPage(TabPage tpSecim = null)
        {
            if (tpSecim == null)
            {
                if (tpage.TabPages.Contains(tpOptions)) { fncSaveTabs(tpOptions); }

                if (tpage.TabPages.Contains(tpMessage)) { fncSaveTabs(tpMessage); }

                if (tpage.TabPages.Contains(tpTable)) { fncSaveTabs(tpTable); }

                if (tpage.TabPages.Contains(tpHelpMessage)) { fncSaveTabs(tpHelpMessage); }

                if (tpage.TabPages.Contains(tpSubQuery)) { fncSaveTabs(tpSubQuery); }                
            }
            else
            {
                if (tpage.SelectedTab.Equals(tpOptions)) { fncSaveTabs(tpOptions); }

                if (tpage.SelectedTab.Equals(tpMessage)) { fncSaveTabs(tpMessage); }

                if (tpage.SelectedTab.Equals(tpTable)) { fncSaveTabs(tpTable); }

                if (tpage.SelectedTab.Equals(tpHelpMessage)) { fncSaveTabs(tpHelpMessage); }

                if (tpage.SelectedTab.Equals(tpSubQuery)) { fncSaveTabs(tpSubQuery); }
               
            }
            fncCreateTreeView();            
        }

        private void fncLoadProject(string strFileName)
        {
            string strError = "";
            cAppHelper.strPath = strFileName;
            bool blKontrol = cAppHelper.xDocLoad(out strError);
            if (blKontrol == true)
            {
                string[] ayir = strFileName.Split("\\".ToCharArray());
                cAppHelper.strProjectName = ayir.ElementAt(ayir.Length - 1);

                fncCreateTreeView();
                fncTabPageReset();
                fncOptions();               
            }
            else
            {
                MessageBox.Show("Dosya yülklenemedi. Detay:" + strError);
            }
        }

        private void fncTreeViewSorgular(string strTabText, List<clsQueryInfo> lstQuery)
        {
            #region Kopyalanan yeni tabPage'a özellikleri atama
            TabPage tpYeni = new TabPage(strTabText);
            tpYeni.Name = strTabText;

            fncTabPageAssign(tpYeni);

            lstTabPageClone.Add(new KeyValuePair<string, List<Control>>(strTabText, lstControl));
            tpage.TabPages.Add(tpYeni);
            #endregion

            txtQuery.BringToFront();

            dtTable = new DataTable();
            dtTable.Columns.Add("Parametre", typeof(string));
            dtTable.Columns.Add("Değer", typeof(string));
            if (cAppHelper.lstQueryInfo != null)
            {
                foreach (var item in lstQuery)
                {
                    if (item.strName.Equals(strTabText))
                    {
                        fncScintillaQuery(item.strName);

                        lstTabPageClone.ElementAt(lstTabPageClone.Count - 1).Value.ElementAt(6).Controls[0].Text = item.strQuery;
                        
                        //Kopyalanan tabpPagelerin özelliklerini atama
                        lstTabPageClone.ElementAt(lstTabPageClone.Count - 1).Value.ElementAt(2).Text = item.strName;
                        lstTabPageClone.ElementAt(lstTabPageClone.Count - 1).Value.ElementAt(4).Text = item.strDescription;
                        lstTabPageClone.ElementAt(lstTabPageClone.Count - 1).Value.ElementAt(6).Text = item.strQuery;

                        //Kurulumda çalışma checkbox eklenmesi
                        CheckBox chkSetup = (CheckBox)lstTabPageClone.ElementAt(lstTabPageClone.Count - 1).Value.ElementAt(12);
                        chkSetup.Checked = item.blnSetup;

                        foreach (var param in item.lstParameters)
                        {
                            dtTable.Rows.Add(param.strName, param.strValue);
                        }
                        break;
                    }
                }
            }

            //Gridview tıklanan sorgununkinin aktarılması                                      
            DataGridView dgvGrid = (DataGridView)lstTabPageClone.ElementAt(lstTabPageClone.Count - 1).Value.ElementAt(8);
            dgvGrid.DataSource = dtTable;

            lstOpenTabQuery.Add(strTabText);
            tpage.SelectedTab = tpYeni;
        }

        private void fncScintillaQuery(string strName)
        {
            #region Scintilla textboxın strSorguya aktarılması
            scTxtQuery = new ScintillaNET.Scintilla();

            scTxtQuery.Dock = DockStyle.Fill;
            scTxtQuery.CharAdded += TextBox_CharAdded;
            scTxtQuery.KeyPress += (this.OnKeyPressed);
            scTxtQuery.AutoCSelection += TextBox_AutoCSelection;

            scTxtQuery.WrapMode = WrapMode.None;
            scTxtQuery.IndentationGuides = IndentView.LookBoth;
            lstTabPageClone.ElementAt(lstTabPageClone.Count - 1).Value.ElementAt(6).Controls.Add(scTxtQuery);

            InitColors(scTxtQuery);
            InitSyntaxColoring(scTxtQuery);

            InitHotkeys(strName);
            #endregion
        }

        private void fncScinttillaSubQuery()
        {
            scTxtSubQuery = new ScintillaNET.Scintilla();

            scTxtSubQuery.Dock = DockStyle.Fill;
            scTxtSubQuery.CharAdded += TextBox_CharAdded;
            scTxtSubQuery.KeyPress += (this.OnKeyPressed);
            scTxtSubQuery.AutoCSelection += TextBox_AutoCSelection;

            scTxtSubQuery.WrapMode = WrapMode.None;
            scTxtSubQuery.IndentationGuides = IndentView.LookBoth;

            InitColors(scTxtSubQuery);
            InitSyntaxColoring(scTxtSubQuery);

            InitHotkeys();

            if (txtSubQuery.Controls.Count == 0)
            {
                txtSubQuery.Controls.Add(scTxtSubQuery);
            }
        }

        public frmMainView()
        {
            InitializeComponent();
            cAppHelper = new clsAppHelper();
            lstTabPageClone = new List<KeyValuePair<string, List<Control>>>();
            lstOpenTabQuery = new List<string>();

            //Tabpageler ilk başta gözükmeme durumu
            foreach (TabPage item in tpage.TabPages)
            {
                tpage.TabPages.Remove(item);
            }
            
            // *Komut satırı ile exe üzerine dosya atınca direk çalışması için yapıldı, debuga yol girildi
            if (Environment.GetCommandLineArgs().Length == 2)
            {
                string strFilePath = Environment.GetCommandLineArgs()[1];

                fncLoadProject(strFilePath);
            }
        }       

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region Yeni proje açılması istendiğinde oluşan durum
            try
            {
                sFDyeniProje.Filter = "XML Dosyası|*.xml";
                sFDyeniProje.CreatePrompt = true;

                if (sFDyeniProje.ShowDialog() == DialogResult.OK)
                {
                    cAppHelper.NewProjectXML(sFDyeniProje.FileName);
                    string[] ayir = sFDyeniProje.FileName.Split("\\".ToCharArray());
                    int i = ayir.ElementAt(ayir.Length - 1).IndexOf(".");
                    cAppHelper.strProjectName = ayir.ElementAt(ayir.Length - 1).Substring(0, i);

                    fncCreateTreeView();
                    fncTabPageReset();
                    fncOptions();
                }
            }
            catch (Exception excMain)
            {
                MessageBox.Show("Yeni proje dosyası oluşturulamadı!Detay:" + excMain.Message);
            }

            #endregion
        }

        private void newQueryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region Yeni sorgu eklenmesine tıklanması durumu
            try
            {
                if (!lstOpenTabQuery.Contains("Yeni Sorgu"))
                {
                    tpage.Visible = true;

                    #region Yeni tabPage oluşturma
                    TabPage tpYeni = new TabPage("Yeni Sorgu");
                    tpYeni.Name = "Yeni Sorgu";

                    fncTabPageAssign(tpYeni);
                    //Klonlanan tabPagelerin liste eklenmesi
                    lstTabPageClone.Add(new KeyValuePair<string, List<Control>>("Yeni Sorgu", lstControl));

                    #endregion

                    fncScintillaQuery("Yeni Sorgu");

                    tpage.TabPages.Add(tpYeni);
                    txtQuery.BringToFront();

                    //DataTable oluşturulması
                    dtTable = new DataTable();
                    dtTable.Columns.Add("Parametre", typeof(string));
                    dtTable.Columns.Add("Değer", typeof(string));

                    DataGridView grid = (DataGridView)lstTabPageClone.ElementAt(lstTabPageClone.Count - 1).Value.ElementAt(8);
                    grid.DataSource = dtTable;

                    lstOpenTabQuery.Add("Yeni Sorgu");
                    tpage.SelectedTab = tpYeni;
                }
                else
                {
                    tpage.SelectTab("Yeni Sorgu");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            #endregion
        }

        private void newHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                txtFormName.Clear();
                rtxtFormDesc.Clear();

                if (!tpage.TabPages.Contains(tpHelpMessage))
                {
                    tpage.Visible = true;
                    tpage.TabPages.Add(tpHelpMessage);
                }
                tpage.SelectedTab = tpHelpMessage;
            }
            catch (Exception exHelp)
            {
                MessageBox.Show("Yeni yardım mesajı açılamadı. Detay: " + exHelp.Message);
            }
            
        }

        private void newWorkFlowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                #region Yeni iş akışı açılması durumu
                cAppHelper.strNodeFileName = "YeniIsAkisi";
                frmNodeEditorView frmNodeEditor = new frmNodeEditorView(cAppHelper, this);

                frmNodeEditor.Show();
                #endregion
            }
            catch (Exception exWorkFlow)
            {
                MessageBox.Show("Yeni iş akışı açılamadı. Detay: " + exWorkFlow.Message);
            }
            
        }

        private void newSubQueryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                fncScinttillaSubQuery();

                if (!tpage.TabPages.Contains(tpSubQuery))
                {
                    tpage.Visible = true;
                    tpage.TabPages.Add(tpSubQuery);
                }
                else
                {
                    txtSubQueryName.Clear();
                    txtSubQueryDesc.Clear();
                    scTxtSubQuery.Clear();
                    txtSubQuery.Controls[txtSubQuery.Controls.Count - 1].Text = "";
                }
                tpage.SelectedTab = tpSubQuery;
                tpSubQuery.Text = "Alt Sorgu";
            }
            catch (Exception exSubQuery)
            {
                MessageBox.Show("Yeni alt sorgu açılamadı. Detay: " + exSubQuery.Message);
            }
            
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region Hazır proje açılması durumu
            try
            {
                OpenFileDialog file = new OpenFileDialog();
                file.InitialDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"_EXT\");
                file.Title = "Dosya seçiniz";
                DialogResult result = file.ShowDialog();

                splitCntMenu.Visible = true;
                pnlTabPage.Visible = true;
                pnlTreeMenu.Visible = true;

                if (result == DialogResult.OK)
                {
                    fncLoadProject(file.FileName);         
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Proje dosyası açılamadı. Detay: " + ex.Message);
            }

            #endregion
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                fncSaveTabPage();
                foreach (var item in lstOpenTabQuery.ToList())
                {
                    fncUpdateQuery(item);
                }
            }
            catch (Exception exSaveAll)
            {
                MessageBox.Show("Tümünü kaydetme hatası. Detay: " + exSaveAll.Message);
            }
            
        }

        private void tvMainMenu_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                #region Sorgulara tıklanma durumu
                if (e.Node.Level == 1 && e.Node.Parent.Text.Equals("Sorgular"))
                {
                    if (!lstOpenTabQuery.Contains(e.Node.Text))
                    {
                        tpage.Visible = true;

                        fncTreeViewSorgular(e.Node.Text, cAppHelper.lstWorkingQuery);
                    }
                    else
                    {
                        //Aktif sekme mevcutsa açılması
                        tpage.SelectTab(e.Node.Text);
                    }
                }
                #endregion

                #region Alt sorgulara tıklanma durumu
                if (e.Node.Level == 1 && e.Node.Parent.Text.Equals("Alt Sorgular"))
                {
                    fncScinttillaSubQuery();
                    if (!tpage.TabPages.Contains(tpSubQuery))
                    {
                        tpage.Visible = true;
                        tpage.TabPages.Add(tpSubQuery);  
                    }

                    tpage.SelectTab(tpSubQuery);

                    clsSubQueryInfo cSubInfo = cAppHelper.GetSubQuery(e.Node.Text);
                    if (cSubInfo != null)
                    {
                        txtSubQueryName.Text = cSubInfo.strName;
                        txtSubQueryDesc.Text = cSubInfo.strDesc;
                        cAppHelper.strSubQueryName = e.Node.Text;
                        txtSubQuery.Controls[txtSubQuery.Controls.Count-1].Text = cSubInfo.strContent;
                    }
                    tpSubQuery.Text = txtSubQueryName.Text;
                }
                #endregion

                #region Kurulum tıklanma durumu
                if (e.Node.Level == 1 && e.Node.Parent.Text.Equals("Kurulum Sorguları"))
                {
                    if (!lstOpenTabQuery.Contains(e.Node.Text))
                    {
                        tpage.Visible = true;

                        fncTreeViewSorgular(e.Node.Text, cAppHelper.lstInstallQuery);
                    }
                    else
                    {
                        //Aktif sekme mevcutsa açılması
                        tpage.SelectTab(e.Node.Text);
                    }
                }
                #endregion

                #region Tablolara tıklanma durumu
                
                if (e.Node.Level == 0 && e.Node.Text.Equals("Tablolar"))
                {
                    if (tpage.TabPages.Contains(tpTable))
                    {
                        tpage.SelectedTab = tpTable;
                    }
                    else 
                    {
                        tpage.Visible = true;
                        if (!tpage.TabPages.Contains(tpTable))
                        {
                            tpage.TabPages.Add(tpTable);
                        }
                        tpage.SelectedTab = tpTable;
                        dgvTable.DataSource = cAppHelper.ReadAllTable();
                    }                    
                }
                #endregion

                #region Mesajlara tıklanması durumu
                if (e.Node.Level == 0 && e.Node.Text.Equals("Mesajlar"))
                {
                    if (tpage.TabPages.Contains(tpMessage))
                    {
                        tpage.SelectedTab = tpMessage;
                    }
                    else
                    {
                        tpage.Visible = true;
                        if (!tpage.TabPages.Contains(tpMessage))
                        {
                            tpage.TabPages.Add(tpMessage);
                        }
                        tpage.SelectedTab = tpMessage;
                        dGvMessage.DataSource = cAppHelper.ReadAllMessage();
                    }                   
                }
                #endregion

                #region Yardım mesaj tıklanma durumu
                if (e.Node.Level == 1 && e.Node.Parent.Text.Equals("Yardım Mesajları"))
                {
                    if (tpage.TabPages.Contains(tpHelpMessage))
                    {
                        tpage.SelectedTab = tpHelpMessage;
                    }
                    else
                    {
                        tpage.Visible = true;
                        if (!tpage.TabPages.Contains(tpHelpMessage))
                        {
                            tpage.TabPages.Add(tpHelpMessage);
                        }
                        tpage.SelectedTab = tpHelpMessage;                        
                    }
                    txtFormName.Text = e.Node.Text;
                    rtxtFormDesc.Text = cAppHelper.GetHelpMessage(e.Node.Text).strDesc;
                }
                #endregion

                #region Proje Ayarlarına tıklanma durumu
                if (e.Node.Level == 0 && e.Node.Text.Equals("Ayarlar"))
                {
                    if (tpage.TabPages.Contains(tpOptions))
                    {
                        tpage.SelectedTab = tpOptions;
                    }
                    else
                    {
                        tpage.Visible = true;
                        tpage.TabPages.Add(tpOptions);
                        tpage.SelectedTab = tpOptions;
                        fncOptions();
                    }                   
                }
                #endregion

                #region İş Akışına tıklanma durumu
                if (e.Node.Level == 1 && e.Node.Parent.Text.Equals("İş Akışları"))
                {
                    cAppHelper.strNodeFileName = e.Node.Text;
                    frmNodeEditorView frmNodeEditor = new frmNodeEditorView(cAppHelper, this);
                    frmNodeEditor.Show();
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show("Treeview tıklanma hatası. Detay: " + ex.Message);
            }
            
        }

        private void MainView_KeyDown(object sender, KeyEventArgs e)
        {
            #region Kaydetme durumu
            try
            {
                if (e.KeyCode == Keys.F12)
                {
                    fncSaveTabPage(tpage.SelectedTab);

                    foreach (var item in lstOpenTabQuery.ToList())
                    {
                        if (item.Equals(tpage.SelectedTab.Name))
                        {
                            fncUpdateQuery(tpage.SelectedTab.Name);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Klavye kaydetme işlemi yapılamadı. Detay: " + ex.Message);
            }
            #endregion

            #region Çalıştırma durumu
            try
            {
                if (e.KeyCode == Keys.F5)
                {
                    foreach (var page in lstTabPageClone)
                    {
                        if (page.Key.Equals(tpage.SelectedTab.Name))
                        {
                            string strSorgu = page.Value.ElementAt(6).Controls[0].Text;
                            DataGridView grid = (DataGridView)page.Value.ElementAt(8);
                            dtTable = (DataTable)(grid.DataSource);

                            //Sorguyu replace etme metodu
                            strSorgu = cAppHelper.GetReplaceQuery(strSorgu);

                            #region GridControlün görselliğini ayarlama
                            GridControl gcClone = (GridControl)page.Value.ElementAt(10);
                            gcClone.Dock = DockStyle.Fill;
                            GridView customPatternView = new GridView();
                            gcClone.ViewCollection.Add(customPatternView);
                            gcClone.MainView = customPatternView;
                            gcClone.BindingContext = new System.Windows.Forms.BindingContext();

                            gcClone.DataSource = cAppHelper.ExecuteQuery(strSorgu, dtTable);
                            customPatternView.OptionsView.ShowGroupPanel = false;
                            customPatternView.OptionsView.ColumnAutoWidth = false;
                            customPatternView.OptionsBehavior.Editable = false;
                            customPatternView.PopulateColumns();
                            customPatternView.BestFitColumns();
                            gcClone.ForceInitialize();
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Klavye çalıştır işlemi yapılamadı. Detay: " + ex.Message);
            }
            #endregion

            #region Tümünü kaydetme işlemi
            try
            {
                if (e.Control && e.KeyCode == Keys.S)
                {
                    fncSaveTabPage();

                    foreach (var item in lstOpenTabQuery.ToList())
                    {
                        fncUpdateQuery(item);
                    }
                }
            }
            catch (Exception exSaveAll)
            {
                MessageBox.Show("Tümünü kaydetme hatası. Detay: " + exSaveAll.Message);
            }            
            #endregion
        }

        private void MainView_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (cAppHelper.sqlConnection != null)
                {
                    if (cAppHelper.sqlConnection.State == ConnectionState.Open)
                    {
                        cAppHelper.sqlConnection.Close();
                    }
                }

                if (cAppHelper.blnIsDecrypt)
                {
                    cAppHelper.Encryption("1000");
                }

                e.Cancel = false;
            }
            catch (Exception exClose)
            {
                MessageBox.Show("Form kapanma hatası. Detay: " + exClose.Message);
            }            
        }

        private void testModeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }       

        #region Scintilla bileşenleri

        private void TextBox_CharAdded(object sender, CharAddedEventArgs e)
        {
            string AutoCompleteKeywords = null;
            Scintilla txtSc = (Scintilla)sender;           
            if (txtSc.Text.Length != 0)
            {
                if (e.Char == '{')
                {
                    cAppHelper.ReadAllTable();
                    cAppHelper.lstTableName.Sort();
                    AutoCompleteKeywords = string.Join(" ", cAppHelper.lstTableName);
                    txtSc.AutoCShow(0, AutoCompleteKeywords);
                    enSecim = eScintillaSecim.TabloIsmi;
                }
                if (e.Char == '.')
                {
                    int pos = txtSc.Text.LastIndexOf(" ", txtSc.CurrentPosition - 1);
                    strTable = txtSc.Text.Substring(pos, txtSc.CurrentPosition - pos -1 );

                    AutoCompleteKeywords = string.Join(" ", cAppHelper.GetTableColumns(strTable, txtSc.Text));
                    txtSc.AutoCShow(0, AutoCompleteKeywords);
                    enSecim = eScintillaSecim.BoslukBirak;
                }
            }  
        } 

        private void TextBox_AutoCSelection(object sender, AutoCSelectionEventArgs e)
        {
            Scintilla txtSc = (Scintilla)sender;
            if (enSecim == eScintillaSecim.TabloIsmi)
            {
                txtSc.InsertText(e.Position, "} ");
            }
            else if (enSecim == eScintillaSecim.BoslukBirak)
            {
                txtSc.InsertText(e.Position, " AS" + strTable + "_" + e.Text);
            }
            else
            {
                MessageBox.Show("Buraya girmemeli!!!");
            }
        }

        private void OnKeyPressed(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar < 32)
            {
                e.Handled = true;
                return;
            }
        }

        private void InitSyntaxColoring(Scintilla scTxtSecim)
        {
            // Configure the default style
            scTxtSecim.StyleResetDefault();
            scTxtSecim.Margins[0].Width = 12;
            scTxtSecim.Styles[Style.Default].Font = "Consolas";
            scTxtSecim.Styles[Style.Default].Size = 10;
            scTxtSecim.Styles[Style.Default].BackColor = IntToColor(0x212121);
            scTxtSecim.Styles[Style.Default].ForeColor = IntToColor(0x777777);
            scTxtSecim.StyleClearAll();

            // Configure the CPP (C#) lexer styles
            scTxtSecim.Styles[Style.Sql.Identifier].ForeColor = IntToColor(0xD0DAE2);
            scTxtSecim.Styles[Style.Sql.Comment].ForeColor = IntToColor(0xBD758B);
            scTxtSecim.Styles[Style.Sql.CommentLine].ForeColor = IntToColor(0x40BF57);
            scTxtSecim.Styles[Style.Sql.CommentDoc].ForeColor = IntToColor(0x2FAE35);
            scTxtSecim.Styles[Style.Sql.Number].ForeColor = IntToColor(0xFFFF00);
            scTxtSecim.Styles[Style.Sql.String].ForeColor = IntToColor(0xFFFF00);
            scTxtSecim.Styles[Style.Sql.Character].ForeColor = IntToColor(0xE95454);
            scTxtSecim.Styles[Style.Sql.Operator].ForeColor = IntToColor(0xE0E0E0);
            scTxtSecim.Styles[Style.Sql.CommentLineDoc].ForeColor = IntToColor(0x77A7DB);
            scTxtSecim.Styles[Style.Sql.Word].ForeColor = IntToColor(0x48A8EE);
            scTxtSecim.Styles[Style.Sql.Word2].ForeColor = IntToColor(0xF98906);
            scTxtSecim.Styles[Style.Sql.CommentDocKeyword].ForeColor = IntToColor(0xB3D991);
            scTxtSecim.Styles[Style.Sql.CommentDocKeywordError].ForeColor = IntToColor(0xFF0000);

            scTxtSecim.Lexer = Lexer.Sql;

            scTxtSecim.SetKeywords(0, "class extends implements import interface new case do while else if for in switch throw get set function var try catch finally while with default break continue delete return each const namespace package include use is as instanceof typeof author copy default deprecated eventType example exampleText exception haxe inheritDoc internal link mtasc mxmlc param private return see serial serialData serialField since throws usage version langversion playerversion productversion dynamic private public partial static intrinsic internal native override protected AS3 final super this arguments null Infinity NaN undefined true false abstract as base bool break by byte case catch char checked class const continue decimal default delegate do double descending explicit event extern else enum false finally fixed float for foreach from goto group if implicit in int interface internal into is lock long new null namespace object operator out override orderby params private protected public readonly ref return switch struct sbyte sealed short sizeof stackalloc static string select this throw true try typeof uint ulong unchecked unsafe ushort using var virtual volatile void while where yield");
            scTxtSecim.SetKeywords(1, "void Null ArgumentError arguments Array Boolean Class Date DefinitionError Error EvalError Function int Math Namespace Number Object RangeError ReferenceError RegExp SecurityError String SyntaxError TypeError uint XML XMLList Boolean Byte Char DateTime Decimal Double Int16 Int32 Int64 IntPtr SByte Single UInt16 UInt32 UInt64 UIntPtr Void Path File System Windows Forms ScintillaNET");

        }

        private void InitColors(Scintilla scTxtSecim)
        {
            scTxtSecim.SetSelectionBackColor(true, IntToColor(0x114D9C));
            scTxtSecim.CaretForeColor = Color.White;
        }

        public static Color IntToColor(int rgb)
        {
            return Color.FromArgb(255, (byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        private void InitHotkeys(string strTabPageName = "")
        {
            HotKeyManager.AddHotKey(this, Uppercase, Keys.U, true);
            HotKeyManager.AddHotKey(this, Lowercase, Keys.L, true);
            HotKeyManager.AddHotKey(this, ZoomIn, Keys.Oemplus, true);
            HotKeyManager.AddHotKey(this, ZoomOut, Keys.OemMinus, true);
            HotKeyManager.AddHotKey(this, ZoomDefault, Keys.D0, true);

            if (!strTabPageName.Equals(""))
            {
                foreach (var page in lstTabPageClone)
                {
                    //Aktif tabPagein bulunmasına göre güncelleme
                    if (page.Key.Equals(strTabPageName))
                    {
                        Scintilla Text = (Scintilla)page.Value.ElementAt(6).Controls[0];
                        Text.ClearCmdKey(Keys.Control | Keys.R);
                        Text.ClearCmdKey(Keys.Control | Keys.H);
                        Text.ClearCmdKey(Keys.Control | Keys.L);
                        Text.ClearCmdKey(Keys.Control | Keys.U);

                        break;
                    }
                }
            }
        }

        #region Uppercase / Lowercase

        private void Lowercase()
        {
            foreach (var page in lstTabPageClone)
            {
                //Aktif tabPagein bulunmasına göre güncelleme
                if (page.Key.Equals(tpage.SelectedTab.Text))
                {
                    Scintilla Text = (Scintilla)page.Value.ElementAt(6).Controls[0];
                    int start = Text.SelectionStart;
                    int end = Text.SelectionEnd;

                    // modify the selected text
                    Text.ReplaceSelection(Text.GetTextRange(start, end - start).ToLower());

                    // preserve the original selection
                    Text.SetSelection(start, end);
                    break;
                }
            }
        }

        private void Uppercase()
        {
            foreach (var page in lstTabPageClone)
            {
                //Aktif tabPagein bulunmasına göre güncelleme
                if (page.Key.Equals(tpage.SelectedTab.Text))
                {
                    Scintilla Text = (Scintilla)page.Value.ElementAt(6).Controls[0];
                    int start = Text.SelectionStart;
                    int end = Text.SelectionEnd;

                    // modify the selected text
                    Text.ReplaceSelection(Text.GetTextRange(start, end - start).ToUpper());

                    // preserve the original selection
                    Text.SetSelection(start, end);
                    break;
                }
            }
        }

        #endregion

        #region Zoom

        private void ZoomIn()
        {
            scTxtQuery.ZoomIn();
        }

        private void ZoomOut()
        {
            scTxtQuery.ZoomOut();
        }

        private void ZoomDefault()
        {
            scTxtQuery.Zoom = 0;
        }


        #endregion

        public void InvokeIfNeeded(Action action)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(action);
            }
            else
            {
                action.Invoke();
            }
        }
        #endregion

    }

}
