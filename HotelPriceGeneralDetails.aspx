<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="HotelPriceGeneralDetails.aspx.cs" Inherits="HotelPriceGeneralDetails" %>

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

        Date.prototype.ddMMyyyy = function () {
            return (this.getDate() + '/' + eval(this.getMonth() + 1) + '/' + this.getFullYear());
        };
        
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

            switch (comboTypeString) {
                case 'compositionBrutto':
                    txtId = 'txtComposition_' + comboId;
                    lblId = 'lblComposition_' + comboId;
                    divToInsertTo = 'divCompositionsDetails';
                    divToInsertTo = $("[id$=_" + divToInsertTo + "]").attr("id");
                    break;
                case 'compositionNetto':
                    txtId = 'txtComposition_Netto' + comboId;
                    lblId = 'lblNettoComposition_' + comboId;
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

            placeContentId = divToInsertTo.split('_')[0];
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
                var textBox = '<input type="text" name="ctl00$' + txtId.replace('_', '$') + '" id="' + txtId + '" value="' + comboAmount + '" onkeypress="isNumberKey" style="float:right; width:25%; margin-right: 128px; margin-top:-' + pixels + 'px;"><br><br>';


                $('#' + divToInsertTo).append(label);
                $('#' + divToInsertTo).append(textBox);
            }
        }

        function changeBaseCombo(comboTypeString, comboId, comboName, selectedChkBox) {
            var isChecked = selectedChkBox.checked;
            debugger;
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
    </script>
    
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server" >
    <asp:HiddenField ID="hiddenClerkId" runat="server" />
    <asp:HiddenField ID="hiddenSupplierId" runat="server" />

    <div style="background: #d0d0d0; text-align:center; width:100%; margin-bottom: 7px; direction:rtl;">
        <asp:Label id="priceHeader" class="headline" runat="server"></asp:Label>
    </div>
    <div>
        <asp:Button ID="btGoToSetCalendarPrices" CssClass="saveButton" OnClick="btGoToSetCalendarPrices_Click" runat="server"/>
        <asp:Button ID="btSaveHotelPrice" CssClass="saveButton" OnClick="btSaveHotelPrice_Click" runat="server"/>
        <asp:Button ID="btBackToMenu" CssClass="saveButton" OnClick="btBackToMenu_Click" runat="server"/>
    </div>
    
    <div id="divGeneralDetails" class="borderDouble" style="height: 19%; float: right;">
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
        <div id="divBasePriceSelect"  style="float: right; margin-bottom: 11px; width: 96%; direction: rtl;">
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
                <div id="divCompositionsDetailsBrutto" class="scroller" style="float: right; width: 30%" runat="server">
                </div>
                <div id="divCompositionsDetailsNetto" class="scroller" style="float: right; width: 40%" runat="server">
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
</asp:Content>

