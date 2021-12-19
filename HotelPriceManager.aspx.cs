//using AgencyPricesWS;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Web.UI.HtmlControls;

public partial class HotelPriceManager : System.Web.UI.Page
{
    private static DateTime fromDate;
    private static DateTime toDate;
    private static Dictionary<string, string> baseNamesToids = new Dictionary<string, string>();
    public const int mDaysToBlockInDatePicker = 183;
    //TODO:
    private bool isUsingAllocations = false;
    //TODO:
    private bool isDisablingOnSite = false;
    private string mLangStr = "1255";
    private eLanguage mLang;
    public HotelPrice mHotelPrice = new HotelPrice();
    private List<PricePerDay> MonthlyPricesPerDay
    {
        get
        {
            return mHotelPrice.getPricePerDayList();
        }
    }
    private string mSupplierId = string.Empty;
    private string mSupplierName = string.Empty;
    private string mAreaId = string.Empty;
    private string mAreaName = string.Empty;
    private string mHotelSupplierId = string.Empty;
    private string mClerkId = string.Empty;
    public int mMonth;
    public int mYear;
	public string mSalesNightsFreeRows;

    private void setAgencyData(string iAgencyId, string iSystemType)
	{
		DAL_SQL.ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"];
		DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^agencyId^", ((iAgencyId.Length == 1) ? "000" + iAgencyId : "00" + iAgencyId));
		DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^systemType^", ((iSystemType == "3") ? "INN" : (iSystemType == "2") ? "ICC" :"OUT"));
	}
	
    protected void Page_Load(object sender, EventArgs e)
    {		
        ListItem liTemp;
        mLang = (eLanguage)int.Parse(mLangStr);

        setPriveteMembers();
		mSalesNightsFreeRows = getSalesFreeNight();

        if (!Page.IsPostBack)
        {
            loadTable(false);
            mYear = int.Parse(Request.QueryString["year"]);
            mMonth = int.Parse(Request.QueryString["month"]);
            hiddenMonth.Value = mMonth.ToString();
            hiddenYear.Value = mYear.ToString();

			ddlStatus.Items.FindByValue("true").Selected = true;
			ddlStatus.Items.FindByValue("true").Text = Utils.getTextFromFile("Active", mLang);
			ddlStatus.Items.FindByValue("false").Text = Utils.getTextFromFile("InActive", mLang);
			
            liTemp = new ListItem() { Text = Utils.getTextFromFile("Without", mLang), Value = "white" };
            ddlColors.Items.Add(liTemp);
            liTemp = new ListItem() { Text = Utils.getTextFromFile("PriceColorGreen", mLang), Value = "#33CC66" };
            ddlColors.Items.Add(liTemp);
            liTemp = new ListItem() { Text = Utils.getTextFromFile("PriceColorYellow", mLang), Value = "#ecd06f" };
            ddlColors.Items.Add(liTemp);
            liTemp = new ListItem() { Text = Utils.getTextFromFile("PriceColorRed", mLang), Value = "#FF6666" };
            ddlColors.Items.Add(liTemp);

            btSave.Text = Utils.getTextFromFile("Save", mLang);
            btBackToMenu.Text = Utils.getTextFromFile("BackToMenu", mLang);
            btSetGeneralDetails.Text = Utils.getTextFromFile("SetGeneralDetails", mLang);
            btSetPricesMonthly.Text = Utils.getTextFromFile("CopyToAllMonth", mLang);
            btCloseDatesRange.Text = Utils.getTextFromFile("CloseDatesRange", mLang);
            btOpenDatesRange.Text = Utils.getTextFromFile("OpenDatesRange", mLang);
            btSaveSalesCyclesMonthly.Text = Utils.getTextFromFile("SaveSalesCycles", mLang);
            btSetAllocationsMonthly.Text = Utils.getTextFromFile("SaveAllocationMonthly", mLang);
			btMoveToMonth.Text = Utils.getTextFromFile("ButtonMoveToMonth", mLang);
			btSaveSaleNightFree.Text = Utils.getTextFromFile("AddNew", mLang);
			
            btSaveMontlhyAllocation.Text = Utils.getTextFromFile("SaveMonthlyAllocation", mLang);
            btNextMonth.Text = Utils.getTextFromFile("Next", mLang);
            btPrevMonth.Text = Utils.getTextFromFile("Previous", mLang);
        }

        if (isUsingAllocations)
        {
            divAllocations.Visible = true;

            if (isDisablingOnSite)
            {
                divRoomDisable.Visible = true;
                trRoomsDisable.Visible = true;
            }
        }

        mMonth = int.Parse(hiddenMonth.Value);
        mYear = int.Parse(hiddenYear.Value);

        //Load calendar
        loadCalendar();
    }
	
	private string getSalesFreeNight()
	{
		string html = string.Empty;
		string query = @"
SELECT id, hotel_price_id, from_date,
	   to_date, nights, nights_free,
	   status
FROM P_HOTEL_PRICE_SALES_FREE_NIGHTS
WHERE status = 1 AND hotel_price_id = (SELECT id FROM P_HOTEL_PRICES WHERE supplier_id = " + mSupplierId + ")";
		DataSet ds = DAL_SQL.RunSqlDataSet(query);
		
		if (Utils.isDataSetRowsNotEmpty(ds))
		{
			foreach (DataRow row in ds.Tables[0].Rows)
			{
				html += @"
					<tr>
						<td>" + DateTime.Parse(row["from_date"].ToString()).ToString("dd/MM/yy") + @" </td>
						<td>" + DateTime.Parse(row["to_date"].ToString()).ToString("dd/MM/yy") + @" </td>
						<td>" + row["nights"].ToString() + @" </td>
						<td>" + row["nights_free"].ToString() + @" </td>
						<td>delete(" + row["id"].ToString() + @")</td>
					</tr>
					";
			}
		}
		
		return html;
	}

    private void setPriveteMembers()
    {
        string msg = string.Empty;

        if (!string.IsNullOrEmpty(Request.QueryString["supplierId"]))
        {
            mSupplierId = Request.QueryString["supplierId"];
            hiddenSupplierId.Value = mSupplierId;
            mHotelSupplierId = Utils.getHotelPriceId(mSupplierId);

            if (!string.IsNullOrEmpty(Request.QueryString["ClerkId"]))
            {
                mClerkId = Request.QueryString["ClerkId"];

                if (!string.IsNullOrEmpty(Request.QueryString["supplierName"]))
                {
                    mSupplierName = Request.QueryString["supplierName"];

                    if (!string.IsNullOrEmpty(Request.QueryString["areaId"]))
                    {
                        mAreaId = Request.QueryString["areaId"];
                        mAreaName = Utils.GetAreaNameById(mAreaId, mLangStr);
                    }
                    else
                    {
                        msg = Utils.getTextFromFile("AreaIdMissing", mLang);
                    }
                }
                else
                {
                    msg = Utils.getTextFromFile("SupplierNameMissing", mLang);
                }
            }
            else
            {
                msg = Utils.getTextFromFile("ClerkIdMissing", mLang);
            }
        }
        else
        {
            msg = Utils.getTextFromFile("SupplierIdMissing", mLang);
        }

        if (!string.IsNullOrEmpty(msg))
            popUpMessage(msg);
    }

    private void loadCalendar()
    {
        calendar.Controls.Clear();
        setHeader();
        setWeekDaysHeader();

        //HotelPrice details
        loadHotelPrice();
        if (!Page.IsPostBack)
        {
            setMonthDays();
        }
    }

    //HotelPrice
    private void loadHotelPrice()
    {
        MonthlyAllocation monthlyAllocation;
		PriceDetailsWs mPriceWs = new PriceDetailsWs();
        mHotelPrice = mPriceWs.getHotelPrice(mSupplierId, mMonth, mYear);
        if (!Page.IsPostBack)
        {
            if (mHotelPrice != null)
            {
                monthlyAllocation = mHotelPrice.mMonthlyAllocations.Find(x => x.mMonth == mMonth && x.mYear == mYear);

                if (monthlyAllocation != null)
                {
                    txtMonthlyAllocation.Text = monthlyAllocation.mMonthlyAllocation.ToString();
                }
                else
                {
                    txtMonthlyAllocation.Text = "0";
                }

                txtMonthlyAllocationSum.Text = getMonthlyAllocationSum().ToString();
                txtMonthlyAllocationUsed.Text = getMonthlyAllocationUsed().ToString();
            }
            else
            {
                txtMonthlyAllocation.Text = "0";
            }
        }
    }

    private int getMonthlyAllocationSum()
    {
        int sum = 0;

        foreach (PricePerDay pricePerDay in MonthlyPricesPerDay)
        {
            sum += pricePerDay.mAllocation.mRoomsAmount;
        }

        return sum;
    }

    private int getMonthlyAllocationUsed()
    {
        int sum = 0;

        foreach (PricePerDay pricePerDay in MonthlyPricesPerDay)
        {
            sum += pricePerDay.mAllocation.mRoomsInUse;
        }

        return sum;
    }

    public void getDatePriceCombosPrices(DateTime iDate)
    {
        XmlDocument dateComboPricesXml = null;
        try
        {
            dateComboPricesXml = Utils.getDateCombosPrices(mHotelPrice, iDate);
        }
        catch (Exception e)
        {

        }

        if (dateComboPricesXml != null &&  dateComboPricesXml.SelectNodes("/Root/DateCombosPrices") != null)
        {
            foreach (XmlNode node in dateComboPricesXml.SelectNodes("/Root/DateCombosPrices"))
            {
                string comboPriceText = node["ComboPrice"].InnerXml + " ,";
                string NettocomboPriceText = node["ComboNettoPrice"].InnerXml;
                string otherDetails = " - " + node["CompositionId"].InnerXml + " ," + node["BaseId"].InnerXml + " ," + node["RoomTypeId"].InnerXml + "</br>";
                Label dateCombosPrices = labelByData(comboPriceText, iDate.Day.ToString(), "");
                Label nettoDateCombosPrices = labelByData(NettocomboPriceText, iDate.Day.ToString(), "green");
                Label otherDetailsPrices = labelByData(otherDetails, iDate.Day.ToString(), "");
                divCombosPrices.Controls.Add(dateCombosPrices);
                divCombosPrices.Controls.Add(nettoDateCombosPrices);
                divCombosPrices.Controls.Add(otherDetailsPrices);
            }
        }
    }

    private Label labelByData(string iText, string iDate, string iColor)
    {
        Label dateCombosPrices = new Label();
        dateCombosPrices.Text += iText;
        dateCombosPrices.Style.Add("display", "none");
        dateCombosPrices.Style.Add("color", iColor);
        dateCombosPrices.CssClass = "displayNone displayNone_" + iDate;
        return dateCombosPrices;
    }

    //Gui Utils
    private void setHeader()
    {
        TableHeaderRow headerRow = new TableHeaderRow();

        if (mSupplierName.Contains(Utils.getTextFromFile("ReshetFatal", mLang)))
        {
            mSupplierName = mSupplierName.Replace(Utils.getTextFromFile("ReshetFatal", mLang), "");
        }

        priceHeader.Text = mAreaName + ", " + mSupplierName + " - " + Utils.ConvertMonthToHebName(mMonth) + " " + mYear;
    }

    private void setWeekDaysHeader()
    {
        TableHeaderRow weekDaysRow = new TableHeaderRow();
        eLanguage lang = eLanguage.Hebrew;

        weekDaysRow.BorderColor = Color.Black;

        addTextCellToHeaderRow(Utils.getTextFromFile("Sunday", lang), weekDaysRow);
        addTextCellToHeaderRow(Utils.getTextFromFile("Monday", lang), weekDaysRow);
        addTextCellToHeaderRow(Utils.getTextFromFile("Tuesday", lang), weekDaysRow);
        addTextCellToHeaderRow(Utils.getTextFromFile("Wednesday", lang), weekDaysRow);
        addTextCellToHeaderRow(Utils.getTextFromFile("Thursday", lang), weekDaysRow);
        addTextCellToHeaderRow(Utils.getTextFromFile("Friday", lang), weekDaysRow);
        addTextCellToHeaderRow(Utils.getTextFromFile("Saturday", lang), weekDaysRow);

        calendar.Rows.Add(weekDaysRow);
    }

    private void setMonthDays()
    {
        int firstDayInMonth = 1;
        DateTime dateFromFirstDayInMonth = new DateTime(mYear, mMonth, firstDayInMonth);
        TableRow row = new TableRow();
        PricePerDay datePrice = null;
        int dayInt = 0;
        bool hasPrice = false;
        decimal price = 0;

        #region days without prices

        divCombosPrices.Controls.Clear();

        //Set empty cell before first day in month
        while ((DayOfWeek)dayInt != dateFromFirstDayInMonth.DayOfWeek)
        {
            addTextCellToRow(string.Empty, row, hasPrice, price.ToString(), datePrice);

            dayInt++;
        }
        calendar.Rows.Add(row);

        #endregion

        hasPrice = true;

        while (dateFromFirstDayInMonth.Month == dateFromFirstDayInMonth.AddDays(1).Month)
        {
            if (mHotelPrice != null)
            {
                datePrice = MonthlyPricesPerDay.Find(x => x != null && x.mDate.Date == dateFromFirstDayInMonth.Date);
            }

            if (datePrice != null)
            {
                price = datePrice.mBasePrice.mPrice;
            }
            else
            {
                price = 0;
            }

            hasPrice = (price != 0);
            addTextCellToRow(dateFromFirstDayInMonth.Day.ToString(), row, hasPrice, price.ToString(), datePrice);
            if ((dateFromFirstDayInMonth.Day + dayInt) % 7 == 0)
            {
                calendar.Rows.Add(row);
                row = new TableRow();
            }

            dateFromFirstDayInMonth = dateFromFirstDayInMonth.AddDays(1);
        }

        if (mHotelPrice != null)
        {
            datePrice = MonthlyPricesPerDay.Find(x => x != null && x.mDate.Date == dateFromFirstDayInMonth.Date);
        }

        if (datePrice != null)
        {
            price = datePrice.mBasePrice.mPrice;
        }
        else
        {
            price = 0;
        }

        hasPrice = (price != 0);
        addTextCellToRow(dateFromFirstDayInMonth.Day.ToString(), row, hasPrice, price.ToString(), datePrice);

        calendar.Rows.Add(row);

    }

    public void addTextCellToRow(string iText, TableRow iRow, bool iHasPrice, string price, PricePerDay iDatePrice)
    {
        TableCell dateCell = new TableCell();
		
        if (!string.IsNullOrEmpty(iText))
        {
            Label labelDay = new Label();

            dateCell.CssClass = "datePrice";
            dateCell.Attributes.Add("onClick", "specificDateSelected(this)");
			
            //Adding label day instead of setting the cell text in order to avoid overwriting

            labelDay.Text = iText;
            labelDay.ForeColor = Color.Gray;
            labelDay.ID = "lblDay_" + iText;

            dateCell.Controls.Add(labelDay);
            dateCell.Controls.Add(new LiteralControl("<br />"));

            if (iHasPrice)
            {
                bool isDateClose = !iDatePrice.mStatus;

                if (isDateClose)
                {
                    int width = 30;
                    int height = 30;

                    System.Web.UI.WebControls.Image myImage = new System.Web.UI.WebControls.Image();
                    myImage.ImageUrl = "Images/DateClose.jpg";
                    myImage.Width = width;
                    myImage.Height = height;
                    myImage.Style.Add("margin", "0% 0% 0% 28%");
                    myImage.Style.Add("background", "#ffd1d1");
                    //dateCell.Controls.Add(myImage);
					//close_date_reason
					
					dateCell.Controls.Add(new LiteralControl(@"
                    <div style='background: #ffd1d1; text-align: center; color: crimson; padding: 10px;border-radius: 52px;width: min-content;margin: 0 17%;'>
	                    סגור
                    </div>
                    <div style='font-size:12px;font-weight:600; direction: rtl;'>סיבת סגירה: </div><div style='font-size:12px;font-weight:500; direction: rtl;'>&nbsp" + iDatePrice.CloseDateReason + "</div>"));
                }
                else
                {
                    dateCell.Attributes.Add("AutoPostBack", "true");
                    Label labelPrice = new Label();
                    labelPrice.ID = "lblPrice_" + iText;
                    labelPrice.Text = Utils.getTextFromFile("Brutto", mLang) + ": " + price;
                    labelPrice.Style.Add("margin-left", "20px");
                    labelPrice.Style.Add("float", "right");
                    labelPrice.Style.Add("direction", "rtl");

                    Label labelPriceNetto = new Label();
                    labelPriceNetto.ID = "lblPriceNetto_" + iText;
                    labelPriceNetto.Text = Utils.getTextFromFile("Netto", mLang) + ": " + iDatePrice.mBasePrice.mPriceNetto.ToString();
                    labelPriceNetto.Style.Add("margin-left", "20px");
                    labelPriceNetto.Style.Add("float", "right");
                    labelPriceNetto.Style.Add("direction", "rtl");
                    labelPriceNetto.ForeColor = Color.Gray;
                    labelPriceNetto.Font.Size = 9;

                    dateCell.Controls.Add(labelPrice);
                    dateCell.Controls.Add(new LiteralControl("<br />"));
                    dateCell.Controls.Add(labelPriceNetto);

                    if (isUsingAllocations)
                    {
                        Label labelRooms = new Label();
                        labelRooms.Text += Utils.getTextFromFile("RoomsLeft", mLang) + ": " + (iDatePrice.mAllocation.mRoomsAmount - iDatePrice.mAllocation.mRoomsInUse).ToString();
						labelRooms.Text += "<br />" + Utils.getTextFromFile("RoomsDisable", mLang) + ": " + iDatePrice.mAllocation.mRoomsDisable.ToString();
                        labelRooms.Style.Add("float", "right");
                        labelRooms.Style.Add("direction", "rtl");
                        labelRooms.ForeColor = Color.Gray;
                        labelRooms.Font.Size = 8;

                        dateCell.Controls.Add(new LiteralControl("<br />"));
                        dateCell.Controls.Add(labelRooms);
                    }

                    if (!string.IsNullOrEmpty(iDatePrice.mColor) && iDatePrice.mColor != "0")
                    {
                        dateCell.Controls.Add(new LiteralControl("<r style='color:" + iDatePrice.mColor + "'> &uhblk; </r>"));
                    }
                }

                //dateCell.Style.Add("padding", "0% 3% 3% 0%");
                getDatePriceCombosPrices(new DateTime(mYear, mMonth, int.Parse(iText)));
            }
            else
            {
                dateCell.Style.Add("padding", "1% 0% 6% 1%");
            }
        }

        iRow.Cells.Add(dateCell);
    }

    public void addTextCellToHeaderRow(string iText, TableHeaderRow iHeaderRow)
    {
        TableHeaderCell headerCell = new TableHeaderCell();
        headerCell.Width = 140;
        headerCell.Text = iText;
        headerCell.ForeColor = Color.White;
        headerCell.Style.Add("font-weight", "100");
        headerCell.BackColor = System.Drawing.ColorTranslator.FromHtml("#353535");
        iHeaderRow.Cells.Add(headerCell);
    }

    private void setComboTypeDictionaries(CheckBoxList chkList, eComboType iComboType)
    {
        bool status = true;
        List<ListItem> itemToSelect = new List<ListItem>();
        float percent = 0;
        decimal amount = 0;
        string hiddenFieldValue;

        foreach (ListItem item in chkList.Items)
        {
            if (item.Selected)
            {
                itemToSelect.Add(item);
            }
        }

        foreach (ListItem item in itemToSelect)
        {
            switch (iComboType)
            {
                case eComboType.Composition:
                    if (item.Selected)
                    {
                        Composition composition = null;
                        composition = mHotelPrice.getCompositionById(item.Value);
                        hiddenFieldValue = Request.Form["ctl00$ContentPlaceHolder2$txtComposition_" + item.Value];

                        if (composition != null && !string.IsNullOrEmpty(hiddenFieldValue) && hiddenFieldValue != "0")
                        {
                            percent = float.Parse(hiddenFieldValue);
                            mHotelPrice.mCompositions[composition] = status;
                            composition.mPercent = percent;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(hiddenFieldValue) && hiddenFieldValue != "0")
                            {
                                percent = float.Parse(hiddenFieldValue);
                                mHotelPrice.mCompositions.Add(new Composition(item.Value, item.Text, percent, 0), status);
                            }
                        }
                    }

                    break;
                case eComboType.Base:
                    if (item.Selected)
                    {
                        Base baseItem = null;
                        baseItem = mHotelPrice.getBaseById(item.Value);
                        hiddenFieldValue = Request.Form["ctl00$ContentPlaceHolder2$txtBase_" + item.Value];

                        if (baseItem != null && !string.IsNullOrEmpty(hiddenFieldValue))
                        {
                            mHotelPrice.mBases[baseItem] = status;
                            if (mHotelPrice.mIsBaseAmount)
                            {
                                baseItem.mAmount = decimal.Parse(hiddenFieldValue);
                                baseItem.mPercent = 0;
                            }
                            else
                            {
                                baseItem.mAmount = 0;
                                baseItem.mPercent = float.Parse(hiddenFieldValue);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(hiddenFieldValue))
                            {
                                if (mHotelPrice.mIsBaseAmount)
                                {
                                    mHotelPrice.mBases.Add(new Base(item.Value, item.Text, decimal.Parse(hiddenFieldValue)), status);
                                }
                                else
                                {
                                    mHotelPrice.mBases.Add(new Base(item.Value, item.Text, float.Parse(hiddenFieldValue)), status);
                                }
                            }
                        }
                    }

                    break;
                case eComboType.RoomType:
                    if (item.Selected)
                    {
                        RoomType roomType = null;
                        roomType = mHotelPrice.getRoomTypeById(item.Value);
                        hiddenFieldValue = Request.Form["ctl00$ContentPlaceHolder2$txtRoomType_" + item.Value];

                        if (roomType != null && !string.IsNullOrEmpty(hiddenFieldValue))
                        {
                            amount = decimal.Parse(hiddenFieldValue);
                            mHotelPrice.mRoomTypes[roomType] = status;
                            roomType.mAmount = amount;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(hiddenFieldValue))
                            {
                                amount = decimal.Parse(hiddenFieldValue);
                                mHotelPrice.mRoomTypes.Add(new RoomType(item.Value, item.Text, amount), status);
                            }
                        }
                    }

                    break;
            }
        }
    }

    protected void btSave_Click(object sender, EventArgs e)
    {
        string day = selectedDay.Value;

        if (mHotelPrice != null)
        {
            if (validateDateSave())
            {
                PricePerDay datePrice = new PricePerDay();
                DateTime selectedDate = new DateTime(mYear, mMonth, int.Parse(selectedDay.Value));

                datePrice.mBasePrice.mPrice = Utils.getForammatedNumber(decimal.Parse(txtBasePrice.Value));
                datePrice.mBasePrice.mPriceNetto = Utils.getForammatedNumber(decimal.Parse(txtBasePriceNetto.Value));
                datePrice.mDate = selectedDate;
                datePrice.mStatus = (ddlStatus.SelectedValue == "true");
                datePrice.mColor = ddlColors.SelectedValue;
                datePrice.CloseDateReason = txtCloseDateReason.Text;

                if (isUsingAllocations)
                {
                    datePrice.mAllocation.mRoomsAmount = int.Parse(txtRoomsAmount.Value);
                    if (!string.IsNullOrEmpty(txtRoomsDisable.Value))
                    {
                        datePrice.mAllocation.mRoomsDisable = int.Parse(txtRoomsDisable.Value);
                    }
                    else
                    {
                        datePrice.mAllocation.mRoomsDisable = 0;
                    }
                }

                if (!string.IsNullOrEmpty(txtSalesCycles.Value))
                {
                    foreach (string cycleDay in txtSalesCycles.Value.Split(','))
                    {
                        datePrice.mSalesCycles.Add(int.Parse(cycleDay));
                    }
                }

                try
                {
					PriceDetailsWs mPriceWs = new PriceDetailsWs();
                    mPriceWs.saveDateDetails(mSupplierId, datePrice);
                    mHotelPrice.savePricePerDay(datePrice);

                    popUpMessage(Utils.getTextFromFile("DateSaved", mLang));
                }
                catch (Exception ex)
                {
                    Logger.Log("Failed to save the selected date details. date = " + selectedDate.ToString("dd-MMM-yy") + ", exception = " + ex.Message);

                    if (mHotelPrice == null)
                    {
                        string msg = Utils.getTextFromFile("PleaseSaveTheGeneralPriceFirst", (eLanguage)int.Parse(mLangStr));
                        popUpMessage(msg);
                    }
                }
            }
        }
        else
        {
            popUpMessage(Utils.getTextFromFile("PleaseSaveTheGeneralPriceFirst", mLang));
        }

        loadCalendarWithoutPostBack();
    }

    protected void btPrevMonth_Click(object sender, EventArgs e)
    {
        mMonth = int.Parse(hiddenMonth.Value) - 1;
        if (mMonth == 0)
        {
            mMonth = 12;
            mYear--;
        }
        hiddenMonth.Value = mMonth.ToString();
        hiddenYear.Value = mYear.ToString();

        loadCalendarWithoutPostBack();
    }

    protected void btNextMonth_Click(object sender, EventArgs e)
    {
        mMonth = int.Parse(hiddenMonth.Value) + 1;
        if (mMonth == 13)
        {
            mMonth = 1;
            mYear++;
        }
        hiddenMonth.Value = mMonth.ToString();
        hiddenYear.Value = mYear.ToString();

        loadCalendarWithoutPostBack();
    }

    protected void btBackToMenu_Click(object sender, EventArgs e)
    {
        Response.Redirect("./Menu.aspx");
    }
	
	protected void btMoveToMonth_Click(object sender, EventArgs e)
    {
        mMonth = int.Parse(ddlMonthes.SelectedValue);
		mYear = int.Parse(ddlYears.SelectedValue);
		
        hiddenMonth.Value = mMonth.ToString();
        hiddenYear.Value = mYear.ToString();

        loadCalendarWithoutPostBack();
    }

    protected void btSetPricesMonthly_Click(object sender, EventArgs e)
    {
        if (mHotelPrice != null)
        {
            if (!string.IsNullOrEmpty(txtFromDateSetPrices.Text) && !string.IsNullOrEmpty(txtToDateSetPrices.Text))
            {
                decimal sundayPrice, mondayPrice, tuesdayPrice, wednesdayPrice, thursdayPrice, fridayPrice, saturdayPrice;
                decimal sundayPriceNetto, mondayPriceNetto, tuesdayPriceNetto, wednesdayPriceNetto, thursdayPriceNetto, fridayPriceNetto, saturdayPriceNetto;
                string quickNum = string.Empty;

                if (isWeeklyAssignNumbersValid(txtSundayPrice.Text, txtMondayPrice.Text, txtTuesdayPrice.Text, txtWednesdayPrice.Text, txtThursdayPrice.Text, txtFridayPrice.Text, txtSaturdayPrice.Text) &&
                    isWeeklyAssignNumbersValid(txtSundayPriceNetto.Text, txtMondayPriceNetto.Text, txtTuesdayPriceNetto.Text, txtWednesdayPriceNetto.Text, txtThursdayPriceNetto.Text, txtFridayPriceNetto.Text, txtSaturdayPriceNetto.Text))
                {
                    DateTime currentDate, fromDate, toDate;

                    decimal.TryParse(txtSundayPrice.Text, out sundayPrice);
                    decimal.TryParse(txtMondayPrice.Text, out mondayPrice);
                    decimal.TryParse(txtTuesdayPrice.Text, out tuesdayPrice);
                    decimal.TryParse(txtWednesdayPrice.Text, out wednesdayPrice);
                    decimal.TryParse(txtThursdayPrice.Text, out thursdayPrice);
                    decimal.TryParse(txtFridayPrice.Text, out fridayPrice);
                    decimal.TryParse(txtSaturdayPrice.Text, out saturdayPrice);

                    decimal.TryParse(txtSundayPriceNetto.Text, out sundayPriceNetto);
                    decimal.TryParse(txtMondayPriceNetto.Text, out mondayPriceNetto);
                    decimal.TryParse(txtTuesdayPriceNetto.Text, out tuesdayPriceNetto);
                    decimal.TryParse(txtWednesdayPriceNetto.Text, out wednesdayPriceNetto);
                    decimal.TryParse(txtThursdayPriceNetto.Text, out thursdayPriceNetto);
                    decimal.TryParse(txtFridayPriceNetto.Text, out fridayPriceNetto);
                    decimal.TryParse(txtSaturdayPriceNetto.Text, out saturdayPriceNetto);

                    fromDate = DateTime.Parse(Utils.ConvertMonthWithSlashToAgencyFormat(txtFromDateSetPrices.Text));
                    currentDate = fromDate;
                    toDate = DateTime.Parse(Utils.ConvertMonthWithSlashToAgencyFormat(txtToDateSetPrices.Text));

					if ((toDate - fromDate).Days <= mDaysToBlockInDatePicker)
					{
						Logger.Log("fromDate = " + fromDate);
						Logger.Log("toDate = " + toDate);
						while (currentDate.Date != toDate.AddDays(1).Date)
						{
							if (isDayAssigend(currentDate, txtSundayPrice.Text, txtMondayPrice.Text, txtTuesdayPrice.Text, txtWednesdayPrice.Text, txtThursdayPrice.Text, txtFridayPrice.Text, txtSaturdayPrice.Text) &&
								isDayAssigend(currentDate, txtSundayPriceNetto.Text, txtMondayPriceNetto.Text, txtTuesdayPriceNetto.Text, txtWednesdayPriceNetto.Text, txtThursdayPriceNetto.Text, txtFridayPriceNetto.Text, txtSaturdayPriceNetto.Text))
							{
								PricePerDay existedPricePerDay = MonthlyPricesPerDay.Find(x => x.mDate.Date == currentDate.Date);
								existedPricePerDay = Utils.getSpecificDatePriceDetails(mSupplierId, currentDate);
								if (existedPricePerDay != null)
								{
									existedPricePerDay.mBasePrice.mPrice = getPriceForWeekDay(currentDate, sundayPrice, mondayPrice, tuesdayPrice, wednesdayPrice, thursdayPrice, fridayPrice, saturdayPrice);
									existedPricePerDay.mBasePrice.mPriceNetto = getPriceForWeekDay(currentDate, sundayPriceNetto, mondayPriceNetto, tuesdayPriceNetto, wednesdayPriceNetto, thursdayPriceNetto, fridayPriceNetto, saturdayPriceNetto);

									mHotelPrice.savePricePerDay(existedPricePerDay);
								}
								else
								{
									PricePerDay newDatePrice = new PricePerDay();
									newDatePrice.mDate = currentDate;
									newDatePrice.mBasePrice.mPrice = getPriceForWeekDay(currentDate, sundayPrice, mondayPrice, tuesdayPrice, wednesdayPrice, thursdayPrice, fridayPrice, saturdayPrice);
									newDatePrice.mBasePrice.mPriceNetto = getPriceForWeekDay(currentDate, sundayPriceNetto, mondayPriceNetto, tuesdayPriceNetto, wednesdayPriceNetto, thursdayPriceNetto, fridayPriceNetto, saturdayPriceNetto);
									newDatePrice.mBasePrice.mComposition = mHotelPrice.mBaseComposition;
									newDatePrice.mBasePrice.mBase = mHotelPrice.mBaseBase;
									newDatePrice.mBasePrice.mRoomType = mHotelPrice.mBaseRoomType;
									newDatePrice.mStatus = true;
									newDatePrice.mAllocation.mRoomsAmount = 0;
									newDatePrice.mAllocation.mRoomsInUse = 0;
									newDatePrice.mAllocation.mRoomsDisable = 0;
									newDatePrice.mColor = "white";

									mHotelPrice.savePricePerDay(newDatePrice);
								}
							}

							currentDate = currentDate.AddDays(1);
						}
					
						try
						{
							PriceDetailsWs mPriceWs = new PriceDetailsWs();
							mPriceWs.SetMonthlyPricePerDay(mSupplierId, MonthlyPricesPerDay);
							popUpMessage(Utils.getTextFromFile("SucceccSetPrices", mLang));
						}
						catch (Exception ex)
						{
							Logger.Log("Failed to update all prices.");
							popUpMessage("" + ex.Message);
						}
					}
					else
					{
						string msg = "ניתן לשנות מקסימום חודש";
						popUpMessage(msg);
					}
                }
            }
            else
            {
                popUpMessage(Utils.getTextFromFile("PleaseEnterFromDateAndToDate", mLang));
            }
        }
        else
        {
            popUpMessage(Utils.getTextFromFile("PleaseSaveTheGeneralPriceFirst", mLang));
        }

        loadCalendarWithoutPostBack();
    }

    protected void btSetAllocationsMonthly_Click(object sender, EventArgs e)
    {
        if (mHotelPrice != null)
        {
            if (!string.IsNullOrEmpty(txtFromDateSetRooms.Text) && !string.IsNullOrEmpty(txtToDateSetRooms.Text))
            {
                int sundayRooms, mondayRooms, tuesdayRooms, wednesdayRooms, thursdayRooms, fridayRooms, saturdayRooms;
                int sundayRoomsDisable = 0, mondayRoomsDisable = 0, tuesdayRoomsDisable = 0, wednesdayRoomsDisable = 0, thursdayRoomsDisable = 0, fridayRoomsDisable = 0, saturdayRoomsDisable = 0;
                string quickNum = string.Empty;

                if (isWeeklyAssignNumbersValid(txtRoomsSunday.Text, txtRoomsMonday.Text, txtRoomsTuesday.Text, txtRoomsWednesday.Text, txtRoomsThursday.Text, txtRoomsFriday.Text, txtRoomsSaturday.Text) &&
                    isWeeklyAssignNumbersValid(txtRoomsDisableSunday.Text, txtRoomsDisableMonday.Text, txtRoomsDisableTuesday.Text, txtRoomsDisableWednesday.Text, txtRoomsDisableThursday.Text, txtRoomsDisableFriday.Text, txtRoomsDisableSaturday.Text))
                {
                    DateTime currentDate, fromDate, toDate;

                    int.TryParse(txtRoomsSunday.Text, out sundayRooms);
                    int.TryParse(txtRoomsMonday.Text, out mondayRooms);
                    int.TryParse(txtRoomsTuesday.Text, out tuesdayRooms);
                    int.TryParse(txtRoomsWednesday.Text, out wednesdayRooms);
                    int.TryParse(txtRoomsThursday.Text, out thursdayRooms);
                    int.TryParse(txtRoomsFriday.Text, out fridayRooms);
                    int.TryParse(txtRoomsSaturday.Text, out saturdayRooms);

                    if (isDisablingOnSite)
                    {
                        int.TryParse(txtRoomsDisableSunday.Text, out sundayRoomsDisable);
                        int.TryParse(txtRoomsDisableMonday.Text, out mondayRoomsDisable);
                        int.TryParse(txtRoomsDisableTuesday.Text, out tuesdayRoomsDisable);
                        int.TryParse(txtRoomsDisableWednesday.Text, out wednesdayRoomsDisable);
                        int.TryParse(txtRoomsDisableThursday.Text, out thursdayRoomsDisable);
                        int.TryParse(txtRoomsDisableFriday.Text, out fridayRoomsDisable);
                        int.TryParse(txtRoomsDisableSaturday.Text, out saturdayRoomsDisable);
                    }

                    fromDate = DateTime.Parse(Utils.ConvertMonthWithSlashToAgencyFormat(txtFromDateSetRooms.Text));
                    currentDate = fromDate;
                    toDate = DateTime.Parse(Utils.ConvertMonthWithSlashToAgencyFormat(txtToDateSetRooms.Text));

					if ((toDate - fromDate).Days <= mDaysToBlockInDatePicker)
					{
						while (currentDate.Date != toDate.AddDays(1).Date)
						{
							if (isDayAssigend(currentDate, txtRoomsSunday.Text, txtRoomsMonday.Text, txtRoomsTuesday.Text, txtRoomsWednesday.Text, txtRoomsThursday.Text, txtRoomsFriday.Text, txtRoomsSaturday.Text) ||
								isDayAssigend(currentDate, txtRoomsDisableSunday.Text, txtRoomsDisableMonday.Text, txtRoomsDisableTuesday.Text, txtRoomsDisableWednesday.Text, txtRoomsDisableThursday.Text, txtRoomsDisableFriday.Text, txtRoomsDisableSaturday.Text))
							{
								//PricePerDay existedPricePerDay = MonthlyPricesPerDay.Find(x => x.mDate.Date == currentDate.Date);
								PricePerDay existedPricePerDay = Utils.getSpecificDatePriceDetails(mSupplierId, currentDate);
								if (existedPricePerDay != null)
								{
									if (isDayAssigend(currentDate, txtRoomsSunday.Text, txtRoomsMonday.Text, txtRoomsTuesday.Text, txtRoomsWednesday.Text, txtRoomsThursday.Text, txtRoomsFriday.Text, txtRoomsSaturday.Text))
									{
										existedPricePerDay.mAllocation.mRoomsAmount = getRoomForWeekDay(currentDate, sundayRooms, mondayRooms, tuesdayRooms, wednesdayRooms, thursdayRooms, fridayRooms, saturdayRooms);
									}

									if (isDisablingOnSite &&
										isDayAssigend(currentDate, txtRoomsDisableSunday.Text, txtRoomsDisableMonday.Text, txtRoomsDisableTuesday.Text, txtRoomsDisableWednesday.Text, txtRoomsDisableThursday.Text, txtRoomsDisableFriday.Text, txtRoomsDisableSaturday.Text))
									{
										existedPricePerDay.mAllocation.mRoomsDisable = getRoomForWeekDay(currentDate, sundayRoomsDisable, mondayRoomsDisable, tuesdayRoomsDisable, wednesdayRoomsDisable, thursdayRoomsDisable, fridayRoomsDisable, saturdayRoomsDisable);
									}

									mHotelPrice.savePricePerDay(existedPricePerDay);
								}
								else
								{
									PricePerDay newDatePrice = new PricePerDay();
									newDatePrice.mDate = currentDate;
									newDatePrice.mAllocation.mRoomsAmount = getRoomForWeekDay(currentDate, sundayRooms, mondayRooms, tuesdayRooms, wednesdayRooms, thursdayRooms, fridayRooms, saturdayRooms);
									if (isDisablingOnSite)
									{
										newDatePrice.mAllocation.mRoomsDisable = getRoomForWeekDay(currentDate, sundayRoomsDisable, mondayRoomsDisable, tuesdayRoomsDisable, wednesdayRoomsDisable, thursdayRoomsDisable, fridayRoomsDisable, saturdayRoomsDisable);
									}
									newDatePrice.mBasePrice.mPrice = 0;
									newDatePrice.mBasePrice.mPriceNetto = 0;
									newDatePrice.mBasePrice.mComposition = mHotelPrice.mBaseComposition;
									newDatePrice.mBasePrice.mBase = mHotelPrice.mBaseBase;
									newDatePrice.mBasePrice.mRoomType = mHotelPrice.mBaseRoomType;
									newDatePrice.mStatus = true;
									newDatePrice.mAllocation.mRoomsInUse = 0;
									newDatePrice.mColor = "white";

									mHotelPrice.savePricePerDay(newDatePrice);
								}
							}

							currentDate = currentDate.AddDays(1);
						}
					
						try
						{
							PriceDetailsWs mPriceWs = new PriceDetailsWs();
							mPriceWs.SetMonthlyAllocations(mSupplierId, MonthlyPricesPerDay);
							popUpMessage(Utils.getTextFromFile("SucceccSetRooms", mLang));
						}
						catch (Exception ex)
						{
							Logger.Log("Failed to update all prices.");
							popUpMessage("" + ex.Message);
						}
					}
					else
					{
						string msg = "ניתן לשנות מקסימום חודש";
						popUpMessage(msg);
					}
                }
            }
            else
            {
                popUpMessage(Utils.getTextFromFile("PleaseEnterFromDateAndToDate", mLang));
            }
        }
        else
        {
            popUpMessage(Utils.getTextFromFile("PleaseSaveTheGeneralPriceFirst", mLang));
        }

        loadCalendarWithoutPostBack();
    }

    private int getRoomForWeekDay(DateTime iCurrentDate, int iSundayRooms, int iMondayRooms, int iTuesdayRooms, int iWednesdayRooms, int iThursdayRooms, int iFridayRooms, int iSaturdayRooms)
    {
        int roomsToAssign = 0;

        switch (iCurrentDate.Date.DayOfWeek)
        {
            case DayOfWeek.Sunday:
                roomsToAssign = iSundayRooms;
                break;
            case DayOfWeek.Monday:
                roomsToAssign = iMondayRooms;
                break;
            case DayOfWeek.Tuesday:
                roomsToAssign = iTuesdayRooms;
                break;
            case DayOfWeek.Wednesday:
                roomsToAssign = iWednesdayRooms;
                break;
            case DayOfWeek.Thursday:
                roomsToAssign = iThursdayRooms;
                break;
            case DayOfWeek.Friday:
                roomsToAssign = iFridayRooms;
                break;
            case DayOfWeek.Saturday:
                roomsToAssign = iSaturdayRooms;
                break;
        }

        return roomsToAssign;
    }

    private decimal getPriceForWeekDay(DateTime iCurrentDate, decimal iSundayPrice, decimal iMondayPrice, decimal iTuesdayPrice, decimal iWednesdayPrice, decimal iThursdayPrice, decimal iFridayPrice, decimal iSaturdayPrice)
    {
        decimal priceToAssign = 0;

        switch (iCurrentDate.Date.DayOfWeek)
        {
            case DayOfWeek.Sunday:
                priceToAssign = iSundayPrice;
                break;
            case DayOfWeek.Monday:
                priceToAssign = iMondayPrice;
                break;
            case DayOfWeek.Tuesday:
                priceToAssign = iTuesdayPrice;
                break;
            case DayOfWeek.Wednesday:
                priceToAssign = iWednesdayPrice;
                break;
            case DayOfWeek.Thursday:
                priceToAssign = iThursdayPrice;
                break;
            case DayOfWeek.Friday:
                priceToAssign = iFridayPrice;
                break;
            case DayOfWeek.Saturday:
                priceToAssign = iSaturdayPrice;
                break;
        }

        return priceToAssign;
    }

    private bool isDayAssigend(DateTime iCurrentDate, string iSundayPrice, string iMondayPrice, string iTuesdayPrice, string iWednesdayPrice, string iThursdayPrice, string iFridayPrice, string iSaturdayPrice)
    {
        bool isDayPriceAsseiged = true;

        switch (iCurrentDate.Date.DayOfWeek)
        {
            case DayOfWeek.Sunday:
                isDayPriceAsseiged = !string.IsNullOrEmpty(iSundayPrice);
                break;
            case DayOfWeek.Monday:
                isDayPriceAsseiged = !string.IsNullOrEmpty(iMondayPrice);
                break;
            case DayOfWeek.Tuesday:
                isDayPriceAsseiged = !string.IsNullOrEmpty(iTuesdayPrice);
                break;
            case DayOfWeek.Wednesday:
                isDayPriceAsseiged = !string.IsNullOrEmpty(iWednesdayPrice);
                break;
            case DayOfWeek.Thursday:
                isDayPriceAsseiged = !string.IsNullOrEmpty(iThursdayPrice);
                break;
            case DayOfWeek.Friday:
                isDayPriceAsseiged = !string.IsNullOrEmpty(iFridayPrice);
                break;
            case DayOfWeek.Saturday:
                isDayPriceAsseiged = !string.IsNullOrEmpty(iSaturdayPrice);
                break;
        }

        return isDayPriceAsseiged;
    }

    private bool isWeeklyAssignNumbersValid(string iSundayPrice, string iMondayPrice, string iTuesdayPrice, string iWednesdayPrice, string iThursdayPrice, string iFridayPrice, string iSaturdayPrice)
    {
        decimal outDecimal = 0;
        bool isPricesValid = true;
        string msg = string.Empty;

        if (!decimal.TryParse(iSundayPrice, out outDecimal) && outDecimal < 0)
        {
            if (!string.IsNullOrEmpty(iSundayPrice))
            {
                isPricesValid = false;
                msg = Utils.getTextFromFile("CastSundayNumberError", mLang);
            }
        }
        else if (!decimal.TryParse(iMondayPrice, out outDecimal) && outDecimal < 0)
        {
            if (!string.IsNullOrEmpty(iMondayPrice))
            {
                isPricesValid = false;
                msg = Utils.getTextFromFile("CastMondayNumberError", mLang);
            }
        }
        else if ((!decimal.TryParse(iTuesdayPrice, out outDecimal) && outDecimal < 0))
        {
            if (!string.IsNullOrEmpty(iTuesdayPrice))
            {
                isPricesValid = false;
                msg = Utils.getTextFromFile("CastTuesdayNumberError", mLang);
            }
        }
        else if (!decimal.TryParse(iWednesdayPrice, out outDecimal) && outDecimal < 0)
        {
            if (!string.IsNullOrEmpty(iWednesdayPrice))
            {
                isPricesValid = false;
                msg = Utils.getTextFromFile("CastWednesdayNumberError", mLang);
            }
        }
        else if (!decimal.TryParse(iThursdayPrice, out outDecimal) && outDecimal < 0)
        {
            if (!string.IsNullOrEmpty(iThursdayPrice))
            {
                isPricesValid = false;
                msg = Utils.getTextFromFile("CastWednesdayNumberError", mLang);
            }
        }
        else if (!decimal.TryParse(iFridayPrice, out outDecimal) && outDecimal < 0)
        {
            if (!string.IsNullOrEmpty(iFridayPrice))
            {
                isPricesValid = false;
                msg = Utils.getTextFromFile("CastThursdayNumberError", mLang);
            }
        }
        else if (!decimal.TryParse(iSaturdayPrice, out outDecimal) && outDecimal < 0)
        {
            if (!string.IsNullOrEmpty(iSaturdayPrice))
            {
                isPricesValid = false;
                msg = Utils.getTextFromFile("CastFridayNumberError", mLang);
            }
        }

        if (!isPricesValid)
        {
            popUpMessage(msg);
        }

        return isPricesValid;
    }

    private bool validateDateSave()
    {
        bool isValid = true;
        string msg = string.Empty;
        decimal outDecimal;
        DateTime outDate;
        int outInt;
        string dateStr = selectedDay.Value + "-" + Utils.ConvertMonthToEngName(mMonth) + "-" + mYear;
        string[] salesCycles;

        if (!decimal.TryParse(txtBasePrice.Value, out outDecimal))
        {
            msg = Utils.getTextFromFile("CastBasePriceError", (eLanguage)int.Parse(mLangStr));
            popUpMessage(msg);
            isValid = false;
        }
        else
        {
            if (!DateTime.TryParse(dateStr, out outDate))
            {
                msg = Utils.getTextFromFile("CastDateError", (eLanguage)int.Parse(mLangStr));
                popUpMessage(msg);
                isValid = false;
            }
            else
            {
                if (ddlStatus.SelectedValue != "true" && ddlStatus.SelectedValue != "false")
                {
                    msg = Utils.getTextFromFile("CastStatusError", (eLanguage)int.Parse(mLangStr));
                    popUpMessage(msg);
                    isValid = false;
                }
                else
                {
                    if (!string.IsNullOrEmpty(txtSalesCycles.Value))
                    {
                        salesCycles = txtSalesCycles.Value.Split(',');
                        foreach (string saleCycle in salesCycles)
                        {
                            if (!int.TryParse(saleCycle, out outInt))
                            {
                                msg = Utils.getTextFromFile("CastSaleCyclesError", (eLanguage)int.Parse(mLangStr));
                                popUpMessage(msg);
                                isValid = false;
                            }
                        }
                    }
                    else
                    {
                        if (isUsingAllocations)
                        {
                            if (!int.TryParse(txtRoomsAmount.Value, out outInt))
                            {
                                msg = Utils.getTextFromFile("CastRoomsAmountError", (eLanguage)int.Parse(mLangStr));
                                ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + msg + "');", true);
                                isValid = false;
                            }
                            else
                            {
                                if (!int.TryParse(txtRoomsDisable.Value, out outInt))
                                {
                                    msg = Utils.getTextFromFile("CastRoomsAmountError", (eLanguage)int.Parse(mLangStr));
                                    ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + msg + "');", true);
                                    isValid = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        return isValid;
    }
    
	protected void btSaveSaleNightFree_Click(object sender, EventArgs e)
    {
		string fromDate = txtFromDateSaleNightFree.Text;
		string toDate = txtToDateSaleNightFree.Text;
		string nights = txtNights.Text;
		string nightsFree = txtNightsFree.Text;
		
		try
		{
			string query = string.Format(@"
INSERT INTO P_HOTEL_PRICE_SALES_FREE_NIGHTS (hotel_price_id, from_date, to_date, 
											 nights, nights_free, status)
									VALUES((SELECT id FROM P_HOTEL_PRICES WHERE supplier_id = {0}), '{1}', '{2}', 
											{3}, {4}, 'True')
", mSupplierId,
Utils.ConvertMonthWithSlashToAgencyFormat(fromDate), 
Utils.ConvertMonthWithSlashToAgencyFormat(toDate),
nights,
nightsFree);

			Logger.Log(query);
			DAL_SQL.RunSql(query);
			
		}
		catch(Exception ex)
		{
			popUpMessage("אירעה שגיאה בהכנסת מבצע" + ex.Message);
		}
		
		loadCalendarWithoutPostBack();
	}
	
    protected void btCloseDatesRange_Click(object sender, EventArgs e)
    {
        if (mHotelPrice != null)
        {
            if (!string.IsNullOrEmpty(txtFromDateToClose.Text) && !string.IsNullOrEmpty(txtToDateToClose.Text))
            {
                DateTime fromDate, toDate, currentDate;
                int i = 0;
                PricePerDay pricePerDay = null;
                List<PricePerDay> pricePerDayList = new List<PricePerDay>();
                bool status = bool.Parse((sender as Button).Attributes["status"]);

                try
                {
                    fromDate = DateTime.Parse(Utils.ConvertMonthWithSlashToAgencyFormat(txtFromDateToClose.Text));
                    currentDate = fromDate;
                    toDate = DateTime.Parse(Utils.ConvertMonthWithSlashToAgencyFormat(txtToDateToClose.Text));

					if ((toDate - fromDate).Days <= mDaysToBlockInDatePicker)
					{
						while (currentDate.Date != toDate.AddDays(1).Date)
						{
							pricePerDay = MonthlyPricesPerDay.Find(x => x.mDate.Date == currentDate.Date);
							if (pricePerDay != null)
							{
								pricePerDay.mStatus = status;
								pricePerDayList.Add(pricePerDay);
							}

							currentDate = currentDate.AddDays(1);
						}

						try
						{
							PriceDetailsWs mPriceWs = new PriceDetailsWs();
							mPriceWs.SetMonthlyPricePerDay(mSupplierId, pricePerDayList);
							if (status)
							{
								popUpMessage(Utils.getTextFromFile("SucceccOpenDates", mLang));
							}
							else
							{
								popUpMessage(Utils.getTextFromFile("SucceccCloseDates", mLang));
							}
						}
						catch (Exception ex)
						{
							Logger.Log("Failed to update all prices.");
							popUpMessage("" + ex.Message);
						}
					}
					else
					{
						string msg = "ניתן לשנות מקסימום חודש";
						popUpMessage(msg);
					}
                }
                catch (Exception ex)
                {
                    popUpMessage(Utils.getTextFromFile("PleaseSelectDateInFormat", mLang));
                }
            }
            else
            {
                popUpMessage(Utils.getTextFromFile("PleaseEnterFromDateAndToDate", mLang));
            }
        }
        else
        {
            popUpMessage(Utils.getTextFromFile("PleaseSaveTheGeneralPriceFirst", mLang));
        }

        loadCalendarWithoutPostBack();
    }

    protected void btSaveSalesCyclesMonthly_Click(object sender, EventArgs e)
    {
        if (mHotelPrice != null)
        {
            if (!string.IsNullOrEmpty(txtFromDateSetCycles.Text) && !string.IsNullOrEmpty(txtToDateSetCycles.Text))
            {
                string[] salesCyclesStr;
                int outInt;
                string msg = string.Empty;
                bool isValid = true;
				List<int>[] weeklySalesCycles = new List<int>[7];
                List<int> salesCycles = new List<int>();

				for (int i = 0; i < 7; i++)
				{
					salesCycles = new List<int>();
					if (!string.IsNullOrEmpty(Request.Form["ctl00$ContentPlaceHolder2$txtSaleCycle" + (i + 1)]))
					{
						salesCyclesStr = Request.Form["ctl00$ContentPlaceHolder2$txtSaleCycle" + (i + 1)].Split(',');
						foreach (string saleCycle in salesCyclesStr)
						{
							if (!int.TryParse(saleCycle, out outInt))
							{
								popUpMessage(Utils.getTextFromFile("CastSaleCyclesError", mLang));
								isValid = false;
								break;
							}

							salesCycles.Add(outInt);
						}
						
						//Response.Write("<br>-salesCyclesStr = " + string.Join(", ", salesCyclesStr) + ", i = " + i);
						weeklySalesCycles[i] = salesCycles;
					}
				}
				
                if (isValid)
                {
                    DateTime currentDate, fromDate, toDate;
                    PricePerDay existedPricePerDay = null;
                    List<PricePerDay> pricePerDayList = new List<PricePerDay>();
                    msg = string.Empty;
                    //DayOfWeek selectedDay = (DayOfWeek)int.Parse(ddlWeekDays.SelectedValue);
                    int daysToAdd = 1;
                
                    fromDate = DateTime.Parse(Utils.ConvertMonthWithSlashToAgencyFormat(txtFromDateSetCycles.Text));
                    //currentDate = fromDate;
                    toDate = DateTime.Parse(Utils.ConvertMonthWithSlashToAgencyFormat(txtToDateSetCycles.Text));
                
					if ((toDate - fromDate).Days <= mDaysToBlockInDatePicker)
					{
						for (int i = 0; i < 7; i++)
						{
							daysToAdd = 1;
							currentDate = fromDate;//.AddDays(i);
							DayOfWeek selectedDay = (DayOfWeek)int.Parse(i.ToString());
							if (weeklySalesCycles[i] != null)
							{
								while (currentDate.Date <= toDate.Date)
								{
									if (selectedDay == currentDate.DayOfWeek)
									{
										//Response.Write("<br>selectedDay = " + selectedDay);
										//existedPricePerDay = MonthlyPricesPerDay.Find(x => x.mDate.Date == currentDate.Date);
										existedPricePerDay = Utils.getSpecificDatePriceDetails(mSupplierId, currentDate);
										if (existedPricePerDay != null)
										{
											existedPricePerDay.mSalesCycles = weeklySalesCycles[i];
											mHotelPrice.savePricePerDay(existedPricePerDay);
											pricePerDayList.Add(existedPricePerDay);
										}
										else
										{
											if (string.IsNullOrEmpty(msg))
											{
												msg += Utils.getTextFromFile("PricePerDayNotExistsStart", mLang);
											}
						
											msg += "</br>* " + currentDate.ToString("dd/MM/yyyy");
										}
						
										daysToAdd = 7;
									}
						
									currentDate = currentDate.AddDays(daysToAdd);
								}
							}
						}
						
						try
						{
							if (pricePerDayList != null && pricePerDayList.Count > 0)
							{
								PriceDetailsWs mPriceWs = new PriceDetailsWs();
								mPriceWs.setMonthlySalesCycles(mSupplierId, pricePerDayList);
								if (string.IsNullOrEmpty(msg))
								{
									msg += "</br>";
								}
								msg += "</br>" + Utils.getTextFromFile("SalesCyclesMonthlySaved", mLang);
                
								foreach (PricePerDay datePrice in pricePerDayList)
								{
									msg += "</br>* " + datePrice.mDate.ToString("dd/MM/yyyy");
								}
							}
						}
						catch (Exception ex)
						{
							Logger.Log("Failed to update all salesCycles." + ex.Message);
							msg = "" + ex.Message;
						}
					}
					else
					{
						msg = "ניתן לשנות מקסימום חודש";
					}
                }
                
                popUpMessage(msg);
            }
            else
            {
                popUpMessage(Utils.getTextFromFile("PleaseEnterFromDateAndToDate", mLang));
            }
        }
        else
        {
            popUpMessage(Utils.getTextFromFile("PleaseSaveTheGeneralPriceFirst", mLang));
        }

        loadCalendarWithoutPostBack();
    }
    protected void btSaveMontlhyAllocation_Click(object sender, EventArgs e)
    {
        string msg;

        try
        {
            Utils.SaveMonthlyAllocation(mSupplierId, mMonth, mYear, txtMonthlyAllocation.Text);
            msg = Utils.getTextFromFile("SuccessSaveMonthlyAllocation", mLang);
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to save monthly allocation. exception = " + ex.Message);
            msg = Utils.getTextFromFile("FailedToUpdateMonthlyAllocation", mLang);
        }

        popUpMessage(msg);
        loadCalendarWithoutPostBack();
    }

    private void loadCalendarWithoutPostBack()
    {
        loadCalendar();
        //setMonthDays() won't be call in load calendar cause !page.IsPostBack
        setMonthDays();
		mSalesNightsFreeRows = getSalesFreeNight();
		
        MonthlyAllocation monthlyAllocaion;
        if (mHotelPrice != null)
        {
            monthlyAllocaion = mHotelPrice.mMonthlyAllocations.Find(x => x.mMonth == mMonth);
            if (monthlyAllocaion != null)
            {
                txtMonthlyAllocation.Text = monthlyAllocaion.mMonthlyAllocation.ToString();
            }
            else
            {
                txtMonthlyAllocation.Text = "0";
            }

            txtMonthlyAllocationSum.Text = getMonthlyAllocationSum().ToString();
            txtMonthlyAllocationUsed.Text = getMonthlyAllocationUsed().ToString();
        }
        else
        {
            txtMonthlyAllocation.Text = "0";
        }
    }

    private void popUpMessage(string msg)
    {
        ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + msg + "');", true);
    }

    protected void btSetGeneralDetails_Click(object sender, EventArgs e)
    {
        //btSetGeneralDetails
        Response.Redirect("HotelPriceGeneralDetails.aspx?month=" + mMonth + "&year=" + mYear + "&supplierId=" + mSupplierId + "&supplierName=" + mSupplierName + "&clerkId=" + mClerkId + "&areaId=" + mAreaId + "&areaName=" + mAreaName + "&SetGeneralDatails=true", false);
    }

    private void loadTable(bool isDateSelected, int iYear = -1)
    {
        DateTime current;
        DateTime currentDate;
        HashSet<DateTime> workingDates;
        Dictionary<DateTime, int> dateToinformedStatus;
        TableCell dayAndMonthHeader;
        List<TableCell> headersCells;
        int sixWeeksAmount;
        int WeekAmount;
        int nextMonth;
        int prevMonth;
        int WeekdaysCounter;
        DayOfWeek currentDay;
        TableCell currDayCell;
        bool isReachedDate;
        int monthDaysCounter;
        DayOfWeek start;
        List<TableCell> monthLine;
        TableCell monthName;
        int hotelPriceId = int.Parse(Utils.getHotelPriceId(Request.QueryString["supplierId"]));
        Dictionary<string, string> namesToColors = new Dictionary<string, string>();
        fillColorMap(ref namesToColors, Request.QueryString["supplierId"]);
        DateTime currentYear;
        DateTime nextYear;


        if (!isDateSelected)
        {
            currentDate = DateTime.Now;
            ddlYearsTable.Items.Add((currentDate.Year - 2).ToString());
            ddlYearsTable.Items.Add((currentDate.Year - 1).ToString());
            ddlYearsTable.Items.Add((currentDate.Year).ToString());
            ddlYearsTable.Items.Add((currentDate.Year + 1).ToString());
            ddlYearsTable.Items.Add((currentDate.Year + 2).ToString());
            ddlYearsTable.Items.Add((currentDate.Year + 3).ToString());
            ddlYearsTable.SelectedIndex = 2;
            current = new DateTime(currentDate.Year, 1, 1);
        }
        else
        {
            current = new DateTime(int.Parse(ddlYearsTable.SelectedValue), 1, 1);
        }

        currentYear = new DateTime(current.Year, 1, 1);
        nextYear = new DateTime(current.Year + 1, 1, 1);

        DataSet colorMap = DAL_SQL_Helper.getYearBaseByHotelId(hotelPriceId, currentYear, nextYear);

        workingDates = new HashSet<DateTime>();
        dateToinformedStatus = new Dictionary<DateTime, int>();
        dayAndMonthHeader = Utils.createLabelInCell("");
        headersCells = new List<TableCell>();
        dayAndMonthHeader.Style.Add("width", "8em");
        headersCells.Add(dayAndMonthHeader);
        sixWeeksAmount = 38;

        for (int daysCounterTotal = 0; daysCounterTotal < sixWeeksAmount;)
        {
            WeekAmount = 6;
            for (int daysCounter = 0; daysCounter <= WeekAmount && daysCounterTotal <= sixWeeksAmount; daysCounter++)
            {
                currentDay = (DayOfWeek)daysCounter;
                currDayCell = Utils.createLabelInCell(Utils.GetDayHebrewLatter(currentDay));
                currDayCell.Style.Add("width", "2em");
                currDayCell.Style.Add("border-bottom-width", "3px");
                currDayCell.CssClass = "td_cell";
                headersCells.Add(currDayCell);
                daysCounterTotal++;
            }
        }

        Utils.AddControlsToTable(headersCells, ref BasesTable);
        nextMonth = 2;
        prevMonth = current.Month;
        WeekdaysCounter = 0;

        int currentYearInt = current.Year;

        for (int monthCount = 0; monthCount < 12;)
        {
            isReachedDate = false;
            monthDaysCounter = 0;
            start = current.DayOfWeek;
            monthLine = new List<TableCell>();
            monthName = Utils.createLabelInCell(Utils.ConvertMonthToName(current.Month));
            monthName.Style.Add("border-left-width", "3px");
            monthName.Style.Add("text-align", "center");
            monthLine.Add(monthName);
            currDayCell = null;

            while (prevMonth != nextMonth)
            {
                if(current.Month == 12 && current.Day == 31)
                {
                    break;
                }
                if (WeekdaysCounter == 7)
                {
                    if (isReachedDate)
                    {
                        start = DayOfWeek.Sunday;
                    }
                    WeekdaysCounter = 0;
                }

                currentDay = (DayOfWeek)WeekdaysCounter;

                if (currentDay != start)

                {
                    currDayCell = Utils.createLabelInCell("");
                    isReachedDate = false;
                }
                else
                {
                    currDayCell = Utils.createLabelInCell(current.Day.ToString());
                    if (Utils.isDataSetRowsNotEmpty(colorMap))
                    {

                        DataRow[] choosenRow = colorMap.Tables[0].Select("date=" + string.Format("'{0}'", current.ToString()));
                        if (choosenRow.Length > 0)
                        {
                            HtmlGenericControl divToAppend = new HtmlGenericControl("div");
                            for (int baseCounter = 0; baseCounter < choosenRow.Length; baseCounter++)
                            {
                                LiteralControl Label = null;
                                string baseName = choosenRow[baseCounter]["base_id"].ToString();
                                if (baseCounter == 0)
                                {

                                    Label = new LiteralControl(string.Format("<span class='base_circle' style='background-color:#{0}'></span>", namesToColors[DAL_SQL_Helper.getBasesName(baseName)]));
                                }
                                else
                                {

                                    Label = new LiteralControl(string.Format("<span class='base_circle' style='background-color:#{0}'></span>", namesToColors[DAL_SQL_Helper.getBasesName(baseName)]));
                                }
                                divToAppend.Controls.Add(Label);
                                divToAppend.Attributes["class"] = "base_container";
                            }
                            currDayCell.Controls.Add(divToAppend);
                        }
                    }
                    current = current.AddDays(1);
                    start = (DayOfWeek)(WeekdaysCounter + 1);
                    isReachedDate = true;

                }

                currDayCell.Style.Add("width", "2em");
                currDayCell.CssClass = "td_cell";
                monthLine.Add(currDayCell);
                WeekdaysCounter++;
                monthDaysCounter++;

                if (prevMonth < current.Month)
                {
                    prevMonth++;
                }
            }

            while (monthDaysCounter <= sixWeeksAmount)
            {
                currDayCell = Utils.createLabelInCell("");
                currDayCell.Style.Add("width", "2em");
                currDayCell.CssClass = "td_cell";
                monthLine.Add(currDayCell);
                monthDaysCounter++;
            }

            Utils.AddControlsToTable(monthLine, ref BasesTable);

            WeekdaysCounter = 0;
            nextMonth++;
            monthCount++;

        }
    }
    private List<string> baseIDs()
    {
        List<string> retVal = new List<string>();

        for(int baseId = 1; baseId < 7; baseId++)
        {
            retVal.Add(baseId.ToString());
        }

        retVal.Add("21");
        retVal.Add("25");
        retVal.Add("26");

        for (int baseId = 37; baseId < 46; baseId++)
        {
            retVal.Add(baseId.ToString());
        }
        return retVal;
    }

    private void fillColorMap(ref Dictionary<string,string> namesToColors, string iSupplierId)
    {
        Dictionary<string, string> baseNames = DAL_SQL_Helper.getBasesNames(iSupplierId);
        Random random = new Random();
        HtmlGenericControl div = new HtmlGenericControl();
        div.Attributes["class"] = "colorMapSample";
        int colorIndex = 0;

        foreach (string key in baseNames.Keys)
        {
            baseNamesToids[baseNames[key]] = key;
            ListItem currItem = new ListItem();
            currItem.Value = baseNames[key];
            ddlGenericSelect.Items.Add(currItem);
            int rand1 = random.Next(255 - colorIndex * 2);
            int rand2 = random.Next(255 - colorIndex * 3);
            int rand3 = random.Next(255 - colorIndex * 4);
            Color myColor = Color.FromArgb(rand1, rand2, rand3);
            string hex = myColor.R.ToString("X2") + myColor.G.ToString("X2") + myColor.B.ToString("X2");
            namesToColors[baseNames[key]] = hex;
            LiteralControl color = new LiteralControl(string.Format("<label class='labelSpace' for='head'>{0}</label>", baseNames[key]));
            LiteralControl Label = new LiteralControl(string.Format("<input type='color' id='head' name='head' disabled value='#{0}' />", hex));
            div.Controls.Add(color);
            div.Controls.Add(Label);
            divColorMap.Controls.Add(div);
            colorIndex++;
        }

        
    }

    private bool isValidDetails(out string ioMessage)
    {
        bool isValid = true;
        ioMessage = "שגיאת מילוי פרטים: ";


        bool fromDateBool = DateTime.TryParseExact(txtFromDate.Text, "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out fromDate);
        bool toDateBool = DateTime.TryParseExact(txtToDate.Text, "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out toDate);

        if (!fromDateBool)
        {
            ioMessage += "תאריך ההתחלה שגוי,";
            isValid = false;
        }

        if (!toDateBool)
        {
            ioMessage += "תאריך סיום שגוי, ";
            isValid = false;
        }

        if(fromDate > toDate)
        {
            ioMessage +=  "תאריך הסיום קטן מתאריך ההתחלה";
            isValid = false;
        }

        //Remove comma at the end
        if (ioMessage.Length >= 2)
        {
            ioMessage = ioMessage.Remove(ioMessage.Length - 2);
        }


        return isValid;
    }

    protected void saveDetails_Click(object sender, EventArgs e)
    {
        string message;
        if (isValidDetails(out message))
        {
            try
            {
                int hotelPriceId = int.Parse(Utils.getHotelPriceId(Request.QueryString["supplierId"]));
                int baseId = int.Parse(baseNamesToids[ddlGenericSelect.SelectedValue]);
                DAL_SQL_Helper.SaveAndUpdateDetails(hotelPriceId, baseId, fromDate, toDate);
            }
            catch (Exception ex)
            {
                Logger.Log("error happend " + ex.Message);
            }
        }

        else
        {
            popUpMessage(message);
            
        }

        Page.Response.Redirect(Page.Request.Url.ToString(), false);
    }



    protected void deleteDetails_Click(object sender, EventArgs e)
    {
        string message;
        if (isValidDetails(out message))
        {
            try
            {
                int hotelPriceId = int.Parse(Utils.getHotelPriceId(Request.QueryString["supplierId"]));
                int baseId = int.Parse(baseNamesToids[ddlGenericSelect.SelectedValue]);
                DAL_SQL_Helper.DelteDetails(hotelPriceId, baseId, fromDate, toDate);
            }
            catch (Exception ex)
            {
                Logger.Log("error happend " + ex.Message);
            }
        }

        else
        {
            popUpMessage(message);

        }

        Page.Response.Redirect(Page.Request.Url.ToString(), false);
    }

    protected void ddlYearsTable_SelectedIndexChanged(object sender, EventArgs e)
    {
        loadTable(true);
    }
}
