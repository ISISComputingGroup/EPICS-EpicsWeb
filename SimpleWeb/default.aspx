<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="SimpleWeb.Default" %>

<%@ Register src="Dashboard.ascx" tagname="Dashboard" tagprefix="uc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Instrument Information</title>
    <meta http-equiv="refresh" content="10" />
    <link href="Styles/Simple.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <uc1:Dashboard ID="Dashboard1" runat="server" />
    
    </div>
    </form>
</body>
</html>
