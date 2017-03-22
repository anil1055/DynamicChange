using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Data;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using System.Data.SqlClient;
using System.ComponentModel;

namespace TRP_APPHELPER_DLL
{
    public class clsAppHelper
    {
        public string strPath { get; set; }
        public string strQueryName { get; set; }
        public string strQueryDesc { get; set; }
        public string strQueryCode { get; set; }
        public string strSubQueryName { get; set; }
        public string strSubQueryDesc { get; set; }
        public string strSubQueryCode { get; set; }
        public string strProjectName { get; set; }
        public string strNodeFileName { get; set; }
        public List<string> lstTableName { get; set; }
        public List<KeyValuePair<string,string>> lstQueryParameters { get; set; }
        
        public List<clsQueryInfo> lstQueryInfo { get; set; }
        public List<clsTableInfo> lstTableInfo { get; set; }
        public List<clsQueryInfo> lstInstallQuery { get; set; }
        public List<clsQueryInfo> lstWorkingQuery { get; set; }
        public List<clsHelpInfo> lstHelpMessage { get; set; }
        public List<clsSubQueryInfo> lstSubQuery { get; set; }
        public List<clsMessageInfo> lstMessageInfo { get; set; }

        public clsQueryInfo cQueryInfo { get; set; } 
        public XDocument xdocFile { get; set; }
        public SqlConnection sqlConnection { get; set; }
        public bool blnIsDecrypt { get; set; }

        public clsAppHelper()
        {
            bool designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
            if (!designMode)
            {
                strPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                    , @"_EXT\Veriler.xml");
                string value;
                xDocLoad(out value);
            }            
        }

        public bool LoadProject(string strProjectFile, out string strMessage)
        {
            bool blnResult = false;

            this.strPath = strProjectFile;

            if (xDocLoad(out strMessage))
            {
                try
                {
                    this.ReadAllQuery();
                    blnResult = true;
                }
                catch (Exception excMain)
                {
                    blnResult = false;
                    strMessage = "Proje dosyasından sorgular okunamadı!Detay:" + excMain.Message;
                }
            }
            return blnResult;
        }

        public bool xDocLoad(out string strErrorMessage, bool blnEncrypted = false)
        {
            strErrorMessage = "";
            bool blKontrol = true;
            try
            {
                if (blnEncrypted)
                {
                    Decryption("1000");
                }
                else
                {
                    xdocFile = XDocument.Load(strPath);
                }
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                blKontrol = false;
            }
            return blKontrol;        
        }

        #region Mesajların metotları
        public DataTable ReadAllMessage()
        {
            #region Mesajları XML'den okuma            
            
            XmlDocument doc = new XmlDocument();
            DataSet ds = new DataSet();
            lstMessageInfo = new List<clsMessageInfo>();

            using (XmlReader xmlFile = XmlReader.Create(strPath,
                                                    new XmlReaderSettings()))
            {               
                doc.Load(xmlFile);
                ds.ReadXml(new XmlNodeReader(doc.SelectNodes("PROJE/mesajlar")[0]));
                //xmlFile.Dispose();
                xmlFile.Close();
            }
            
            //XML'de mesajlar altında veri kontrolü
            if (ds.Tables.Count != 0)
            {
                //Okunan XML'in mesaj açıklamalarının List'e aktarılması(Güncellerken silme kontrolü için)
                foreach (DataRow item in ds.Tables[0].Rows)
                {
                    lstMessageInfo.Add(new clsMessageInfo(item.ItemArray[0].ToString(), item.ItemArray[1].ToString()));
                }
            }
            //Yeni Proje-XML dosya açılması durumu
            else
            {
                DataTable table = new DataTable();                
                table.Columns.Add("Aciklama", typeof(string));
                table.Columns.Add("Metin", typeof(string));
                ds.Tables.Add(table);
            }
            
            return ds.Tables[0];
            #endregion
        }

        public clsMessageInfo GetMessage(string strMessageID, bool blnDebug = false)
        {
            if (blnDebug)
            {
                string strOut = "";
                this.LoadProject(strPath, out strOut);
                ReadAllMessage();
            }
            if (lstMessageInfo == null)
            {
                ReadAllMessage();
            }
            
            #region Gelen parametreye göre mesajın Mesajın metninin return edilmesi
           
            clsMessageInfo cMessageInfo = null;
            foreach (clsMessageInfo item in lstMessageInfo)
            {
                if (item.strID.Equals(strMessageID))
                {
                    cMessageInfo = item;
                    break;
                }
            }
            if (cMessageInfo == null)
            {
                throw new Exception( strMessageID + " ile ilgili mesaj bulunamadı!");
            }

            return cMessageInfo;
            #endregion
        }

        public bool TransactionMessage(DataTable dtTable)
        {
            #region Mesaj XML'lerinin güncellenmesi
            
            bool blnCakisma = false;
            List<object> lstTableRow = dtTable.AsEnumerable().Select(r => r.ItemArray[0]).ToList();

            //XML Açıklama sütununda tekrar eden veri kontrolü
            if (lstTableRow.Count != lstTableRow.Distinct().ToList().Count)
            {
                blnCakisma = true;
            }

            if (!blnCakisma)
            {
                foreach (DataRow row in dtTable.Rows)
                {
                    if (row != null)
                    {
                        //Gridviewdeki açıklama değerinin önceki XML'deki kontrolü(target)
                        var target = xdocFile.Descendants("mesaj")
                            .Where(X => X.Element("Aciklama").Value.ToString() == row[0].ToString()).SingleOrDefault();

                        if (target != null)
                        {
                            target.Element("Metin").Value = row[1].ToString();
                        }

                        else
                        {
                            //Yeni mesaj eklenmesi
                            XElement root = new XElement("mesaj");
                            root.Add(new XElement("Aciklama", row[0].ToString()));
                            root.Add(new XElement("Metin", row[1].ToString()));
                            xdocFile.Element("PROJE").Element("mesajlar").Add(root);
                            lstMessageInfo.Add(new clsMessageInfo(row[0].ToString(), row[1].ToString()));
                        }
                    }
                }

                ReadAllMessage();
                //Önceki XML'de olmayanların silinmesi
                foreach (var item in lstMessageInfo)
                {
                    if (!dtTable.AsEnumerable().Select(r => r.ItemArray[0]).ToList().Contains(item.strID))
                    {
                        xdocFile.Descendants("mesaj").Where(X => X.Element("Aciklama").Value == item.strID).SingleOrDefault().Remove();
                    }
                }
                xdocFile.Save((strPath));            
            }

            return blnCakisma;
            #endregion
        }
        #endregion

        #region Sorguların metotları
        public void ReadAllQuery()
        {
            #region Sorgu adının ve cSorguInfo name ve value değerlerinin List'e aktarılması
            lstQueryInfo = new List<clsQueryInfo>();
            
            //Sorgu adlarının list'e aktarılması
            var deger = xdocFile.Descendants("sorgu")
                        .Select(X => X.Element("Adi").Value.ToString()).ToList();

            if (deger.Count != 0)
            {
                foreach (var item in deger)
                {                    
                    cQueryInfo = new clsQueryInfo();
                    //Sorgu adına göre verilerin List<SorguIcerik>'e aktarılması
                    var sorgu = xdocFile.Descendants("sorgu").Where(X => X.Element("Adi").Value == item).SingleOrDefault();
                                      
                    foreach (var param in sorgu.Descendants("parametre").Select(X => X).ToList())
                    {
                        cQueryInfo.lstParameters.Add(new clsParametre(param.Attribute("Name").Value, param.Value));
                    }
                    //Sorgu içeriklerinin Treeview'de kullanılmak üzere List<cs>'e eklenmesi
                    lstQueryInfo.Add(new clsQueryInfo(item, sorgu.Element("Aciklama").Value, sorgu.Element("Sorgu_metin").Value, Convert.ToBoolean(sorgu.Element("Kurulum").Value), cQueryInfo.lstParameters));
                }
            }
                       
            #endregion
        }

        public clsQueryInfo GetQueryInfo(string strQueryID, bool blnDebug = false)
        {
            if (blnDebug)
            {
                string strOut = "";
                this.LoadProject(strPath, out strOut);
                ReadAllQuery();
            }
            if (lstQueryInfo == null)
            {
                ReadAllQuery();
            }

            clsQueryInfo cSorgu = null;
            foreach (var item in lstQueryInfo)
            {
                if (item.strName.Equals(strQueryID))
                {
                    cSorgu = item;
                    if (item.strQuery.IndexOf("--") == -1)
                    {                        
                        cSorgu.strQuery = "--Adı:" + cSorgu.strName + "\n--Açıklama:" + cSorgu.strDescription + "\n" + cSorgu.strQuery;
                    }
                    break;
                }
            }
            if (cSorgu == null)
            {
                throw new Exception(strQueryID + " ile ilgili sorgu bulunamadı!");
            }
                        
            return cSorgu;
        }
        
        public void TransactionQuery(string strFirstQuery, bool chkAcilis)
        {
            #region Sorgu adına göre içeriklerin güncellenmesi
            
            //Sorgu adina göre içerik kontrolü
            var target = xdocFile.Descendants("sorgu")
                .Where(X => X.Element("Adi").Value.ToString() == strFirstQuery).SingleOrDefault();

            //Sorgu içerik güncelleme
            if (target != null)
            {
                target.Element("Adi").SetValue(strQueryName);
                target.Element("Aciklama").SetValue(strQueryDesc);
                target.Element("Sorgu_metin").SetValue(strQueryCode);
                target.Element("Kurulum").SetValue(chkAcilis);

                foreach (var item in lstQueryParameters)
                {
                    //Parametrelerin Name Attribute için kontrolü
                    var node = target.Descendants("parametre").Where(X => X.Attribute("Name").Value == item.Key).SingleOrDefault();
                    if (node != null)
                    {
                        node.SetValue(item.Value);
                    }
                    else
                    {
                        target.Add(new XElement("parametre", new XAttribute("Name", item.Key), item.Value));
                    }
                }
                //Önceki XML'de olmayan lstParametrelerin silinmesi
                foreach (var sil in target.Descendants("parametre").Select(X => X.Attribute("Name").Value).ToList())
                {
                    var key = from item in lstQueryParameters select item.Key;
                    if (!key.Contains(sil))
                    {
                        target.Descendants("parametre").Where(X => X.Attribute("Name").Value == sil).Remove();
                    }
                }

                //Classta verilerin güncellenmesi
                foreach (var item in lstQueryInfo)
                {
                    if (item.strName.Equals(strFirstQuery))
                    {
                        item.strName = strQueryName;
                        item.strDescription = strQueryDesc;
                        item.strQuery = strQueryCode;
                        item.blnSetup = chkAcilis;

                        item.lstParameters = new List<clsParametre>();
                        foreach (var param in lstQueryParameters)
                        {
                            item.lstParameters.Add(new clsParametre(param.Key, param.Value));
                        }
                        break;
                    }
                }

            }
            //Yeni Sorgu Ekleme
            else
            {
                cQueryInfo = new clsQueryInfo();//Class lstParametreler

                XElement root = new XElement("sorgu");
                root.Add(new XElement("Adi", strQueryName));
                root.Add(new XElement("Aciklama", strQueryDesc));
                root.Add(new XElement("Sorgu_metin", strQueryCode));
                root.Add(new XElement("Kurulum", chkAcilis));

                foreach (var item in lstQueryParameters)
                {
                    root.Add((new XElement("parametre", new XAttribute("Name", item.Key),item.Value)));
                    cQueryInfo.lstParameters.Add(new clsParametre(item.Key, item.Value));
                }
                xdocFile.Element("PROJE").Element("sorgular").Add(root);

                //Classa yeni sorgu ekleme
                lstQueryInfo.Add(new clsQueryInfo(strQueryName, strQueryDesc, strQueryCode, chkAcilis, cQueryInfo.lstParameters));
            }
            xdocFile.Save((strPath));

            #endregion
        }

        public DataTable ExecuteQuery(string strQuery, DataTable dtNameValue)
        {
            //Bağlantı komutunun xmlden çekilmesi
            string strBaglanti = xdocFile.Descendants("ayarlar").Select(X => X.Element("baglanti").Value.ToString()).SingleOrDefault().ToString();

            sqlConnection = new SqlConnection(strBaglanti);

            if (sqlConnection.State == ConnectionState.Closed)
            {
                sqlConnection.Open();
            }

            SqlCommand komut = new SqlCommand(strQuery, sqlConnection);

            if (dtNameValue != null)
            {
                foreach (DataRow item in dtNameValue.Rows)
                {
                    komut.Parameters.AddWithValue(item.ItemArray[0].ToString(), item.ItemArray[1].ToString());
                }
            }
            SqlDataAdapter adapter = new SqlDataAdapter(komut);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

            return dt;
        }

        public void GetWorkingQueryInfo()
        {
            ReadAllQuery();

            lstWorkingQuery = new List<clsQueryInfo>();
            //Kurulum harici sorguların eklenmesi
            foreach (var item in lstQueryInfo)
            {
                if (!item.blnSetup)
                {
                    lstWorkingQuery.Add(item);
                }
            }
        }

        public void GetInstallQueryInfo()
        {
            ReadAllQuery();

            //Kurulum sorgularının yüklenmesi
            lstInstallQuery = new List<clsQueryInfo>();
            foreach (var item in lstQueryInfo)
            {
                if (item.blnSetup)
                {
                    lstInstallQuery.Add(item);
                }
            }
        }

        public string GetAllInstallQuery()
        {
            string strInstallQuery = "";
            GetInstallQueryInfo();

            foreach (var item in lstInstallQuery)
            {
                strInstallQuery += item.strQuery + "\n";
            }
            return strInstallQuery;
        }
        #endregion

        #region Ayarların metotları
        public clsOptionInfo GetOptions(bool blnDebug = false)
        {
            #region Proje ayarlarının XML'den okunması
            if (blnDebug)
            {
                string strOut = "";
                this.LoadProject(strPath, out strOut);
            }

            clsOptionInfo cOption = null;
            //XML ayarları kontrolü
            if (xdocFile.Descendants("ayarlar").Elements().Count() != 0)
            {
                cOption = new clsOptionInfo(xdocFile.Descendants("ayarlar").Select(X => X.Element("aciklama").Value.ToString()).SingleOrDefault(),
                    xdocFile.Descendants("ayarlar").Select(X => X.Element("baglanti").Value.ToString()).SingleOrDefault(),
                    xdocFile.Descendants("ayarlar").Select(X => X.Element("firma").Value.ToString()).SingleOrDefault(),
                    xdocFile.Descendants("ayarlar").Select(X => X.Element("donem").Value.ToString()).SingleOrDefault());
            }

            return cOption;
            #endregion
        }

        public void TransactionOptions(List<string> lstOptions)
        {
            #region Proje ayarlarının güncellenmesi
            //XML ayarlarının olup olmama kontrolü
            if (xdocFile.Descendants("ayarlar").Elements().Count() != 0)
            {
                var target = xdocFile.Descendants("ayarlar").SingleOrDefault();
                target.Element("aciklama").SetValue(lstOptions.ElementAt(0).ToString());
                target.Element("baglanti").SetValue(lstOptions.ElementAt(1).ToString());
                target.Element("firma").SetValue(lstOptions.ElementAt(2).ToString());
                target.Element("donem").SetValue(lstOptions.ElementAt(3).ToString());
            }
            else
            {
                XElement root = new XElement("aciklama", lstOptions.ElementAt(0).ToString());
                XElement rootBag = new XElement("baglanti", lstOptions.ElementAt(1).ToString());
                XElement rootFirm = new XElement("firma", lstOptions.ElementAt(2).ToString());
                XElement rootDon = new XElement("donem", lstOptions.ElementAt(3).ToString());
                xdocFile.Element("PROJE").Element("ayarlar").Add(root);    
                xdocFile.Element("PROJE").Element("ayarlar").Add(rootBag); 
                xdocFile.Element("PROJE").Element("ayarlar").Add(rootFirm); 
                xdocFile.Element("PROJE").Element("ayarlar").Add(rootDon); 
            }

            xdocFile.Save((strPath));
            #endregion
        }
        #endregion

        #region Yeni XML Dosyası oluşturma
        public void NewProjectXML(string strNewPath)
        {                        
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml("<PROJE><ayarlar></ayarlar><sorgular></sorgular><mesajlar></mesajlar><tablolar></tablolar><yardim_mesajlar></yardim_mesajlar><is_akislari></is_akislari><alt_sorgular></alt_sorgular></PROJE>");

            strPath = strNewPath;
            xmlDoc.Save(strNewPath);
            xdocFile = XDocument.Load(strNewPath);            
        }
        #endregion

        #region Tabloların metotları
        public DataTable ReadAllTable()
        {
            #region Mesajları XML'den okuma

            lstTableInfo = new List<clsTableInfo>();
            lstTableName = new List<string>();

            var deger = xdocFile.Descendants("tablo")
                        .Select(X => X).ToList();
            //Okunan tablonun class liste aktarılması
            DataTable dtTable = new DataTable();
            dtTable.Columns.Add("Adı", typeof(string));
            dtTable.Columns.Add("Açıklaması", typeof(string));
            dtTable.Columns.Add("İçeriği", typeof(string));
            if (deger.Count != 0)
            {
                foreach (var item in deger)
                {
                    lstTableInfo.Add(new clsTableInfo(item.Element("Adi").Value, item.Element("Aciklama").Value, item.Element("Icerik").Value));
                    lstTableName.Add(item.Element("Adi").Value);
                    dtTable.Rows.Add(item.Element("Adi").Value, item.Element("Aciklama").Value, item.Element("Icerik").Value);
                }
            }
            return dtTable;
            #endregion
        }

        public bool TransactionTable(DataTable dtTable)
        {
            #region Tablo XML'lerinin güncellenmesi

            bool blnCakisma = false;
            List<object> lstTableRow = dtTable.AsEnumerable().Select(r => r.ItemArray[0]).ToList();

            //XML Açıklama sütununda tekrar eden veri kontrolü
            if (lstTableRow.Count != lstTableRow.Distinct().ToList().Count)
            {
                blnCakisma = true;
            }

            if (!blnCakisma)
            {
                lstTableInfo = new List<clsTableInfo>();
                foreach (DataRow row in dtTable.Rows)
                {
                    if (row != null)
                    {
                        //Gridviewdeki açıklama değerinin önceki XML'deki kontrolü(target)
                        var target = xdocFile.Descendants("tablo")
                            .Where(X => X.Element("Adi").Value.ToString() == row[0].ToString()).SingleOrDefault();

                        if (target != null)
                        {
                            target.Element("Aciklama").SetValue(row[1].ToString());
                            target.Element("Icerik").SetValue(row[2].ToString());
                        }

                        else
                        {
                            //Yeni mesaj eklenmesi
                            XElement root = new XElement("tablo");
                            root.Add(new XElement("Adi", row[0].ToString()));
                            root.Add(new XElement("Aciklama", row[1].ToString()));
                            root.Add(new XElement("Icerik", row[2].ToString()));
                            xdocFile.Element("PROJE").Element("tablolar").Add(root);

                            lstTableInfo.Add(new clsTableInfo(row[0].ToString(), row[1].ToString(), row[2].ToString()));
                        }                        
                    }
                }

                ReadAllTable();
                //Önceki XML'de olmayanların silinmesi
                foreach (var item in lstTableName)
                {
                    if (!dtTable.AsEnumerable().Select(r => r.ItemArray[0]).ToList().Contains(item))
                    {
                        xdocFile.Descendants("tablo").Where(X => X.Element("Adi").Value == item).SingleOrDefault().Remove();
                    }
                }
                xdocFile.Save((strPath));
            }

            return blnCakisma;
            #endregion
        }

        public clsTableInfo GetTableContent(string strTableName, bool blnDebug = false)
        {
            if (blnDebug)
            {
                string strOut = "";
                this.LoadProject(strPath, out strOut);
                ReadAllTable();
            }
            if (lstTableInfo == null)
            {
                ReadAllTable();
            }

            clsTableInfo cTable = null;
            foreach (clsTableInfo item in lstTableInfo)
            {
                if (item.strName.Equals(strTableName))
                {
                    cTable = item;
                    cTable.strDesc = item.strDesc;
                    cTable.strName = item.strName;
                    cTable.strContent = GetFirmPeriodContent(item.strContent);
                    break;
                }
            }
            if (cTable == null)
            {
                throw new Exception(strTableName + " ile ilgili tablo bulunamadı");
            }

            return cTable;
        }
       
        public List<string> GetTableColumns(string strTableName, string strQueryText)
        {
            List<string> lstTableColumns = new List<string>();
            string strQuery = "", strTable;

            #region Kullanılan tablo isminin bulunması ve sql sorguya atanması
            strTableName = strTableName.Trim();
            strTable = GetTableContent(strTableName).strContent;

            if (strTable != null)
            {
                strQuery = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + strTable + "'";
            }
            else
            {
                int first = strQueryText.IndexOf(strTableName);
                while (true)
                {
                    int posStart = strQueryText.LastIndexOf("{", first);
                    int posFinish = strQueryText.LastIndexOf("}", first);
                    // *Aralarında en fazla 4 boşluk olabilir!
                    if (posStart != -1 && posFinish != -1 && (first - posFinish < 5))
                    {
                        strTableName = strQueryText.Substring(posStart + 1, posFinish - posStart - 1);

                        strTable = GetTableContent(strTableName).strContent;
                        break;
                    }
                    else
                    {
                        first = strQueryText.IndexOf(strTableName, first + 1);
                    }
                }
                strQuery = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + strTable + "'";
            }
            #endregion

            #region Sorgunun sqlde çalıştırılıp List'e aktarılması
            DataTable dtTableColumn = ExecuteQuery(strQuery, null);
            foreach (DataRow item in dtTableColumn.Rows)
            {
                lstTableColumns.Add(item.ItemArray[0].ToString());
            }
            #endregion

            return lstTableColumns;
        }
        #endregion

        #region Sorgunun replace edildiği metotlar
        public string GetReplaceQuery(string strQuery)
        {
            #region Sorguyu replace etme işlemi

            while (strQuery.IndexOf("{") != -1)
            {
                int i = strQuery.IndexOf("{");
                int j = strQuery.IndexOf("}");
                if (i != -1 && j != -1)
                {
                    string strMetin = strQuery.Substring(i + 1, j - i - 1);
                    if (strMetin != null)
                    {
                        if (!strMetin.Equals("Firm") && !strMetin.Equals("Period"))
                        {
                            string strIcerik = GetTableContent(strMetin).strContent;
                            if (strIcerik != null)
                            {
                                strQuery = strQuery.Replace("{" + strMetin + "}", strIcerik);
                            }
                            else
                            {
                                strIcerik = GetReplaceSubQuery(strMetin);
                                strQuery = strQuery.Replace("{" + strMetin + "}", strIcerik);
                            }
                        }
                        else
                        {
                            strQuery = GetFirmPeriodContent(strQuery);
                        }
                    }
                }
            }
            #endregion

            return strQuery;
        }

        public string GetReplaceSubQuery(string strSubQuery)
        {
            ReadSubQuery();
            string strName = "";
            int i = strSubQuery.IndexOf("(");
            int j = strSubQuery.IndexOf(")", i);

            if (i != -1 && j != -1 && j - i != 1)
            {
                string strSubName = strSubQuery.Substring(0, i);
                string[] ayirDeger = strSubQuery.Substring(i + 1, j - i - 1).IndexOf(",") == -1 ? null : strSubQuery.Substring(i + 1, j - i - 1).ToString().Split(',');

                if (ayirDeger == null && !strSubQuery.Substring(i + 1, j - i - 1).Equals(""))
                {
                    ayirDeger = new string[1];
                    ayirDeger[0] = strSubQuery.Substring(i + 1, j - i - 1);
                }

                string[] ayirParam;
                foreach (var item in lstSubQuery)
                {
                    if (item.strName.Contains(strSubName))
                    {
                        strName = item.strName;
                        i = strName.IndexOf("(");
                        j = strName.IndexOf(")", i);
                        ayirParam = strName.Substring(i + 1, j - i - 1).IndexOf(",") == -1 ? null : strName.Substring(i + 1, j - i - 1).ToString().Split(',');

                        if (ayirParam == null && !strName.Substring(i + 1, j - i - 1).Equals(""))
                        {
                            ayirParam = new string[1];
                            ayirParam[0] = strName.Substring(i + 1, j - i - 1);
                        }

                        strSubQuery = GetSubQuery(item.strName).strContent;

                        if (ayirParam.Length == ayirDeger.Length)
                        {
                            for (int k = 0; k < ayirParam.Length; k++)
                            {
                                strSubQuery = strSubQuery.Replace("{" + ayirParam[k].Trim() + "}", ayirDeger[k].Trim());
                            }
                            break;
                        }
                    }
                }

            }
            else if (i != -1 && j != -1 && j - i == 1)
            {
                strSubQuery = GetSubQuery(strSubQuery.Trim()).strContent;
            }

            return strSubQuery;
        }
       
        public string GetFirmPeriodContent(string strText)
        {
            if (strText.Contains("{Firm}"))
            {
                string f_no = xdocFile.Descendants("ayarlar").Select(X => X.Element("firma").Value.ToString()).SingleOrDefault().ToString();

                strText = strText.Replace("{Firm}", f_no);
            }

            if (strText.Contains("{Period}"))
            {
                string d_no = xdocFile.Descendants("ayarlar").Select(X => X.Element("donem").Value.ToString()).SingleOrDefault().ToString();

                strText = strText.Replace("{Period}", d_no);
            }

            return strText;
        }
        #endregion

        #region Yardım mesajı metotları
        public void ReadAllHelpMessage()
        {
            XmlDocument doc = new XmlDocument();
            DataSet ds = new DataSet();
            lstHelpMessage = new List<clsHelpInfo>();

            using (XmlReader xmlFile = XmlReader.Create(strPath,
                                                    new XmlReaderSettings()))
            {
                doc.Load(xmlFile);
                ds.ReadXml(new XmlNodeReader(doc.SelectNodes("PROJE/yardim_mesajlar")[0]));
                xmlFile.Close();
            }

            if (ds.Tables.Count != 0)
            {
                foreach (DataRow item in ds.Tables[0].Rows)
                {
                    lstHelpMessage.Add(new clsHelpInfo(item.ItemArray[0].ToString(), item.ItemArray[1].ToString()));
                }
            }
        }

        public void TransactionHelpMessage(string strFormName, string strFormDesc)
        {
            var target = xdocFile.Descendants("yardim_mesaj")
                                .Where(X => X.Element("form_ismi").Value.ToString() == strFormName).SingleOrDefault();

            if (target == null)
            {
                XElement root = new XElement("yardim_mesaj");
                root.Add(new XElement("form_ismi", strFormName));
                root.Add(new XElement("icerik", strFormDesc));
                xdocFile.Element("PROJE").Element("yardim_mesajlar").Add(root);
                lstHelpMessage.Add(new clsHelpInfo(strFormName, strFormDesc));
            }
            else
            {
                target.Element("icerik").SetValue(strFormDesc);
                foreach (var item in lstHelpMessage)
                {
                    if (item.strName.Equals(strFormName))
                    {
                        item.strDesc = strFormDesc;
                        break;
                    }
                }
            }
            xdocFile.Save(strPath);
        }

        public clsHelpInfo GetHelpMessage(string strFormName, bool blnDebug = false)
        {
            if (blnDebug)
            {
                string strOut = "";
                this.LoadProject(strPath, out strOut);
                ReadAllHelpMessage();
            }
            if (lstHelpMessage == null)
            {
                ReadAllHelpMessage();
            }
            
            clsHelpInfo cHelp = null;
            foreach (clsHelpInfo item in lstHelpMessage)
            {
                if (item.strName.Equals(strFormName))
                {
                    cHelp = item;
                    break;
                }
            }
            if (cHelp == null)
            {
                throw new Exception(strFormName + "ile ilgili yardım mesajı bulunamadı");
            }
            return cHelp;
        }
        #endregion

        #region İş akışı metotları
        public List<string> ReadWorkFlowName()
        {
            List<string> lstWorkFlow = xdocFile.Descendants("is_akis")
                        .Select(X => X.Element("Adi").Value.ToString()).ToList();

            return lstWorkFlow;
        }

        public void TransactionWorkFlow(string strFlowName, byte[] byteArray)
        {
            var deger = xdocFile.Descendants("is_akis")
                        .Where(X => X.Element("Adi").Value == strFlowName).SingleOrDefault();

            string strWorkflowContent = Convert.ToBase64String(byteArray);
            if (deger == null)
            {
                XElement root = new XElement("is_akis");
                root.Add(new XElement("Adi", strFlowName));
                root.Add(new XElement("Icerik", strWorkflowContent));

                xdocFile.Element("PROJE").Element("is_akislari").Add(root);
            }
            else
            {
                deger.Element("Icerik").SetValue(strWorkflowContent);
            }

            xdocFile.Save(strPath);
        }

        public byte[] GetWorkFlow(string strFlowName, bool blnDebug = false)
        {
            if (blnDebug)
            {
                string strOut = "";
                this.LoadProject(strPath, out strOut);
            }

            var deger = xdocFile.Descendants("is_akis")
                        .Where(X => X.Element("Adi").Value == strFlowName).SingleOrDefault();

            string str = deger.Element("Icerik").Value.ToString();
            byte[] bytes = Convert.FromBase64String(str);

            return bytes;
        }
        #endregion

        #region Alt sorgu metotları
        public void ReadSubQuery()
        {            
            var lstSQ = xdocFile.Descendants("alt_sorgu")
                        .Select(X => X.Element("Adi").Value.ToString()).ToList();

            lstSubQuery = new List<clsSubQueryInfo>();
            if(lstSQ.Count != 0)
            {                
                foreach (var item in lstSQ)
                {
                    var value = xdocFile.Descendants("alt_sorgu").Where(X => X.Element("Adi").Value == item).SingleOrDefault();
                    lstSubQuery.Add(new clsSubQueryInfo(item, value.Element("Aciklama").Value, value.Element("Icerik").Value));
                }
            }
        }

        public void TransactionSubQuery(string strSubQueryTag)
        {
            ReadSubQuery();

            var deger = xdocFile.Descendants("alt_sorgu")
                        .Where(X => X.Element("Adi").Value == strSubQueryTag).SingleOrDefault();

            if (deger == null)
            {
                XElement root = new XElement("alt_sorgu");
                root.Add(new XElement("Adi", strSubQueryName));
                root.Add(new XElement("Aciklama", strSubQueryDesc));
                root.Add(new XElement("Icerik", strSubQueryCode));

                xdocFile.Element("PROJE").Element("alt_sorgular").Add(root);

                lstSubQuery.Add(new clsSubQueryInfo(strSubQueryName, strSubQueryDesc, strSubQueryCode));
            }
            else 
            {
                deger.Element("Adi").SetValue(strSubQueryName);
                deger.Element("Aciklama").SetValue(strSubQueryDesc);
                deger.Element("Icerik").SetValue(strSubQueryCode);

                foreach (var item in lstSubQuery)
                {
                    if (item.strName.Equals(strSubQueryTag))
                    {
                        item.strName = strSubQueryName;
                        item.strDesc = strSubQueryDesc;
                        item.strContent = strSubQueryCode;
                        break;
                    }
                }
            }

            xdocFile.Save(strPath);
        }

        public clsSubQueryInfo GetSubQuery(string strSubQueryName, bool blnDebug = false)
        {
            if (blnDebug)
            {
                string strOut = "";
                this.LoadProject(strPath, out strOut);
                ReadSubQuery();
            }
            if (lstSubQuery == null)
            {
                ReadSubQuery();
            }

            clsSubQueryInfo cSubInfo = null;
            foreach (var item in lstSubQuery)
            {
                if (item.strName.Equals(strSubQueryName))
                {
                    cSubInfo = item;
                    break;
                }
            }
            if (cSubInfo == null)
            {
                throw new Exception(strSubQueryName + "ile ilgili alt sorgu bulunamadı");
            }
            return cSubInfo;
        }
        #endregion

        #region Şifreleme metotları
        public void Encryption(string strPassword)
        {
            //Dosyanın şifrelenip kaydedilmesi
            string strEnc = new clsEncryption(strPassword).Encrypt(xdocFile.ToString());

            File.WriteAllText(strPath, strEnc);
        }

        public void Decryption(string strPassword)
        {
            #region Dosyanın şifresinin çözülmesi, Xdocumente aktarılması ve kaydedilmesi
            blnIsDecrypt = true;
            string strEnc = File.ReadAllText(strPath);

            string strDec = new clsEncryption(strPassword).Decrypt(strEnc);

            TextReader tr = new StringReader(strDec);
            xdocFile = XDocument.Load(tr);

            File.WriteAllText(strPath, strDec);

            #endregion
        }
        #endregion
    }

}
