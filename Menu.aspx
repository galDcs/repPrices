<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Menu.aspx.cs" Inherits="Menu" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <script>
        function moveToHotelGeneralDetails(supplierId, supplierName, areaId, areaName) {
            var d = new Date();

            window.location = 'HotelPriceGeneralDetails.aspx?month=' + (d.getMonth() + 1) + '&year=' + d.getFullYear() + '&supplierId=' + supplierId + '&supplierName=' + supplierName + '&clerkId=' + $('#' + '<%= hiddenClerkId.ClientID %>').val() + "&areaId=" + areaId + '&areaName=' + areaName;
        }
    </script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server">
    <asp:HiddenField ID="hiddenClerkId" runat="server" />

    <div id="formView" runat="server" class="formView" >
        <h1><%= Utils.getTextFromFile("HotelPriceSearchHeadLine", mLang) %></h1>

        <div id="divSelectHotel">
            <asp:TextBox ID="txtHotelName" runat="server" style="text-align:right; " placeholder="בחר מלון">
            </asp:TextBox>
            <asp:Button ID="btSearchHotels" runat="server" OnClick="btSearchHotels_Click"/>
            <asp:Table ID="tableHotelsSearchResult" runat="server" CssClass="tableResult" Visible="false">
                <asp:TableHeaderRow>
                    <asp:TableHeaderCell Width="10%">קוד מלון</asp:TableHeaderCell>
                    <asp:TableHeaderCell Width="62%">שם מלון</asp:TableHeaderCell>
                    <asp:TableHeaderCell Width="20%">אזור</asp:TableHeaderCell>
                    <asp:TableHeaderCell Width="8%">פתוח/סגור</asp:TableHeaderCell>
                </asp:TableHeaderRow>
            </asp:Table>
        </div>
    </div>
</asp:Content>

