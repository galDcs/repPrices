//using AgencyPricesWS;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;

public partial class HotelSearch : System.Web.UI.Page
{
    private string mLangStr = "1255";
	private string agencyId = "85";
    private string systemType = "3";
		
    protected void Page_Load(object sender, EventArgs e)
    {
		setAgencyData(agencyId, systemType);
		
        if (!Page.IsPostBack)
        {
            loadAreas();
            getAllComboDedailByComboType(eComboType.Composition);
            getAllComboDedailByComboType(eComboType.Base);
            getAllComboDedailByComboType(eComboType.RoomType);
        }
    }
	
	private void setAgencyData(string iAgencyId, string iSystemType)
	{
		DAL_SQL.ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"];
		DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^agencyId^", ((iAgencyId.Length == 1) ? "000" + iAgencyId : "00" + iAgencyId));
		DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^systemType^", ((iSystemType == "3") ? "INN" : (iSystemType == "2") ? "ICC" :"OUT"));
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
        AgencyPricesSearch priceWs = new AgencyPricesSearch();
        string clerkId = "1";
        string areaId = ddlGeneralAreaId.SelectedValue;
        string supplierId = ddlHotels.SelectedValue;
        string priceType = "1";
        DateTime fromDate = DateTime.Parse(Utils.ConvertMonthWithSlashToAgencyFormat(txtFromDate.Text));
        DateTime toDate = DateTime.Parse(Utils.ConvertMonthWithSlashToAgencyFormat(txtToDate.Text));
        string compositionId = ddlCompositions.SelectedValue;
        string baseId = ddlBases.SelectedValue;
        string roomTypeId = ddlRoomTypes.SelectedValue;
        //List<FinalPrice> finalPrices = new List<FinalPrice>();
        string finalPrices;

        if (!string.IsNullOrEmpty(areaId) && areaId != "0")
        {
            try
            {
                if (!string.IsNullOrEmpty(supplierId) && supplierId != "0")
                {
                    finalPrices = priceWs.getHotelPriceByCombo(agencyId, systemType, clerkId, supplierId, priceType, fromDate.ToString("dd-MMM-yy"), toDate.ToString("dd-MMM-yy"), compositionId, baseId, roomTypeId);
                }
                else
                {
					Logger.Log("agencyId = " + agencyId+ " | systemType = " + systemType+ " | clerkId = " + clerkId+ " | areaId =  " + areaId+ " | priceType = " + priceType+ " | mLangStr = " + mLangStr+ " | fromDate = " + fromDate.ToString("dd-MMM-yy")+ " | toDate = " + toDate.ToString("dd-MMM-yy")+ " | compositionId = " + compositionId+ " | baseId = " + baseId);
                    finalPrices = priceWs.getHotelPriceAgencyXml(agencyId, systemType, clerkId, areaId, supplierId, priceType, mLangStr, fromDate.ToString("dd-MMM-yy"), toDate.ToString("dd-MMM-yy"), compositionId, baseId);
					Logger.Log("result = " + finalPrices);
                }

                //Gov Addition
                try
                {
                    //buildHotelViews(finalPrices);
                }
                catch (Exception exGetAvailableAllocation)
                {
                    Logger.Log("Failed to search ,Exception = " + exGetAvailableAllocation.Message);
                }
            }
            catch (Exception ex)
            {
				Logger.Log(ex.StackTrace);
                ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + ex.Message + "');", true);
            }
        }
        else
        {
            string msg = "please select area";
            ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + msg + "');", true);
        }
    }

    private void buildHotelViews(List<FinalPrice> iFinalPrices)
    {
        //XmlNodeList allocationsNodes = iXmlDoc.GetElementsByTagName("root/allocation");
        string hotelAttributes = string.Empty;
        string areaName = string.Empty;
        int rowIndex = 0;
        int i = 0;
        string supplierId = string.Empty;
        string allocationId = string.Empty;
        
        foreach (FinalPrice finalPrice in iFinalPrices)
        {
            if (!finalPrice.mHasError)
            {
                areaName = ddlGeneralAreaId.SelectedItem.Text;

                HtmlGenericControl newControl = new HtmlGenericControl("div");
                newControl.ID = i.ToString();
                rowIndex++;
                newControl.Attributes.Add("class", "deal center-div");
                string hotelName = DAL_SQL.GetRecord("Agency_Admin.dbo.SUPPLIERS", "name", "id", finalPrice.mSupplierId);
                newControl.InnerHtml = "<h2><label id='name" + rowIndex + "'>" + hotelName + "</h2>"
                                        + "<div class='img'>"
                                         + "<span class='tree'></span>"
                                            + "<img src='http://web14.agency2000.co.il/hotel_images/"
                                            + finalPrice.mSupplierId + "_1.jpg" + "' alt='" + hotelName + " תמונה ' image='img' />"
                                        + "</div>"
                                        + "<div class='text'>"
                                            + "<label class='font-color'>הרכב " + finalPrice.mComposition.getName() + "</label></br>"
                                            + "<label class='font-color'>בסיס " + finalPrice.mBase.getName() + "</label></br>"
                                            + "<label class='font-color'>סוג חדר " + finalPrice.mRoomType.getName() + "</label></br>"
                                            + "<label class='font-color' style='color:red;'>מחיר " + finalPrice.mFinalPrice + "</label></br>"
                                        + "</div>";

                PlaceHolder1.Controls.Add(newControl);
            }
        }
    }
}