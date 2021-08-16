<%@ Page Title="Scheduling" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Scheduling.aspx.cs" Inherits="ScheduleApp.Pages.Scheduling" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel runat="server" ID="pnlDayList">
    </asp:Panel>
    <div class="Availability">
        <div class="Title">
            <span runat="server" id="lblUsername"></span> Availability
        </div>
        <div class="AvailabilityDropdowns">
            <asp:DropDownList runat="server" ID="drpAvailabilityDay" AutoPostBack="True" OnSelectedIndexChanged="drpAvailabilityDay_SelectedIndexChanged"></asp:DropDownList>
            <asp:DropDownList runat="server" ID="drpAvailabilityStart"></asp:DropDownList>
            <asp:DropDownList runat="server" ID="drpAvailabilityEnd"></asp:DropDownList>
        </div>
        <div style="padding:10px;">
            Comments (Optional):
        </div>
        <div>
            <asp:TextBox runat="server" ID="txtComments" TextMode="MultiLine" MaxLength="100" CssClass="Comments"></asp:TextBox>
        </div>
        <div style="padding-top: 10px;">
            <asp:Button runat="server" ID="btnAvailability" AutoPostBack="True" OnClick="btnAvailability_Click" Text="I'm Available" CssClass="AvailabilityButton"/>
        </div>
    </div>
</asp:Content>
