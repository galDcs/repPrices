//using AgencyPricesWS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;

public partial class HotelSearchIPS : System.Web.UI.Page
{
    private string mLangStr = "1255";
    private int mDaysToShow = 15;
    private int mDaysBefore;
    private int mDaysAfter;
    private bool isSite = false;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            loadAreas();
            getAllComboDedailByComboType(eComboType.Composition);
            getAllComboDedailByComboType(eComboType.Base);
            getAllComboDedailByComboType(eComboType.RoomType);
        }
    }

    private void loadAreas()
    {
        try
        {
            DataSet ds1 = DAL_SQL_Helper.GetGeneralAreas("027235", "");
            ds1.Tables[0].Columns.Add("GeneralAreaName", typeof(string), "general_area_name");
            DataRow AllRow = ds1.Tables[0].NewRow();
            AllRow[0] = 0;
            AllRow[1] = Utils.getTextFromFile("ChooseArea", eLanguage.Hebrew);
            AllRow[2] = "";
            ds1.Tables[0].Rows.InsertAt(AllRow, 0);
            ddlGeneralAreaId.DataSource = ds1.Tables[0];
            ddlGeneralAreaId.DataBind();
        }
        catch (Exception ex)
        {
            string message = "Failed to load general areas.";
            Logger.Log(message + ex.Message);
            ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + message + "');", true);
        }
    }

    private void getAllComboDedailByComboType(eComboType iComboType)
    {
        string query = string.Empty;
        DropDownList chkList = null;
        string comboTypeStr = string.Empty;
        string funcString = string.Empty;

        switch (iComboType)
        {
            case eComboType.Composition:
                query = "SELECT * FROM Agency_Admin.dbo.HOTEL_ROOM_TYPE WHERE isDisabled = 0";
                comboTypeStr = "composition";
                chkList = ddlCompositions;
                break;
            case eComboType.Base:
                query = "SELECT * FROM Agency_Admin.dbo.HOTEL_ON_BASE WHERE isDisabled = 0";
                comboTypeStr = "base";
                chkList = ddlBases;
                break;
            case eComboType.RoomType:
                query = "SELECT * FROM Agency_Admin.dbo.SUPPLIERS_HOTEL_ADDS WHERE status = 1";
                comboTypeStr = "roomType";
                chkList = ddlRoomTypes;
                break;
        }

        ddlCompositions.Items.Add(new ListItem(Utils.getTextFromFile("Choose", eLanguage.Hebrew), "0"));
        ddlBases.Items.Add(new ListItem(Utils.getTextFromFile("Choose", eLanguage.Hebrew), "0"));
        ddlRoomTypes.Items.Add(new ListItem(Utils.getTextFromFile("Choose", eLanguage.Hebrew), "0"));

        DataSet ds = DAL_SQL.RunSqlDataSet(query);

        if (Utils.isDataSetRowsNotEmpty(ds))
        {
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                ListItem ddlItem = new ListItem(row["name_1255"].ToString(), row["id"].ToString());
                switch (iComboType)
                {
                    case eComboType.Composition:
                        break;
                    case eComboType.Base:
                        break;
                    case eComboType.RoomType:
                        break;
                }

                chkList.Items.Add(ddlItem);
            }
        }
    }

    protected void AreasIndexChanged(object sender, EventArgs e)
    {
        string lang = "1255";
        string nameLang = "name_" + lang;
        string selectedArea = (sender as DropDownList).SelectedValue.ToString();

        try
        {
            DataSet ds1 = DAL_SQL_Helper.GetHotelsByArea(selectedArea, lang);
            ds1.Tables[0].Columns.Add("HotelName", typeof(string), nameLang);
            DataRow AllRow = ds1.Tables[0].NewRow();
            AllRow[0] = 0;
            AllRow[1] = Utils.getTextFromFile("ChooseHotel", eLanguage.Hebrew);
            ds1.Tables[0].Rows.InsertAt(AllRow, 0);
            ddlHotels.DataSource = ds1.Tables[0];
            ddlHotels.DataBind();
        }
        catch (Exception ex)
        {
            string message = "Failed to load hotels.";
            Logger.Log("Failed to load hotels." + ex.Message);
            ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + message + "');", true);
        }
    }

    protected void btSearchHotels_Click(object sender, EventArgs e)
    {
        string agencyId = "88";
        string systemType = "3";
        string clerkId = "1";
        string areaId = ddlGeneralAreaId.SelectedValue;
        string selectedSupplierId = ddlHotels.SelectedValue;
        string priceType = "1";
        DateTime fromDate = DateTime.Parse(Utils.ConvertMonthWithSlashToAgencyFormat(txtFromDate.Text));
        DateTime toDate = DateTime.Parse(Utils.ConvertMonthWithSlashToAgencyFormat(txtToDate.Text));
        DateTime periodFromDate = DateTime.Parse(Utils.ConvertMonthWithSlashToAgencyFormat(txtFromDate.Text)).AddDays((-1) * 3);
        DateTime periodToDate = DateTime.Parse(Utils.ConvertMonthWithSlashToAgencyFormat(txtToDate.Text)).AddDays(5);
        //string compositionId = ddlCompositions.SelectedValue;
        string adultsAmount = ddlUCAdults.SelectedValue;
        string kidsAmount = (int.Parse(ddlUCKidsFrom2To10.SelectedValue) + int.Parse(ddlUCKidsFrom10To21.SelectedValue)).ToString();
        string infantsAmount = ddlUCKidsFrom0To2.SelectedValue;
        string baseId = ddlBases.SelectedValue;
        string roomTypeId = ddlRoomTypes.SelectedValue;
        string finalPrices = string.Empty;
        int roomsAmount = int.Parse(lblRoomsAmount.Text);

        if (!string.IsNullOrEmpty(areaId) && areaId != "0")
        {
            try
            {
                finalPrices += "<Root>";
                if (!string.IsNullOrEmpty(selectedSupplierId) && selectedSupplierId != "0")
                {
                    finalPrices += getFinalPrice(agencyId, systemType, clerkId, selectedSupplierId, priceType, fromDate.ToString("dd-MMM-yy"), toDate.ToString("dd-MMM-yy"), adultsAmount, kidsAmount, infantsAmount, baseId, roomsAmount, isSite);
                }
                else
                {
                    string supplierId;
                    foreach (ListItem item in ddlHotels.Items)
                    {
                        supplierId = item.Value;
                        if (!string.IsNullOrEmpty(supplierId) && supplierId != "0")
                        {
                            finalPrices += getFinalPrice(agencyId, systemType, clerkId, supplierId, priceType, fromDate.ToString("dd-MMM-yy"), toDate.ToString("dd-MMM-yy"), adultsAmount, kidsAmount, infantsAmount, baseId, roomsAmount, isSite);
                        }
                    }
                }
                finalPrices += "</Root>";
Logger.Log("final = " + finalPrices);
                try
                {
                    //Calculate the days to show in the table
                    calculadeDatesToShowInTable(fromDate, toDate, periodFromDate, periodToDate);
                    buildHotelViews(finalPrices, fromDate, toDate, roomsAmount);
                }
                catch (Exception exGetAvailableAllocation)
                {
                    Logger.Log("Failed to search ,Exception = " + exGetAvailableAllocation.Message);
                }
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + ex.Message + "');", true);
            }
        }
        else
        {
            string msg = "please select area";
            ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + msg + "');", true);
        }
    }

    private string getFinalPrice(string iAgencyId, string iSystemType, string iClerkId, string iSupplierId, string iPriceType, string iFromDate, string iToDate, string iAdultsAmount, string iKidsAmount, string iInfantsAmount, string iBaseId, int iRoomsAmount, bool iIsSite)
    {
        string compositionId;
        
        AgencyPricesSearch priceWs = new AgencyPricesSearch();
        string result = string.Empty;

        if (iRoomsAmount == 1)
        {
            compositionId = getCompositionId(iAdultsAmount, iKidsAmount, iInfantsAmount);
            result = priceWs.getHotelPricePerAreaWithAllocations(iAgencyId, iSystemType, iClerkId, iSupplierId, iPriceType, iFromDate, iToDate, compositionId, iBaseId, iRoomsAmount, iIsSite);
        }
        else
        {
            int oneRoom = 1;
            string tempResult = "";

            for (int i = 0; i < iRoomsAmount; i++)
            {
                compositionId = getCompositionId(iAdultsAmount, iKidsAmount, iInfantsAmount);
                tempResult += priceWs.getHotelPricePerAreaWithAllocations(iAgencyId, iSystemType, iClerkId, iSupplierId, iPriceType, iFromDate, iToDate, compositionId, iBaseId, oneRoom, iIsSite);
                //added the temp if will be needed to make any manipulations.
                result += tempResult;
            }
        }

        return result;
    }

    private string getCompositionId(string iAdultsAmount, string iKidsAmount, string iInfantsAmount)
    {
        string compositionId;

        compositionId = DAL_SQL_Helper.getCompositionId(iAdultsAmount, iKidsAmount, iInfantsAmount);
        if (string.IsNullOrEmpty(compositionId))
        {
            compositionId = "0";
        }

        return compositionId;
    }


    private void calculadeDatesToShowInTable(DateTime iFromDate, DateTime iToDate, DateTime iPeriodFromDate, DateTime iPeriodToDate)
    {
        int daysDiffFromDate = 0, daysDiffToDate = 0;

        if ((iPeriodToDate - iPeriodFromDate).Days > mDaysToShow) //more than 15
        {
            if ((iFromDate - iPeriodFromDate).Days < mDaysToShow / 2)
            {
                daysDiffFromDate = (iFromDate - iPeriodFromDate).Days; //13
                daysDiffToDate = mDaysToShow - daysDiffFromDate;       //2
            }
            else if ((iPeriodToDate - iToDate).Days < mDaysToShow / 2)
            {
                daysDiffToDate = (iPeriodToDate - iToDate).Days;       //2
                daysDiffFromDate = mDaysToShow - daysDiffToDate;       //13
            }
            else
            {
                daysDiffFromDate = mDaysToShow / 2;                    //7
                daysDiffToDate = mDaysToShow / 2 + 1;                  //8
            }

            //Check again to make sure that not passing the periods dates.
            if ((iFromDate - iPeriodFromDate).Days < daysDiffFromDate)
            {
                daysDiffFromDate = (iFromDate - iPeriodFromDate).Days;
                daysDiffToDate = mDaysToShow - daysDiffFromDate;       //2
            }
            else if ((iPeriodToDate - iToDate).Days < mDaysToShow / 2)
            {
                daysDiffToDate = (iPeriodToDate - iToDate).Days;       //2
                daysDiffFromDate = mDaysToShow - daysDiffToDate;       //13
            }
        }
        else
        {
            daysDiffFromDate = (iFromDate - iPeriodFromDate).Days;
            daysDiffToDate = (iPeriodToDate - iToDate).Days;
        }

        mDaysBefore = daysDiffFromDate;
        mDaysAfter = daysDiffToDate;
    }

    private void buildHotelViews(string iFinalPrices, DateTime iFromDate, DateTime iToDate, int iRoomsAmount)
    {
        XmlDocument finalPricexXml = new XmlDocument();
        finalPricexXml.LoadXml(iFinalPrices);
        
        //Set table headers
        setHotelsTableHeadear(iFromDate, iToDate);

        foreach (XmlNode finalPriceXml in finalPricexXml.SelectNodes("Root//FinalPrices//FinalPrice"))
        {
            ////////////////////////
            addHotelsTableRows(finalPriceXml, iFromDate, iToDate, iRoomsAmount);
        }
    }

    private void addHotelsTableRows(XmlNode iFinalPriceXml, DateTime iFromDate, DateTime iToDate, int iRoomsAmount)
    {
        string supplierId = string.Empty;
        string supplierName = string.Empty;
        string compositionId = string.Empty;
        string baseId = string.Empty;
        string roomTypeId = string.Empty;
        string compositionName = string.Empty;
        string baseName = string.Empty;
        string roomTypeName = string.Empty;
        string color = string.Empty;
        string roomsLeftStr = string.Empty;
        string dateStr = string.Empty;
        string statusStr = string.Empty;
        string brutto = string.Empty;        
        
        TableRow row = new TableRow();
        TableCell cell = new TableCell();
        XmlNode finalPricePerDayNode;
        DateTime currentDate = iFromDate.AddDays((-1) * mDaysBefore);
        XmlDocument weekBefoeAndAfterXml = new XmlDocument();
        Label lblDateAvailability = new Label();

        supplierId = iFinalPriceXml.SelectSingleNode("SupplierId").InnerText;
        supplierName = DAL_SQL.GetRecord("Agency_Admin.dbo.SUPPLIERS", "name", "id", supplierId);
        compositionId = iFinalPriceXml.SelectSingleNode("Composition//Id").InnerText;
        baseId = iFinalPriceXml.SelectSingleNode("Base//Id").InnerText;
        roomTypeId = iFinalPriceXml.SelectSingleNode("RoomType//Id").InnerText;
        compositionName = iFinalPriceXml.SelectSingleNode("Composition//Name").InnerText;
        baseName = iFinalPriceXml.SelectSingleNode("Base//Name").InnerText;
        roomTypeName = iFinalPriceXml.SelectSingleNode("RoomType//Name").InnerText;
        brutto = iFinalPriceXml.SelectSingleNode("FinalPriceBrutto").InnerText;

        //Add empty cell
        lblDateAvailability.Text = supplierName;
        cell.Controls.Add(lblDateAvailability);
        row.Cells.Add(cell);

        row.Attributes["SupplierId"] = supplierId;
        row.Attributes["CompositionId"] = compositionId;
        row.Attributes["BaseId"] = baseId;
        row.Attributes["RoomTypeId"] = roomTypeId;
        row.Attributes["FinalPriceBrutto"] = brutto;
        int i = 0;

        while (currentDate != iToDate.AddDays(mDaysAfter))
        {
            cell = new TableCell();
            finalPricePerDayNode = iFinalPriceXml.SelectNodes("FinalPricesPerDays//FinalPricePerDay")[i];

            if (currentDate.Date >= iFromDate.Date && currentDate < iToDate.Date && finalPricePerDayNode != null)
            { 
                dateStr = finalPricePerDayNode.SelectSingleNode("Date").InnerText;
                statusStr = finalPricePerDayNode.SelectSingleNode("Status").InnerText;
                color = finalPricePerDayNode.SelectSingleNode("Color").InnerText;
                roomsLeftStr = finalPricePerDayNode.SelectSingleNode("RoomsLeft").InnerText;
                i++;
            }
            else
            {
                int roomsAmountFromDb, roomsInUse, roomsDisable;

                dateStr = string.Empty;
                weekBefoeAndAfterXml = Utils.getDatePriceBaseDetails(supplierId, currentDate);
                if (weekBefoeAndAfterXml.SelectSingleNode("PricePerDayDetails").HasChildNodes)
                {
                    int.TryParse(weekBefoeAndAfterXml.SelectSingleNode("PricePerDayDetails//RoomsAmount").InnerText, out roomsAmountFromDb);
                    int.TryParse(weekBefoeAndAfterXml.SelectSingleNode("PricePerDayDetails//RoomsInUse").InnerText, out roomsInUse);
                    int.TryParse(weekBefoeAndAfterXml.SelectSingleNode("PricePerDayDetails//RoomsDisable").InnerText, out roomsDisable);
                    statusStr = weekBefoeAndAfterXml.SelectSingleNode("PricePerDayDetails//Status").InnerText;
                    color = weekBefoeAndAfterXml.SelectSingleNode("PricePerDayDetails//Color").InnerText;
                    roomsLeftStr = (roomsAmountFromDb - roomsInUse).ToString();
                    if (isSite)
                    {
                        roomsLeftStr = (int.Parse(roomsLeftStr) - roomsDisable).ToString();
                    }
                }
                else
                {
                    statusStr = "False";
                    color = string.Empty;
                    roomsLeftStr = "לא מוגדר";
                }
            }

            if (isHoliday(currentDate))
            {
                cell.BackColor = Color.Gray;
            }
            cell.Style.Add("font-family", "arial");
            cell.Style.Add("font-weight", "100");
            cell.Style.Add("font-size", "12px");
            cell.Style.Add("padding", "1% 2% 1% 2%");

            cell.Controls.Add(getCellLabel(roomsLeftStr, statusStr, color, iRoomsAmount));
            row.Cells.Add(cell);
            currentDate = currentDate.AddDays(1);
        }

        HotelsTable.Rows.Add(row);
    }

    private Label getCellLabel(string roomsLeftStr, string statusStr, string color, int iRoomsAmount)
    {
        int roomsLeft = 0;
        bool dayStatus;
        Label lblDateAvailability = new Label();

        if (!int.TryParse(roomsLeftStr, out roomsLeft))
        {
            roomsLeft = 0;
        }

        if (!bool.TryParse(statusStr, out dayStatus))
        {
            dayStatus = false;
        }

        if (roomsLeft - iRoomsAmount > 0 && dayStatus)
        {
            lblDateAvailability.Text = "יש";
        }
        else
        {
            lblDateAvailability.Text = "אין";
        }

        lblDateAvailability.Text += " " + roomsLeft;
        if (!string.IsNullOrEmpty(color))
        {
            lblDateAvailability.Text += "<r style='color:" + color + "'> &uhblk; </r>";
        }

        return lblDateAvailability;

    }

    private void setHotelsTableHeadear(DateTime iFromDate, DateTime iToDate)
    {
        TableRow row = new TableRow();
        TableCell cell = new TableCell(); 

        DateTime currentDate = iFromDate.AddDays((-1) * (mDaysBefore));
        //Add empty cell
        cell.Text = "מלון";
        row.Cells.Add(cell);

        while (currentDate != iToDate.AddDays(mDaysAfter))
        {
            cell = new TableCell();
            cell.Text = currentDate.ToString("dd/MM");
            if (isHoliday(currentDate))
            {
                cell.BackColor = Color.Gray;
            }
            
            if (currentDate >= iFromDate && currentDate < iToDate)
            {
                cell.BackColor = System.Drawing.ColorTranslator.FromHtml("#6195ec");
            }

            row.Cells.Add(cell);
            currentDate = currentDate.AddDays(1);
        }

        HotelsTable.Rows.Add(row);
    }

    private bool isHoliday(DateTime currentDate)
    {
        return false;
    }
}