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

public partial class HotelPriceGeneralDetails : System.Web.UI.Page
{
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

	private void setAgencyData(string iAgencyId, string iSystemType)
	{
		DAL_SQL.ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"];
		DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^agencyId^", ((iAgencyId.Length == 1) ? "000" + iAgencyId : "00" + iAgencyId));
		DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^systemType^", ((iSystemType == "3") ? "INN" : (iSystemType == "2") ? "ICC" :"OUT"));
	}
	
    protected void Page_Load(object sender, EventArgs e)
    {
		//string agencyId = Request.Cookies["Agency2000"]["AgencyId"];
		//string systemType = Request.Cookies["Agency2000"]["SystemType"];
		//setAgencyData(agencyId, systemType);
		
        ListItem liTemp;
        mLang = (eLanguage)int.Parse(mLangStr);

        setPriveteMembers();
        mYear = int.Parse(Request.QueryString["year"]);
        mMonth = int.Parse(Request.QueryString["month"]);
    
        if (!Page.IsPostBack)
        {
            liTemp = new ListItem() { Text = Utils.getTextFromFile("Active", mLang), Value = "true" };
            ddlPriceStatus.Items.Add(liTemp);
            liTemp = new ListItem() { Text = Utils.getTextFromFile("InActive", mLang), Value = "false" };
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

            btGoToSetCalendarPrices.Text = Utils.getTextFromFile("SetCalendarPrices", mLang);
            btBackToMenu.Text = Utils.getTextFromFile("BackToMenu", mLang);
            btSaveHotelPrice.Text = Utils.getTextFromFile("SaveHotelPrice", mLang);
            rbAmount.Text = Utils.getTextFromFile("BaseAmount", mLang);
            rbPercent.Text = Utils.getTextFromFile("BasePercent", mLang);
        }

        loadHotelPrice();
    }

    //HotelPrice
    private void loadHotelPrice()
    {
        setHeader();
        //Preperations
        loadAllComboTypes();
		PriceDetailsWs mPriceWs = new PriceDetailsWs();
        mHotelPrice = mPriceWs.getHotelPrice(mSupplierId, mMonth, mYear);

        if (mHotelPrice != null && string.IsNullOrEmpty(Request.QueryString["SetGeneralDatails"]))
        {
            btGoToSetCalendarPrices_Click(null, null);
        }

        if (!Page.IsPostBack)
        {
            loadHotelPriceCombos();
            attachComboDetails();
			
			if (mHotelPrice != null)
			{
				rbAmount.Checked = mHotelPrice.mIsBaseAmount;
				rbPercent.Checked = !mHotelPrice.mIsBaseAmount;
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
		else
		{
			List<ListItem> selectedFromBaseCheckList = chkBaseComposition.Items.Cast<ListItem>()
																		    .Where(li => li.Selected)
																		    .ToList();
			if (selectedFromBaseCheckList != null && selectedFromBaseCheckList.Count > 0)
			{
				hiddenBaseCompositionId.Value = selectedFromBaseCheckList[0].Value;
				lblBaseComposition.Text = selectedFromBaseCheckList[0].Text;
			}
			
			selectedFromBaseCheckList = chkBaseBase.Items.Cast<ListItem>()
														.Where(li => li.Selected)
														.ToList();
			if (selectedFromBaseCheckList != null && selectedFromBaseCheckList.Count > 0)
			{            
				hiddenBaseBaseId.Value = selectedFromBaseCheckList[0].Value;
				lblBaseBase.Text = selectedFromBaseCheckList[0].Text;
			}			
			
			selectedFromBaseCheckList = chkBaseRoomType.Items.Cast<ListItem>()
															.Where(li => li.Selected)
															.ToList();

			if (selectedFromBaseCheckList != null && selectedFromBaseCheckList.Count > 0)
			{
				hiddenBaseRoomTypeId.Value = selectedFromBaseCheckList[0].Value;
				lblBaseRoomType.Text = selectedFromBaseCheckList[0].Text;
			}
		}
    }
    
    private void setHeader()
    {
        TableHeaderRow headerRow = new TableHeaderRow();

        if (mSupplierName.Contains(Utils.getTextFromFile("ReshetFatal", mLang)))
        {
            mSupplierName = mSupplierName.Replace(Utils.getTextFromFile("ReshetFatal", mLang), "");
        }

        priceHeader.Text = mAreaName + ", " + mSupplierName;
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
                            string function = "addComboList('compositionBrutto', " + listItem.Value + ", '" + listItem.Text + "', '" + (comboType as Composition).mPercent + "', this);";
                            function += "addComboList('compositionNetto', " + listItem.Value + ", '" + listItem.Text + "', '" + (comboType as Composition).mPercentNetto + "', this)";
                            listItem.Attributes.Add("onclick", function);
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
                        li.Attributes.Add("onclick", "addComboList('composition', " + li.Value + ", '" + li.Text + "', '" + li.Attributes["percent"] + "', this)");
                    }

                    chkBaseComposition.Items.FindByValue(li.Value).Attributes.Add("onclick", "changeBaseCombo('composition', '" + li.Value + "', '" + li.Text + "', this)");
					if (mHotelPrice != null && mHotelPrice.mBaseRoomType != null)
					{
						if (li.Value == mHotelPrice.mBaseComposition.getId())
						{
							chkBaseComposition.Items.FindByValue(li.Value).Selected = true;
						}
					}
                    break;
                case eComboType.Base:
                    if (!li.Selected)
                    {
                        li.Attributes.Add("onclick", "addComboList('base', " + li.Value + ", '" + li.Text + "', '0', this)");
                    }

                    chkBaseBase.Items.FindByValue(li.Value).Attributes.Add("onclick", "changeBaseCombo('base', '" + li.Value + "', '" + li.Text + "', this)");
					if (mHotelPrice != null && mHotelPrice.mBaseBase != null)
					{
						if (li.Value == mHotelPrice.mBaseBase.getId())
						{
							chkBaseBase.Items.FindByValue(li.Value).Selected = true;
						}
					}
                    break;
                case eComboType.RoomType:
                    if (!li.Selected)
                    {
                        li.Attributes.Add("onclick", "addComboList('roomType', " + li.Value + ", '" + li.Text + "', '0', this)");
                    }

                    chkBaseRoomType.Items.FindByValue(li.Value).Attributes.Add("onclick", "changeBaseCombo('roomType', '" + li.Value + "', '" + li.Text + "', this)");
					if (mHotelPrice != null && mHotelPrice.mBaseRoomType != null)
					{
						if (li.Value == mHotelPrice.mBaseRoomType.getId())
						{
							chkBaseRoomType.Items.FindByValue(li.Value).Selected = true;
						}
					}
                    break;
            }
        }
    }

    private void attachComboDetails()
    {
        if (mHotelPrice != null)
        {
            divCompositionsDetailsBrutto.Controls.Clear();
            divCompositionsDetailsNetto.Controls.Clear();
            divBasesDetails.Controls.Clear();
            divRoomTypesDetails.Controls.Clear();

            divCompositionsDetailsBrutto.Controls.Add(new LiteralControl("<h2 style='direction: rtl;'>אחוזי ברוטו<h2>"));
            divCompositionsDetailsNetto.Controls.Add(new LiteralControl("<h2 style='direction: rtl;'>אחוזי נטו<h2>"));

            setComboTypeDetails(eComboType.Composition, mHotelPrice.mCompositions.Keys.Cast<ComboDetails>().ToList(), chkCompositions);
            setComboTypeDetails(eComboType.Base, mHotelPrice.mBases.Keys.Cast<ComboDetails>().ToList(), chkBases);
            setComboTypeDetails(eComboType.RoomType, mHotelPrice.mRoomTypes.Keys.Cast<ComboDetails>().ToList(), chkRoomTypes);
        }
    }

    private void setComboTypeDetails(eComboType iComboType, List<ComboDetails> iComboList, CheckBoxList iCheckList)
    {
        int pixelsToMarginTopFinal = 0;
        const int pixelsToMarginTop = 8;
        //string lblHiddenId;

        foreach (ComboDetails comboType in iComboList)
        {
            switch (iComboType)
            {
                case eComboType.Composition:
                    Composition composition = (comboType as Composition);
                    string txtCompId = "txtComposition_" + composition.getId();
                    string txtCompIdNetto = "txtComposition_Netto" + composition.getId();
                    string lblCompId = "lblComposition_" + composition.getId();
                    string lblNettoCompId = "lblNettoComposition_" + composition.getId();
                    //lblHiddenId = "hiddenCompositionPercent_" + composition.getId();

                    if (mHotelPrice.mCompositions[(composition)] == true)
                    {
                        pixelsToMarginTopFinal = pixelsToMarginTop * (divCompositionsDetailsBrutto.Controls.Count / 4);
                        Label lblCompositionName = new Label() { ID = lblCompId, Text = comboType.getName() };
                        lblCompositionName.Style.Add("font-size", "13px");
                        lblCompositionName.Style.Add("float", "right");
                        lblCompositionName.Style.Add("margin-right", "3px");
                        lblCompositionName.Style.Add("margin-top", "-" + pixelsToMarginTopFinal + "px");
                        lblCompositionName.Style.Add("margin-top", "-" + pixelsToMarginTopFinal + "px");
                        lblCompositionName.Style.Add("direction", "rtl");
                        lblCompositionName.Style.Add("text-align", "center");
                        lblCompositionName.Style.Add("width", "30%");

                        Label lblNettoCompositionName = new Label() { ID = lblNettoCompId, Text = comboType.getName() };
                        lblNettoCompositionName.Style.Add("font-size", "13px");
                        lblNettoCompositionName.Style.Add("float", "right");
                        lblNettoCompositionName.Style.Add("margin-right", "3px");
                        lblNettoCompositionName.Style.Add("margin-top", "-" + pixelsToMarginTopFinal + "px");
                        lblNettoCompositionName.Style.Add("margin-top", "-" + pixelsToMarginTopFinal + "px");
                        lblNettoCompositionName.Style.Add("direction", "rtl");
                        lblNettoCompositionName.Style.Add("text-align", "center");
                        lblNettoCompositionName.Style.Add("width", "30%");

                        TextBox txtCompositionPercent = new TextBox() { ID = txtCompId, Text = composition.mPercent.ToString() };
                        txtCompositionPercent.Style.Add("float", "left");
                        txtCompositionPercent.Style.Add("width", "25%");
                        txtCompositionPercent.Style.Add("margin-top", "-" + pixelsToMarginTopFinal + "px");
                        txtCompositionPercent.Attributes.Add("isNumberKey", "isNumberKey");

                        TextBox txtCompositionPercentNetto = new TextBox() { ID = txtCompIdNetto, Text = composition.mPercentNetto == null || composition.mPercentNetto == 0 ? 
                                                                                                        composition.mPercent.ToString() : composition.mPercentNetto.ToString() };
                        txtCompositionPercentNetto.Style.Add("float", "left");
                        txtCompositionPercentNetto.Style.Add("width", "25%");
                        txtCompositionPercentNetto.Style.Add("margin-top", "-" + pixelsToMarginTopFinal + "px");
                        txtCompositionPercentNetto.Attributes.Add("isNumberKey", "isNumberKey");


                        divCompositionsDetailsBrutto.Controls.Add(lblCompositionName);
                        divCompositionsDetailsBrutto.Controls.Add(txtCompositionPercent);
                        divCompositionsDetailsBrutto.Controls.Add(new LiteralControl("<br />"));
                        divCompositionsDetailsBrutto.Controls.Add(new LiteralControl("<br />"));

                        divCompositionsDetailsNetto.Controls.Add(lblNettoCompositionName);
                        divCompositionsDetailsNetto.Controls.Add(txtCompositionPercentNetto);
                        divCompositionsDetailsNetto.Controls.Add(new LiteralControl("<br />"));
                        divCompositionsDetailsNetto.Controls.Add(new LiteralControl("<br />"));
                    }

                    break;

                case eComboType.Base:
                    Base baseItem = (comboType as Base);
                    string txtBaseId = "txtBase_" + baseItem.getId();
                    string lblBaseId = "lblBase_" + baseItem.getId();
                    //lblHiddenId = "hiddenBaseAmount_" + baseItem.getId();

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
                    //lblHiddenId = "hiddenRoomTypeAmount_" + roomType.getId();

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
                query = "SELECT room_id as id, percent_value FROM SUPPLIER_TO_ROOMS_PERCENT WHERE supplier_id = " + mSupplierId;
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
                    string compPercent;

                    if (iComboType == eComboType.Composition)
                    {
                        Composition comp = new Composition(row["id"].ToString(), 0, 0);
                        name = comp.getName();
                        compPercent = row["percent_value"].ToString();
                        ddlItem = new ListItem(name, row["id"].ToString());
                        ddlItem.Attributes["percent"] = compPercent;
                    }
                    else
                    {
                        name = row["name_" + mLangStr].ToString();
                        ddlItem = new ListItem(name, row["id"].ToString());
                    }

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
			PriceDetailsWs mPriceWs = new PriceDetailsWs();
			
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
        float percent = 0, nettoPrecent = 0;
        decimal amount = 0;
        string hiddenFieldValue,hiddenFieldValuePercent, hiddenFieldValueNettoPercent;

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
                        hiddenFieldValuePercent = Request.Form["ctl00$ContentPlaceHolder2$txtComposition_" + item.Value];
                        hiddenFieldValueNettoPercent = Request.Form["ctl00$ContentPlaceHolder2$txtComposition_Netto" + item.Value];

                        if (composition != null && !string.IsNullOrEmpty(hiddenFieldValuePercent) && hiddenFieldValuePercent != "0"
                            && !string.IsNullOrEmpty(hiddenFieldValueNettoPercent) && hiddenFieldValueNettoPercent != "0")
                        {
                            percent = float.Parse(hiddenFieldValuePercent);
                            nettoPrecent = float.Parse(hiddenFieldValueNettoPercent);
                            mHotelPrice.mCompositions[composition] = status;
                            composition.mPercent = percent;
                            composition.mPercentNetto = nettoPrecent;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(hiddenFieldValuePercent) && hiddenFieldValuePercent != "0" 
                                && !string.IsNullOrEmpty(hiddenFieldValueNettoPercent) && hiddenFieldValueNettoPercent != "0")
                            {
                                percent = float.Parse(hiddenFieldValuePercent);
                                nettoPrecent = float.Parse(hiddenFieldValueNettoPercent);
                                mHotelPrice.mCompositions.Add(new Composition(item.Value, item.Text, percent, nettoPrecent), status);
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
								if (baseItem.mPercent == 0)
								{
									baseItem.mPercent = 100;
								}
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

    protected void btBackToMenu_Click(object sender, EventArgs e)
    {
        Response.Redirect("./Menu.aspx", false);
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
    
    private void loadCalendarWithoutPostBack()
    {
        loadHotelPriceCombos();
        attachComboDetails();
    }

    private void popUpMessage(string msg)
    {
        ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + msg + "');", true);
    }

    protected void btGoToSetCalendarPrices_Click(object sender, EventArgs e)
    {
        if (mHotelPrice != null)
        {
            Response.Redirect("HotelPriceManager.aspx?month=" + DateTime.Now.Month + "&year=" + DateTime.Now.Year + "&supplierId=" + mSupplierId + "&supplierName=" + mSupplierName + "&clerkId=" + mClerkId + "&areaId=" + mAreaId + "&areaName=" + mAreaName, false);
        }
        else
        {
            popUpMessage(Utils.getTextFromFile("PleaseSaveTheGeneralPriceFirst", mLang));
        }
    }
}