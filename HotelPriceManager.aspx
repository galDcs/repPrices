<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="HotelPriceManager.aspx.cs" Inherits="HotelPriceManager" %>

<asp:Content ID="Content0" ContentPlaceHolderID="head" runat="Server">
    <style>
        .select_height{
            height: 3vh;
        }
        .select_width{
            width:20vh;
            text-align:center;
        }
        .header{
            margin: 0% 8%;
        }
        .labelSpace{
            margin: 0.3% 1%;
        }
        .colorMapSample
        {
            display:inline-flex;
        }
        label, span {
            font-size: 12px;
        }

        .tableSales th, .tableSales td {
            border: 1px solid gainsboro;
        }

        .fourInRowDiv input {
            width: 40%;
        }

        .fourInRowDiv label {
            width: 10%;
        }

        .filterHeaderYearAssign {
            width: 100%;
        }

        .docket_modal {
            display: none;
            position: fixed;
            z-index: 20;
            padding-top: 10px;
            top: 0;
            width: 99%;
            left: 0.5%;
            height: 100vh;
            overflow-x: hidden;
            overflow-y: auto;
            background-color: rgba(255,255,255,0.85);
            margin: 0%;
        }

        .none {
            display: none;
        }

        .textCenter {
            text-align: center
        }

        .BasesTable {
            font-weight: 800;
            background-color: white;
            direction:rtl;
            width:95%;
            margin:0% 2.5%;
        }

        .filterHeaderAssign {
            width: 100%;
        }

        .left_space{
            margin-left: 2%;
        }
        .bm_space{
            margin-bottom:1%;
        }

        .saveDetails{

        }
        .td_cell{
            height:7vh;
            position:relative
        }
        .td_cell>span{
            position:absolute;
            top:5%;
            left:40%;
        }
        .base_circle{
                 height: 2px;
                width: 2px;
                display: inline-block;
                padding: 4px;
                margin: 1px 1px;
                border-radius: 50%;
                float: right;
        }
        .base_container
        {
            position:absolute;
            top:40%;
        }

    </style>

    <script type="application/javascript">
        var dateDay;
        var monthNames = ["jan", "feb", "mar", "apr", "may", "jun",
            "jul", "aug", "sep", "oct", "nov", "dec"
        ];

        Date.prototype.ddMMyyyy = function () {
            return (this.getDate() + '/' + eval(this.getMonth() + 1) + '/' + this.getFullYear());
        };

        Date.prototype.addDays = function (days) {
            var dat = new Date(this.valueOf());
            dat.setDate(dat.getDate() + days);
            return dat;
        }

        $(document).ready(function () {

            $('.date_validation').datepicker({
                changeMonth: false,
                changeYear: true,
                showButtonPanel: true,
                dateFormat: 'dd/mm/yy',
                onClose: function (dateText, inst) {

                }
            });

            var minDateForDatePicker = new Date('<%= mMonth %>' + "/01" + '/' + '<%= mYear %>');
            var maxDateForDatePicker = new Date('<%= (mMonth + 1) %>' + "/01" + '/' + '<%= mYear %>');
            maxDateForDatePicker = maxDateForDatePicker.addDays(-1);
			//var maxDateForDatePicker = minDateForDatePicker.addDays(<%= mDaysToBlockInDatePicker %>);
            var maxDateForToDateDatePicker = minDateForDatePicker.addDays(<%= mDaysToBlockInDatePicker %>);



            $("#" + '<%= txtFromDateToClose.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForDatePicker,
            });

            $("#" + '<%= txtToDateToClose.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForToDateDatePicker,
            });

            $("#" + '<%= txtFromDateSetPrices.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForDatePicker,
            });

            $("#" + '<%= txtToDateSetPrices.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForToDateDatePicker,
            });

            $("#" + '<%= txtFromDateSetRooms.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForDatePicker,
            });

            $("#" + '<%= txtToDateSetRooms.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForToDateDatePicker,
            });

            $("#" + '<%= txtFromDateSetCycles.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForDatePicker,
            });

            $("#" + '<%= txtToDateSetCycles.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForToDateDatePicker,
            });

            $("#" + '<%= txtFromDateSaleNightFree.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForToDateDatePicker,
            });

            $("#" + '<%= txtToDateSaleNightFree.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForToDateDatePicker,
            });

            var acc = document.getElementsByClassName("accordion");
            var i;

            for (i = 0; i < acc.length; i++) {
                acc[i].onclick = function () {
                    /* Toggle between adding and removing the "active" class,
                    to highlight the button that controls the panel */
                    this.classList.toggle("active");

                    /* Toggle between hiding and showing the active panel */
                    var panel = this.nextElementSibling;
                    if (panel.style.display === "block") {
                        panel.style.display = "none";
                    } else {
                        panel.style.display = "block";
                    }
                }
            }
        });


        function specificDateSelected(selectedDate) {
            $('#divSpecificDateDetails').css('display', 'block');
            dateDay = selectedDate.innerText.split('\n')[0];
            $('#' + '<%= selectedDay.ClientID %>').val(dateDay);
            $('.displayNone').hide();

            specificDateHeader.innerHTML = dateDay;
            var dateForWS = getFormattedDate(dateDay);
            var supplierId = $('#' + '<%= hiddenSupplierId.ClientID %>').val();
            clearDateDetails();

            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: "{'supplierId':'" + supplierId + "', 'date':'" + dateForWS + "'}",
                url: "./PriceDetailsWs.asmx/getDatePriceDetails",
                error: function (xhr, status, error) {
                    alert("Server error accured, please try again later.");
                    return false;
                },
                success: function (data, status, xhr) {
                    debugger;
                    var parser, xmlDoc, xmlElem;
                    var rootElem;
                    var salesCyclesElem;
                    var status = "false";

                    parser = new DOMParser();
                    xmlDoc = parser.parseFromString(data.d.toString(), "text/xml");
                    rootElem = xmlDoc.getElementsByTagName("Root")[0];

                    xmlElem = rootElem.getElementsByTagName("PricePerDayDetails")[0];

                    if (getValueFromXml(xmlElem, "Status") == "True" || getValueFromXml(xmlElem, "Status") == "") {
                        status = "true";
                    }
                    $('#' + '<%= txtBasePrice.ClientID %>').val(getValueFromXml(xmlElem, "BasePrice"));
                    $('#' + '<%= txtBasePriceNetto.ClientID %>').val(getValueFromXml(xmlElem, "BasePriceNetto"));
                    $('#' + '<%= ddlStatus.ClientID %>').val(status);
                    $('#' + '<%= txtCloseDateReason.ClientID %>').val(getValueFromXml(xmlElem, "CloseDateReason"));
                    $('#' + '<%= ddlColors.ClientID %>').val(getValueFromXml(xmlElem, "Color"));
                    $('#' + '<%= txtRoomsAmount.ClientID %>').val(getValueFromXml(xmlElem, "RoomsAmount"));
                    if (document.getElementById('txtRoomsInUse') != null)
                        txtRoomsInUse.value = getValueFromXml(xmlElem, "RoomsInUse");
                    $('#' + '<%= txtRoomsDisable.ClientID %>').val(getValueFromXml(xmlElem, "RoomsDisable"));

                    //Show the current datePrice combos prices.
                    $('.displayNone_' + dateDay).show();

                    if (xmlElem.getElementsByTagName("SalesCycles") != null) {

                        var spanHeader = document.createElement('u');
                        spanHeader.innerText = '<%= Utils.getTextFromFile("SalesCycles", (eLanguage)1255) %>';
                        divSalesCyclesDates.appendChild(spanHeader);
                        divSalesCyclesDates.appendChild(document.createElement('br'));

                        salesCyclesElem = xmlElem.getElementsByTagName("SalesCycles")[0];
                        for (var i = 0; i < salesCyclesElem.childNodes.length; i++) {
                            $('#' + '<%= txtSalesCycles.ClientID %>').val($('#' + '<%= txtSalesCycles.ClientID %>').val() + xmlElem.getElementsByTagName("CycleDays")[i].innerHTML);

                            //Add text of dates for sale cycles.s
                            var selectedDateObj, newSpan = document.createElement('span');
                            selectedDateObj = new Date(dateForWS);
                            selectedDateObj.setDate(selectedDateObj.getDate() + parseInt(xmlElem.getElementsByTagName("CycleDays")[i].innerHTML));
                            newSpan.innerText = getGuiFormatDate(dateForWS) + ' - ' + selectedDateObj.ddMMyyyy();
                            divSalesCyclesDates.appendChild(newSpan);
                            divSalesCyclesDates.appendChild(document.createElement('br'));

                            if (i != salesCyclesElem.childNodes.length - 1) {
                                $('#' + '<%= txtSalesCycles.ClientID %>').val($('#' + '<%= txtSalesCycles.ClientID %>').val() + ',');
                            }
                        }
                    }
                    divSalesCyclesDates.appendChild(document.createElement('br'));
                }
            });
        }

        //Utils
        function clearDateDetails() {
            $('#' + '<%= txtSalesCycles.ClientID %>').val('');
            $('#' + '<%= txtBasePrice.ClientID %>').val('');
            $('#' + '<%= txtRoomsAmount.ClientID %>').val('');
            $('#' + '<%= ddlColors.ClientID %>').val('');
            if (document.getElementById('txtRoomsInUse') != null)
                txtRoomsInUse.value = '';
            $('#' + '<%= txtRoomsDisable.ClientID %>').val('');
            divSalesCyclesDates.innerText = '';
        }

        function getFormattedDate(day) {

            var month = monthNames[$('#' + '<%= hiddenMonth.ClientID %>').val() - 1];
            var year = $('#' + '<%= hiddenYear.ClientID %>').val();
            var date = day + '-' + month + '-' + year;

            return date;
        }

        function getGuiFormatDate(date) {
            var splittedDate = date.split('-');
            var newDate, day, month, year;

            if (splittedDate[0].length == 1)
                day = "0" + splittedDate[0];
            else
                day = splittedDate[0];

            month = monthNames.indexOf(splittedDate[1]) + 1;
            year = splittedDate[2];
            newDate = day + '/' + month + '/' + year;

            return newDate;
        }

        function getValueFromXml(xmlElem, tagName) {
            if (xmlElem.getElementsByTagName(tagName)[0] != null) {
                return xmlElem.getElementsByTagName(tagName)[0].innerHTML;
            }
            else {
                return "";
            }
        }

        function btNextMonth_Click() {
            var url = window.location.href;
            var queryString = url.split('?')[1];
            var queryStringWithoutDate = 'supplierId' + queryString.split('supplierId')[1];
            var month = '<%= mMonth %>';
            var year;

            month++;
            if (month > 12) {
                year = '<%= mYear %>';
                $('#' + '<%= hiddenYear.ClientID %>').val(year++);
                month = 1;
            }

            $('#' + '<%= hiddenMonth.ClientID %>').val(month);
            window.location = "./HotelPriceManager.aspx?month=" + month + "&year=" + year + "&" + queryStringWithoutDate;
        }

        function btPrevMonth_Click() {
            var url = window.location.href;
            var queryString = url.split('?')[1];
            var queryStringWithoutDate = 'supplierId' + queryString.split('supplierId')[1];
            var month = '<%= mMonth %>';
            var year;

            month--;
            if (month < 1) {
                year = '<%= mYear %>';
                $('#' + '<%= hiddenYear.ClientID %>').val(year--);
                month = 12;
            }

            $('#' + '<%= hiddenMonth.ClientID %>').val(month);
            window.location = "./HotelPriceManager.aspx?month=" + month + "&year=" + year + "&" + queryStringWithoutDate;
        }
    </script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <asp:HiddenField ID="selectedDay" runat="server" />
    <asp:HiddenField ID="hiddenClerkId" runat="server" />
    <asp:HiddenField ID="hiddenSupplierId" runat="server" />
    <asp:HiddenField ID="hiddenMonth" runat="server" />
    <asp:HiddenField ID="hiddenYear" runat="server" />

    <div id="divSpecificDateDetails" class="form-one" style="float: right; width: 22%; display: none; height: auto;">
        <h2 id="specificDateHeader"></h2>
        <div id="divDateDetails" style="direction: rtl; float: right; width: 100%;">
            <div class="divDateDetails">
                <label class="priceLable"><%= Utils.getTextFromFile("Price", (eLanguage)1255) %></label>
                <input id="txtBasePrice" runat="server" onkeypress="return isNumberKey(event);" autocomplete="off" />
            </div>
            <div class="divDateDetails">
                <label class="priceLable"><%= Utils.getTextFromFile("PriceNetto", (eLanguage)1255) %></label>
                <input id="txtBasePriceNetto" runat="server" onkeypress="return isNumberKey(event)" autocomplete="off" />
            </div>
            <div class="divStatus">
                <label class="priceLable"><%= Utils.getTextFromFile("Status", (eLanguage)1255) %></label>
                <asp:DropDownList ID="ddlStatus" runat="server">
                    <asp:ListItem Value="true"></asp:ListItem>
                    <asp:ListItem Value="false"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="divStatus">
                <label class="priceLable"><%= Utils.getTextFromFile("CloseDateReason", (eLanguage)1255) %></label>
                <asp:TextBox runat="server" ID="txtCloseDateReason" TextMode="multiline"></asp:TextBox>
            </div>
            <div class="divColor">
                <label class="priceLable"><%= Utils.getTextFromFile("Color", (eLanguage)1255) %></label>
                <asp:DropDownList ID="ddlColors" runat="server">
                </asp:DropDownList>
            </div>
            <div id="divAllocations" visible="false" runat="server">
                <div class="divDateDetails">
                    <label class="priceLable"><%= Utils.getTextFromFile("RoomsAmount", (eLanguage)1255) %></label>
                    <input id="txtRoomsAmount" runat="server" onkeypress="return isIntegerKey(event)" autocomplete="off" />
                </div>
                <div class="divDateDetails">
                    <label class="priceLable"><%= Utils.getTextFromFile("RoomsInUse", (eLanguage)1255) %></label>
                    <input type="text" id="txtRoomsInUse" disabled="disabled" autocomplete="off" />
                </div>
                <div id="divRoomDisable" visible="false" runat="server">
                    <div class="divDateDetails">
                        <label class="priceLable"><%= Utils.getTextFromFile("RoomsDisable", (eLanguage)1255) %></label>
                        <input type="text" id="txtRoomsDisable" runat="server" autocomplete="off" />
                    </div>
                </div>
            </div>
            <div class="divDateDetails">
                <label class="priceLable"><%= Utils.getTextFromFile("SalesCycles", (eLanguage)1255) %></label>
                <input type="text" id="txtSalesCycles" runat="server" onkeypress="return isNumberOrCommaKey(event)" autocomplete="off" />
            </div>
            <div id="divSalesCyclesDates" style="text-align: right;">
            </div>

            <div id="divCombosPrices" style="text-align: right;" runat="server">
            </div>

            <div id="divSave">
                <asp:Button ID="btSave" CssClass="saveButton" runat="server" OnClick="btSave_Click" />
            </div>
        </div>
    </div>

    <div class="form-one" id="divCalendar">
        <div id="docket_modal" class="docket_modal">
            <div class="docket_modal_header ">
                <span id="docket_modal_close" class="docket_modal_close closeContact">&times;</span>
                <h1 id="dockerHeaderByType" class="guideHeader textCenter">שינוי בסיס אירוח</h1>
                <div class="container textCenter" dir="rtl">
                    <div class="row bm_space">
                            <div class="col-md-1"></div>

                            <div class="col-md-1">

                            </div>
                            <div class="col-md-2 left_space">
                                <label class="control-label filterHeaderAssign">עד תאריך</label>
                                <asp:TextBox ID="txtToDate" autocomplete="off" runat="server" CssClass="date_validation"></asp:TextBox>
                            </div>
                            <div class="col-md-2 left_space">
                                <label class="control-label filterHeaderAssign">מתאריך</label>
                                <asp:TextBox ID="txtFromDate" autocomplete="off" runat="server" CssClass="date_validation"></asp:TextBox>
                            </div>
                            <div class="col-md-2 left_space">
                                <label class="control-label filterHeaderAssign">בסיסי אירוח</label>
                                <asp:HiddenField ID="ddlGenericSelectSelectedId" runat="server"/>
                                <asp:DropDownList id="ddlGenericSelect"  CssClass="select_height" AutoPostBack="false" runat="server"></asp:DropDownList>
                            </div>
                    </div>
                    <div class="row bm_space" style="text-align: left;">
                        <div class="col-md-3">

                        </div>
                        <div class="col-md-1">
                            <asp:Button ID="deleteDetails" OnClick="deleteDetails_Click" OnClientClick="setSelectedIndex()" CssClass="btn btn-primary saveDetails" runat="server" Text="מחק פרטי בסיס" />
                        </div>
                        <div class="col-md-1">

                        </div>
                        <div class="col-md-1">
                            <asp:Button ID="saveDetails" OnClick="saveDetails_Click" OnClientClick="setSelectedIndex()" CssClass="btn btn-primary saveDetails" runat="server" Text="שמור פרטים" />
                        </div>
                        <div class="col-md-6">

                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-1">
                        </div>
                        <div class="col-md-5">
                            <label class="control-label filterHeaderYearAssign">עבור שנה</label>
                            <asp:DropDownList ID="ddlYearsTable" CssClass="select_width" OnSelectedIndexChanged="ddlYearsTable_SelectedIndexChanged" AutoPostBack="true" runat="server"></asp:DropDownList>
                        </div>
                        <div id="divColorMap" class="col-md-5" style="overflow-x:auto" runat="server">
                           <label class="control-label filterHeaderYearAssign textCenter">מפת צבעים</label>
                        </div>
                        <asp:Label id="lbColorMap" Text="" runat="server" />
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <asp:Label ID="notice" runat="server"></asp:Label>
                            <asp:Label ID="MissingDaysAtWork" CssClass="missingLabel" runat="server"></asp:Label>
                        </div>
                    </div>
                </div>
                <asp:Table CssClass="BasesTable" ID="BasesTable" Width="95%" GridLines="Both" BorderColor="Black" BorderWidth="1px" runat="server"></asp:Table>
                <asp:Table CssClass="RoomsTable none" ID="Table1" Width="95%" GridLines="Both" BorderColor="Black" BorderWidth="1px" runat="server"></asp:Table>
            </div>

        </div>
        <div style="background: #d0d0d0; text-align: center; width: 100%; margin-bottom: 7px; direction: rtl;">
            <asp:Label ID="priceHeader" class="headline" runat="server"></asp:Label>
        </div>

        <div id="divMainButtons" style="width: 100%; float: right;">
            <asp:Button ID="btBackToMenu" CssClass="saveButton" OnClick="btBackToMenu_Click" runat="server" />
            <asp:Button ID="btSetGeneralDetails" CssClass="saveButton" OnClick="btSetGeneralDetails_Click" runat="server" />
        </div>

        <div style="text-align: center; direction: rtl;">
            <asp:Button ID="btPrevMonth" OnClick="btPrevMonth_Click" class="buttonMonth" runat="server"></asp:Button>
            <asp:Button ID="btNextMonth" OnClick="btNextMonth_Click" class="buttonMonth" runat="server"></asp:Button>
        </div>
        <div style="text-align: center; direction: rtl;">
            <asp:DropDownList ID="ddlYears" runat="server">
                <asp:ListItem Text="2018" Value="2018"></asp:ListItem>
                <asp:ListItem Text="2019" Value="2019"></asp:ListItem>
                <asp:ListItem Text="2020" Value="2020"></asp:ListItem>
                <asp:ListItem Text="2021" Value="2021"></asp:ListItem>
                <asp:ListItem Text="2022" Value="2022"></asp:ListItem>
            </asp:DropDownList>
            <asp:DropDownList ID="ddlMonthes" runat="server">
                <asp:ListItem Text="ינואר" Value="1"></asp:ListItem>
                <asp:ListItem Text="פברואר" Value="2"></asp:ListItem>
                <asp:ListItem Text="מרץ" Value="3"></asp:ListItem>
                <asp:ListItem Text="אפריל" Value="4"></asp:ListItem>
                <asp:ListItem Text="מאי" Value="5"></asp:ListItem>
                <asp:ListItem Text="יוני" Value="6"></asp:ListItem>
                <asp:ListItem Text="יולי" Value="7"></asp:ListItem>
                <asp:ListItem Text="אוגוסט" Value="8"></asp:ListItem>
                <asp:ListItem Text="ספטמבר" Value="9"></asp:ListItem>
                <asp:ListItem Text="אוקטובר" Value="10"></asp:ListItem>
                <asp:ListItem Text="נובמבר" Value="11"></asp:ListItem>
                <asp:ListItem Text="דצמבר" Value="12"></asp:ListItem>
            </asp:DropDownList>
            <asp:Button ID="btMoveToMonth" OnClick="btMoveToMonth_Click" class="buttonMonth" Style="width: 71px; margin-top: 2px;" runat="server"></asp:Button>
        </div>

        <asp:Table runat="server" CssClass="calendar" ID="calendar" BackColor="White" BorderColor="Black"
            BorderWidth="1px" ForeColor="Black" GridLines="Both" BorderStyle="Solid">
        </asp:Table>

        <div class="accordion"><%= Utils.getTextFromFile("QuickAssignPriceSection",(eLanguage)1255) %></div>
        <div id="divQuickAssign" class="borderDouble panel" style="height: 7%; direction: rtl;">
            <table style="width: 100%">
                <tr style="text-align: center;">
                    <td></td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Sunday", (eLanguage)1255) %>   </label>
                    </td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Monday", (eLanguage)1255) %>   </label>
                    </td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Tuesday", (eLanguage)1255) %>  </label>
                    </td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Wednesday", (eLanguage)1255) %></label></td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Thursday", (eLanguage)1255) %> </label>
                    </td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Friday", (eLanguage)1255) %>   </label>
                    </td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Saturday", (eLanguage)1255) %> </label>
                    </td>
                </tr>
                <tr>
                    <td>
                        <label><%= Utils.getTextFromFile("Brutto", (eLanguage)1255) %></label></td>
                    <td>
                        <asp:TextBox ID="txtSundayPrice" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtMondayPrice" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtTuesdayPrice" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtWednesdayPrice" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtThursdayPrice" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtFridayPrice" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtSaturdayPrice" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                </tr>
                <tr>
                    <td>
                        <label style="margin-left: 16px;"><%= Utils.getTextFromFile("Netto", (eLanguage)1255) %></label></td>
                    <td>
                        <asp:TextBox ID="txtSundayPriceNetto" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtMondayPriceNetto" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtTuesdayPriceNetto" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtWednesdayPriceNetto" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtThursdayPriceNetto" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtFridayPriceNetto" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtSaturdayPriceNetto" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                </tr>
            </table>
            <label><%= Utils.getTextFromFile("FromDate",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtFromDateSetPrices" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
            <label><%= Utils.getTextFromFile("ToDate",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtToDateSetPrices" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
            <asp:Button ID="btSetPricesMonthly" CssClass="saveButton" runat="server" OnClick="btSetPricesMonthly_Click" />
        </div>

        <div class="accordion"><%= Utils.getTextFromFile("SalesCycleMonthlySection",(eLanguage)1255) %></div>
        <div id="divSaleCyclesMonthly" class="borderDouble panel" style="height: 6%; direction: rtl;">
            <label><%= Utils.getTextFromFile("FromDate",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtFromDateSetCycles" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
            <label><%= Utils.getTextFromFile("ToDate",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtToDateSetCycles" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
            <asp:Button ID="btSaveSalesCyclesMonthly" CssClass="saveButton" runat="server" OnClick="btSaveSalesCyclesMonthly_Click" />
            <table style="width: 100%">
                <tr style="text-align: center;">
                    <td></td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Sunday", (eLanguage)1255) %>   </label>
                    </td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Monday", (eLanguage)1255) %>   </label>
                    </td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Tuesday", (eLanguage)1255) %>  </label>
                    </td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Wednesday", (eLanguage)1255) %></label></td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Thursday", (eLanguage)1255) %> </label>
                    </td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Friday", (eLanguage)1255) %>   </label>
                    </td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Saturday", (eLanguage)1255) %> </label>
                    </td>
                </tr>
                <tr>
                    <td>
                        <label><%= Utils.getTextFromFile("SalesCycles", (eLanguage)1255) %></label></td>
                    <td>
                        <asp:TextBox ID="txtSaleCycle1" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtSaleCycle2" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtSaleCycle3" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtSaleCycle4" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtSaleCycle5" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtSaleCycle6" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtSaleCycle7" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                </tr>
            </table>
        </div>

        <div style="display: none;" class="accordion"><%= Utils.getTextFromFile("QuickAssignAllocationsSection",(eLanguage)1255) %></div>
        <div id="divAllocationMonthly" class="borderDouble panel" style="height: 7%; direction: rtl;">
            <table style="width: 100%">
                <tr style="text-align: center;">
                    <td></td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Sunday", (eLanguage)1255) %></label></td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Monday", (eLanguage)1255) %></label></td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Tuesday", (eLanguage)1255) %></label></td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Wednesday", (eLanguage)1255) %></label></td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Thursday", (eLanguage)1255) %></label></td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Friday", (eLanguage)1255) %></label></td>
                    <td>
                        <label class="quickPriceHeader"><%= Utils.getTextFromFile("Saturday", (eLanguage)1255) %></label></td>
                </tr>
                <tr>
                    <td>
                        <label style="margin-left: 16px;"><%= Utils.getTextFromFile("Rooms", (eLanguage)1255) %></label></td>
                    <td>
                        <asp:TextBox ID="txtRoomsSunday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtRoomsMonday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtRoomsTuesday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtRoomsWednesday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtRoomsThursday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtRoomsFriday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtRoomsSaturday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                </tr>
                <tr id="trRoomsDisable" runat="server" visible="false">
                    <td>
                        <label style="margin-left: 16px;"><%= Utils.getTextFromFile("RoomsDisable", (eLanguage)1255) %></label></td>
                    <td>
                        <asp:TextBox ID="txtRoomsDisableSunday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtRoomsDisableMonday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtRoomsDisableTuesday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtRoomsDisableWednesday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtRoomsDisableThursday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtRoomsDisableFriday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td>
                        <asp:TextBox ID="txtRoomsDisableSaturday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                </tr>
            </table>
            <label><%= Utils.getTextFromFile("FromDate",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtFromDateSetRooms" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
            <label><%= Utils.getTextFromFile("ToDate",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtToDateSetRooms" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
            <asp:Button ID="btSetAllocationsMonthly" CssClass="saveButton" runat="server" OnClick="btSetAllocationsMonthly_Click" />
        </div>

        <div class="accordion"><%= Utils.getTextFromFile("OpenCloseDatesSection",(eLanguage)1255) %></div>
        <div id="divCloseDatesRange" class="borderDouble panel" style="height: 4%; direction: rtl;">
            <label><%= Utils.getTextFromFile("FromDate",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtFromDateToClose" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
            <label><%= Utils.getTextFromFile("ToDate",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtToDateToClose" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
            <asp:Button ID="btCloseDatesRange" CssClass="saveButton" OnClick="btCloseDatesRange_Click" runat="server" status="false" />
            <asp:Button ID="btOpenDatesRange" CssClass="saveButton" OnClick="btCloseDatesRange_Click" runat="server" status="true" />
        </div>

        <div style="display: none;" class="accordion">מכסה חודשית</div>
        <div id="divMonthlyAllocation" class="borderDouble panel" style="height: 3%; direction: rtl;">
            <label><%= Utils.getTextFromFile("MonthlyAllocation",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtMonthlyAllocation" runat="server" Style="width: 6%;"></asp:TextBox>
            <label><%= Utils.getTextFromFile("MonthlyAllocationSum",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtMonthlyAllocationSum" runat="server" Style="width: 6%;"></asp:TextBox>
            <label><%= Utils.getTextFromFile("MonthlyAllocationUsed",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtMonthlyAllocationUsed" runat="server" Style="width: 6%;"></asp:TextBox>
            <asp:Button ID="btSaveMontlhyAllocation" CssClass="saveButton" OnClick="btSaveMontlhyAllocation_Click" runat="server" />
        </div>
        <div class="accordion">מבצעים</div>
        <div id="" class="borderDouble panel" style="height: 14%; direction: rtl; overflow: scroll;">
            <div class="fourInRowDiv">
                <label><%= Utils.getTextFromFile("FromDate",(eLanguage)1255)%></label>
                <asp:TextBox ID="txtFromDateSaleNightFree" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
                <label><%= Utils.getTextFromFile("ToDate",(eLanguage)1255)%></label>
                <asp:TextBox ID="txtToDateSaleNightFree" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
                <label><%= Utils.getTextFromFile("Nights",(eLanguage)1255)%></label>
                <asp:TextBox ID="txtNights" runat="server"></asp:TextBox>
                <label><%= Utils.getTextFromFile("NightsFree",(eLanguage)1255)%></label>
                <asp:TextBox ID="txtNightsFree" runat="server"></asp:TextBox>
            </div>
            <asp:Button ID="btSaveSaleNightFree" CssClass="saveButton" OnClick="btSaveSaleNightFree_Click" runat="server" />
            <table class="tableSales">
                <tr>
                    <th>מתאריך</th>
                    <th>עד תאריך</th>
                    <th>כמות לילות</th>
                    <th>לילות חינם</th>
                </tr>
                <%= mSalesNightsFreeRows %>
            </table>
        </div>
        <div id="" style="height: 14%; display: block; direction: rtl; overflow: scroll;">
            <asp:Button ID="btAddStandBy" OnClientClick="openFrame(); return false;" runat="server" Style="margin: 2% 0%" Text=" שינוי בסיסי אירוח" />
        </div>
    </div>
    <script>
        var docket_modal = document.getElementById("docket_modal");
        var docket_modal_close = document.getElementById("docket_modal_close");

        docket_modal_close.onclick = function (e) {
            e.preventDefault();
            hideDocketModal();
        }

        function hideDocketModal() {
            docket_modal.style.display = "none";
        }

        function openFrame() {
            docket_modal.style.display = "block";
        }

    </script>
</asp:Content>

