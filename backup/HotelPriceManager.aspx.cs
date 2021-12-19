//using AgencyPricesWS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

public partial class HotelPriceManager : System.Web.UI.Page
{
    PriceDetailsWs mPriceWs = new PriceDetailsWs();

    //TODO:
    private bool isUsingAllocations = true;
    //TODO:
    private bool isDisablingOnSite = true;
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

    protected void Page_Load(object sender, EventArgs e)
    {
        ListItem liTemp;
        mLang = (eLanguage)int.Parse(mLangStr);

        setPriveteMembers();

        if (!Page.IsPostBack)
        {
            mYear = int.Parse(Request.QueryString["year"]);
            mMonth = int.Parse(Request.QueryString["month"]);
            hiddenMonth.Value = mMonth.ToString();
            hiddenYear.Value = mYear.ToString();

            liTemp = new ListItem() { Text = Utils.getTextFromFile("Active", mLang), Value = "true" };
            ddlStatus.Items.Add(liTemp);
            ddlPriceStatus.Items.Add(liTemp);
            liTemp = new ListItem() { Text = Utils.getTextFromFile("InActive", mLang), Value = "false" };
            ddlStatus.Items.Add(liTemp);
            ddlPriceStatus.Items.Add(liTemp);
            if (mHotelPrice != null)
            {
                ddlPriceStatus.SelectedValue = (mHotelPrice.mStatus) ? "true" : "false";
            }
            else
            {
                ddlPriceStatus.SelectedValue = "true";
            }
            ddlPriceStatus.SelectedIndex = 0;

            liTemp = new ListItem() { Text = Utils.getTextFromFile("Without", mLang), Value = "0" };
            ddlPriceColor.Items.Add(liTemp);
            liTemp = new ListItem() { Text = Utils.getTextFromFile("PriceColorGreen", mLang), Value = "Green" };
            ddlPriceColor.Items.Add(liTemp);
            liTemp = new ListItem() { Text = Utils.getTextFromFile("PriceColorYellow", mLang), Value = "#ecd06f" };
            ddlPriceColor.Items.Add(liTemp);
            liTemp = new ListItem() { Text = Utils.getTextFromFile("PriceColorRed", mLang), Value = "Red" };
            ddlPriceColor.Items.Add(liTemp);

            liTemp = new ListItem() { Text = Utils.getTextFromFile("Without", mLang), Value = "0" };
            ddlColors.Items.Add(liTemp);
            liTemp = new ListItem() { Text = Utils.getTextFromFile("PriceColorGreen", mLang), Value = "Green" };
            ddlColors.Items.Add(liTemp);
            liTemp = new ListItem() { Text = Utils.getTextFromFile("PriceColorYellow", mLang), Value = "#ecd06f" };
            ddlColors.Items.Add(liTemp);
            liTemp = new ListItem() { Text = Utils.getTextFromFile("PriceColorRed", mLang), Value = "Red" };
            ddlColors.Items.Add(liTemp);

            ddlWeekDays.Items.Add(new ListItem(Utils.getTextFromFile("Sunday", mLang), "0"));
            ddlWeekDays.Items.Add(new ListItem(Utils.getTextFromFile("Monday", mLang), "1"));
            ddlWeekDays.Items.Add(new ListItem(Utils.getTextFromFile("Tuesday", mLang), "2"));
            ddlWeekDays.Items.Add(new ListItem(Utils.getTextFromFile("Wednesday", mLang), "3"));
            ddlWeekDays.Items.Add(new ListItem(Utils.getTextFromFile("Thursday", mLang), "4"));
            ddlWeekDays.Items.Add(new ListItem(Utils.getTextFromFile("Friday", mLang), "5"));
            ddlWeekDays.Items.Add(new ListItem(Utils.getTextFromFile("Saturday", mLang), "6"));

            btSave.Text = Utils.getTextFromFile("Save", mLang);
            btSaveHotelPrice.Text = Utils.getTextFromFile("SaveHotelPrice", mLang);
            btBackToMenu.Text = Utils.getTextFromFile("BackToMenu", mLang);
            btSetPricesMonthly.Text = Utils.getTextFromFile("CopyToAllMonth", mLang);
            btCloseDatesRange.Text = Utils.getTextFromFile("CloseDatesRange", mLang);
            btOpenDatesRange.Text = Utils.getTextFromFile("OpenDatesRange", mLang);
            btSaveSalesCyclesMonthly.Text = Utils.getTextFromFile("SaveSalesCycles", mLang);
            btSetAllocationsMonthly.Text = Utils.getTextFromFile("SaveAllocationMonthly", mLang);

            rbAmount.Text = Utils.getTextFromFile("BaseAmount", mLang);
            rbPercent.Text = Utils.getTextFromFile("BasePercent", mLang);
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

        //Preperations
        loadAllComboTypes();

        mMonth = int.Parse(hiddenMonth.Value);
        mYear = int.Parse(hiddenYear.Value);

        //Load calendar
        loadCalendar();
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

        attachComboDetails();
    }

    //HotelPrice
    private void loadHotelPrice()
    {
        MonthlyAllocation monthlyAllocation;

        mHotelPrice = mPriceWs.getHotelPrice(mSupplierId, mMonth, mYear);
        if (!Page.IsPostBack)
        {
            loadHotelPriceCombos();
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

        if (mHotelPrice != null)
        {
            rbAmount.Checked = mHotelPrice.mIsBaseAmount;
            //If monthly allocation already assigned.

            //Set price base combo
            lblBaseComposition.Text = mHotelPrice.mBaseComposition.getName();
            lblBaseBase.Text = mHotelPrice.mBaseBase.getName();
            lblBaseRoomType.Text = mHotelPrice.mBaseRoomType.getName();
            hiddenBaseCompositionId.Value = mHotelPrice.mBaseComposition.getId();
            hiddenBaseBaseId.Value = mHotelPrice.mBaseBase.getId();
            hiddenBaseRoomTypeId.Value = mHotelPrice.mBaseRoomType.getId();

            txtSiteDiscount.Value = mHotelPrice.mSiteDiscount.ToString();
            txtOfficeDiscount.Value = mHotelPrice.mOfficeDiscount.ToString();
            txtCommission.Value = mHotelPrice.mCommission.ToString();
            txtGeneralAdditive.Value = mHotelPrice.mGeneralAdditive.ToString();
            txtBabiesAdditive.Value = mHotelPrice.mBabiesAdditive.ToString();

            ddlPriceStatus.SelectedValue = (mHotelPrice.mStatus) ? "true" : "false";

            //Extra gui stuff
            ddlPriceColor.SelectedValue = mHotelPrice.mPriceColor;
        }
		else
		{
			txtSiteDiscount.Value = "0";
            txtOfficeDiscount.Value = "0";
            txtCommission.Value = "0";
            txtGeneralAdditive.Value = "0";
            txtBabiesAdditive.Value = "0";

		}
    }
	
	private int getMonthlyAllocationSum()
	{
		int sum = 0;
		
		foreach(PricePerDay pricePerDay in MonthlyPricesPerDay)
		{
			sum += pricePerDay.mAllocation.mRoomsAmount;
		}
		
		return sum;
	}
	
	private int getMonthlyAllocationUsed()
	{
		int sum = 0;
		
		foreach(PricePerDay pricePerDay in MonthlyPricesPerDay)
		{
			sum += pricePerDay.mAllocation.mRoomsInUse;
		}
		
		return sum;
	}

    private void loadHotelPriceCombos()
    {
        if (mHotelPrice != null)
        {
            setHotelPriceComboDetailsByComboType(eComboType.Composition, mHotelPrice.mCompositions.Keys.Cast<ComboDetails>().ToList(), chkCompositions);
            setHotelPriceComboDetailsByComboType(eComboType.Base, mHotelPrice.mBases.Keys.Cast<ComboDetails>().ToList(), chkBases);
            setHotelPriceComboDetailsByComboType(eComboType.RoomType, mHotelPrice.mRoomTypes.Keys.Cast<ComboDetails>().ToList(), chkRoomTypes);
        }
        else
        {
            setHotelPriceComboDetailsByComboType(eComboType.Composition, null, chkCompositions);
            setHotelPriceComboDetailsByComboType(eComboType.Base, null, chkBases);
            setHotelPriceComboDetailsByComboType(eComboType.RoomType, null, chkRoomTypes);
        }
    }

    private void setHotelPriceComboDetailsByComboType(eComboType iComboType, List<ComboDetails> iComboList, CheckBoxList iCheckList)
    {
        ListItem listItem = null;

        if (iComboList != null)
        {
            foreach (ComboDetails comboType in iComboList)
            {
                listItem = iCheckList.Items.FindByValue(comboType.getId());
                switch (iComboType)
                {
                    case eComboType.Composition:
                        if (listItem != null)
                        {
                            listItem.Attributes.Add("onclick", "addComboList('composition', " + listItem.Value + ", '" + listItem.Text + "', '" + (comboType as Composition).mPercent + "', this)");
                            if (mHotelPrice.mCompositions[(comboType as Composition)] == true)
                            {
                                listItem.Selected = true;
                            }

                            if (listItem.Value == mHotelPrice.mBaseComposition.getId())
                            {
                                chkBaseComposition.Items.FindByValue(listItem.Value).Attributes.Add("onclick", "changeBaseCombo('composition', '" + listItem.Value + "','" + listItem.Text + "', this)");
                                chkBaseComposition.Items.FindByValue(listItem.Value).Selected = true;
                            }
                        }

                        break;
                    case eComboType.Base:
                        if (listItem != null)
                        {
                            //(comboType as Base).mPercent == 0 means that using amount
                            if ((comboType as Base).mPercent == 0)
                            {
                                listItem.Attributes.Add("onclick", "addComboList('base', " + listItem.Value + ", '" + listItem.Text + "', '" + (comboType as Base).mAmount + "', this)");
                            }
                            else
                            {
                                listItem.Attributes.Add("onclick", "addComboList('base', " + listItem.Value + ", '" + listItem.Text + "', '" + (comboType as Base).mPercent + "', this)");
                            }

                            if (mHotelPrice.mBases[(comboType as Base)] == true)
                            {
                                listItem.Selected = true;
                            }

                            if (listItem.Value == mHotelPrice.mBaseBase.getId())
                            {
                                chkBaseBase.Items.FindByValue(listItem.Value).Attributes.Add("onclick", "changeBaseCombo('base', '" + listItem.Value + "','" + listItem.Text + "', this)");
                                chkBaseBase.Items.FindByValue(listItem.Value).Selected = true;
                            }
                        }

                        break;
                    case eComboType.RoomType:
                        if (listItem != null)
                        {
                            listItem.Attributes.Add("onclick", "addComboList('roomType', " + listItem.Value + ", '" + listItem.Text + "', '" + (comboType as RoomType).mAmount + "', this)");
                            if (mHotelPrice.mRoomTypes[(comboType as RoomType)] == true)
                            {
                                listItem.Selected = true;
                            }

                            if (listItem.Value == mHotelPrice.mBaseRoomType.getId())
                            {
                                chkBaseRoomType.Items.FindByValue(listItem.Value).Attributes.Add("onclick", "changeBaseCombo('roomType', '" + listItem.Value + "','" + listItem.Text + "', this)");
                                chkBaseRoomType.Items.FindByValue(listItem.Value).Selected = true;
                            }
                        }

                        break;
                }
            }
        }

        //Update onclick for the not selected listitems
        foreach (ListItem li in iCheckList.Items)
        {
            switch (iComboType)
            {
                case eComboType.Composition:
                    if (!li.Selected)
                    {
                        li.Attributes.Add("onclick", "addComboList('composition', " + li.Value + ", '" + li.Text + "', '0', this)");
                    }

                    chkBaseComposition.Items.FindByValue(li.Value).Attributes.Add("onclick", "changeBaseCombo('composition', '" + li.Value + "', '" + li.Text + "', this)");
                    break;
                case eComboType.Base:
                    if (!li.Selected)
                    {
                        li.Attributes.Add("onclick", "addComboList('base', " + li.Value + ", '" + li.Text + "', '0', this)");
                    }

                    chkBaseBase.Items.FindByValue(li.Value).Attributes.Add("onclick", "changeBaseCombo('base', '" + li.Value + "', '" + li.Text + "', this)");
                    break;
                case eComboType.RoomType:
                    if (!li.Selected)
                    {
                        li.Attributes.Add("onclick", "addComboList('roomType', " + li.Value + ", '" + li.Text + "', '0', this)");
                    }

                    chkBaseRoomType.Items.FindByValue(li.Value).Attributes.Add("onclick", "changeBaseCombo('roomType', '" + li.Value + "', '" + li.Text + "', this)");
                    break;
            }
        }
    }

    private void attachComboDetails()
    {
        if (mHotelPrice != null)
        {
            divCompositionsDetails.Controls.Clear();
            divBasesDetails.Controls.Clear();
            divRoomTypesDetails.Controls.Clear();

            setComboTypeDetails(eComboType.Composition, mHotelPrice.mCompositions.Keys.Cast<ComboDetails>().ToList(), chkCompositions);
            setComboTypeDetails(eComboType.Base, mHotelPrice.mBases.Keys.Cast<ComboDetails>().ToList(), chkBases);
            setComboTypeDetails(eComboType.RoomType, mHotelPrice.mRoomTypes.Keys.Cast<ComboDetails>().ToList(), chkRoomTypes);
        }
    }

    private void setComboTypeDetails(eComboType iComboType, List<ComboDetails> iComboList, CheckBoxList iCheckList)
    {
        int pixelsToMarginTopFinal = 0;
        const int pixelsToMarginTop = 8;
        string lblHiddenId;

        foreach (ComboDetails comboType in iComboList)
        {
            switch (iComboType)
            {
                case eComboType.Composition:
                    Composition composition = (comboType as Composition);
                    string txtCompId = "txtComposition_" + composition.getId();
                    string lblCompId = "lblComposition_" + composition.getId();
                    lblHiddenId = "hiddenCompositionPercent_" + composition.getId();

                    if (mHotelPrice.mCompositions[(composition)] == true)
                    {
                        pixelsToMarginTopFinal = pixelsToMarginTop * (divCompositionsDetails.Controls.Count / 4);
                        Label lblCompositionName = new Label() { ID = lblCompId, Text = comboType.getName() };
                        lblCompositionName.Style.Add("font-size", "13px");
                        lblCompositionName.Style.Add("float", "right");
                        lblCompositionName.Style.Add("margin-right", "3px");
                        lblCompositionName.Style.Add("margin-top", "-" + pixelsToMarginTopFinal + "px");
                        lblCompositionName.Style.Add("margin-top", "-" + pixelsToMarginTopFinal + "px");
                        lblCompositionName.Style.Add("direction", "rtl");
                        lblCompositionName.Style.Add("text-align", "center");
                        lblCompositionName.Style.Add("width", "30%");

                        TextBox txtCompositionPercent = new TextBox() { ID = txtCompId, Text = composition.mPercent.ToString() };
                        txtCompositionPercent.Style.Add("float", "left");
						txtCompositionPercent.Style.Add("width", "25%");
                        txtCompositionPercent.Style.Add("margin-top", "-" + pixelsToMarginTopFinal + "px");
                        txtCompositionPercent.Attributes.Add("isNumberKey", "isNumberKey");

                        divCompositionsDetails.Controls.Add(lblCompositionName);
                        divCompositionsDetails.Controls.Add(txtCompositionPercent);
                        divCompositionsDetails.Controls.Add(new LiteralControl("<br />"));
                        divCompositionsDetails.Controls.Add(new LiteralControl("<br />"));
                    }

                    break;

                case eComboType.Base:
                    Base baseItem = (comboType as Base);
                    string txtBaseId = "txtBase_" + baseItem.getId();
                    string lblBaseId = "lblBase_" + baseItem.getId();
                    lblHiddenId = "hiddenBaseAmount_" + baseItem.getId();

                    if (mHotelPrice.mBases[(baseItem)] == true)
                    {
                        pixelsToMarginTopFinal = pixelsToMarginTop * (divBasesDetails.Controls.Count / 4);
                        Label lblBaseName = new Label() { ID = lblBaseId, Text = baseItem.getName() };
                        lblBaseName.Style.Add("font-size", "13px");
                        lblBaseName.Style.Add("float", "right");
                        lblBaseName.Style.Add("margin-right", "3px");
                        lblBaseName.Style.Add("margin-top", "-" + pixelsToMarginTopFinal + "px");
                        lblBaseName.Style.Add("direction", "rtl");
                        lblBaseName.Style.Add("text-align", "center");
                        lblBaseName.Style.Add("width", "30%");

                        TextBox txtBaseAmount = new TextBox() { ID = txtBaseId };
                        if (mHotelPrice.mIsBaseAmount)
                        {
                            txtBaseAmount.Text = baseItem.mAmount.ToString();
                        }
                        else
                        {
                            txtBaseAmount.Text = baseItem.mPercent.ToString();
                        }

                        txtBaseAmount.Style.Add("float", "left");
						txtBaseAmount.Style.Add("width", "25%");
                        txtBaseAmount.Style.Add("margin-top", "-" + pixelsToMarginTopFinal + "px");
                        txtBaseAmount.Attributes.Add("isNumberKey", "isNumberKey");

                        divBasesDetails.Controls.Add(lblBaseName);
                        divBasesDetails.Controls.Add(txtBaseAmount);
                        divBasesDetails.Controls.Add(new LiteralControl("<br />"));
                        divBasesDetails.Controls.Add(new LiteralControl("<br />"));
                    }

                    break;

                case eComboType.RoomType:
                    RoomType roomType = (comboType as RoomType);
                    string txtRoomTypeId = "txtRoomType_" + roomType.getId();
                    string lblRoomTypeId = "lblRoomType_" + roomType.getId();
                    lblHiddenId = "hiddenRoomTypeAmount_" + roomType.getId();

                    if (mHotelPrice.mRoomTypes[(roomType)] == true)
                    {
                        pixelsToMarginTopFinal = pixelsToMarginTop * (divRoomTypesDetails.Controls.Count / 4);
                        Label lblRoomTypeName = new Label() { ID = lblRoomTypeId, Text = roomType.getName() };
                        lblRoomTypeName.Style.Add("font-size", "13px");
                        lblRoomTypeName.Style.Add("float", "right");
                        lblRoomTypeName.Style.Add("margin-right", "3px");
                        lblRoomTypeName.Style.Add("margin-top", "-" + pixelsToMarginTopFinal + "px");
                        lblRoomTypeName.Style.Add("direction", "rtl");
                        lblRoomTypeName.Style.Add("text-align", "center");
                        lblRoomTypeName.Style.Add("width", "30%");

                        TextBox txtRoomTypePercent = new TextBox() { ID = txtRoomTypeId, Text = roomType.mAmount.ToString() };
                        txtRoomTypePercent.Style.Add("float", "left");
						txtRoomTypePercent.Style.Add("width", "25%");
                        txtRoomTypePercent.Style.Add("margin-top", "-" + pixelsToMarginTopFinal + "px");
                        txtRoomTypePercent.Attributes.Add("isNumberKey", "isNumberKey");

                        divRoomTypesDetails.Controls.Add(lblRoomTypeName);
                        divRoomTypesDetails.Controls.Add(txtRoomTypePercent);
                        divRoomTypesDetails.Controls.Add(new LiteralControl("<br />"));
                        divRoomTypesDetails.Controls.Add(new LiteralControl("<br />"));
                    }

                    break;
            }
        }
    }

    public void getDatePriceCombosPrices(DateTime iDate)
    {
        XmlDocument dateComboPricesXml;

        dateComboPricesXml = Utils.getDateCombosPrices(mHotelPrice, iDate);

        if (dateComboPricesXml != null && dateComboPricesXml.SelectNodes("/Root/DateCombosPrices") != null)
        {
            foreach (XmlNode node in dateComboPricesXml.SelectNodes("/Root/DateCombosPrices"))
            {
                Label dateCombosPrices = new Label();
                dateCombosPrices.Text = node["ComboPrice"].InnerXml + " - " + node["CompositionId"].InnerXml + " ," + node["BaseId"].InnerXml + " ," + node["RoomTypeId"].InnerXml + "</br>";
                dateCombosPrices.Style.Add("display", "none");
                dateCombosPrices.CssClass = "displayNone displayNone_" + iDate.Day;

                divCombosPrices.Controls.Add(dateCombosPrices);
            }
        }
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
                    dateCell.Controls.Add(myImage);

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

    private void loadAllComboTypes()
    {
        //TODO: Get by specific hotel.
        getAllComboDetailByComboType(eComboType.Composition);
        getAllComboDetailByComboType(eComboType.Base);
        getAllComboDetailByComboType(eComboType.RoomType);
    }

    private void getAllComboDetailByComboType(eComboType iComboType)
    {
        string query = string.Empty;
        CheckBoxList chkList = null;
        CheckBoxList chkBaseList = null;
        string comboTypeStr = string.Empty;
        string funcString = string.Empty;

        switch (iComboType)
        {
            case eComboType.Composition:
                //query = "SELECT * FROM Agency_Admin.dbo.HOTEL_ROOM_TYPE WHERE isDisabled = 0";
                query = "SELECT room_id as id FROM SUPPLIER_TO_ROOMS_PERCENT WHERE supplier_id = " + mSupplierId;
                comboTypeStr = "composition";
                chkList = chkCompositions;
                chkBaseList = chkBaseComposition;
                break;
            case eComboType.Base:
                query = "SELECT * FROM Agency_Admin.dbo.HOTEL_ON_BASE WHERE isDisabled = 0";
                comboTypeStr = "base";
                chkList = chkBases;
                chkBaseList = chkBaseBase;
                break;
            case eComboType.RoomType:
                //query = "SELECT * FROM Agency_Admin.dbo.SUPPLIERS_HOTEL_ADDS WHERE status = 1";
                query = string.Format(@" SELECT    S_T_A.add_id as id, S_A.name as name_{0}, S_A.description,
												   S_T_A.status as supp_add_status, S_A.status as add_status 
										 FROM	   AGENCY_ADMIN.dbo.SUPPLIERS_TO_HOTEL_ADDS S_T_A INNER JOIN 
												   AGENCY_ADMIN.dbo.SUPPLIERS_HOTEL_ADDS S_A ON S_T_A.add_id = S_A.id 
										 WHERE	   S_T_A.status = 1 AND S_T_A.supplier_id = {1}", mLangStr, mSupplierId);
                comboTypeStr = "roomType";
                chkList = chkRoomTypes;
                chkBaseList = chkBaseRoomType;
                break;
        }

        DataSet ds = DAL_SQL.RunSqlDataSet(query);

        if (Utils.isDataSetRowsNotEmpty(ds))
        {
            ListItem ddlItem, temp = null;

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                switch (iComboType)
                {
                    case eComboType.Composition:
                        temp = chkCompositions.Items.FindByValue(row["id"].ToString());
                        break;
                    case eComboType.Base:
                        temp = chkBases.Items.FindByValue(row["id"].ToString());
                        break;
                    case eComboType.RoomType:
                        temp = chkRoomTypes.Items.FindByValue(row["id"].ToString());
                        break;
                }

                if (temp == null)
                {
                    string name;
                    if (iComboType == eComboType.Composition)
                    {
                        Composition comp = new Composition(row["id"].ToString(), 0, 0);
                        name = comp.getName();
                    }
                    else
                    {
                        name = row["name_" + mLangStr].ToString();
                    }

                    ddlItem = new ListItem(name, row["id"].ToString());
                    chkList.Items.Add(ddlItem);
                    chkBaseList.Items.Add(new ListItem(ddlItem.Text, ddlItem.Value));
                }
            }
        }
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

    protected void btSaveHotelPrice_Click(object sender, EventArgs e)
    {
        if (validatePriceSave())
        {
            if (mHotelPrice == null)
            {
                mPriceWs.createNewEmptyHotelPrice(mSupplierId, mClerkId);
                mHotelPrice = Utils.getHotelPriceDetails(mSupplierId);
            }

            mHotelPrice.mIsBaseAmount = rbAmount.Checked;
            mHotelPrice.mStatus = bool.Parse(ddlPriceStatus.SelectedValue);
            mHotelPrice.mPriceColor = ddlPriceColor.SelectedValue;
            mHotelPrice.mCommission = (!string.IsNullOrEmpty(txtCommission.Value)) ? float.Parse(txtCommission.Value.Replace("%", "")) : 0;
            mHotelPrice.mOfficeDiscount = (!string.IsNullOrEmpty(txtOfficeDiscount.Value)) ? float.Parse(txtOfficeDiscount.Value.Replace("%", "")) : 0;
            mHotelPrice.mSiteDiscount = (!string.IsNullOrEmpty(txtSiteDiscount.Value)) ? float.Parse(txtSiteDiscount.Value.Replace("%", "")) : 0;
            mHotelPrice.mGeneralAdditive = (!string.IsNullOrEmpty(txtGeneralAdditive.Value)) ? float.Parse(txtGeneralAdditive.Value.Replace("%", "")) : 0;
            mHotelPrice.mBabiesAdditive = (!string.IsNullOrEmpty(txtBabiesAdditive.Value)) ? float.Parse(txtBabiesAdditive.Value.Replace("%", "")) : 0;
            mHotelPrice.mBaseComposition = new Composition(hiddenBaseCompositionId.Value, lblBaseComposition.Text, 0, 0);
            
            mHotelPrice.mBaseBase = new Base(hiddenBaseBaseId.Value, lblBaseBase.Text, (decimal)0);
            mHotelPrice.mBaseRoomType = new RoomType(hiddenBaseRoomTypeId.Value, lblBaseRoomType.Text, 0);

            mHotelPrice.setAllComboListsFalseStatus();
            setComboTypeDictionaries(chkCompositions, eComboType.Composition);
            setComboTypeDictionaries(chkBases, eComboType.Base);
            setComboTypeDictionaries(chkRoomTypes, eComboType.RoomType);

            try
            {
                if (mPriceWs.saveHotelPrice(mHotelPrice, mClerkId))
                {
                    popUpMessage(Utils.getTextFromFile("HotelPriceSaved", mLang));
                }
                else
                {
                    popUpMessage("Failed to update the price.");
                }
            }
            catch (Exception ex)
            {
                popUpMessage("Failed to update the price. exception = " + ex.Message);
                Logger.Log("Failed to save the hotel price, exception = " + ex.Message);
            }
        }

        loadCalendarWithoutPostBack();
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
                                mHotelPrice.mCompositions.Add(new Composition(item.Value, item.Text, percent,0), status);
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

                    while (currentDate.Date != toDate.AddDays(1).Date)
                    {
                        if (isDayAssigend(currentDate, txtSundayPrice.Text, txtMondayPrice.Text, txtTuesdayPrice.Text, txtWednesdayPrice.Text, txtThursdayPrice.Text, txtFridayPrice.Text, txtSaturdayPrice.Text) &&
                            isDayAssigend(currentDate, txtSundayPriceNetto.Text, txtMondayPriceNetto.Text, txtTuesdayPriceNetto.Text, txtWednesdayPriceNetto.Text, txtThursdayPriceNetto.Text, txtFridayPriceNetto.Text, txtSaturdayPriceNetto.Text))
                        {
                            PricePerDay existedPricePerDay = MonthlyPricesPerDay.Find(x => x.mDate.Date == currentDate.Date);
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
                                newDatePrice.mColor = "0";

                                mHotelPrice.savePricePerDay(newDatePrice);
                            }
                        }

                        currentDate = currentDate.AddDays(1);
                    }

                    try
                    {
                        mPriceWs.SetMonthlyPricePerDay(mSupplierId, MonthlyPricesPerDay);
                        popUpMessage(Utils.getTextFromFile("SucceccSetPrices", mLang));
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Failed to update all prices.");
                        popUpMessage("" + ex.Message);
                    }
                }
            }
            else
            {
                popUpMessage(Utils.getTextFromFile("PleaseEnterFromDateAndToDate",mLang));
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

                    while (currentDate.Date != toDate.AddDays(1).Date)
                    {
                        if (isDayAssigend(currentDate, txtRoomsSunday.Text, txtRoomsMonday.Text, txtRoomsTuesday.Text, txtRoomsWednesday.Text, txtRoomsThursday.Text, txtRoomsFriday.Text, txtRoomsSaturday.Text) ||
                            isDayAssigend(currentDate, txtRoomsDisableSunday.Text, txtRoomsDisableMonday.Text, txtRoomsDisableTuesday.Text, txtRoomsDisableWednesday.Text, txtRoomsDisableThursday.Text, txtRoomsDisableFriday.Text, txtRoomsDisableSaturday.Text))
                        {
                            PricePerDay existedPricePerDay = MonthlyPricesPerDay.Find(x => x.mDate.Date == currentDate.Date);
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
                                newDatePrice.mColor = "0";

                                mHotelPrice.savePricePerDay(newDatePrice);
                            }
                        }

                        currentDate = currentDate.AddDays(1);
                    }

                    try
                    {
                        mPriceWs.SetMonthlyAllocations(mSupplierId, MonthlyPricesPerDay);
                        popUpMessage(Utils.getTextFromFile("SucceccSetRooms", mLang));
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Failed to update all prices.");
                        popUpMessage("" + ex.Message);
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

    //Validations
    private bool validatePriceSave()
    {
        float outFloat;
        bool isVaildData = false;

        if (float.TryParse(txtCommission.Value.Replace("%", ""), out outFloat))
        {
            if (float.TryParse(txtOfficeDiscount.Value.Replace("%", ""), out outFloat))
            {
                if (float.TryParse(txtSiteDiscount.Value.Replace("%", ""), out outFloat))
                {
                    if (float.TryParse(txtGeneralAdditive.Value.Replace("%", ""), out outFloat))
                    {
                        if (float.TryParse(txtBabiesAdditive.Value.Replace("%", ""), out outFloat))
                        {
                            if (float.TryParse(hiddenBaseCompositionId.Value, out outFloat))
                            {
                                if (float.TryParse(hiddenBaseBaseId.Value, out outFloat))
                                {
                                    if (float.TryParse(hiddenBaseRoomTypeId.Value, out outFloat))
                                    {
                                        isVaildData = true;
                                    }
                                    else
                                    {
                                        popUpMessage(Utils.getTextFromFile("CastBaseRoomTypeError", mLang));
                                    }
                                }
                                else
                                {
                                    popUpMessage(Utils.getTextFromFile("CastBaseBaseError", mLang));
                                }
                            }
                            else
                            {
                                popUpMessage(Utils.getTextFromFile("CastBaseCompositionError", mLang));
                            }
                        }
                        else
                        {
                            popUpMessage(Utils.getTextFromFile("CastBabiesAdditveError", mLang));
                        }
                    }
                    else
                    {
                        popUpMessage(Utils.getTextFromFile("CastGeneralAdditveError", mLang));
                    }
                }
                else
                {
                    popUpMessage(Utils.getTextFromFile("CastOfficeDiscountError", mLang));
                }
            }
            else
            {
                popUpMessage(Utils.getTextFromFile("CastSiteDiscountError", mLang));
            }
        }
        else
        {
            popUpMessage(Utils.getTextFromFile("CastCommissionError", mLang));
        }

        return isVaildData;
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

    private void loadCalendarWithoutPostBack()
    {
        loadCalendar();
        //setMonthDays() won't be call in load calendar cause !page.IsPostBack
        setMonthDays();
        loadHotelPriceCombos();
        attachComboDetails();

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
                List<int> salesCycles = new List<int>();

                if (!string.IsNullOrEmpty(txtSalesCyclesMothly.Text))
                {
                    salesCyclesStr = txtSalesCyclesMothly.Text.Split(',');
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
                }

                if (isValid)
                {
                    DateTime currentDate, fromDate, toDate;
                    PricePerDay existedPricePerDay = null;
                    List<PricePerDay> pricePerDayList = new List<PricePerDay>();
                    msg = string.Empty;
                    DayOfWeek selectedDay = (DayOfWeek)int.Parse(ddlWeekDays.SelectedValue);
                    int daysToAdd = 1;

                    fromDate = DateTime.Parse(Utils.ConvertMonthWithSlashToAgencyFormat(txtFromDateSetCycles.Text));
                    currentDate = fromDate;
                    toDate = DateTime.Parse(Utils.ConvertMonthWithSlashToAgencyFormat(txtToDateSetCycles.Text));


                    while (currentDate.Date <= toDate.AddDays(1).Date)
                    {
                        if (selectedDay == currentDate.DayOfWeek)
                        {
                            existedPricePerDay = MonthlyPricesPerDay.Find(x => x.mDate.Date == currentDate.Date);
                            if (existedPricePerDay != null)
                            {
                                existedPricePerDay.mSalesCycles = salesCycles;
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

                    try
                    {
                        if (pricePerDayList != null && pricePerDayList.Count > 0)
                        {
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
        catch(Exception ex)
        {
            Logger.Log("Failed to save monthly allocation. exception = " + ex.Message);
            msg = Utils.getTextFromFile("FailedToUpdateMonthlyAllocation",mLang);
        }

        popUpMessage(msg);
        loadCalendarWithoutPostBack();
    }
}