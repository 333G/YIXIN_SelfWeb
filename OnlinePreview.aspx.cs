using ElationSys.Utils;
using System;
using System.Data;
using System.Web.UI;

public partial class OnlineMapping_OnlinePreview : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var DataF_ID = Convert.ToInt64(Request.Params["DataF_ID"]);
        //string F_ID = Request.QueryString["F_ID"];

        string sqltxt = "select F_FileData,F_FileType from IridianDev2.dbo.OnlinemappingFileData  Where F_ID = " + DataF_ID;
        DataTable dtFilePath = Utils.accessDB(sqltxt, "");

        if (dtFilePath.Rows.Count > 0)
        {
            lblIndicator.Visible = false;
            textarea.Visible = false;

            string strExtenstion = dtFilePath.Rows[0]["F_FileType"].ToString().ToLower();
            Byte[] bytFile = (Byte[])dtFilePath.Rows[0]["F_FileData"];
            /*
            string fileName = DateTime.Now.Ticks + "." + strExtenstion;
            string filePath = System.Configuration.ConfigurationManager.AppSettings["ApplicationRoot"].ToString() + "\\Uploads\\" + fileName;
            MemoryStream m = new MemoryStream(bytFile);
            FileStream f = new FileStream(filePath, FileMode.Create);
            m.WriteTo(f);
            m.Close();
            f.Close();

            string script = "<script language='javascript'>document.all.ifImage.src = '../Uploads/" + fileName + "' </script>";
            lblJavaScript.Text = script;
            */
            /* It's better to write byte[] into response directly. But I haven't got it work as below.
            ** Now, I have to write it into a file and then display the file. The performance is lowered.
            ** If someone can solve this problem, I would be more than happy to shift to direct byte writing.
            ** Tiebiao Shi 12/11/2008
            */

            Response.Clear();
            Response.Buffer = true;

            if (strExtenstion == "0")
            {
                Response.ContentType = "text";
                Response.AddHeader("content-disposition", "inline;filename=file.txt");
            }
            else if (strExtenstion == "1" || strExtenstion == "xlsx")
            {
                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("content-disposition", "inline;filename=file.xls");
            }
            else if (strExtenstion == "2")
            {
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "inline;filename=file.pdf");
            }
            else if (strExtenstion == "3")
            {
                Response.ContentType = "text/xml";
                Response.AddHeader("content-disposition", "inline;filename=file.xml");
            }
            else if (strExtenstion == "4")
            {
                Response.ContentType = "application/octet-stream";
                Response.AddHeader("content-disposition", "attachment;filename=file.csv");
            }
            Response.Charset = "";

            Response.BinaryWrite(bytFile);
            Response.End();
        }
        else
            lblIndicator.Visible = true;
    }
}