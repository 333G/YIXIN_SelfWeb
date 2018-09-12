using ElationSys.Utils;
using System;
using System.Data;
using System.Web.UI;

public partial class OnlineMapping_ShowDebugImage : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        var F_ID = Convert.ToInt64(Request.Params["F_ID"]);

        string sqltxt = "select F_DetailImg from IridianDev2.dbo.OnlieMapping  Where F_ID = " + F_ID;
        DataTable dtFilePath = Utils.accessDB(sqltxt, "");

        if (dtFilePath.Rows.Count > 0)
        {
            lblIndicator.Visible = false;
            textarea.Visible = false;
            Byte[] bytFile = (Byte[])dtFilePath.Rows[0]["F_DetailImg"];
            Response.Clear();
            Response.Buffer = true;

            Response.OutputStream.Write(bytFile, 0, bytFile.Length);
            Response.ContentType = "image";
           // Response.AddHeader("content-disposition", "inline;filename=file.txt");
            Response.End();
        }
        else
            lblIndicator.Visible = true;
    }
}