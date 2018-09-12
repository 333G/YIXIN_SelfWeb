<%@ Page Language="C#" AutoEventWireup="true" CodeFile="OnlinePreview.aspx.cs" Inherits="OnlineMapping_OnlinePreview" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <table width="100%">
                <tr>
                    <td align="center">
                        <asp:Label ID="lblIndicator" Visible="false" runat="server" Text="No original file for Email"></asp:Label>
                    </td>
                    <td align="center">
                        <textarea id="textarea" rows="10" visible="false" runat="server" cols="20" style="resize: none" readonly="readonly"></textarea>
                    </td>
                </tr>
            </table>
        </div>
    </form>
</body>
</html>