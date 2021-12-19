using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Menu : System.Web.UI.Page
{
    public eLanguage mLang;
    public string langStr = "1255";
    private string mClerkId;
    private int mAgencyId;
    private int mSystemType;
    private int mResourceId;
    private string mUserName = string.Empty;
    private string mPassword = string.Empty;
    
    protected void Page_Load(object sender, EventArgs e)
    {
        mClerkId = "1";
        mLang = (eLanguage)int.Parse(langStr);
        hiddenClerkId.Value = mClerkId;
        btSearchHotels.Text = Utils.getTextFromFile("Search", mLang);
        
		
        //Get form session / cookie
        mAgencyId = int.Parse(Request.Cookies["Agency2000"]["AgencyId"]);
        mSystemType = int.Parse(Request.Cookies["Agency2000"]["SystemType"]);
        mUserName = Request.Cookies["Agency2000"]["LoginName"];
        mPassword = DAL_SQL.GetRecord("CLERKS", "password", "'" + mUserName + "'", "login_name");
        mResourceId = 282;
		Logger.Log("mAgencyId = " + mAgencyId + ", mSystemType = " + mSystemType+ ", mUserName = "+  mUserName + ", mPassword = " + mPassword);

        Agency2000WS.Agency2000WSSoapClient agency2000Ws = new Agency2000WS.Agency2000WSSoapClient();
        //
        if (!agency2000Ws.CheckSecurity(mAgencyId, mSystemType, mUserName, mPassword, mResourceId))
        {
            Response.Redirect("./AccessDenied.aspx?resourceId=" + mResourceId);
        }
    }
	
	//private string getFromCookie(out string iAgencyId, out string iSystemType, out string iUserName, out string iPassword)
	//{
	//	string[] cookies = Request.Cookies.AllKeys;
	//	string language = string.Empty;
	//	string[] splittedCookie;
	//	
    //    foreach (string cookie in cookies)
    //    {
    //        if (Request.Cookies[cookie].Value.ToString().Contains("agency"))
    //        {
	//			splittedCookie = Request.Cookies[cookie].Value.Split("&");
	//			iAgencyId = splittedCookie[0].Split("=")[1];
	//			iSystemType = splittedCookie[0].Split("=")[1];
	//			AgencyId = splittedCookie[0].Split("=")[1];
	//			AgencyId = splittedCookie[0].Split("=")[1];
    //        }
    //    }
	//	
	//	if (string.IsNullOrEmpty(language))
	//		language = "1255";
	//	
	//	MySession.Current.Language = language;		
	//	
	//	return language;
	//}	

    protected void btSearchHotels_Click(object sender, EventArgs e)
    {
        DataSet hotels = DAL_SQL_Helper.searchHotels(txtHotelName.Text, langStr);

        if (Utils.isDataSetRowsNotEmpty(hotels))
        {
            tableHotelsSearchResult.Visible = true;

            foreach (DataRow row in hotels.Tables[0].Rows)
            {
                TableRow newRow = new TableRow();
                addCellToRowByText(row["hotel_code"].ToString(), newRow);
                addCellToRowByText(row["name"].ToString(), newRow);
                addCellToRowByText(row["area_description"].ToString(), newRow);
                addCellToRowByText((row["isActive"].ToString() == "1") ? Utils.getTextFromFile("Open", mLang) : Utils.getTextFromFile("Close", mLang) , newRow);

                newRow.Attributes.Add("onclick", "moveToHotelPrice('" + row["hotel_code"].ToString() + "','" + row["name"].ToString() + "','" + row["areaId"].ToString() + "','" + row["area_description"].ToString() + "')");
                tableHotelsSearchResult.Rows.Add(newRow);
            }
        }
    }

    public void addCellToRowByText(string iText, TableRow row)
    {
        TableCell cell = new TableCell();

        cell.Text = iText;
        row.Cells.Add(cell);
    }

    private void popUpMessage(string msg)
    {
        ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + msg + "');", true);
    }
}