<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="HotelPriceManager.aspx.cs" Inherits="HotelPriceManager" %>

<asp:Content ID="Content0" ContentPlaceHolderID="head" Runat="Server">
<style>
label, span{
font-size:12px;
}
</style>

    <script  type="application/javascript">
        var dateDay;
        var monthNames = ["jan", "feb", "mar", "apr", "may", "jun",
                             "jul", "aug", "sep", "oct", "nov", "dec"
        ];

        $(document).ready(function () {
            
            var minDateForDatePicker = new Date('<%= mMonth %>' + "/01" + '/' + '<%= mYear %>');
            var maxDateForDatePicker = new Date('<%= (mMonth + 1) %>' + "/01" + '/' + '<%= mYear %>');

            $("#" + '<%= txtFromDateToClose.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForDatePicker,
            });

            $("#" + '<%= txtToDateToClose.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForDatePicker,
            });

            $("#" + '<%= txtFromDateSetPrices.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForDatePicker,
            });

            $("#" + '<%= txtToDateSetPrices.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForDatePicker,
            });

            $("#" + '<%= txtFromDateSetRooms.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForDatePicker,
            });

            $("#" + '<%= txtToDateSetRooms.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForDatePicker,
            });

            $("#" + '<%= txtFromDateSetCycles.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForDatePicker,
            });

            $("#" + '<%= txtToDateSetCycles.ClientID%>').datepicker({
                minDate: minDateForDatePicker,
                maxDate: maxDateForDatePicker,
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

            var divCombo = $('#' + '<%= divCombosPrices.ClientID %>');
            var text = $('#' + '<%= txtBasePriceNetto.ClientID %>')[0].value;
            var NettoPriceSpan = document.createElement('span');
            NettoPriceSpan.css("display", "none");
            NettoPriceSpan.addClass("displayNone displayNone_" + specificDateHeader.innerText);
            NettoPriceSpan.innerText = text;
            var br = document.createElement('br');
            NettoPriceSpan.appendChild(br);
            divCombo.children.insertBefore(NettoPriceSpan, children[0]);

        });

        Date.prototype.ddMMyyyy = function () {
            return (this.getDate() + '/' + eval(this.getMonth() + 1) + '/' + this.getFullYear());
        };

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

        function addComboList(comboTypeString, comboId, comboName, comboAmount, selectedCheckBox) {
            var txtId, lblId, placeContentId;
            var pixels, divToInsertTo;
            var comboTypeAmount;

            divToInsertTo = 'divCompositionsDetails';
            divToInsertTo = $("[id$=_" + divToInsertTo + "]").attr("id");
            placeContentId = divToInsertTo.split('_')[0];
            
            switch (comboTypeString) {
                case 'composition':
                    txtId = 'txtComposition_' + comboId;
                    lblId = 'lblComposition_' + comboId;
                    divToInsertTo = 'divCompositionsDetails';
                    divToInsertTo = $("[id$=_" + divToInsertTo + "]").attr("id");
                    break;
                case 'base':
                    txtId = 'txtBase_' + comboId;
                    lblId = 'lblBase_' + comboId;
                    divToInsertTo = 'divBasesDetails';
                    divToInsertTo = $("[id$=_" + divToInsertTo + "]").attr("id");
                    break;
                case 'roomType':
                    txtId = 'txtRoomType_' + comboId;
                    lblId = 'lblRoomType_' + comboId;
                    divToInsertTo = 'divRoomTypesDetails';
                    divToInsertTo = $("[id$=_" + divToInsertTo + "]").attr("id");
                    break;
            }

            txtId = placeContentId + '_' + txtId;
            lblId = placeContentId + '_' + lblId;

            comboTypeAmount = document.getElementById(divToInsertTo).getElementsByTagName('*').length;
            pixels = 8 * (comboTypeAmount / 4);

            if (!selectedCheckBox.checked || document.getElementById(txtId) != null) {
                $('#' + txtId).prop("disabled", !($('#' + txtId).prop("disabled")));
                $('#' + lblId).prop("disabled", !($('#' + txtId).prop("disabled")));
            }
            else {
                var label = '<span id="' + lblId + '" style="float:right; margin-right:3px; margin-top:-' + pixels + 'px; direction:rtl; text-align:center; width:30%;">' + comboName + '</span>';
                var textBox = '<input type="text" value="0" name="ctl00$' + txtId.replace('_', '$') + '" id="' + txtId + '" onkeypress="isNumberKey" style="float:right; width:25%; margin-right: 128px; margin-top:-' + pixels + 'px;"><br><br>';


                $('#' + divToInsertTo).append(label);
                $('#' + divToInsertTo).append(textBox);
            }
        }

        function changeBaseCombo(comboTypeString, comboId, comboName, selectedChkBox) {
            var isChecked = selectedChkBox.checked;
            
            if (!selectedChkBox.checked) {

                comboName = "";
                comboId = "";
            }

            switch (comboTypeString) {
                case 'composition':
                    $("INPUT[id^='" + '<%= chkBaseComposition.ClientID %>' + "_']").attr('checked', false);
                    $('#' + '<%= lblBaseComposition.ClientID %>').text(comboName);
                    $('#' + '<%= hiddenBaseCompositionId.ClientID %>').val(comboId);
                    break;
                case 'base':
                    $("INPUT[id^='" + '<%= chkBaseBase.ClientID %>' + "_']").attr('checked', false);
                    $('#' + '<%= lblBaseBase.ClientID %>').text(comboName);
                    $('#' + '<%= hiddenBaseBaseId.ClientID %>').val(comboId);
                    break;
                case 'roomType':
                    $("INPUT[id^='" + '<%= chkBaseRoomType.ClientID %>' + "_']").attr('checked', false);
                    $('#' + '<%= lblBaseRoomType.ClientID %>').text(comboName);
                    $('#' + '<%= hiddenBaseRoomTypeId.ClientID %>').val(comboId);
                    break;
            }

            selectedChkBox.checked = isChecked;
        }

        function isNumberKey(evt) {
            var charCode = (evt.which) ? evt.which : evt.keyCode;
            if ((charCode >= 48 && charCode <= 57) || charCode == 17 || charCode == 16 || charCode == 46)
                return true;

            return false;
        }

        function isIntegerKey(evt) {
            var charCode = (evt.which) ? evt.which : evt.keyCode;
            if ((charCode >= 48 && charCode <= 57) || charCode == 17 || charCode == 16)
                return true;

            return false;
        }

        function isNumberOrCommaKey(evt) {
            var charCode = (evt.which) ? evt.which : evt.keyCode;
            if ((charCode >= 48 && charCode <= 57) || charCode == 44 || charCode == 39 || charCode == 17 || charCode == 16)
                return true;

            return false;
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

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server" >
     <asp:HiddenField ID="selectedDay" runat="server" />
    <asp:HiddenField ID="hiddenClerkId" runat="server" />
    <asp:HiddenField ID="hiddenSupplierId" runat="server" />
    <asp:HiddenField ID="hiddenMonth" runat="server" />
    <asp:HiddenField ID="hiddenYear" runat="server" />

    <div id="divSpecificDateDetails" class="form-one" style="float: right; width: 25%; max-height: 600px; display:none;">
        <h2 id="specificDateHeader"></h2>
        <div id="divDateDetails" style="direction: rtl; float: right; width: 100%;">
            <div class="divDateDetails">
                <label class="priceLable"><%= Utils.getTextFromFile("Price", (eLanguage)1255) %></label>
                <input id="txtBasePrice" runat="server" onkeypress="return isNumberKey(event)"/>
            </div>
            <div class="divDateDetails">
                <label class="priceLable"><%= Utils.getTextFromFile("PriceNetto", (eLanguage)1255) %></label>
                <input id="txtBasePriceNetto" runat="server" onkeypress="return isNumberKey(event)"/>
            </div>
            <div class="divStatus">
                <label class="priceLable"><%= Utils.getTextFromFile("Status", (eLanguage)1255) %></label>
                <asp:DropDownList id="ddlStatus" runat="server">
                </asp:DropDownList>
            </div>
            <div class="divColor">
                <label class="priceLable"><%= Utils.getTextFromFile("Color", (eLanguage)1255) %></label>
                <asp:DropDownList id="ddlColors" runat="server">
                </asp:DropDownList>
            </div>
            <div id="divAllocations" visible="false" runat="server">
                <div class="divDateDetails">
                    <label class="priceLable"><%= Utils.getTextFromFile("RoomsAmount", (eLanguage)1255) %></label>
                    <input id="txtRoomsAmount" runat="server" onkeypress="return isIntegerKey(event)"/>
                </div>
                <div class="divDateDetails">
                    <label class="priceLable"><%= Utils.getTextFromFile("RoomsInUse", (eLanguage)1255) %></label>
                    <input type="text" id="txtRoomsInUse" disabled="disabled" />
                </div>
                <div id="divRoomDisable" visible="false" runat="server">
                    <div class="divDateDetails">
                        <label class="priceLable"><%= Utils.getTextFromFile("RoomsDisable", (eLanguage)1255) %></label>
                        <input type="text" id="txtRoomsDisable" runat="server"/>
                    </div>
                </div>
            </div>
            <div class="divDateDetails" >
                <label class="priceLable"><%= Utils.getTextFromFile("SalesCycles", (eLanguage)1255) %></label>
                <input type="text" id="txtSalesCycles" runat="server" onkeypress="return isNumberOrCommaKey(event)"/>
            </div>
            <div id="divSalesCyclesDates" style="text-align: right;">
            </div>

            <div id="divCombosPrices" style="text-align: right;" runat="server">
            </div>

            <div id="divSave">    
                <asp:Button id="btSave" CssClass="saveButton" runat="server" OnClick="btSave_Click"/>
            </div>
        </div>
    </div>

    <div class="form-one" id="divCalendar">
        <div style="background: #d0d0d0; text-align:center; width:100%; margin-bottom: 7px; direction:rtl;">
            <asp:Button id="btPrevMonth" OnClick="btPrevMonth_Click" class="buttonMonth" runat="server"></asp:Button>
            <asp:Label id="priceHeader" class="headline" runat="server"></asp:Label>
			<asp:Button id="btNextMonth" OnClick="btNextMonth_Click" class="buttonMonth" runat="server"></asp:Button>
        </div>

        <div id="divMainButtons" style="width:100%; float:right;">
            <asp:Button ID="btBackToMenu" CssClass="saveButton" OnClick="btBackToMenu_Click" runat="server"/>
            <asp:Button ID="btSaveHotelPrice" CssClass="saveButton" OnClick="btSaveHotelPrice_Click" runat="server"/>
        </div>
        
        <asp:Table runat="server" CssClass="calendar" ID="calendar" BackColor="White" BorderColor="Black"
            BorderWidth="1px" ForeColor="Black" GridLines="Both" BorderStyle="Solid" style="margin-top: 43px;">
        </asp:Table>

		
        <div class="accordion"><%= Utils.getTextFromFile("QuickAssignPriceSection",(eLanguage)1255) %></div>
        <div id="divQuickAssign" class="borderDouble panel" style="height: 7%; direction:rtl;">
            <table style="width: 100%">
                <tr style="text-align:center;">
                    <td></td>
                    <td><label class="quickPriceHeader"><%= Utils.getTextFromFile("Sunday", (eLanguage)1255) %>   </label></td>
                    <td><label class="quickPriceHeader"><%= Utils.getTextFromFile("Monday", (eLanguage)1255) %>   </label></td>
                    <td><label class="quickPriceHeader"><%= Utils.getTextFromFile("Tuesday", (eLanguage)1255) %>  </label></td>
                    <td><label class="quickPriceHeader"><%= Utils.getTextFromFile("Wednesday", (eLanguage)1255) %></label></td>
                    <td><label class="quickPriceHeader"><%= Utils.getTextFromFile("Thursday", (eLanguage)1255) %> </label></td>
                    <td><label class="quickPriceHeader"><%= Utils.getTextFromFile("Friday", (eLanguage)1255) %>   </label></td>
                    <td><label class="quickPriceHeader"><%= Utils.getTextFromFile("Saturday", (eLanguage)1255) %> </label></td>
                </tr>
                <tr>
                    <td><label><%= Utils.getTextFromFile("Brutto", (eLanguage)1255) %></label></td>
		            <td><asp:TextBox ID="txtSundayPrice" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
		            <td><asp:TextBox ID="txtMondayPrice" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
		            <td><asp:TextBox ID="txtTuesdayPrice" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
		            <td><asp:TextBox ID="txtWednesdayPrice" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
		            <td><asp:TextBox ID="txtThursdayPrice" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
		            <td><asp:TextBox ID="txtFridayPrice" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
		            <td><asp:TextBox ID="txtSaturdayPrice" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                </tr>
                <tr>
                    <td><label style="margin-left:16px;"><%= Utils.getTextFromFile("Netto", (eLanguage)1255) %></label></td>
                    <td><asp:TextBox ID="txtSundayPriceNetto" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtMondayPriceNetto" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtTuesdayPriceNetto" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtWednesdayPriceNetto" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtThursdayPriceNetto" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtFridayPriceNetto" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtSaturdayPriceNetto" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                </tr>
            </table>
            <label><%= Utils.getTextFromFile("FromDate",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtFromDateSetPrices" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
            <label><%= Utils.getTextFromFile("ToDate",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtToDateSetPrices" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
	        <asp:Button ID="btSetPricesMonthly" CssClass="saveButton" runat="server" OnClick="btSetPricesMonthly_Click"/>
        </div>
		
        <div class="accordion"><%= Utils.getTextFromFile("SalesCycleMonthlySection",(eLanguage)1255) %></div>
        <div id="divSaleCyclesMonthly" class="borderDouble panel" style="height: 4%; direction:rtl;">
            <asp:DropDownList runat="server" id="ddlWeekDays">
            </asp:DropDownList>
            <label><%= Utils.getTextFromFile("SalesCycles",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtSalesCyclesMothly" runat="server"></asp:TextBox>
            <label><%= Utils.getTextFromFile("FromDate",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtFromDateSetCycles" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
            <label><%= Utils.getTextFromFile("ToDate",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtToDateSetCycles" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
	        <asp:Button ID="btSaveSalesCyclesMonthly" CssClass="saveButton" runat="server" OnClick="btSaveSalesCyclesMonthly_Click"/>
        </div>
		
		<div class="accordion"><%= Utils.getTextFromFile("QuickAssignAllocationsSection",(eLanguage)1255) %></div>
        <div id="divAllocationMonthly" class="borderDouble panel" style="height: 7%; direction:rtl;">
            <table style="width: 100%">
                <tr style="text-align:center;">
                    <td></td>
                    <td><label class="quickPriceHeader"><%= Utils.getTextFromFile("Sunday", (eLanguage)1255) %></label></td>
                    <td><label class="quickPriceHeader"><%= Utils.getTextFromFile("Monday", (eLanguage)1255) %></label></td>
                    <td><label class="quickPriceHeader"><%= Utils.getTextFromFile("Tuesday", (eLanguage)1255) %></label></td>
                    <td><label class="quickPriceHeader"><%= Utils.getTextFromFile("Wednesday", (eLanguage)1255) %></label></td>
                    <td><label class="quickPriceHeader"><%= Utils.getTextFromFile("Thursday", (eLanguage)1255) %></label></td>
                    <td><label class="quickPriceHeader"><%= Utils.getTextFromFile("Friday", (eLanguage)1255) %></label></td>
                    <td><label class="quickPriceHeader"><%= Utils.getTextFromFile("Saturday", (eLanguage)1255) %></label></td>
                </tr>
                <tr>
                    <td><label style="margin-left:16px;"><%= Utils.getTextFromFile("Rooms", (eLanguage)1255) %></label></td>
                    <td><asp:TextBox ID="txtRoomsSunday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtRoomsMonday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtRoomsTuesday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtRoomsWednesday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtRoomsThursday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtRoomsFriday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtRoomsSaturday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                </tr>
                <tr id="trRoomsDisable" runat="server" visible="false">
                    <td><label style="margin-left:16px;"><%= Utils.getTextFromFile("RoomsDisable", (eLanguage)1255) %></label></td>
                    <td><asp:TextBox ID="txtRoomsDisableSunday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtRoomsDisableMonday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtRoomsDisableTuesday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtRoomsDisableWednesday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtRoomsDisableThursday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtRoomsDisableFriday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                    <td><asp:TextBox ID="txtRoomsDisableSaturday" runat="server" CssClass="quickPriceTextBox"></asp:TextBox></td>
                </tr>
            </table>
            <label><%= Utils.getTextFromFile("FromDate",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtFromDateSetRooms" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
            <label><%= Utils.getTextFromFile("ToDate",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtToDateSetRooms" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
            <asp:Button ID="btSetAllocationsMonthly" CssClass="saveButton" runat="server" OnClick="btSetAllocationsMonthly_Click"/>
        </div>
		
        <div class="accordion"><%= Utils.getTextFromFile("OpenCloseDatesSection",(eLanguage)1255) %></div>
        <div id="divCloseDatesRange" class="borderDouble panel" style="height: 4%; direction:rtl;">
            <label><%= Utils.getTextFromFile("FromDate",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtFromDateToClose" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
            <label><%= Utils.getTextFromFile("ToDate",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtToDateToClose" runat="server" placeholder="dd/mm/yy"></asp:TextBox>
            <asp:Button ID="btCloseDatesRange" CssClass="saveButton" OnClick="btCloseDatesRange_Click" runat="server" status="false"/>
            <asp:Button ID="btOpenDatesRange" CssClass="saveButton" OnClick="btCloseDatesRange_Click" runat="server" status="true"/>
        </div>
		
        <div id="divMonthlyAllocation" class="borderDouble" style="height: 3%; direction:rtl;">
            <label><%= Utils.getTextFromFile("MonthlyAllocation",(eLanguage)1255)%></label>
            <asp:TextBox ID="txtMonthlyAllocation" runat="server"  style="width:6%;"></asp:TextBox>
			<label><%= Utils.getTextFromFile("MonthlyAllocationSum",(eLanguage)1255)%></label>
			<asp:TextBox ID="txtMonthlyAllocationSum" runat="server"  style="width:6%;"></asp:TextBox>
			<label><%= Utils.getTextFromFile("MonthlyAllocationUsed",(eLanguage)1255)%></label>
			<asp:TextBox ID="txtMonthlyAllocationUsed" runat="server"  style="width:6%;"></asp:TextBox>
            <asp:Button ID="btSaveMontlhyAllocation" CssClass="saveButton" OnClick="btSaveMontlhyAllocation_Click" runat="server"/>
        </div>		
		
        <div id="divGeneralDetails" class="borderDouble" style="height: 19%;">
            <div id="divMainDetails" style="float: right; margin-bottom: 11px; margin-top: 8px; padding-left: 3%; width: 100%;">
                <label class="labelGeneralDetails"><%= Utils.getTextFromFile("PriceColor", (eLanguage)1255) %></label>
                <asp:DropDownList ID="ddlPriceColor" class="txtGeneralDetails" runat="server"></asp:DropDownList>
                <label class="labelGeneralDetails"> <%= Utils.getTextFromFile("Status", (eLanguage)1255) %></label>
                <asp:DropDownList id="ddlPriceStatus" class="txtGeneralDetails" runat="server"></asp:DropDownList>
            </div>
            <div id="divDiscounts" style="float: right; margin-bottom: 11px; width: 100%;">
                <label class="labelGeneralDetails"><%= Utils.getTextFromFile("SiteDiscount", (eLanguage)1255) %></label>
                <input id="txtSiteDiscount" value="0" class="txtGeneralDetails" runat="server" type="text" onkeypress="return isNumberKey(event)"/>
                <label class="labelGeneralDetails"><%= Utils.getTextFromFile("OfficeDiscount", (eLanguage)1255) %></label>
                <input id="txtOfficeDiscount" class="txtGeneralDetails" runat="server" type="text" onkeypress="return isNumberKey(event)"/>
                <label class="labelGeneralDetails"><%= Utils.getTextFromFile("Commission", (eLanguage)1255) %></label>
                <input id="txtCommission" class="txtGeneralDetails" runat="server" type="text" onkeypress="return isNumberKey(event)"/>
            </div>
            <div id="divAddtives" style="float: right; margin-bottom: 11px; width: 100%;">
                <label class="labelGeneralDetails"><%= Utils.getTextFromFile("GeneralAdditive", (eLanguage)1255) %></label>
                <input id="txtGeneralAdditive" class="txtGeneralDetails" runat="server" type="text" onkeypress="return isNumberKey(event)"/>
                <label class="labelGeneralDetails"><%= Utils.getTextFromFile("BabiesAdditive", (eLanguage)1255) %></label>
                <input id="txtBabiesAdditive" class="txtGeneralDetails" runat="server" type="text" onkeypress="return isNumberKey(event)"/>
            </div>

            <label class="labelGeneralDetails" style="width: 13%; text-decoration: underline;"><%= Utils.getTextFromFile("BasePriceCombo", (eLanguage)1255) %></label>
            <div style="width:100%; float:right;">
                <label class="labelGeneralDetails"><%= Utils.getTextFromFile("Composition", (eLanguage)1255) %>:</label>
                <asp:Label class="labelGeneralDetails" style="font-weight: 100; width:30%; " ID="lblBaseComposition" runat="server"></asp:Label>
                <asp:HiddenField ID="hiddenBaseCompositionId" runat="server"></asp:HiddenField>
            </div>
            <div style="width:100%; float:right;">    
                <label class="labelGeneralDetails"><%= Utils.getTextFromFile("Base", (eLanguage)1255) %>:</label>
                <asp:Label class="labelGeneralDetails" style="font-weight: 100; width:30%; " ID="lblBaseBase" runat="server"></asp:Label>
                <asp:HiddenField ID="hiddenBaseBaseId" runat="server"></asp:HiddenField>
            </div>
            <div style="width:100%; float:right;">
                <label class="labelGeneralDetails"><%= Utils.getTextFromFile("RoomType", (eLanguage)1255) %>:</label>
                <asp:Label class="labelGeneralDetails" style="font-weight: 100; width:30%; " ID="lblBaseRoomType" runat="server"></asp:Label>
                <asp:HiddenField ID="hiddenBaseRoomTypeId" runat="server"></asp:HiddenField>
            </div>

            <div class="accordion" style="margin-top: 18%;"><%= Utils.getTextFromFile("BaseComboSection",(eLanguage)1255) %></div>
            <div id="divBasePriceSelect" class="panel" style="float: right; margin-bottom: 11px; width: 96%; direction: rtl;">
                <table>
                    <tr>
                        <td><label class="labelGeneralDetails" style="width:100%;"><%= Utils.getTextFromFile("BaseCompositionDdlHead", (eLanguage)1255) %></label></td>
                        <td><label class="labelGeneralDetails" style="width:100%;"><%= Utils.getTextFromFile("BaseBaseDdlHead", (eLanguage)1255) %></label></td>
                        <td><label class="labelGeneralDetails" style="width:100%;"><%= Utils.getTextFromFile("BaseRoomTypeDdlHead", (eLanguage)1255) %></label></td>
                    </tr>
                    <tr>
                        <td>
                            <div class="dropDownList" style="max-height: 120px;">
                                <asp:CheckBoxList ID="chkBaseComposition" runat="server"></asp:CheckBoxList>
                            </div>
                        </td>
                        <td>
                            <div class="dropDownList" style="max-height: 120px;">
                                <asp:CheckBoxList ID="chkBaseBase" runat="server"></asp:CheckBoxList>
                            </div>
                        </td>
                        <td>
                            <div class="dropDownList" style="max-height: 120px;">
                                <asp:CheckBoxList ID="chkBaseRoomType" runat="server"></asp:CheckBoxList>
                            </div>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
        
        <div id="divComboDetails">
            <div id="divCombo" style="float: right; margin-top: 2px; width: 100%;">
                <div id="divComposition" class="borderDouble" style="height: 60%;" runat="server">
                    <label style="float: right; width: 12%"><%= Utils.getTextFromFile("Compositions", (eLanguage)1255) %></label>
                    <div id="divCompositionsDetails" class="scroller" style="float: right; width: 40%" runat="server">
                    </div>
                    <div class="dropDownList">
                        <asp:CheckBoxList ID="chkCompositions" runat="server"></asp:CheckBoxList>
                    </div>
                </div>
                <div id="divBases" class="borderDouble" style="height: 60%;" runat="server">
                    <label style="float: right; width: 12%"><%= Utils.getTextFromFile("Bases", (eLanguage)1255) %></label>
                    
                    <div style="direction:rtl; width: 12%; float:right;">
                        <asp:RadioButton runat="server" ID="rbAmount" GroupName="base" />
                        <asp:RadioButton runat="server" ID="rbPercent"  GroupName="base"/>
                    </div>
                    <div id="divBasesDetails" class="scroller" style="float: right; width: 40%;" runat="server">
                    </div>
                    <div class="dropDownList">
                        <asp:CheckBoxList ID="chkBases" runat="server"></asp:CheckBoxList>
                    </div>
                </div>
                <div id="divRoomTypes" class="borderDouble" style="height: 60%;" runat="server">
                    <label style="float: right; width: 12%"><%= Utils.getTextFromFile("RoomTypes", (eLanguage)1255) %></label>
                    <div id="divRoomTypesDetails" class="scroller" style="float: right; width: 40%" runat="server">
                    </div>
                    <div class="dropDownList">
                        <asp:CheckBoxList ID="chkRoomTypes" runat="server"></asp:CheckBoxList>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

