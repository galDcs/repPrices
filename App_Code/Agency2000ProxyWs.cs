//using AgencyPricesWS;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Xml;

/// <summary>
/// Summary description for Agency2000ProxyWs
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class Agency2000ProxyWs : System.Web.Services.WebService 
{
    private int mResourceId = 1;
    private string agencyBclientId = ConfigurationManager.AppSettings.Get("AgencyDocketBclientId");
    private string createDocketAction = ConfigurationManager.AppSettings.Get("xmlService.asp");
    //From Gov, nt neccesary right now.
    //private string agencyXmlServicesBaseUrl = ConfigurationManager.AppSettings.Get("AgencyXmlServices");
    //private string agencyXmlServicesPricesBaseUrl = ConfigurationManager.AppSettings.Get("AgencyXmlServicesPrices");
    //private string baseTypesAction = ConfigurationManager.AppSettings.Get("agency_on_bases_xml.asp");
    //private string hotelPricesAction = ConfigurationManager.AppSettings.Get("hotel_price_selector_xml_server_proxy.aspx"); 
    
    private string docketType = "2";
    private string mAgencyId { get; set; }
    private string mSystemType { get; set; }
    private string mUserName { get; set; }
    private string mPassword { get; set; }
    private string mClerkId { get; set; }

    //OrderFrom - webSite/ agency/ requestFile
    [WebMethod]
    public void makeOrder(string iAgencyId, string iSystemType, string iClerkId, string iSupplierId, string iFromDate, string iToDate, string iCompositionId, string iBaseId, string iRoomTypeId, string iRoomsAmount, string iAreaId, string iOrderFrom)
    {
        DateTime fromDate, toDate;
        DataSet ds;
        bool isTakeAllocationSuccedded;
        const string priceType = "1";
        int roomsAmount = 0;
        CreateDocketResult createDocketResult;
        Agency2000WS.Agency2000WSSoapClient agency2000Ws = new Agency2000WS.Agency2000WSSoapClient();
        AgencyPricesSearch agencyPricesWs = new AgencyPricesSearch();
        AgencyAllocationWs agencyAllocationWs = new AgencyAllocationWs();

        try
        {
            if (int.TryParse(iRoomsAmount, out roomsAmount) && roomsAmount > 0)
            {
                fromDate = DateTime.Parse(iFromDate);
                toDate = DateTime.Parse(iToDate);
                ds = DAL_SQL_Helper.getUserNameAndPassByClerkId(iClerkId);

                if (Utils.isDataSetRowsNotEmpty(ds))
                {
                    mUserName = ds.Tables[0].Rows[0]["login_name"].ToString();
                    mPassword = ds.Tables[0].Rows[0]["password"].ToString();
                    mAgencyId = iAgencyId;
                    mSystemType = iSystemType;
                    mClerkId = iClerkId;

                    //if (!agency2000Ws.CheckSecurity(int.Parse(iAgencyId), int.Parse(iSystemType), mUserName, mPassword, mResourceId))
                    if (true)
                    {
                        string finalPriceStr = agencyPricesWs.getHotelPriceByCombo(iAgencyId, iSystemType, iClerkId, iSupplierId, priceType, iFromDate, iToDate, iCompositionId, iBaseId, iRoomTypeId);
                        decimal brutto, netto;
                        XmlDocument finalPriceXml = new XmlDocument();

                        finalPriceXml.LoadXml(finalPriceStr);
                        if (!decimal.TryParse(finalPriceXml.SelectSingleNode("Root//FinalPrices//FinalPrice//FinalPriceBrutto").InnerText, out brutto))
                        {
                            brutto = 0;
                        }
                        if (decimal.TryParse(finalPriceXml.SelectSingleNode("Root//FinalPrices//FinalPrice//FinalPriceNetto").InnerText, out netto))
                        {
                            netto = 0;
                        }

                        //TODO: remove comment on production
                        //createDocketResult = createDocket(iTraveller, iSupplierId, fromDate, toDate, iCompositionId, iBaseId, iRoomTypeId, roomsAmount, brutto, netto, iAreaId, iOrderFrom);
                        //
                        //if (createDocketResult.mStatus)
                        //{
                        //    if (agencyAllocationWs.hasEnoughRooms(iSupplierId, roomsAmount, iFromDate, iToDate))
                        //    {
                        //        isTakeAllocationSuccedded = agencyAllocationWs.takeAllocationsFromDateToDate(iSupplierId, roomsAmount, fromDate, toDate);
                        //        if (!isTakeAllocationSuccedded)
                        //        {
                        //            createDocketResult.mMessage = Utils.getTextFromFile("FailedToTakeAllocations", eLanguage.Hebrew);
                        //            //TODO: cancel the docket!!!!!!!!
                        //        }
                        //    }
                        //    else
                        //    {
                        //        //TODO: not enough rooms
                        //    }
                        //}
                    }
                    else
                    {
                        //TODO: not enough permissions
                    }
                }
            }
        }
        catch (Exception ex)
        {

        }
    }

    private CreateDocketResult createDocket(IPSTraveller iTraveller, string iSupplierId, DateTime iFromDate, DateTime iToDate, string iCompositionId, string iBaseId, string iRoomTypeId, int iRoomsAmount, decimal iBrutto, decimal iNetto, string iAreaId, string iOrderFrom)
    {
        bool isCreateDocketSuccedded = true;
        string createDocketXml;
        string createDocketXmlUrl;
        XmlDocument doc;
        CreateDocketResult createDocketResult = new CreateDocketResult();

        createDocketXml = getAgencyCreateDocketXML(iSupplierId, iFromDate, iToDate, iCompositionId, iBaseId, iRoomTypeId, iRoomsAmount, iBrutto, iNetto, iTraveller, iOrderFrom);
        createDocketXmlUrl = getAgencyCreateDocketUrl() + "&Query=" + createDocketXml;

        doc = getAgencyRequestXmlDoc(createDocketXmlUrl);

        if (doc != null)
        {
            XmlNodeList statuses = doc.SelectNodes("//STATUS");
            foreach (XmlNode status in statuses)
            {
                if (status.InnerText.ToLower() != "ok")
                {
                    createDocketResult.mMessage = Utils.getTextFromFile("FailedToCreateDocketInAgencyStatusNotOk", eLanguage.Hebrew);
                }
                else
                {
                    createDocketResult.mDocketId = getXmlValueByPath(doc,"/ROOT/DOCKET/DOCKET_ID");
                    createDocketResult.mVoucherId = getXmlValueByPath(doc,"/ROOT/SERVICES/HOTEL/VOUCHER_ID");

                    if (string.IsNullOrEmpty(createDocketResult.mDocketId))
                    {
                        createDocketResult.mMessage = Utils.getTextFromFile("FailedToGetDocketId", eLanguage.Hebrew);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(createDocketResult.mVoucherId))
                        {
                            createDocketResult.mMessage = Utils.getTextFromFile("FailedToGetVoucherId", eLanguage.Hebrew);
                        }
                    }
                }
            }
        }
        else
        {
            createDocketResult.mMessage = Utils.getTextFromFile("FailedToCreateDocketInAgency", eLanguage.Hebrew);
        }

        return createDocketResult;
    }

    private string getAgencyCreateDocketXML(string iSupplierId, DateTime iFromDate, DateTime iToDate, string iCompositionId, string iBaseId, string iRoomTypeId, int iRoomsAmount, decimal iBrutto, decimal iNetto, IPSTraveller iTraveller, string iOrderFrom)
    {
        string hotelPriceId = Utils.getHotelPriceId(iSupplierId);
        StringBuilder request = new StringBuilder();
        const string docketType = "2";
        request.Append("<ROOT>");
            // docket
            request.Append("<DOCKET>");
                request.Append("<DOCKET_TYPE>" + docketType + "</DOCKET_TYPE>");
                request.Append("<DOCKET_SYSTEM_TYPE>" + mSystemType + "</DOCKET_SYSTEM_TYPE>"); 
                request.Append("<BCLIENT_ID>" + agencyBclientId + "</BCLIENT_ID>");
                request.Append("<DOCKET_REMARK>created by " + iOrderFrom + "</DOCKET_REMARK>");
                request.Append("<DOCKET_SOURCE_TYPE>2</DOCKET_SOURCE_TYPE>");
            request.Append("</DOCKET>");
            // travellers
            request.Append(getTravellersXml(iTraveller));
            // service
            request.Append("<SERVICES>");
                //hotels
                request.Append("<HOTELS>");
                    //hotel
                    request.Append("<HOTEL>");
                        request.Append("<SUPPLIER_ID>" + iSupplierId + "</SUPPLIER_ID>");
                        request.Append("<BUNDLE_NUMBER></BUNDLE_NUMBER>");
                        request.Append("<STATUS>OK</STATUS>");
                        request.Append("<FROM_DATE>" + iToDate.ToString("dd-MMM-yy") + "</FROM_DATE>");
                        request.Append("<TO_DATE>" + iFromDate.ToString("dd-MMM-yy") + "</TO_DATE>");
                        request.Append("<ROOMS>" + iRoomsAmount + "</ROOMS>");
                        request.Append("<PAID_TO_SUPPLIER_ID>" + iSupplierId + "</PAID_TO_SUPPLIER_ID>");
                        request.Append("<PRICE_AGENCY_ID>" + mAgencyId + "</PRICE_AGENCY_ID>");
                        request.Append("<HOTEL_PRICE_ID>" + hotelPriceId + "</HOTEL_PRICE_ID>"); //hotelPriceId
                        request.Append("<SUPPLIER_PRICE_ID>" + hotelPriceId + "</SUPPLIER_PRICE_ID>"); //hotelPriceId
                        request.Append("<CREATE_VOUCHER>true</CREATE_VOUCHER>");
                    request.Append("</HOTEL>");
                // service travellers
                request.Append("<CUSTOMERS>");
                request.Append(getServiceCustomers(iTraveller, iRoomsAmount, iBrutto, iCompositionId, iBaseId, iRoomTypeId));
                request.Append("</CUSTOMERS>");
                request.Append("</HOTELS>");
            request.Append("</SERVICES>");
        request.Append("</ROOT>");

        return request.ToString();
    }

    private string getTravellersXml(IPSTraveller iTraveller)
    {
        StringBuilder str = new StringBuilder();
        string remark = string.Empty;

        str.Append("<TRAVELLERS>");
            str.Append("<TRAVELLER>");
                str.Append("<TITLE>" + (iTraveller.mGenderId == "1" ? "Mr" : "Mrs") + "</TITLE>");
                str.Append("<FIRST_NAME>" + Utils.ConvertStringToAgencyUtf(iTraveller.mFirstName) + "</FIRST_NAME>");
                str.Append("<LAST_NAME>" + Utils.ConvertStringToAgencyUtf(iTraveller.mLastName) + "</LAST_NAME>");
                str.Append("<ID_NUM>" + iTraveller.mId + "</ID_NUM>");
                str.Append("<PHONES>");
                    str.Append("<MOBILE>" + iTraveller.mCellPhone + "</MOBILE>");
                    str.Append("<HOME>" + iTraveller.mPhone + "</HOME>");
                str.Append("</PHONES>");
                str.Append("<EMAIL>" + iTraveller.mMail + "</EMAIL>");
                str.Append("<REMARK>" + remark + "</REMARK>");
            str.Append("</TRAVELLER>");

        //Escorts
        foreach (IPSTraveller.Escort escort in iTraveller.mEscorts)
        {
            str.Append("<TRAVELLER>");
                str.Append("<TITLE></TITLE>");
                //The id of the escort is the same as the main traveller to know that they are connected
                str.Append("<ID_NUM>" + iTraveller.mId + "</ID_NUM>");
                str.Append("<FIRST_NAME>" + Utils.ConvertStringToAgencyUtf(escort.mFirstName) + "</FIRST_NAME>");
                str.Append("<LAST_NAME>" + Utils.ConvertStringToAgencyUtf(escort.mLastName) + "</LAST_NAME>");
            str.Append("</TRAVELLER>");
        }

        str.Append("</TRAVELLERS>");

        return str.ToString();
    }

    private string getServiceCustomers(IPSTraveller iTraveller, int iRoomsAmount, decimal iBrutto, string iCompositionId, string iBaseId, string iRoomTypeId)
    {
        StringBuilder str = new StringBuilder();
        decimal subsid = iBrutto, travPay = 0, zero = 0;

        str.Append("<CUSTOMER>");
            str.Append("<AMOUNT>" + iBrutto + "</AMOUNT>");
            str.Append("<SUBSID>" + subsid + "</SUBSID>");
            str.Append("<TRAV_PAY>" + travPay + "</TRAV_PAY>");
            str.Append("<CURRENCY>1</CURRENCY>");
            str.Append("<ROOM_TYPE_ID>" + iCompositionId + "</ROOM_TYPE_ID>");
            str.Append("<ROOM_TYPE_ITM>1</ROOM_TYPE_ITM>");
            str.Append("<ON_BASE_ID>" + iBaseId + "</ON_BASE_ID>");
            str.Append("<ON_BASE_ITM>1</ON_BASE_ITM>");
            str.Append("<BABIES>0</BABIES>");
        str.Append("</CUSTOMER>");

        
        foreach (IPSTraveller.Escort escort in iTraveller.mEscorts)
        {
            str.Append("<CUSTOMER>");
                str.Append("<AMOUNT>" + zero + "</AMOUNT>");
                str.Append("<SUBSID>" + zero + "</SUBSID>");
                str.Append("<TRAV_PAY>" + zero + "</TRAV_PAY>");
                str.Append("<CURRENCY>1</CURRENCY>");
                
            if (iRoomsAmount > 1)
            {
                if (escort.mIsMain)
                {
                    str.Append("<ROOM_TYPE_ID>" + iCompositionId + "</ROOM_TYPE_ID>");
                    str.Append("<ROOM_TYPE_ITM>1</ROOM_TYPE_ITM>");
                    str.Append("<ON_BASE_ID>" + iBaseId + "</ON_BASE_ID>");
                    str.Append("<ON_BASE_ITM>1</ON_BASE_ITM>");
                }
                else
                {
                    str.Append("<ROOM_TYPE_ID></ROOM_TYPE_ID>");
                    str.Append("<ROOM_TYPE_ITM>0</ROOM_TYPE_ITM>");
                    str.Append("<ON_BASE_ID></ON_BASE_ID>");
                    str.Append("<ON_BASE_ITM>0</ON_BASE_ITM>");
                }
            }

            str.Append("</CUSTOMER>");
        }           
    
        return str.ToString();
    }

    private XmlDocument getAgencyRequestXmlDoc(string url)
    {
        XmlDocument doc = new XmlDocument();
        StringBuilder oBuilder = new StringBuilder();
        StringWriter oStringWriter = new StringWriter(oBuilder);

        XmlTextReader oXmlReader = new XmlTextReader(url);
        XmlTextWriter oXmlWriter = new XmlTextWriter(oStringWriter);

        try
        {
            Logger.Log(url);
            //Logger.Log(url, "Log", @"Logs\log_" + DateTime.Now.ToShortDateString() + ".txt");
            while (oXmlReader.Read())
            {
                oXmlWriter.WriteNode(oXmlReader, true);
            }
            oXmlReader.Close();
            oXmlWriter.Close();
            doc.LoadXml(oBuilder.ToString());

            Logger.Log(oBuilder.ToString());
            //Logger.Log(oBuilder.ToString(), "Log", @"Logs\log_" + DateTime.Now.ToShortDateString() + ".txt");
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
            //Logger.Log(ex.Message, "Log create docket response error", @"Logs\log_" + DateTime.Now.ToShortDateString() + ".txt");
            return null;
        }
        return doc;
    }
   
    private string getAgencyCreateDocketUrl()
    {
        string retUrl = string.Empty;
        string agencyXmlServicesCreateDocketUrl = ConfigurationManager.AppSettings.Get("AgencyXmlServicesCreateDocket");

        retUrl = agencyXmlServicesCreateDocketUrl + createDocketAction + getAgencyLoginUrlPart();
        return retUrl;
    }

    private string getAgencyLoginUrlPart()
    {
        string str_url = string.Empty;
       
        str_url = "?AgencyID=" + mAgencyId + "&SystemType=" + mSystemType + "&UserName=" + mUserName + "&Password=" + mPassword + "&ClerkID=" + mClerkId + "&language=1";

        return str_url;

    }
    private string getXmlValueByPath(XmlDocument iDoc, string iPath)
    {
        string value = string.Empty;

        if (iDoc.SelectSingleNode(iPath) != null)
        {
            value = iDoc.SelectSingleNode(iPath).InnerText;
        }

        return value;
    }
}
