<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.ascx.cs" Inherits="SimpleWeb.Dashboard" %>
<%@ OutputCache Duration="30" VaryByParam="PvRoot" %>
<table style="width:600px;">
    <tr>
        <td class="style1">
            <asp:Label ID="lblName" runat="server" CssClass="namelabel" Text="InstName"></asp:Label>
        </td>
        <td>
            
            &nbsp;</td>
    </tr>
    <tr>
        <td class="style1">
            &nbsp;</td>
        <td>
            &nbsp;</td>
    </tr>
    <tr>
        <td valign="top">
            <label>Run Information</label>
            <asp:BulletedList ID="lstRunInfo" runat="server">
            </asp:BulletedList>
        </td>
        <td valign="top">
            <label>Blocks</label>
            <br />

            <asp:Label ID="lblBlocks" runat="server" Text="" ></asp:Label>
        </td>
    </tr>
    <tr>
        <td class="style1">
            <asp:Label ID="lblUpdated" runat="server" Text="" ></asp:Label></td>
        <td valign="top">
            &nbsp;</td>
    </tr>
</table>
