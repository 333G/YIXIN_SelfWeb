using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace CommonResponseCenter.Responser
{
    internal class OnlineMappingRP : ResponserBase
    {
        public OnlineMappingRP(int userID, int companyID, HttpContext context)
           : base(userID, companyID, context)
        { }

        private string escapeSqlString(string likeString)
        {
            return likeString.Replace("'", "''").Replace("/", "//").Replace("%", "/%").Replace("_", "/_");
        }

        public string UpLoadFile()
        {
            try
            {
                var Filename = PostObjectDateValue("Filename");
                //var File = PostObjectValue("File");
                DateTime dateTime = Convert.ToDateTime(Filename);
                string year = dateTime.Year.ToString();
                string month = dateTime.Month.ToString();
                string day = dateTime.Day.ToString();
                string filename = year + "-" + month + "-" + day;
                string tagpath = @"C:\Users\HK\Desktop\" + filename + @"(Mapping)\";
                string listfilepath = @"C:\Users\HK\Desktop\" + filename + @"(Mapping)\List.xls";
                List<MailInfo> MailInfoList = new List<MailInfo>();
                List<string> FailedList = new List<string>();
                // DataSet ds = new DataSet();
                //DataTable dt = null;
                NPOI.HSSF.UserModel.HSSFWorkbook book;
                try
                {
                    FileStream fs = new FileStream(listfilepath, FileMode.Open, FileAccess.Read);
                    book = new NPOI.HSSF.UserModel.HSSFWorkbook(fs);
                }
                catch (Exception e)
                {
                    return GetJsonString("Error", "打开文件错误!" + e);
                }
                DateTime now = DateTime.Now;
                int sheetCount = book.NumberOfSheets;
                for (int sheetIndex = 0; sheetIndex < sheetCount; sheetIndex++)
                {
                    NPOI.SS.UserModel.ISheet sheet = book.GetSheetAt(sheetIndex);
                    if (sheet == null) continue;

                    NPOI.SS.UserModel.IRow row = sheet.GetRow(0);
                    if (row == null) continue;

                    int firstCellNum = row.FirstCellNum;
                    int lastCellNum = 7;
                    if (firstCellNum == lastCellNum) continue;

                    //dt = new DataTable(sheet.SheetName);
                    //for (int i = firstCellNum; i < lastCellNum; i++)
                    //{
                    //    dt.Columns.Add(row.GetCell(i).StringCellValue, typeof(string));
                    //}
                    try
                    {
                        for (int i = 0; i <= sheet.LastRowNum; i++)
                        {
                            //DataRow newRow = dt.Rows.Add();
                            MailInfo mailInfo = new MailInfo();
                            mailInfo.F_UploadDate = now.Date;
                            mailInfo.F_IshaveFile = 0;
                            for (int j = firstCellNum; j < lastCellNum; j++)
                            {
                                //newRow[j] = sheet.GetRow(i).GetCell(j).StringCellValue;
                                if (j == firstCellNum)
                                {
                                    if (sheet.GetRow(i).GetCell(j) != null)
                                        mailInfo.CompanyName = (sheet.GetRow(i).GetCell(j).ToString().Trim());
                                    else
                                        mailInfo.CompanyName = "";
                                    if (mailInfo.CompanyName[mailInfo.CompanyName.Length - 1] == '.')
                                        mailInfo.CompanyName = mailInfo.CompanyName.Substring(0, mailInfo.CompanyName.Length - 1);
                                }
                                else if (j == firstCellNum + 1)
                                {
                                    if (sheet.GetRow(i).GetCell(j) != null)
                                        mailInfo.F_FaildReason = sheet.GetRow(i).GetCell(j).ToString();
                                    else
                                        mailInfo.F_FaildReason = "";
                                }
                                else if (j == firstCellNum + 2)
                                {
                                    if (sheet.GetRow(i).GetCell(j) != null)
                                        mailInfo.F_ClassName = sheet.GetRow(i).GetCell(j).ToString().Replace(".", "");
                                    else
                                        mailInfo.F_ClassName = "";
                                }
                                else if (j == firstCellNum + 3)
                                {
                                    if (sheet.GetRow(i).GetCell(j) != null)
                                        mailInfo.F_DealType = sheet.GetRow(i).GetCell(j).ToString();
                                    else
                                        mailInfo.F_DealType = "";
                                }
                                else if (j == firstCellNum + 4)
                                {
                                    if (sheet.GetRow(i).GetCell(j) != null)
                                        mailInfo.F_DealResult = sheet.GetRow(i).GetCell(j).ToString();
                                    else
                                        mailInfo.F_DealResult = "";
                                }
                                else if (j == firstCellNum + 5)
                                {
                                    if (sheet.GetRow(i).GetCell(j) != null)
                                        mailInfo.F_FileEndingDate = sheet.GetRow(i).GetCell(j).ToString().ToDateTime().Date;
                                    else
                                        mailInfo.F_FileEndingDate = DateTime.MinValue;
                                }
                                else if (j == firstCellNum + 6)
                                {
                                    if (sheet.GetRow(i).GetCell(j) != null)
                                        mailInfo.F_ChangeDetails = sheet.GetRow(i).GetCell(j).ToString();
                                    else
                                        mailInfo.F_ChangeDetails = "";
                                }

                            }
                            if (mailInfo.F_ClassName.Length > 3)
                                if (mailInfo.F_ClassName.Substring(0, 3).ToUpper().IndexOf("XLS") != -1)
                                    mailInfo.F_FileType = 1;
                                else if (mailInfo.F_ClassName.Substring(0, 3).ToUpper().IndexOf("PDF") != -1)
                                    mailInfo.F_FileType = 2;
                                else if (mailInfo.F_ClassName.Substring(0, 3).ToUpper().IndexOf("XML") != -1)
                                    mailInfo.F_FileType = 3;
                                else if (mailInfo.F_ClassName.Substring(0, 3).ToUpper().IndexOf("CVS") != -1)
                                    mailInfo.F_FileType = 4;
                                else mailInfo.F_FileType = 0;
                            if (mailInfo.F_DealResult.ToUpper().IndexOf("PENDING") != -1)
                            {
                                mailInfo.F_FileType = -1;
                                mailInfo.F_ClassName = "";
                            }
                            MailInfoList.Add(mailInfo);
                        }
                    }
                    catch
                    {

                    }
                    //ds.Tables.Add(dt);
                }

                //SqlConnection conn = new SqlConnection(SqlHelper.MyCONNECTSTRING);
                if (MailInfoList.Count > 0)
                {
                    try
                    {
                        string filepath = tagpath;

                        filepath = tagpath + "Debug";
                        List<FilePath> DebugFilelist = new List<FilePath>();
                        string[] diarrdebug = Directory.GetDirectories(@filepath, "*", SearchOption.AllDirectories);
                        for (int i = 0; i < diarrdebug.Length; i++)
                        {
                            string[] rootfilelist = System.IO.Directory.GetFiles(diarrdebug[i]);
                            List<File> filelist = new List<File>();
                            for (int j = 0; j < rootfilelist.Length; j++)
                            {
                                if (rootfilelist[j].Contains("rar") || rootfilelist[j].ToUpper().IndexOf("MAIL.TXT") != -1)
                                    continue;
                                else
                                {
                                    File file = new File();
                                    file.Path = rootfilelist[j];
                                    file.Type = rootfilelist[j].Substring(rootfilelist[j].LastIndexOf(".") + 1).ToUpper();
                                    file.isbeenread = false;
                                    filelist.Add(file);
                                }
                            }
                            FilePath filePath = new FilePath();
                            filePath.Path = diarrdebug[i];
                            filePath.ChildPath = filelist;
                            DebugFilelist.Add(filePath);
                        }

                        filepath = tagpath + "New";
                        List<FilePath> NewFilelist = new List<FilePath>();
                        string[] diarrnew = Directory.GetDirectories(@filepath, "*", SearchOption.AllDirectories);
                        for (int i = 0; i < diarrnew.Length; i++)
                        {
                            string[] rootfilelist = Directory.GetFiles(diarrnew[i]);
                            List<File> filelist = new List<File>();
                            for (int j = 0; j < rootfilelist.Length; j++)
                            {
                                if (rootfilelist[j].Contains("rar") || rootfilelist[j].ToUpper().IndexOf("MAIL.TXT") != -1)
                                    continue;
                                else
                                {
                                    File file = new File();
                                    file.Path = rootfilelist[j];
                                    file.Type = rootfilelist[j].Substring(rootfilelist[j].LastIndexOf(".") + 1).ToUpper();
                                    file.isbeenread = false;
                                    filelist.Add(file);
                                }
                            }
                            FilePath filePath = new FilePath();
                            filePath.Path = diarrnew[i];
                            filePath.ChildPath = filelist;
                            NewFilelist.Add(filePath);
                        }

                        filepath = tagpath + "Renew";
                        List<FilePath> RenewFilelist = new List<FilePath>();
                        string[] diarrrenew = Directory.GetDirectories(@filepath, "*", SearchOption.AllDirectories);
                        for (int i = 0; i < diarrrenew.Length; i++)
                        {
                            string[] rootfilelist = System.IO.Directory.GetFiles(diarrrenew[i]);
                            List<File> filelist = new List<File>();
                            for (int j = 0; j < rootfilelist.Length; j++)
                            {
                                if (rootfilelist[j].Contains("rar") || rootfilelist[j].ToUpper().IndexOf("MAIL.TXT") != -1)
                                    continue;
                                else
                                {
                                    File file = new File();
                                    file.Path = rootfilelist[j];
                                    file.Type = rootfilelist[j].Substring(rootfilelist[j].LastIndexOf(".") + 1).ToUpper();
                                    file.isbeenread = false;
                                    filelist.Add(file);
                                }
                            }
                            FilePath filePath = new FilePath();
                            filePath.Path = diarrrenew[i];
                            filePath.ChildPath = filelist;
                            RenewFilelist.Add(filePath);
                        }

                        foreach (var item in MailInfoList)
                        {
                            item.CompanyID = GetCompanyIdbyNameDB(item.CompanyName);
                            item.F_LastModifiedTime = DateTime.Now;
                            item.F_LastModifiedUser = "Admin";
                            long InsertId = InserttotableDB(item);
                            if (InsertId > 0)
                            {
                                //插入到另一个数据表中
                                MailData mailData = new MailData
                                {
                                    EmailFID = InsertId,
                                    F_UploadDate = item.F_UploadDate,
                                    F_LastModifiedDate = DateTime.Now,
                                };
                                if (item.F_DealType.ToUpper().IndexOf("DEBUG") != -1)
                                {
                                    var fileTypeAndData = GetFileDataAndType(item, DebugFilelist);
                                    mailData.F_FileData = fileTypeAndData.Data;
                                    mailData.F_FileType = fileTypeAndData.Type;
                                }
                                else if (item.F_DealType.ToUpper() == "NEW")
                                {
                                    var fileTypeAndData = GetFileDataAndType(item, NewFilelist);
                                    mailData.F_FileData = fileTypeAndData.Data;
                                    mailData.F_FileType = fileTypeAndData.Type;
                                }
                                else if (item.F_DealType.ToUpper() == "RENEW")
                                {
                                    var fileTypeAndData = GetFileDataAndType(item, RenewFilelist);
                                    mailData.F_FileData = fileTypeAndData.Data;
                                    mailData.F_FileType = fileTypeAndData.Type;
                                }
                                mailData.F_LastModifiedUser = "Admin";
                                if (mailData.F_FileData != null)
                                {
                                    long FID = DataInsertToTableDB(mailData);
                                    if (FID > 0)
                                        UpdateishavefileDB(InsertId, FID);
                                }
                                else
                                    FailedList.Add(item.CompanyName + "未找到文件，请检查");
                            }
                        }

                        //===========================================BulkInsert========================================================
                        //SqlBulkCopy SqlBulkCopy =new SqlBulkCopy( SqlHelper.MyCONNECTSTRING);
                        //SqlBulkCopy.DestinationTableName = "IridianDev2.dbo.OnlieMapping";
                        //DataTable dt = ToDataTable(MailInfoList);
                        //for (int i = 0; i < dt.Columns.Count; i++)
                        //{
                        //    SqlBulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                        //}
                        //SqlBulkCopy.WriteToServer(dt);
                        //===========================================BulkInsert========================================================
                    }
                    catch (Exception ex)
                    {
                        return GetJsonString("Error", ex.Message);
                    }
                    finally
                    {
                        // conn.Dispose();
                    }
                    return GetJsonString("Result", MailInfoList.Count, "Info", FailedList);
                }

                return GetJsonString("Error", "本次提交的文件中没有数据.");
            }
            catch (Exception ex)
            {
                return GetJsonString("Error", ex.Message);
            }
        }

        public string GetCount()
        {
            List<string> DebugList= new List<string>();
            List<string> NewList = new List<string>();
            List<string> RenewList = new List<string>();

            int debugcount = 0, newcount = 0 ,renewcount = 0;
            var txtSearch_CompanyName = PostObjectStringValue("txtSearch_CompanyName").Trim();
            var textSearch_Emailcontent = PostObjectStringValue("textSearch_Emailcontent").Trim();
            var txtSearch_ClassName = PostObjectStringValue("txtSearch_ClassName").Trim();
            DateTime? textSearch_DateStart = PostObjectDateValue("textSearch_DateStart");
            DateTime? textSearch_DateEnd = PostObjectDateValue("textSearch_DateEnd");
            try
            {
                DataSet ds = GetMappinglistdataSet(txtSearch_CompanyName, textSearch_Emailcontent, txtSearch_ClassName, textSearch_DateStart, textSearch_DateEnd);
                //=============================================================================================
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        if (ds.Tables[0].Rows[i][5].ToString().ToUpper() == "DEBUG" && !DebugList.Contains(ds.Tables[0].Rows[i][1]))
                        {
                            debugcount++;
                            DebugList.Add(ds.Tables[0].Rows[i][1].ToString());
                        }
                        else if (ds.Tables[0].Rows[i][5].ToString().ToUpper() == "NEW" && !NewList.Contains(ds.Tables[0].Rows[i][1]))
                        {
                            newcount++;
                            NewList.Add(ds.Tables[0].Rows[i][1].ToString());
                        }
                        else if (ds.Tables[0].Rows[i][5].ToString().ToUpper() == "RENEW" && !RenewList.Contains(ds.Tables[0].Rows[i][1]))
                        {
                            renewcount++;
                            RenewList.Add(ds.Tables[0].Rows[i][1].ToString());
                        }
                    }
                    return GetJsonString("type", "Debug: " + debugcount + ", " + "New: " + newcount + ", " + "Renew: " + renewcount);
                }
                else
                    return GetJsonString("type", "0,0,0");
            }
            catch
            {
                return GetJsonString("type", "0,0,0");
            }
        }

        private fileTypeAndData GetFileDataAndType(MailInfo mailInfo, List<FilePath> filePathlist)
        {
            fileTypeAndData fileTypeAndData = new fileTypeAndData();
            FilePath filePath = new FilePath();
            string ComapanyName = mailInfo.CompanyName.ToUpper().Replace(".", "").Replace("/", "").Replace(" ", "");
            string thisfilepath = "";
            foreach (var item in filePathlist)
            {
                if (item.Path.Substring(item.Path.LastIndexOf("\\") + 1).ToUpper().Replace(".", "").Replace("/","").Replace(" ","") == ComapanyName)
                {
                    for (int i = 0; i < item.ChildPath.Count; i++)
                    {
                        if (!item.ChildPath[i].isbeenread)
                        {
                            thisfilepath = item.ChildPath[i].Path;
                            item.ChildPath[i].isbeenread = true;

                            if (item.ChildPath[i].Type.ToUpper() == "TXT")
                                fileTypeAndData.Type = 0;
                            else if (item.ChildPath[i].Type.ToUpper().IndexOf("XLS") != -1)
                                fileTypeAndData.Type = 1;
                            else if (item.ChildPath[i].Type.ToUpper().IndexOf("PDF") != -1)
                                fileTypeAndData.Type = 2;
                            else if (item.ChildPath[i].Type.ToUpper().IndexOf("XML") != -1)
                                fileTypeAndData.Type = 3;
                            else if (item.ChildPath[i].Type.ToUpper().IndexOf("CSV") != -1)
                                fileTypeAndData.Type = 4;
                            break;
                        }
                    }
                }
            }
            if (thisfilepath != "")
            {
                FileStream fs = new FileStream(thisfilepath, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                fileTypeAndData.Data = br.ReadBytes((int)fs.Length);
                fs.Close();
            }
            return fileTypeAndData;
        }

        public string GetMappingListData()
        {
            try
            {
                var txtSearch_CompanyName = PostObjectStringValue("txtSearch_CompanyName").Trim();
                var textSearch_Emailcontent = PostObjectStringValue("textSearch_Emailcontent").Trim();
                var txtSearch_ClassName = PostObjectStringValue("txtSearch_ClassName").Trim();
                DateTime? textSearch_DateStart = PostObjectDateValue("textSearch_DateStart");
                DateTime? textSearch_DateEnd = PostObjectDateValue("textSearch_DateEnd");
                DataSet ds = GetMappinglistdataSet(txtSearch_CompanyName, textSearch_Emailcontent, txtSearch_ClassName, textSearch_DateStart, textSearch_DateEnd);
                //=============================================================================================
                if (ds.Tables[0].Rows.Count > 0)
                    return GetJsonString("type", ds.Tables[0], "MappingDataList", ds.Tables[0].Rows[0]);
                else
                    return GetJsonString("type", null);
            }
            catch (Exception ex)
            {
                return GetJsonString("Errorinfo", ex.Message);
            }
        }

        private DataSet GetMappinglistdataSet(string txtSearch_CompanyName,string textSearch_Emailcontent,string txtSearch_ClassName, DateTime? textSearch_DateStart, DateTime? textSearch_DateEnd)
        {
            if (txtSearch_CompanyName != "")
                txtSearch_CompanyName = "%" + escapeSqlString(txtSearch_CompanyName) + "%";
            if (textSearch_Emailcontent != "")
                textSearch_Emailcontent = "%" + escapeSqlString(textSearch_Emailcontent) + "%";
            //=====================================数据库操作==============================================
            ///sql///
            string sqltxt = "select * from IridianDev2.dbo.OnlieMapping  Where 1 = 1 "
                + "AND(@txtSearch_CompanyName = '' OR CompanyName like @txtSearch_CompanyName escape '/')"
                + "AND(@txtSearch_ClassName = '' OR F_ClassName like @txtSearch_ClassName escape '/') "
                + "AND(@textSearch_DateStart is NULL OR F_UploadDate >= @textSearch_DateStart)"
                + "AND(@textSearch_DateEnd IS NULL OR F_UploadDate <= @textSearch_DateEnd)"
                + "AND(@textSearch_Emailcontent = '' OR F_FaildReason like @textSearch_Emailcontent escape '/')"
                + "Order By F_DealType,CompanyName";

            //AND(@From is NULL OR Coalesce(A.LastModifiedDate, A.CreatedDate) >= @From)
            //AND(@To IS NULL OR Coalesce(A.LastModifiedDate, A.CreatedDate) <= @To)
            //AND(@ApplicantName = '' OR CompanyName like @ApplicantName escape '/')

            ///dataset
            DataSet ds = SqlHelper.ExecuteDataset(SqlHelper.MyCONNECTSTRING, CommandType.Text, sqltxt, new SqlParameter("@txtSearch_CompanyName", txtSearch_CompanyName)
            , new SqlParameter("@textSearch_DateStart", (textSearch_DateStart.HasValue ? textSearch_DateStart.Value : Convert.DBNull))
            , new SqlParameter("@textSearch_DateEnd", (textSearch_DateEnd.HasValue ? textSearch_DateEnd.Value : Convert.DBNull))
            , new SqlParameter("@textSearch_Emailcontent", textSearch_Emailcontent)
            , new SqlParameter("@txtSearch_ClassName", txtSearch_ClassName));
            return ds;
        }
        //public string GetMailData()
        //{
        //    var DataF_ID = Convert.ToInt64(PostObjectStringValue("DataF_ID"));

        //    if (DataF_ID > 0)
        //    {
        //        try
        //        {
        //            string sqltxt = "select F_FileData,F_FileType from IridianDev2.dbo.OnlinemappingFileData  Where F_ID = @DataF_ID";
        //            DataSet ds = SqlHelper.ExecuteDataset(SqlHelper.MyCONNECTSTRING, CommandType.Text, sqltxt, new SqlParameter("@DataF_ID", DataF_ID));
        //            if (ds.Tables[0].Rows.Count > 0)
        //                return GetJsonString("type", ds.Tables[0]);
        //            else
        //                return GetJsonString("Errorinfo", "No data found!");
        //        }
        //        catch(Exception ex)
        //        {
        //            return GetJsonString("Errorinfo", ex.Message);
        //        }
        //    }
        //    return GetJsonString("Errorinfo", "NotValid.");
        //}

        public string Submit()
        {
            var input_CompanyName = escapeSqlString(PostObjectStringValue("input_CompanyName").Trim());
            var input_ClassName = PostObjectStringValue("input_ClassName").Trim();
            var input_FailedReason = PostObjectStringValue("input_FailedReason").Trim();
            var input_FailedType = PostObjectStringValue("input_FailedType").Trim();
            var input_Result = PostObjectStringValue("input_Result").Trim();
            var input_WeakEndingDate = PostObjectDateValue("input_WeakEndingDate");
            var input_FileType = PostObjectStringValue("input_FileType").Trim();
            var input_ChangeDetails = PostObjectStringValue("input_ChangeDetails").Trim();

            long Insert_FID = -1;
            try
            {
                MailInfo mailInfo = new MailInfo();
                mailInfo.F_ClassName = input_ClassName;
                mailInfo.CompanyName = input_CompanyName;
                mailInfo.F_FaildReason = input_FailedReason;
                mailInfo.F_DealType = input_FailedType;
                mailInfo.F_DealResult = input_Result;
                string CompanyId = GetCompanyIdbyNameDB(input_CompanyName);
                mailInfo.F_UploadDate = DateTime.Now.Date;
                mailInfo.F_FileType = Convert.ToInt32(input_FileType);
                mailInfo.F_LastModifiedTime = DateTime.Now;
                mailInfo.F_IshaveFile = 0;
                mailInfo.F_LastModifiedUser = "Admin";
                mailInfo.F_FileEndingDate = input_WeakEndingDate;
                mailInfo.F_ChangeDetails = input_ChangeDetails;
                mailInfo.F_ID = InserttotableDB(mailInfo);

                long DataF_ID = InserttoDataDB(mailInfo);
                if (DataF_ID > 0)
                {
                    UpdateishavefileDB(mailInfo.F_ID , DataF_ID);
                }
            }
            catch (Exception ex)
            {
                return GetJsonString("Error", ex.Message);
            }
            finally
            {
            }
            return GetJsonString("type", Insert_FID);
        }

        public string UploadData()
        {
            var F_ID = Convert.ToInt64(PostObjectStringValue("F_ID"));
            var CompanyName = PostObjectStringValue("CompanyName");
            MailInfo mailInfo = new MailInfo()
            {
                F_ID = F_ID,
                CompanyName = CompanyName,
            };
            try
            {
                int i = InserttoDataDB(mailInfo);
                if (i > 0)
                {
                    UpdateishavefileDB(F_ID, i);
                    return GetJsonString("type", i);
                }
                return GetJsonString("Error", CompanyName + " 的数据提交失败了，请重试或联系管理员！");
            }
            catch (Exception ex)
            {
                return GetJsonString("Error", CompanyName + " 的数据提交失败了，请重试或联系管理员！" + ex.Message);
            }
        }

        public string ReUploadData()
        {
            var F_ID = Convert.ToInt64(PostObjectStringValue("F_ID"));
            var DataF_ID = Convert.ToInt64(PostObjectStringValue("DataF_ID"));
            var CompanyName = PostObjectStringValue("CompanyName");
            MailInfo mailInfo = new MailInfo()
            {
                F_ID = F_ID,
                CompanyName = CompanyName,
            };
            try
            {
                int i = InserttoDataDB(mailInfo);
                if (i > 0)
                {
                    UpdateishavefileDB(F_ID, i);
                }
                if (DataF_ID > 0)
                {
                    DeleteDataDB(DataF_ID);
                }
                return GetJsonString("type", "操作成功。");
            }
            catch(Exception ex)
            {
                return GetJsonString("Error", CompanyName + "的数据提交失败了，请重试或者联系管理员！" + ex.Message);
            }
        }

        public string Delete()
        {
            var F_ID = Convert.ToInt64(PostObjectStringValue("F_ID"));
            var DataF_ID = Convert.ToInt64(PostObjectStringValue("DataF_ID"));
            try
            {
                if (F_ID > 0)
                {
                    DeleteMailDB(F_ID);
                }
                if (DataF_ID > 0)
                {
                    DeleteDataDB(DataF_ID);
                }
            }
            catch(Exception ex)
            {
                return GetJsonString("Error", ex.Message);
            }
            return GetJsonString("Type", "删除成功");
        }

        public string ChangeClassName()
        {
            var F_ID = Convert.ToInt64(PostObjectStringValue("F_ID"));
            var F_ClassName = Convert.ToString(PostObjectStringValue("F_ClassName")).Trim();
            try
            {
                if (F_ID > 0 && F_ClassName != "")
                {
                    ChangeClassNameDB(F_ID, F_ClassName);
                }
                else
                {
                    return GetJsonString("Error", "请输入有效类名");
                }
            }
            catch (Exception ex)
            {
                return GetJsonString("Error", ex.Message);
            }
            return GetJsonString("Type", "类名已经修改为" + F_ClassName + ".");
        }

        
        public string GetImageAjax()
        {
            var filedata = CurrentContext;
            var file = CurrentContext.Request.Files[0];
            var F_ID = Convert.ToInt64(CurrentContext.Request.Form["F_ID"]);

            try
            {
                Byte[] fileData = new Byte[] { };
                if (file != null)
                {
                    Stream stream = file.InputStream;
                    BinaryReader br = new BinaryReader(stream);
                    fileData = br.ReadBytes((int)stream.Length);
                    stream.Close();
                    if (filedata != null)
                    {
                        UpdateImageDataDB(F_ID, fileData);
                    }
                }
                return GetJsonString("Type", "Success");
            }
            catch (Exception ex)
            {
                return GetJsonString("Error", ex.Message);
            }

        }
        public string ChangeDetails()
        {
            var F_ID = Convert.ToInt64(PostObjectStringValue("F_ID"));
            var F_ChangeDetails = Convert.ToString(PostObjectStringValue("F_ChangeDetails")).Trim();
            try
            {
                if (F_ID > 0 )
                {
                    ChangeDetailsDB(F_ID, F_ChangeDetails);
                }
                else
                {
                    return GetJsonString("Error", "ID未识别或不存在");
                }
            }
            catch (Exception ex)
            {
                return GetJsonString("Error", ex.Message);
            }
            return GetJsonString("Type", "Success");
        }
        /// <summary>
        /// Convert a List{T} to a DataTable.
        /// </summary>
        private DataTable ToDataTable<T>(List<T> items)
        {
            var tb = new DataTable(typeof(T).Name);

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                Type t = GetCoreType(prop.PropertyType);
                tb.Columns.Add(prop.Name, t);
            }

            foreach (T item in items)
            {
                var values = new object[props.Length];

                for (int i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            return tb;
        }

        /// <summary>
        /// Return underlying type if type is Nullable otherwise return the type
        /// </summary>
        public static Type GetCoreType(Type t)
        {
            if (t != null && IsNullable(t))
            {
                if (!t.IsValueType)
                {
                    return t;
                }
                else
                {
                    return Nullable.GetUnderlyingType(t);
                }
            }
            else
            {
                return t;
            }
        }

        /// <summary>
        /// Determine of specified type is nullable
        /// </summary>
        public static bool IsNullable(Type t)
        {
            return !t.IsValueType || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        private string GetCompanyIdbyNameDB(string CompanyName)
        {
            string CompanyId = "0";
            string query = "select CompanyID,CompanyName from IridianDev2.dbo.AccountingFileClasses where CompanyName =  @CompanyName";
            DataSet ds = SqlHelper.ExecuteDataset(SqlHelper.MyCONNECTSTRING, CommandType.Text, query, new SqlParameter("@CompanyName", CompanyName));
            if (ds != null)
            {
                if (ds.Tables[0].Rows.Count > 0)
                    CompanyId = ds.Tables[0].Rows[0]["CompanyID"].ToString();
            }
            return CompanyId;
        }

        private int InserttotableDB(MailInfo mailInfo)
        {
            string sqltxt = "insert into IridianDev2.dbo.OnlieMapping(CompanyName,CompanyID,F_FaildReason,F_ClassName,F_DealType,F_DealResult,F_UploadDate,F_FileEndingDate,F_FileType,F_LastModifiedTime,F_LastModifiedUser,F_IshaveFile,F_ChangeDetails)"
                    + " values(@CompanyName,@CompanyID,@F_FaildReason,@F_ClassName,@F_DealType,@F_DealResult,@F_UploadDate,@F_FileEndingDate,@F_FileType,@F_LastModifiedTime,@F_LastModifiedUser ,@F_IshaveFile,@F_ChangeDetails) SELECT CAST(scope_identity() AS int)";

            int i = (int)SqlHelper.ExecuteScalar(SqlHelper.MyCONNECTSTRING, CommandType.Text, sqltxt, new SqlParameter("@CompanyName", mailInfo.CompanyName)
                , new SqlParameter("@CompanyID", (mailInfo.CompanyID == null ? "" : mailInfo.CompanyID))
                , new SqlParameter("@F_FaildReason", (mailInfo.F_FaildReason == null ? "" : mailInfo.F_FaildReason))
                , new SqlParameter("@F_ClassName", (mailInfo.F_ClassName == null ? "" : mailInfo.F_ClassName))
                , new SqlParameter("@F_DealType", (mailInfo.F_DealType == null ? "" : mailInfo.F_DealType))
                , new SqlParameter("@F_DealResult", (mailInfo.F_DealResult == null ? "" : mailInfo.F_DealResult))
                , new SqlParameter("@F_UploadDate", (mailInfo.F_UploadDate.HasValue ? mailInfo.F_UploadDate : Convert.DBNull))
                , new SqlParameter("@F_FileEndingDate", (mailInfo.F_FileEndingDate != DateTime.MinValue && mailInfo.F_FileEndingDate.HasValue ? mailInfo.F_FileEndingDate : Convert.DBNull))
                , new SqlParameter("@F_FileType", mailInfo.F_FileType)
                , new SqlParameter("@F_LastModifiedTime", (mailInfo.F_LastModifiedTime != DateTime.MinValue ? mailInfo.F_LastModifiedTime : new DateTime(1900, 1, 1)))
                , new SqlParameter("@F_LastModifiedUser", (mailInfo.F_LastModifiedUser == null ? "" : mailInfo.F_LastModifiedUser))
                , new SqlParameter("@F_IshaveFile", mailInfo.F_IshaveFile)
                , new SqlParameter("@F_ChangeDetails", (mailInfo.F_ChangeDetails == null ? "" : mailInfo.F_ChangeDetails))
                );
            return i;
        }

        private int InserttoDataDB(MailInfo mailInfo)
        {
            MailData mailData = new MailData();
            mailData.EmailFID = mailInfo.F_ID;
            mailData.F_UploadDate = DateTime.Now.Date;
            mailData.F_LastModifiedDate = DateTime.Now;
            mailData.F_LastModifiedUser = "Admin";

            string filepath = @"C:\Users\HK\Desktop\tmpfile\dailyupdate";

            List<FilePath> Filelist = new List<FilePath>();
            string[] diarr = Directory.GetDirectories(@filepath, "*", SearchOption.AllDirectories);
            for (int i = 0; i < diarr.Length; i++)
            {
                string[] rootfilelist = Directory.GetFiles(diarr[i]);
                List<File> filelist = new List<File>();
                for (int j = 0; j < rootfilelist.Length; j++)
                {
                    if (rootfilelist[j].Contains("rar") || rootfilelist[j].ToUpper().IndexOf("MAIL.TXT") != -1)
                        continue;
                    else
                    {
                        File file = new File();
                        file.Path = rootfilelist[j];
                        file.Type = rootfilelist[j].Substring(rootfilelist[j].LastIndexOf(".") + 1).ToUpper();
                        file.isbeenread = false;
                        filelist.Add(file);
                    }
                }
                FilePath filePath = new FilePath();
                filePath.Path = diarr[i];
                filePath.ChildPath = filelist;
                Filelist.Add(filePath);
            }
            var fileTypeAndData = GetFileDataAndType(mailInfo, Filelist);
            mailData.F_FileData = fileTypeAndData.Data;
            mailData.F_FileType = fileTypeAndData.Type;
            try
            {
                return DataInsertToTableDB(mailData);
            }
            catch(Exception ex)
            {
                return -1;
            }

        }

        private void UpdateishavefileDB(long F_ID, long State)
        {
            string sqltxt = "update IridianDev2.dbo.OnlieMapping set F_IshaveFile = @F_IshaveFile  where F_ID = @F_ID";
            SqlHelper.ExecuteNonQuery(SqlHelper.MyCONNECTSTRING, CommandType.Text, sqltxt, new SqlParameter("@F_IshaveFile", State)
                , new SqlParameter("@F_ID", F_ID));
        }

        private int DataInsertToTableDB(MailData mailData)
        {
            string sqltxt = "insert into IridianDev2.dbo.OnlinemappingFileData(EmailFID,F_UploadDate,F_LastModifiedDate,F_LastModifiedUser,F_FileType,F_FileData)"
                   + " values(@EmailFID,@F_UploadDate,@F_LastModifiedDate,@F_LastModifiedUser,@F_FileType,@F_FileData) SELECT CAST(scope_identity() AS int)";

            int i = (int)SqlHelper.ExecuteScalar(SqlHelper.MyCONNECTSTRING, CommandType.Text, sqltxt, new SqlParameter("@EmailFID", mailData.EmailFID)
                , new SqlParameter("@F_UploadDate", (mailData.F_UploadDate.HasValue ? mailData.F_UploadDate.Value : Convert.DBNull))
                , new SqlParameter("@F_LastModifiedDate", (mailData.F_LastModifiedDate.HasValue ? mailData.F_LastModifiedDate.Value : Convert.DBNull))
                , new SqlParameter("@F_LastModifiedUser", mailData.F_LastModifiedUser)
                , new SqlParameter("@F_FileType", mailData.F_FileType)
                , new SqlParameter("@F_FileData", (mailData.F_FileData != null ? mailData.F_FileData : Convert.DBNull)));
            return i;
        }

        private void DeleteMailDB(long F_ID)
        {
            string sqltxt = "delete from IridianDev2.dbo.OnlieMapping where F_ID=@F_ID";
            SqlHelper.ExecuteNonQuery(SqlHelper.MyCONNECTSTRING, CommandType.Text, sqltxt, new SqlParameter("@F_ID", F_ID));
        }

        private void DeleteDataDB(long F_ID)
        {
            string sqltxt = "delete from IridianDev2.dbo.OnlinemappingFileData where F_ID=@F_ID";
            SqlHelper.ExecuteNonQuery(SqlHelper.MyCONNECTSTRING, CommandType.Text, sqltxt, new SqlParameter("@F_ID", F_ID));
        }

        private void ChangeClassNameDB(long F_ID,string newClassName)
        {
            string sqltxt = "update IridianDev2.dbo.OnlieMapping set F_ClassName = @F_ClassName  where F_ID = @F_ID";
            SqlHelper.ExecuteNonQuery(SqlHelper.MyCONNECTSTRING, CommandType.Text, sqltxt, new SqlParameter("@F_ClassName", newClassName)
                , new SqlParameter("@F_ID", F_ID));
        }
        private void ChangeDetailsDB(long F_ID, string details)
        {
            string sqltxt = "update IridianDev2.dbo.OnlieMapping set F_ChangeDetails = @F_ChangeDetails  where F_ID = @F_ID";
            SqlHelper.ExecuteNonQuery(SqlHelper.MyCONNECTSTRING, CommandType.Text, sqltxt, new SqlParameter("@F_ChangeDetails", details)
                , new SqlParameter("@F_ID", F_ID));
        }
        private void UpdateImageDataDB(long F_ID, Byte[] fileData)
        {
            string sqltxt = "update IridianDev2.dbo.OnlieMapping set F_DetailImg = @F_DetailImg  where F_ID = @F_ID";
            SqlHelper.ExecuteNonQuery(SqlHelper.MyCONNECTSTRING, CommandType.Text, sqltxt, new SqlParameter("@F_DetailImg", fileData)
                , new SqlParameter("@F_ID", F_ID));
        }

        private class MailInfo
        {
            public Int64 F_ID { get; set; }
            public string CompanyName { get; set; }
            public string CompanyID { get; set; }
            public string F_FaildReason { get; set; }
            public string F_ClassName { get; set; }
            public string F_DealType { get; set; }
            public string F_DealResult { get; set; }
            public DateTime? F_UploadDate { get; set; }
            public DateTime? F_FileEndingDate { get; set; }

            /// <summary>
            /// 0:TXT||1:XLS||2:PDF||3:XML||4:CVS||......
            /// </summary>
            public int F_FileType { get; set; }

            public DateTime? F_LastModifiedTime { get; set; }
            public string F_LastModifiedUser { get; set; }

            /// <summary>
            /// 0,没有文件绑定||>0,绑定文件的FID
            /// </summary>
            public long F_IshaveFile { get; set; }
            public string F_ChangeDetails { get; set; }
        }

        private class MailData
        {
            public Int64 F_ID { get; set; }

            public Int64 EmailFID { get; set; }

            public DateTime? F_UploadDate { get; set; }

            public DateTime? F_LastModifiedDate { get; set; }

            public string F_LastModifiedUser { get; set; }

            /// <summary>
            /// 0:TXT||1:XLS||2:PDF||3:XML||4:CSV||......
            /// </summary>
            public int F_FileType { get; set; }

            /// <summary>
            /// 用来存储文件转换的二进制数据流,从而将文件数据存入数据库
            /// </summary>
            public Byte[] F_FileData { get; set; }
        }

        private class FilePath
        {
            public string Path { get; set; }

            public List<File> ChildPath { get; set; }
        }

        private class File
        {
            public string Path { get; set; }

            public string Type { get; set; }

            /// <summary>
            /// 此文件是否已经被读取过
            /// </summary>
            public bool isbeenread { get; set; }
        }

        public class fileTypeAndData
        {
            public Byte[] Data { get; set; }

            public int Type { get; set; }
        }
    }
}