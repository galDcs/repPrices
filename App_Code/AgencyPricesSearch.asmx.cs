using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml;

//namespace AgencyPricesWS
//{
    /// <summary>
    /// Summary description for AgencyPricesSearch
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class AgencyPricesSearch : System.Web.Services.WebService
    {
		[WebMethod]
        public XmlNode getSpecificDatePriceDetails(string iSupplierId, DateTime iDate)
		{
			return (XmlNode) Utils.getDatePriceBaseDetails(iSupplierId, iDate);
		}
		
		[WebMethod]
        public XmlDocument getSpecificDatePriceDetailsXmlDocument(string iSupplierId, DateTime iDate)
		{
			return Utils.getDatePriceBaseDetails(iSupplierId, iDate);
		}
		
        //Gui methods
        [WebMethod]
        public string getHotelPriceByCombo(string iAgencyId, string iSystemType, string iClerkId, string iSupplierId, string iPriceType, string iFromDate, string iToDate, string iCompositionId, string iBaseId, string iRoomTypeId)
        {
            FinalPrice finalPrice = new FinalPrice();
            DateTime fromDate, toDate;
            HotelPrice hotelPrice = null;
            Composition composition = null;
            Base baseItem = null;
            RoomType roomType = null;

            try
            {
                //Initialize all parameters
                fromDate = DateTime.Parse(iFromDate);
                toDate = DateTime.Parse(iToDate);
                hotelPrice = Utils.getHotelPriceDetails(iSupplierId);
                composition = hotelPrice.getCompositionById(iCompositionId);
                baseItem = hotelPrice.getBaseById(iBaseId);
                roomType = hotelPrice.getRoomTypeById(iRoomTypeId);

                if (composition != null && baseItem != null && roomType != null && hotelPrice.mCompositions[composition] && hotelPrice.mBases[baseItem] && hotelPrice.mRoomTypes[roomType])
                {
                    finalPrice = getFinalPrice(hotelPrice, fromDate, toDate, composition, baseItem, roomType);
                }
                else
                {
                    if (composition == null || !hotelPrice.mCompositions[composition])
                    {
                        finalPrice.ErrorDescription = "1|" + Utils.getTextFromFile("CompositionNotFound", eLanguage.Hebrew);
                    }
                    else if (baseItem == null || !hotelPrice.mBases[baseItem])
                    {
                        finalPrice.ErrorDescription = "2|" + Utils.getTextFromFile("BaseNotFound", eLanguage.Hebrew);
                    }
                    else if (roomType == null || !hotelPrice.mRoomTypes[roomType])
                    {
                        finalPrice.ErrorDescription = "3|" + Utils.getTextFromFile("RoomTypeNotFound", eLanguage.Hebrew);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Exception = " + ex.Message);
                throw new Exception("Failed to parse dates. from = " + iFromDate + ", to = " + iToDate + ". exception = " + ex.Message);
            }

            if (finalPrice.mHasError)
            {
                finalPrice.mFinalPrice = 0;
				finalPrice.mFinalPriceNetto = 0;
            }

            return finalPrice.toXmlString();
        }

        [WebMethod]
        public string getHotelPricePerArea(string iAgencyId, string iSystemType, string iClerkId, 
										   string iAreaId, string iSupplierId, string iPriceType, string iLanguage,
                                           string iFromDate, string iToDate, string iCompositionId, string iBaseId)
        {
			setAgencyData(iAgencyId, iSystemType);
			
            List<FinalPrice> finalPrices = new List<FinalPrice>();
            List<string> suppliersIds;
			
			if (iSupplierId == "0")
			{
				if (iAgencyId == "85") //For Shikum.
				{
					suppliersIds = Utils.getSuppliersIdsListByGovArea(iAreaId, iLanguage);
					//suppliersIds = Utils.getSuppliersIdsListByArea(iAreaId, iLanguage);
				}
				else
				{
					//Change to get general hotels
					suppliersIds = Utils.getSuppliersIdsListByArea(iAreaId, iLanguage);
				}
			}
			else
			{
				suppliersIds = new List<string>();
				suppliersIds.Add(iSupplierId);
			}
			
			HotelPrice hotelPrice = null;
            DateTime fromDate, toDate;
            Composition composition = null;
            Base baseItem = null;

            try
            {
                fromDate = DateTime.Parse(iFromDate);
                toDate = DateTime.Parse(iToDate);
            }
            catch
            {
                Logger.Log("Failed to parse dates. fromDate = " + iFromDate + ", toDate = " + iToDate);
                throw new Exception("Failed to parse dates. fromDate = " + iFromDate + ", toDate = " + iToDate);
            }
			
            foreach (string supplierId in suppliersIds)
            {
	    
                hotelPrice = Utils.getHotelPriceDetails(supplierId);
                if (hotelPrice != null)
                {
                    composition = hotelPrice.getCompositionById(iCompositionId);
                    if (!string.IsNullOrEmpty(iBaseId) && iBaseId != "0")
                    {
                        baseItem = hotelPrice.getBaseById(iBaseId);
                    }

                    if (baseItem != null)
                    {
                        finalPrices.AddRange(getFinalPrices(iAgencyId, iSystemType, iClerkId, fromDate, toDate, hotelPrice, composition, baseItem));
                    }
                    else
                    {
                        foreach (Base baseToSearch in hotelPrice.mBases.Keys)
                        {
                            finalPrices.AddRange(getFinalPrices(iAgencyId, iSystemType, iClerkId, fromDate, toDate, hotelPrice, composition, baseToSearch));
                        }
                    }
                }
            }

            string finalPricesXml = string.Empty;
            finalPricesXml += "<Root>";

            foreach (FinalPrice finalPrice in finalPrices)
            {
                finalPricesXml += finalPrice.toXmlString();
            }
            finalPricesXml += "</Root>";

            return finalPricesXml;
        }


    [WebMethod]
    public string getHotelPricePerAreaByMultipleBaseIds(string iAgencyId, string iSystemType, string iClerkId,
                                   string iAreaId, string iSupplierId, string iPriceType, string iLanguage,
                                   string iFromDate, string iToDate, string iCompositionId, int[] iBaseIds)
    {
        setAgencyData(iAgencyId, iSystemType);

        List<FinalPrice> finalPrices = new List<FinalPrice>();
        List<string> suppliersIds;

        if (iSupplierId == "0")
        {
            if (iAgencyId == "85") //For Shikum.
            {
                suppliersIds = Utils.getSuppliersIdsListByGovArea(iAreaId, iLanguage);
                //suppliersIds = Utils.getSuppliersIdsListByArea(iAreaId, iLanguage);
            }
            else
            {
                //Change to get general hotels
                suppliersIds = Utils.getSuppliersIdsListByArea(iAreaId, iLanguage);
            }
        }
        else
        {
            suppliersIds = new List<string>();
            suppliersIds.Add(iSupplierId);
        }

        HotelPrice hotelPrice = null;
        DateTime fromDate, toDate;
        Composition composition = null;
        Base baseItem = null;

        try
        {
            fromDate = DateTime.Parse(iFromDate);
            toDate = DateTime.Parse(iToDate);
        }
        catch
        {
            Logger.Log("Failed to parse dates. fromDate = " + iFromDate + ", toDate = " + iToDate);
            throw new Exception("Failed to parse dates. fromDate = " + iFromDate + ", toDate = " + iToDate);
        }

        bool ignoreBsarePerDatesPeriod = true;

        foreach (string supplierId in suppliersIds)
        {
            hotelPrice = Utils.getHotelPriceDetails(supplierId);
            if (hotelPrice != null)
            {
                composition = hotelPrice.getCompositionById(iCompositionId);
                if (iBaseIds != null)
                {
                    //About to run over all the recieved bases and get ruseult even if one of them is not overall the dates
                    foreach(int baseId in iBaseIds)
                    {
                        baseItem = hotelPrice.getBaseById(baseId.ToString());
                        finalPrices.AddRange(getFinalPrices(iAgencyId, iSystemType, iClerkId, 
                                                            fromDate, toDate, hotelPrice, 
                                                            composition, baseItem, ignoreBsarePerDatesPeriod));
                    }
                }
            }
        }

        string finalPricesXml = string.Empty;
        finalPricesXml += "<Root>";

        foreach (FinalPrice finalPrice in finalPrices)
        {
            finalPricesXml += finalPrice.toXmlString();
        }
        finalPricesXml += "</Root>";

        return finalPricesXml;
    }

    [WebMethod]
        public string getHotelPricePerAreaWithoutSaleCycle(string iAgencyId, string iSystemType, string iClerkId, 
										   string iAreaId, string iSupplierId, string iPriceType, string iLanguage,
                                           string iFromDate, string iToDate, string iCompositionId, string iBaseId)
        {
			setAgencyData(iAgencyId, iSystemType);
		
            List<FinalPrice> finalPrices = new List<FinalPrice>();
            List<string> suppliersIds;
			
			if (iSupplierId == "0")
			{
				if (iAgencyId == "85") //For Shikum.
				{
					suppliersIds = Utils.getSuppliersIdsListByGovArea(iAreaId, iLanguage);
				}
				else
				{
					//Change to get general hotels
					suppliersIds = Utils.getSuppliersIdsListByArea(iAreaId, iLanguage);
				}
			}
			else
			{
				suppliersIds = new List<string>();
				suppliersIds.Add(iSupplierId);
			}
			
			HotelPrice hotelPrice = null;
            DateTime fromDate, toDate;
            Composition composition = null;
            Base baseItem = null;

            try
            {
                fromDate = DateTime.Parse(iFromDate);
                toDate = DateTime.Parse(iToDate);
            }
            catch
            {
                Logger.Log("Failed to parse dates. fromDate = " + iFromDate + ", toDate = " + iToDate);
                throw new Exception("Failed to parse dates. fromDate = " + iFromDate + ", toDate = " + iToDate);
            }

            foreach (string supplierId in suppliersIds)
            {
				//Logger.Log("supplierId = " + supplierId);
                hotelPrice = Utils.getHotelPriceDetails(supplierId);
                if (hotelPrice != null)
                {
                    composition = hotelPrice.getCompositionById(iCompositionId);
                    if (!string.IsNullOrEmpty(iBaseId) && iBaseId != "0")
                    {
                        baseItem = hotelPrice.getBaseById(iBaseId);
                    }

                    if (baseItem != null)
                    {
                        finalPrices.AddRange(getFinalPricesWithoutSaleCycle(iAgencyId, iSystemType, iClerkId, fromDate, toDate, hotelPrice, composition, baseItem));
                    }
                    else
                    {
                        foreach (Base baseToSearch in hotelPrice.mBases.Keys)
                        {
                            finalPrices.AddRange(getFinalPricesWithoutSaleCycle(iAgencyId, iSystemType, iClerkId, fromDate, toDate, hotelPrice, composition, baseToSearch));
                        }
                    }
                }
            }

            string finalPricesXml = string.Empty;
            finalPricesXml += "<Root>";

            foreach (FinalPrice finalPrice in finalPrices)
            {
                finalPricesXml += finalPrice.toXmlString();
            }
            finalPricesXml += "</Root>";

            return finalPricesXml;
        }

        private List<FinalPrice> getFinalPrices(string iAgencyId, string iSystemType, string iClerkId, 
												DateTime iFromDate, DateTime iToDate, HotelPrice iHotelPrice, 
												Composition iComposition, Base iBase, bool iIsIgnoreBaseIdByDatePeriod = false)
        {			
            FinalPrice finalPrice = null;
            List<FinalPrice> finalPrices = new List<FinalPrice>();
				
            foreach (RoomType roomType in iHotelPrice.mRoomTypes.Keys)
            {
                try
                {
                    //GAL
                    finalPrice = getFinalPrice(iHotelPrice, iFromDate, iToDate, iComposition, iBase, roomType, iIsIgnoreBaseIdByDatePeriod);
                    finalPrices.Add(finalPrice);
                }
                catch (Exception exGetPrice)
                {
					Logger.Log("a = " + iHotelPrice.mSupplierId);
					Logger.Log("b = " + iComposition.getId());
					Logger.Log("c = " + iBase.getId());
					Logger.Log("d = " + roomType.getId());
                    //Dont throw exception to get all prices exept this value.
                    Logger.Log(string.Format("Did not get finalPrice for supplier {0}, compId = {1}, baseId = {2}, roomTypeId = {3}",
                                                iHotelPrice.mSupplierId, iComposition.getId(), iBase.getId(), roomType.getId()) + exGetPrice.Message);
                }
            }

            return finalPrices;
        }
		
		private List<FinalPrice> getFinalPricesWithoutSaleCycle(string iAgencyId, string iSystemType, string iClerkId, DateTime iFromDate, DateTime iToDate, HotelPrice iHotelPrice, Composition iComposition, Base iBase)
        {
            FinalPrice finalPrice = null;
            List<FinalPrice> finalPrices = new List<FinalPrice>();
				
            foreach (RoomType roomType in iHotelPrice.mRoomTypes.Keys)
            {
                try
                {
					finalPrice = getFinalPriceWithoutSaleCycle(iHotelPrice, iFromDate, iToDate, iComposition, iBase, roomType);
					finalPrices.Add(finalPrice);
                }
                catch (Exception exGetPrice)
                {
					Logger.Log("a = " + iHotelPrice.mSupplierId);
					Logger.Log("b = " + iComposition.getId());
					Logger.Log("c = " + iBase.getId());
					Logger.Log("d = " + roomType.getId());
                    //Dont throw exception to get all prices exept this value.
                    Logger.Log(string.Format("Did not get finalPrice for supplier {0}, compId = {1}, baseId = {2}, roomTypeId = {3}",
                                                iHotelPrice.mSupplierId, iComposition.getId(), iBase.getId(), roomType.getId()) + exGetPrice.Message);
                }
            }

            return finalPrices;
        }
		
		private FinalPrice getFinalPriceWithoutSaleCycle(HotelPrice iHotelPrice, DateTime iFromDate, DateTime iToDate, Composition iComposition, Base iBase, RoomType iRoomType)
        {
            int nights = (iToDate - iFromDate).Days;
            bool hasSaleCycle;
            DateTime currentDate = iFromDate;
            List<PricePerDay> pricesPerDays = new List<PricePerDay>();
            FinalPrice finalPrice = new FinalPrice(iComposition, iBase, iRoomType, iHotelPrice.mCommission, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mCurrency);

            finalPrice.mComposition = iComposition;
            finalPrice.mBase = iBase;
            finalPrice.mRoomType = iRoomType;
            finalPrice.mSupplierId = iHotelPrice.mSupplierId;

            if (iHotelPrice.mCompositions[iComposition] && iHotelPrice.mBases[iBase] && iHotelPrice.mRoomTypes[iRoomType])
            {
                try
                {
                    PricePerDay tempDatePrice = null, firstDatePrice;
                    firstDatePrice = Utils.getSpecificDatePriceDetails(iHotelPrice.mSupplierId, iFromDate);
                    if (firstDatePrice != null)
                    {
                        //hasSaleCycle = firstDatePrice.mSalesCycles.Find(x => x == nights) != 0;
                        //
						//if (hasSaleCycle)
						//{
							while (currentDate != iToDate)
							{
								tempDatePrice = Utils.getSpecificDatePriceDetails(iHotelPrice.mSupplierId, currentDate);
								if (tempDatePrice != null)// && tempDatePrice.mStatus)
								{
									pricesPerDays.Add(tempDatePrice);
									decimal pricePerCombo = tempDatePrice.getFinalPriceForCombo(iHotelPrice.mSupplierId, iComposition, iBase, iRoomType, iHotelPrice.mIsBaseAmount, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mGeneralAdditive, iHotelPrice.mBabiesAdditive);
									finalPrice.mFinalPrice += pricePerCombo;
									decimal priceNettoPerCombo = tempDatePrice.getFinalPriceNettoForCombo(iHotelPrice.mSupplierId, iComposition, iBase, iRoomType, iHotelPrice.mIsBaseAmount, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mGeneralAdditive, iHotelPrice.mBabiesAdditive);
									finalPrice.mFinalPriceNetto += priceNettoPerCombo;
									int roomsLeft = -1; //Default value
									finalPrice.mFinalPricePerDay.Add(new FinalPricePerDay(currentDate, pricePerCombo, priceNettoPerCombo, roomsLeft, tempDatePrice.mStatus, tempDatePrice.mColor));
								}
								else
								{
									//finalPrice.ErrorDescription = "4|" + Utils.getTextFromFile("DateIsClose", eLanguage.Hebrew);
									//finalPrice.ErrorDescription = finalPrice.ErrorDescription.Replace("**currentDate**", currentDate.ToString("dd/MM/yy"));
									finalPrice.ErrorDescription = "4|" + "**currentDate** Date is null";
									finalPrice.ErrorDescription = finalPrice.ErrorDescription.Replace("**currentDate**", currentDate.ToString("dd/MM/yy"));
									break;
								}

								currentDate = currentDate.AddDays(1);
							}
						//}
						//else
						//{
						//	finalPrice.ErrorDescription = "5|" + Utils.getTextFromFile("NoSaleCycle", eLanguage.Hebrew);
						//	finalPrice.ErrorDescription = finalPrice.ErrorDescription.Replace("**fromDate**", iFromDate.ToString("dd/MM/yy"));
						//	int roomsLeft = -1;
						//	decimal priceByCombo = firstDatePrice.getFinalPriceForCombo(iHotelPrice.mSupplierId, iComposition, iBase, iRoomType, iHotelPrice.mIsBaseAmount, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mGeneralAdditive, iHotelPrice.mBabiesAdditive);
						//	finalPrice.mFinalPricePerDay.Add(new FinalPricePerDay(firstDatePrice.mDate, priceByCombo, roomsLeft, firstDatePrice.mStatus, firstDatePrice.mColor));
						//}
					}
                    else
                    {
                        finalPrice.ErrorDescription = "8|" + Utils.getTextFromFile("DateNotDefined", eLanguage.Hebrew);
						finalPrice.ErrorDescription = finalPrice.ErrorDescription.Replace("**currentDate**", currentDate.ToString("dd/MM/yy"));
                        finalPrice.mFinalPricePerDay.Add(new FinalPricePerDay(iFromDate, 0, 0, 0, false, string.Empty));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception = " + ex.Message);
                    throw new Exception("Failed to get all base prices. exception = " + ex.Message);
                }
            }
            else
            {
                if (iComposition == null || !iHotelPrice.mCompositions[iComposition])
                {
                    finalPrice.ErrorDescription = "1|"  + Utils.getTextFromFile("CompositionNotFound", eLanguage.Hebrew);
                }
                else if (iBase == null || !iHotelPrice.mBases[iBase])
                {
                    finalPrice.ErrorDescription = "2|" + Utils.getTextFromFile("BaseNotFound", eLanguage.Hebrew);
                }
                else if (iRoomType == null || !iHotelPrice.mRoomTypes[iRoomType])
                {
                    finalPrice.ErrorDescription = "3|" + Utils.getTextFromFile("RoomTypeNotFound", eLanguage.Hebrew);
                }
            }

            return finalPrice;
        }

        private FinalPrice getFinalPrice(HotelPrice iHotelPrice, DateTime iFromDate, DateTime iToDate, 
                                         Composition iComposition, Base iBase, RoomType iRoomType,
                                         bool iIsIgnoreBaseIdByDatePeriod = false)
        {
            int nights = (iToDate - iFromDate).Days;
            bool hasSaleCycle;
            DateTime currentDate = iFromDate;
            List<PricePerDay> pricesPerDays = new List<PricePerDay>();
            FinalPrice finalPrice = new FinalPrice(iComposition, iBase, iRoomType, iHotelPrice.mCommission, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mCurrency);

            finalPrice.mComposition = iComposition;
            finalPrice.mBase = iBase;
            finalPrice.mRoomType = iRoomType;
            finalPrice.mSupplierId = iHotelPrice.mSupplierId;
            
			if (iHotelPrice.mCompositions[iComposition] && iHotelPrice.mBases[iBase] && iHotelPrice.mRoomTypes[iRoomType])
            {
                try
                {
                    PricePerDay tempDatePrice = null, firstDatePrice;
                    firstDatePrice = Utils.getSpecificDatePriceDetails(iHotelPrice.mSupplierId, iFromDate);
                    if (firstDatePrice != null)
                    {
						int saleNightsCount = 0;
						int saleNights = 0;
						decimal saleNightsFree = 0;
						bool isSaleNightFree = hasSaleNightFree(iHotelPrice.mSupplierId, iFromDate, iToDate, out saleNights, out saleNightsFree);
						bool isSaleNightFreeLessThan1Night = false;
						bool isSaleNightFreeMoreThan1Night = false;
						int orderNights = (iToDate - iFromDate).Days;
						
						//Logger.Log("isSaleNightFree = " + isSaleNightFree);
						//Logger.Log("saleNights = " + saleNights);
						//Logger.Log("saleNightsFree = " + saleNightsFree);
						if (isSaleNightFree)
						{
							if (saleNightsFree <= 1)
							{
								isSaleNightFreeLessThan1Night = true;
							}
							else if (saleNightsFree > 1)
							{
								isSaleNightFreeMoreThan1Night = true;
							}
						}
						
                        hasSaleCycle = firstDatePrice.mSalesCycles.Find(x => x == nights) != 0;
						
						if (hasSaleCycle)
						{
							while (currentDate != iToDate)
							{
								saleNightsCount++;
								
								tempDatePrice = Utils.getSpecificDatePriceDetails(iHotelPrice.mSupplierId, currentDate);
								if (tempDatePrice != null && tempDatePrice.mStatus && !iIsIgnoreBaseIdByDatePeriod ||
                                    !(iIsIgnoreBaseIdByDatePeriod && tempDatePrice.AvailableCombos.Find(x => x.mBase.mId == iBase.mId) == null))
								{
									pricesPerDays.Add(tempDatePrice);
									decimal pricePerCombo = tempDatePrice.getFinalPriceForCombo(iHotelPrice.mSupplierId, iComposition, iBase, iRoomType, iHotelPrice.mIsBaseAmount, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mGeneralAdditive, iHotelPrice.mBabiesAdditive);
									finalPrice.mFinalPrice += pricePerCombo;
									decimal priceNettoPerCombo = tempDatePrice.getFinalPriceNettoForCombo(iHotelPrice.mSupplierId, iComposition, iBase, iRoomType, iHotelPrice.mIsBaseAmount, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mGeneralAdditive, iHotelPrice.mBabiesAdditive);
									
									if (isSaleNightFree)
									{			
										decimal freeNightPercent = 1; //1, 0.2, 0.5 etc...
										
										if (isSaleNightFreeLessThan1Night && saleNightsCount % saleNights == 0)
										{
											freeNightPercent = saleNightsFree;
											//Logger.Log("here. freeNightPercent = " + freeNightPercent);
											priceNettoPerCombo = priceNettoPerCombo * (1 - freeNightPercent);
										}
										else if (isSaleNightFreeMoreThan1Night && saleNightsFree > 0)
										{
											//Every time decrease 1 night from saleNightsFree. if its got floating point set whats left 
											//(e.g 2.5: 2 nights free and the last 0.5 will be half price)
											if (saleNightsFree < 1 && saleNightsFree > 0)
											{
												freeNightPercent = saleNightsFree;
											}
											
											saleNightsFree--;
											priceNettoPerCombo = priceNettoPerCombo * (1 - freeNightPercent);
										}
									}
									
									finalPrice.mFinalPriceNetto += priceNettoPerCombo;
									int roomsLeft = -1; //Default value
									finalPrice.mFinalPricePerDay.Add(new FinalPricePerDay(currentDate, pricePerCombo, priceNettoPerCombo, roomsLeft, tempDatePrice.mStatus, tempDatePrice.mColor));
								}
								else
								{
                                    if((tempDatePrice == null || !tempDatePrice.mStatus) && !iIsIgnoreBaseIdByDatePeriod)
                                    {
									    finalPrice.ErrorDescription = "4|" + Utils.getTextFromFile("DateIsClose", eLanguage.Hebrew);
									    finalPrice.ErrorDescription = finalPrice.ErrorDescription.Replace("**currentDate**", currentDate.ToString("dd/MM/yy"));
									    break;                               
                                    }
								}

								currentDate = currentDate.AddDays(1);
							}
						}
						else
						{
							finalPrice.ErrorDescription = "5|" + Utils.getTextFromFile("NoSaleCycle", eLanguage.Hebrew);
							finalPrice.ErrorDescription = finalPrice.ErrorDescription.Replace("**fromDate**", iFromDate.ToString("dd/MM/yy"));
							int roomsLeft = -1;
							decimal priceByCombo = firstDatePrice.getFinalPriceForCombo(iHotelPrice.mSupplierId, iComposition, iBase, iRoomType, iHotelPrice.mIsBaseAmount, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mGeneralAdditive, iHotelPrice.mBabiesAdditive);
							decimal priceNettoByCombo = firstDatePrice.getFinalPriceNettoForCombo(iHotelPrice.mSupplierId, iComposition, iBase, iRoomType, iHotelPrice.mIsBaseAmount, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mGeneralAdditive, iHotelPrice.mBabiesAdditive);
							finalPrice.mFinalPricePerDay.Add(new FinalPricePerDay(firstDatePrice.mDate, priceByCombo, priceNettoByCombo, roomsLeft, firstDatePrice.mStatus, firstDatePrice.mColor));
						}
					}
                    else
                    {
                        if(firstDatePrice.AvailableCombos.Count == 0)
                        {
                            finalPrice.ErrorDescription = "2|" + Utils.getTextFromFile("BaseNotFound", eLanguage.Hebrew);
                        }
                        finalPrice.ErrorDescription = "8|" + Utils.getTextFromFile("DateNotDefined", eLanguage.Hebrew);
						finalPrice.ErrorDescription = finalPrice.ErrorDescription.Replace("**currentDate**", currentDate.ToString("dd/MM/yy"));
                        finalPrice.mFinalPricePerDay.Add(new FinalPricePerDay(iFromDate, 0, 0, 0, false, string.Empty));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception = " + ex.Message);
                    throw new Exception("Failed to get all base prices. exception = " + ex.Message);
                }
            }
            else
            {
                if (iComposition == null || !iHotelPrice.mCompositions[iComposition])
                {
                    finalPrice.ErrorDescription = "1|"  + Utils.getTextFromFile("CompositionNotFound", eLanguage.Hebrew);
                }
                else if (iBase == null || !iHotelPrice.mBases[iBase])
                {
                    finalPrice.ErrorDescription = "2|" + Utils.getTextFromFile("BaseNotFound", eLanguage.Hebrew);
                }
                else if (iRoomType == null || !iHotelPrice.mRoomTypes[iRoomType])
                {
                    finalPrice.ErrorDescription = "3|" + Utils.getTextFromFile("RoomTypeNotFound", eLanguage.Hebrew);
                }
            }

            return finalPrice;
        }

        [WebMethod]
        public string getHotelPricePerAreaWithAllocations(string iAgencyId, string iSystemType, string iClerkId, string iSupplierId, string iPriceType,
                                                          string iFromDate, string iToDate, string iCompositionId, string iBaseId , int iRoomsAmount, bool iIsSite)
        {
            List<FinalPrice> finalPrices = new List<FinalPrice>();
            HotelPrice hotelPrice = null;
            DateTime fromDate, toDate;
            Composition composition = null;
            Base baseItem = null;
            bool isCompositionExists = true;

            try
            {
                fromDate = DateTime.Parse(iFromDate);
                toDate = DateTime.Parse(iToDate);
            }
            catch
            {
                Logger.Log("Failed to parse dates. fromDate = " + iFromDate + ", toDate = " + iToDate);
                throw new Exception("Failed to parse dates. fromDate = " + iFromDate + ", toDate = " + iToDate);
            }

            hotelPrice = Utils.getHotelPriceDetails(iSupplierId);
            if (hotelPrice != null)
            {
                composition = hotelPrice.getCompositionById(iCompositionId);
                if (composition != null)
                {
                    if (!string.IsNullOrEmpty(iBaseId) && iBaseId != "0")
                    {
                        baseItem = hotelPrice.getBaseById(iBaseId);
                    }

                    if (baseItem != null)
                    {
                        finalPrices.AddRange(getFinalPricesWithAllocations(iAgencyId, iSystemType, iClerkId, fromDate, toDate, hotelPrice, composition, baseItem, iRoomsAmount, iIsSite));
                    }
                    else
                    {
                        foreach (Base baseToSearch in hotelPrice.mBases.Keys)
                        {
                            finalPrices.AddRange(getFinalPricesWithAllocations(iAgencyId, iSystemType, iClerkId, fromDate, toDate, hotelPrice, composition, baseToSearch, iRoomsAmount, iIsSite));
                        }
                    }
                }
                else
                {
                    FinalPrice finalPrice = new FinalPrice();

                    finalPrice.ErrorDescription = "1|" + Utils.getTextFromFile("CompositionNotFound", eLanguage.Hebrew);
                    finalPrices.Add(finalPrice);
                    isCompositionExists = false;
                }
            }

            string finalPricesXml = string.Empty;
            
            if (isCompositionExists)
            {
                foreach (FinalPrice finalPrice in finalPrices)
                {
                    finalPricesXml += finalPrice.toXmlString();
                }
            }

            return finalPricesXml;
        }

        private List<FinalPrice> getFinalPricesWithAllocations(string iAgencyId, string iSystemType, string iClerkId, DateTime iFromDate, DateTime iToDate, HotelPrice iHotelPrice, Composition iComposition, Base iBase, int iRoomsAmount, bool iIsSite)
        {
            FinalPrice finalPrice = null;
            List<FinalPrice> finalPrices = new List<FinalPrice>();

            foreach (RoomType roomType in iHotelPrice.mRoomTypes.Keys)
            {
                try
                {
                    finalPrice = getFinalPriceWithAllocations(iHotelPrice, iFromDate, iToDate, iComposition, iBase, roomType, iRoomsAmount, iIsSite);
                    finalPrices.Add(finalPrice);
                }
                catch (Exception exGetPrice)
                {
                    //Dont throw exception to get all prices exept this value.
                    Logger.Log(string.Format("Did not get finalPrice for supplier {0}, compId = {1}, baseId = {2}, roomTypeId = {3}",
                                                iHotelPrice.mSupplierId, iComposition.getId(), iBase.getId(), roomType.getId()) + exGetPrice.Message);
                }
            }

            return finalPrices;
        }

        private FinalPrice getFinalPriceWithAllocations(HotelPrice iHotelPrice, DateTime iFromDate, DateTime iToDate, Composition iComposition, Base iBase, RoomType iRoomType, int iRoomsAmount, bool iIsSite)
        {
            int nights = (iToDate - iFromDate).Days;
            bool hasSaleCycle;
            DateTime currentDate = iFromDate;
            List<PricePerDay> pricesPerDays = new List<PricePerDay>();
            FinalPrice finalPrice = new FinalPrice(iComposition, iBase, iRoomType, iHotelPrice.mCommission, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mCurrency);

            finalPrice.mComposition = iComposition;
            finalPrice.mBase = iBase;
            finalPrice.mRoomType = iRoomType;
            finalPrice.mSupplierId = iHotelPrice.mSupplierId;

            if (iHotelPrice.mCompositions[iComposition] && iHotelPrice.mBases[iBase] && iHotelPrice.mRoomTypes[iRoomType])
            {
                try
                {
                    PricePerDay tempDatePrice = null, firstDatePrice;
                    firstDatePrice = Utils.getSpecificDatePriceDetails(iHotelPrice.mSupplierId, iFromDate);
                    if (firstDatePrice != null)
                    {
                        hasSaleCycle = firstDatePrice.mSalesCycles.Find(x => x == nights) != 0;

                        if (hasSaleCycle)
                        {
                            if (isMonthlyAllocationEnough(iHotelPrice, iFromDate, iToDate, nights, iRoomsAmount))
                            {
                                while (currentDate != iToDate)
                                {
                                    tempDatePrice = Utils.getSpecificDatePriceDetails(iHotelPrice.mSupplierId, currentDate);
                                    if (tempDatePrice != null && tempDatePrice.mStatus)
                                    {
                                        int roomsLeft = 0;
                                        roomsLeft = tempDatePrice.mAllocation.mRoomsAmount - tempDatePrice.mAllocation.mRoomsInUse;

                                        if (iIsSite)
                                        {
                                            roomsLeft -= tempDatePrice.mAllocation.mRoomsDisable;
                                        }

                                        if (roomsLeft - iRoomsAmount >= 0)
                                        {
                                            pricesPerDays.Add(tempDatePrice);
                                            finalPrice.mFinalPrice += tempDatePrice.getFinalPriceForCombo(iHotelPrice.mSupplierId, iComposition, iBase, iRoomType, iHotelPrice.mIsBaseAmount, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mGeneralAdditive, iHotelPrice.mBabiesAdditive);
											finalPrice.mFinalPriceNetto += tempDatePrice.getFinalPriceNettoForCombo(iHotelPrice.mSupplierId, iComposition, iBase, iRoomType, iHotelPrice.mIsBaseAmount, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mGeneralAdditive, iHotelPrice.mBabiesAdditive);
                                        }
                                        else
                                        {
                                            finalPrice.ErrorDescription = "6|" + Utils.getTextFromFile("NotEnoughRooms", eLanguage.Hebrew);
											finalPrice.ErrorDescription = finalPrice.ErrorDescription.Replace("**currentDate**", currentDate.ToString("dd/MM/yy"));
                                        }

										decimal priceByCombo = tempDatePrice.getFinalPriceForCombo(iHotelPrice.mSupplierId, iComposition, iBase, iRoomType, iHotelPrice.mIsBaseAmount, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mGeneralAdditive, iHotelPrice.mBabiesAdditive);
										decimal priceNettoByCombo = tempDatePrice.getFinalPriceForCombo(iHotelPrice.mSupplierId, iComposition, iBase, iRoomType, iHotelPrice.mIsBaseAmount, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mGeneralAdditive, iHotelPrice.mBabiesAdditive);
                                        finalPrice.mFinalPricePerDay.Add(new FinalPricePerDay(tempDatePrice.mDate, priceByCombo, priceNettoByCombo, roomsLeft, tempDatePrice.mStatus, tempDatePrice.mColor));
                                    }
                                    else
                                    {
                                        finalPrice.ErrorDescription = "4|" + Utils.getTextFromFile("DateIsClose", eLanguage.Hebrew);
										finalPrice.ErrorDescription = finalPrice.ErrorDescription.Replace("**currentDate**", currentDate.ToString("dd/MM/yy"));
                                    }

                                    currentDate = currentDate.AddDays(1);
                                }
                            }
                            else
                            {
                                finalPrice.ErrorDescription = "7|" + Utils.getTextFromFile("MonthlyAllocationexceeded", eLanguage.Hebrew);
                            }
                        }
                        else
                        {
                            finalPrice.ErrorDescription = "5|" + Utils.getTextFromFile("NoSaleCycle", eLanguage.Hebrew);
							finalPrice.ErrorDescription = finalPrice.ErrorDescription.Replace("**fromDate**", iFromDate.ToString("dd/MM/yy"));
							int roomsLeft = 0;
							roomsLeft = firstDatePrice.mAllocation.mRoomsAmount - firstDatePrice.mAllocation.mRoomsInUse;
							decimal priceByCombo = firstDatePrice.getFinalPriceForCombo(iHotelPrice.mSupplierId, iComposition, iBase, iRoomType, iHotelPrice.mIsBaseAmount, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mGeneralAdditive, iHotelPrice.mBabiesAdditive);
							decimal priceNettoByCombo = firstDatePrice.getFinalPriceForCombo(iHotelPrice.mSupplierId, iComposition, iBase, iRoomType, iHotelPrice.mIsBaseAmount, iHotelPrice.mOfficeDiscount, iHotelPrice.mSiteDiscount, iHotelPrice.mGeneralAdditive, iHotelPrice.mBabiesAdditive);
							finalPrice.mFinalPricePerDay.Add(new FinalPricePerDay(firstDatePrice.mDate, priceByCombo, priceNettoByCombo, roomsLeft, firstDatePrice.mStatus, firstDatePrice.mColor));
                        }
                    }
                    else
                    {
                        finalPrice.ErrorDescription = "8|" + Utils.getTextFromFile("DateNotDefined", eLanguage.Hebrew);
						finalPrice.ErrorDescription = finalPrice.ErrorDescription.Replace("**currentDate**", currentDate.ToString("dd/MM/yy"));
                        finalPrice.mFinalPricePerDay.Add(new FinalPricePerDay(iFromDate, 0, 0, 0, false, string.Empty));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception = " + ex.Message);
                    throw new Exception("Failed to get all base prices. exception = " + ex.Message);
                }
            }
            else
            {
                if (iComposition == null || !iHotelPrice.mCompositions[iComposition])
                {
                    finalPrice.ErrorDescription = "1|" + Utils.getTextFromFile("", eLanguage.Hebrew);
                }
                else if (iBase == null || !iHotelPrice.mBases[iBase])
                {
                    finalPrice.ErrorDescription = "2|" + Utils.getTextFromFile("", eLanguage.Hebrew);
                }
                else if (iRoomType == null || !iHotelPrice.mRoomTypes[iRoomType])
                {
                    finalPrice.ErrorDescription = "3|" + Utils.getTextFromFile("", eLanguage.Hebrew);
                }
            }

            return finalPrice;
        }

        private bool isMonthlyAllocationEnough(HotelPrice iHotelPrice, DateTime iFromDate, DateTime iToDate, int iNights, int iRoomsAmount)
        {
            bool isMonthlyAllocationEnough = true;
            List<int> monthlyUsedAllocations = new List<int>();
            MonthlyAllocation monthlAllocation;

            if (Utils.isCrossMonth(iFromDate, iToDate))
            {
                int daysTillNextMonth = 0;
                int daysInNextMonth;
                int fromDateMonthlyUsed = Utils.getMonthlyUsedAllocations(iHotelPrice.mSupplierId, iFromDate.Month, iFromDate.Year);
                int toDateMonthlyUsed = Utils.getMonthlyUsedAllocations(iHotelPrice.mSupplierId, iToDate.Month, iToDate.Year);

                while (iFromDate.AddDays(daysTillNextMonth).Month != iToDate.Month)
                {
                    daysTillNextMonth++;
                }

                daysInNextMonth = iNights - daysTillNextMonth;             
                monthlAllocation = iHotelPrice.mMonthlyAllocations.Find(x => x.mMonth == iFromDate.Month && x.mYear == iFromDate.Year);
                if (monthlAllocation != null)
                {
                    if (fromDateMonthlyUsed + iRoomsAmount * daysTillNextMonth <= monthlAllocation.mMonthlyAllocation)
                    {
                        monthlAllocation = iHotelPrice.mMonthlyAllocations.Find(x => x.mMonth == iToDate.Month && x.mYear == iToDate.Year);
                        if (monthlAllocation != null)
                        {
                            if (toDateMonthlyUsed + iRoomsAmount * daysInNextMonth > monthlAllocation.mMonthlyAllocation)
                            {
                                isMonthlyAllocationEnough = false;
                            }
                        }
                        else
                        {
                            isMonthlyAllocationEnough = false;
                        }
                    }
                    else
                    {
                        isMonthlyAllocationEnough = false;
                    }
                }
                else
                {
                    isMonthlyAllocationEnough = false;
                }
            }
            else
            {
                int numOfRoomsForMonthlyAllocation = iNights * iRoomsAmount;
                int fromDateMonthlyUsed = Utils.getMonthlyUsedAllocations(iHotelPrice.mSupplierId, iFromDate.Month, iFromDate.Year);

                monthlAllocation = iHotelPrice.mMonthlyAllocations.Find(x => x.mMonth == iFromDate.Month && x.mYear == iFromDate.Year);
                if (monthlAllocation != null)
                {
                    if (fromDateMonthlyUsed + numOfRoomsForMonthlyAllocation > monthlAllocation.mMonthlyAllocation)
                    {
                        isMonthlyAllocationEnough = false;
                    }
                }
                else
                {
                    isMonthlyAllocationEnough = false;
                }
            }

            return isMonthlyAllocationEnough;
        }
		
			
		//[WebMethod]
        //public void xxx(string iLala)
        //{
        //    if (iLala == "chen12343219")
        //    {
        //        int lastId = 10733;
        //
        //        List<int> suppliers = new List<int>() { 2291, 44, 12, 576, 337,32,575,15,33,293,24,2902,406
        //            ,2288,25,4857,17,50,8,2286,2895,41,40,2742,10066,15600,9831,39,31,577
        //            ,9716,277,14057,12206,14543,96,2741,13748,97,4859
        //            ,2099,4860,102,99,2050,107,335,326,6321,
        //            131,382,2323,756,146,112,329,114,2298,161,300,309,183,9240,2759,178,176,2353,628,2544,4864,
        //            2299,134,15961,2345,92,70,14813,9502,95,90,266,4872,239,
        //            241,267,16849,10164,14452,237,231,
        //            2308,311,306,220,2311,197,2756,2307,12693,2545,214,2760,209, 17238,
		//			15748, 16685, 13226, 2487, 3015};
        //
        //        Dictionary<int, int> compositions = new Dictionary<int, int>() { 
        //        { 1, 120 }, 
        //        {2, 200}, 
        //        {3,290},
        //        {4,245},
        //        {5,290},
        //        {10,150},
        //        {11,195},
        //        {2334,240},
        //        {2337,120},
        //        {2338,120},
        //        {2339,120},
        //        {2340,120},
        //        {2341,120},
        //        {2342,120},
		//		{2343, 150},
		//		{2344, 150},
		//		{78, 200},
		//		{113, 200},
		//		{2345, 195},
		//		{2346, 245}
        //        };
        //            
        //        string record;
        //        string query;
        //
        //        foreach (int supplierId in suppliers)
        //        {
        //            foreach (int composition in compositions.Keys)
        //            {
        //                record = DAL_SQL.GetRecord("AGN_INN_0088.dbo.SUPPLIER_TO_ROOMS_PERCENT", "room_id", supplierId + " and room_id = " + composition, "supplier_id");
        //                if (string.IsNullOrEmpty(record))
        //                {
        //                    query = string.Format(@"INSERT INTO SUPPLIER_TO_ROOMS_PERCENT (id, supplier_id, room_id, percent_value)
        //                                                  VALUES({0}, {1}, {2}, {3})", lastId, supplierId, composition, compositions[composition]);
        //                    DAL_SQL.RunSql(query);
        //                    lastId++;
        //                }
        //            }
        //        }
        //    }
        //}

		[WebMethod]
        public string getHotelPriceAgencyXml(string iAgencyId, string iSystemType, string iClerkId, string iAreaId, string iSupplierId, string iPriceType, string iLanguage,
                                                     string iFromDate, string iToDate, string iCompositionId, string iBaseId)
        {
			setAgencyData(iAgencyId, iSystemType);
		
            List<FinalPrice> finalPrices = new List<FinalPrice>();
            List<string> suppliersIds;
			
			if (iSupplierId == "0")
			{
				if (iAgencyId == "85") //For Shikum.
				{
					suppliersIds = Utils.getSuppliersIdsListByGovArea(iAreaId, iLanguage);
				}
				else
				{
					//Change to get general hotels
					suppliersIds = Utils.getSuppliersIdsListByArea(iAreaId, iLanguage);
				}
			}
			else
			{
				suppliersIds = new List<string>();
				suppliersIds.Add(iSupplierId);
			}
				
			HotelPrice hotelPrice = null;
            DateTime fromDate, toDate;
            Composition composition = null;
            Base baseItem = null;

            try
            {
                fromDate = DateTime.Parse(iFromDate);
                toDate = DateTime.Parse(iToDate);
            }
            catch
            {
                Logger.Log("Failed to parse dates. fromDate = " + iFromDate + ", toDate = " + iToDate);
                throw new Exception("Failed to parse dates. fromDate = " + iFromDate + ", toDate = " + iToDate);
            }

            foreach (string supplierId in suppliersIds)
            {
                hotelPrice = Utils.getHotelPriceDetails(supplierId);
                if (hotelPrice != null)
                {
					Logger.Log("iCompositionId = " + iCompositionId);
                    composition = hotelPrice.getCompositionById(iCompositionId);
                    if (!string.IsNullOrEmpty(iBaseId) && iBaseId != "0")
                    {
                        baseItem = hotelPrice.getBaseById(iBaseId);
                    }

                    if (baseItem != null)
                    {
                        finalPrices.AddRange(getFinalPrices(iAgencyId, iSystemType, iClerkId, fromDate, toDate, hotelPrice, composition, baseItem));
                    }
                    else
                    {
                        foreach (Base baseToSearch in hotelPrice.mBases.Keys)
                        {
                            finalPrices.AddRange(getFinalPrices(iAgencyId, iSystemType, iClerkId, fromDate, toDate, hotelPrice, composition, baseToSearch));
                        }
                    }
                }
            }

            string finalPricesXml = string.Empty;
            finalPricesXml += "<Root>";

            foreach (FinalPrice finalPrice in finalPrices)
            {
                finalPricesXml += finalPrice.toXmlString();
            }
            finalPricesXml += "</Root>";
						
            return finalPricesXml;
        }
			
		private void setAgencyData(string iAgencyId, string iSystemType)
		{
			DAL_SQL.ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"];
			DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^agencyId^", ((iAgencyId.Length == 1) ? "000" + iAgencyId : "00" + iAgencyId));
			DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^systemType^", ((iSystemType == "3") ? "INN" : (iSystemType == "2") ? "ICC" :"OUT"));
		}
		
		private bool hasSaleNightFree(string iSupplierId, DateTime iFromDate, DateTime iToDate, out int iSaleNight, out decimal iSaleNightFree)
		{
			bool hasSaleNightFree = false;
			iSaleNight = 0;
			iSaleNightFree = 0M;
			
			string query = string.Format(@"
			SELECT nights, nights_free
			FROM P_HOTEL_PRICE_SALES_FREE_NIGHTS
			WHERE hotel_price_id = (SELECT id FROM P_HOTEL_PRICES WHERE supplier_id = {0})
				  and from_date <= '{1}'
				  and to_date >= '{2}'
				  and nights <= {3}
				  and status = 1
			Order by nights desc",
				  iSupplierId,
				  iFromDate.ToString("dd-MMM-yy"),
				  iToDate.ToString("dd-MMM-yy"),
				  (iToDate - iFromDate).Days);
			
			//Logger.Log("query = " + query);
			
			DataSet ds = DAL_SQL.RunSqlDataSet(query);
			
			if (Utils.isDataSetRowsNotEmpty(ds))
			{
				iSaleNight = int.Parse(ds.Tables[0].Rows[0]["nights"].ToString());
				iSaleNightFree = decimal.Parse(ds.Tables[0].Rows[0]["nights_free"].ToString());
				hasSaleNightFree = true;
			}
  
			return hasSaleNightFree;
		}
    }
//}
