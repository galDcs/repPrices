using System;
using System.Configuration;
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
        btSearchHotels.Text = Utils.getTextFromFile("Search", mLang);
        
        //Get form session / cookie
       // mAgencyId = int.Parse(Request.Cookies["Agency2000"]["AgencyId"]);
        //mSystemType = int.Parse(Request.Cookies["Agency2000"]["SystemType"]);
		//setAgencyData(mAgencyId.ToString(), mSystemType.ToString());
		//mUserName = Request.Cookies["Agency2000"]["LoginName"];
		//mClerkId = DAL_SQL.GetRecord("CLERKS", "id", "'" + mUserName + "'", "login_name");
		//mPassword = DAL_SQL.GetRecord("CLERKS", "password", "'" + mUserName + "'", "login_name");
		
        hiddenClerkId.Value = mClerkId;
        //mUserName = "Agency2000";
        //mPassword = "11071964";
        mResourceId = 999;

        Agency2000WS.Agency2000WSSoapClient agency2000Ws = new Agency2000WS.Agency2000WSSoapClient();
        //
		if (mAgencyId != 77 && mUserName != "malid")
		{
			/*if (!agency2000Ws.CheckSecurity(mAgencyId, mSystemType, mUserName, mPassword, mResourceId) )
			{
				Response.Redirect("./AccessDenied.aspx?resourceId=" + mResourceId + "&mAgencyId=" + mAgencyId  + "&mSystemType=" + mSystemType  + "&mUserName=" + mUserName  + "&mPassword=" + mPassword);
			}*/
		}
    }

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

                newRow.Attributes.Add("onclick", "moveToHotelGeneralDetails('" + row["hotel_code"].ToString() + "','" + row["name"].ToString().Replace("'","") + "','" + row["areaId"].ToString() + "','" + row["area_description"].ToString() + "')");
                tableHotelsSearchResult.Rows.Add(newRow);
            }
        }
    }
	
	private void setAgencyData(string iAgencyId, string iSystemType)
	{
		DAL_SQL.ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"];
		DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^agencyId^", ((iAgencyId.Length == 1) ? "000" + iAgencyId : "00" + iAgencyId));
		DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^systemType^", ((iSystemType == "3") ? "INN" : (iSystemType == "2") ? "ICC" :"OUT"));
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
