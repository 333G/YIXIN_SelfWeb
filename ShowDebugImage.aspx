<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ShowDebugImage.aspx.cs" Inherits="OnlineMapping_ShowDebugImage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
             <table >
                <tr>
                    <td>
                        <asp:Label ID="lblIndicator" Visible="false" runat="server" Text="No original file for Email"></asp:Label>
                    </td>
                    <td>
                        <textarea id="textarea" rows="10" visible="false" runat="server" cols="20" style="resize: none" readonly="readonly"></textarea>
                    </td>
                </tr>
            </table>
        </div>
    </form>
</body>
</html>
