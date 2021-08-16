<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ScheduleApp._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div style="width:200px; margin: auto; padding-top:100px;">
        <h4>DnD Scheduling App</h4>
        <div>
            Name:
        </div>
        <div>
            <asp:TextBox runat="server" ID="txtName"></asp:TextBox>
        </div>
        <div>
            Password:
        </div>
        <div>
            <asp:TextBox runat="server" ID="txtPassword"></asp:TextBox>
        </div>
        <div style="padding-top:5px;">
            <asp:Button runat="server" ID="btnGo" Text="Go" OnClick="btnGo_Click"/>
        </div>
        <div>
            <asp:Label runat="server" ID="lblError" Visible="false" CssClass="ErrorText"></asp:Label>
        </div>
    </div>
</asp:Content>
