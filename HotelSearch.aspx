<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="HotelSearch.aspx.cs" Inherits="HotelSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <script>
        var txtFromDateID = '<%= txtFromDate.ClientID%>';
        var txtToDateID = '<%= txtToDate.ClientID%>';
        var txtNightsID = '<%= txtNights.ClientID%>';
        var minDateForDatePicker = (new Date("03/01/2017").setHours(0, 0, 0, 0) > new Date().setHours(0, 0, 0, 0)) ? new Date("03/01/2017") : new Date().setHours(0, 0, 0, 0);

        $(document).ready(function () {
            $("#" + txtFromDateID).datepicker({
                minDate: minDateForDatePicker,
                onSelect: function (date) {
                    $("#" + txtToDateID).datepicker('option', 'minDate', date);
                    CalculateToDate();

                }
            });

            $("#" + txtNightsID).bind('blur', function () {
                CalculateToDate();
            });

            $("#" + txtToDateID).datepicker({
                firstDay: 0,
                //minDate: new Date(),
                onSelect: function () {
                    CalculateNights();
                }

            });

            function CalculateNights() {
                var toDateStr = $("#" + txtToDateID).val();
                var fromDateStr = $("#" + txtFromDateID).val();
                var one_day = 1000 * 60 * 60 * 24; // milisec
                var daysDiff = Math.round((ConvertToDate(toDateStr) - ConvertToDate(fromDateStr)) / one_day);
                $("#" + txtNightsID).val(daysDiff);
            }


            function ConvertToDate(dateStr)//converts from dd/mm/yy format
            {
                var arr = dateStr.split("/");
                return new Date(arr[2], arr[1] - 1, arr[0]);
            }

            function CalculateToDate() {
                var nights = parseInt($("#" + txtNightsID).val());
                var fromDate = ConvertToDate($("#" + txtFromDateID).val());
                var newToDate = new Date(fromDate.setDate(fromDate.getDate() + nights));
                $("#" + txtToDateID).val($.datepicker.formatDate("dd/mm/yy", newToDate));
            }
        });
    </script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server">
    <div class="formView">
        <div dir="rtl" style="height: 22px; font-size: 18px; font-weight: bold;">פרטי חיפוש:
        </div>
        <div class="searchRow">
            <h3 style="float: right; width: 30%;">מתאריך</h3>
            <h3 style="float: right; width: 30%;">עד תאריך</h3>
            <h3 style="float: right; width: 30%;">מספר לילות</h3>
            <asp:TextBox runat="server" ID="txtFromDate" CssClass="inputText" style="float: right;"></asp:TextBox>
            <asp:TextBox runat="server" ID="txtToDate" CssClass="inputText" style="float: right;"></asp:TextBox>
            <asp:TextBox runat="server" ID="txtNights" MaxLength="2" CssClass="inputText"></asp:TextBox>
        </div>
        <div class="searchRow">
            <h3 style="float: right; width: 30%;">אזור</h3>
            <h3 style="float: right; width: 60%;">מלון</h3>
            <asp:DropDownList runat="server" ID="ddlGeneralAreaId" DataValueField="general_area_id" DataTextField="GeneralAreaName" CssClass="inputText"  PlaceHolder="אזור" OnSelectedIndexChanged="AreasIndexChanged" AutoPostBack="true">
            </asp:DropDownList>
            <asp:DropDownList runat="server" ID="ddlHotels" DataValueField="id" DataTextField="HotelName" CssClass="inputText">
            </asp:DropDownList>
        </div>
        <div class="searchRow">
            <h3 style="float: right; width: 30%;">הרכב</h3>
            <h3 style="float: right; width: 30%;">בסיס אירוח</h3>
            <h3 style="float: right; width: 30%;">סוג חדר</h3>
            <asp:DropDownList runat="server" ID="ddlCompositions" DataValueField="compositionId" DataTextField="compositionName" CssClass="inputText"  PlaceHolder="אזור" OnSelectedIndexChanged="AreasIndexChanged" AutoPostBack="true">
            </asp:DropDownList>
            <asp:DropDownList runat="server" ID="ddlBases" DataValueField="baseId" DataTextField="baseName" CssClass="inputText">
            </asp:DropDownList>
            <asp:DropDownList runat="server" ID="ddlRoomTypes" DataValueField="roomTypeId" DataTextField="roomTypeName" CssClass="inputText">
            </asp:DropDownList>
        </div>    
            
        <asp:TextBox runat="server" ID="txtFinalPriceIs" CssClass="inputText"></asp:TextBox>
        <asp:Button runat="server" ID="btSearchHotels" Text="חפש" CssClass="button" OnClick="btSearchHotels_Click" />

        <div class="content">
            <div class="deals" style="width: auto;">
            <asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder>
            </div>
        </div>
    </div>
</asp:Content>

