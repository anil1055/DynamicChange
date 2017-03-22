using NodeEditor;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace TRP_APPHELPER_DLL
{
    public class clsNodeInfo : INodesContext
    {
        public NodeVisual CurrentProcessingNode { get; set; }
        public event Action<string, NodeVisual, FeedbackType, object, bool> FeedbackInfo;
        public clsAppHelper cAppHelper { get; set; }
        public DataTable dtQuery { get; set; }
        public static DataTable dtResult { get; set; }
        public clsQueryInfo cQuery { get; set; }
        public bool blnAralik = false;
        public List<Tuple<string, string>> lstDatas { get; set; }

        public clsNodeInfo(clsAppHelper cAppHelp)
        {
            cAppHelper = cAppHelp;
            dtResult = new DataTable();
            lstDatas = new List<Tuple<string,string>>();
        }


        [Node("Starter", "Helper", "Basic", "Starts execution", true, true)]
        public void Starter()
        {

        }

        [Node("XML Data", "Input", "Basic", "Allows to output a XML value.", false)]
        public void InputQueryParam(string inValue, out string outValue)
        {
            outValue = inValue;
        }

        [Node("Query Data", "Input", "Basic", "Allows to output a query value.", false)]
        public void InputQuery(string QueryName, string Param1, string Param2, string Param3, string Param4, string Param5, out string Output)
        {
            List<string> lstParam = new List<string>();

            if (!Param1.Equals("")) { lstParam.Add(Param1); }
            if (!Param2.Equals("")) { lstParam.Add(Param2); }
            if (!Param3.Equals("")) { lstParam.Add(Param3); }
            if (!Param4.Equals("")) { lstParam.Add(Param4); }
            if (!Param5.Equals("")) { lstParam.Add(Param5); }

            string strOut = "";
            cAppHelper.LoadProject(cAppHelper.strPath, out strOut);

            cQuery = cAppHelper.GetQueryInfo(QueryName);
            dtQuery = new DataTable();
            dtQuery.Columns.Add("Parametre");
            dtQuery.Columns.Add("Değer");
            int index = 0;
            foreach (var item in cQuery.lstParameters)
            {
                //Parametre girilmediyse kontrolü
                if (lstParam.Count != 0)
                {
                    dtQuery.Rows.Add(item.strName, lstParam.ElementAt(index).ToString());
                    index++;
                }
                else
                {
                    dtQuery.Rows.Add(item.strName, item.strValue);
                }
            }

            Output = "Execute";
        }

        [Node("XML Save File", "Helper", "Basic", "Save file in the message box.")]
        public void ShowXmlSave(object strResult)
        {
            if (strResult.ToString().Equals("Execute"))
            {
                string strValue = cAppHelper.GetReplaceQuery(cQuery.strQuery);
                dtResult = cAppHelper.ExecuteQuery(strValue, dtQuery);
            }
            File.WriteAllText("deneme.txt", strResult.ToString());
        }

        [Node("Starter Mail", "Mail", "Basic", "Starts execution", true, true)]
        public void StarterMail(int Aralik)
        {
            int boyut = 60 / Aralik, deger = 0;
            int[] araliklar = new int[boyut];
            for (int j = 0; j < boyut; j++)
            {
                araliklar[j] = deger;
                deger += Aralik;
            }

            for (int i = 0; i < boyut; i++)
            {
                if (DateTime.Now.TimeOfDay.Minutes == araliklar[i])
                {
                    blnAralik = true;
                    break;
                }
            }
        }

        [Node("Email Data", "Mail", "Basic", "Allows to output a Email value.", false)]
        public void InputEmailParam(string Email, string Icerik, out string outEmail, out string outIcerik)
        {
            if (lstDatas.Count != 0)
            {
                outEmail = lstDatas.ElementAt(0).Item1.ToString();
                outIcerik = lstDatas.ElementAt(0).Item2.ToString();               
            }
            else
            {
                outEmail = Email;
                outIcerik = Icerik;
            }
        }

        [Node("Email Submit", "Mail", "Basic", "Submit a Email value.")]
        public void SubmitEmail(string Email, string Content)
        {
            if (blnAralik)
            {
                MailMessage ePosta = new MailMessage();
                ePosta.From = new MailAddress("bulten@terapiyazilim.com");

                ePosta.To.Add(Email);
                ePosta.Subject = "Deneme";
                ePosta.Body = Content;

                SmtpClient smtp = new SmtpClient("mail.terapiyazilim.com");
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("bulten@terapiyazilim.com", "erikli123");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Port = 587;
                smtp.EnableSsl = false;
                try
                {
                    smtp.Send(ePosta);
                    lstDatas.RemoveAt(0);
                }
                catch (SmtpException ex)
                {
                    throw new Exception("Email gönderilemedi. Detay: " + ex.Message);
                }
            }
        }
    }
}

