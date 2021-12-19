using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class IPSTraveller
{
    public string mClientCode { get; set; }
    public string mId { get; set; }
    public string mLastName { get; set; }
    public string mFirstName { get; set; }
    public string mGenderId { get; set; }
    public string mPersonalNumber { get; set; }
    public string mMaritalStatusId { get; set; }
    public string mKidsUpTo2 { get; set; }
    public string mKidsFrom2To10 { get; set; }
    public string mKidsFrom10To21 { get; set; }
    public string mUnitNumber { get; set; }
    public string mPhone { get; set; }
    public string mCellPhone { get; set; }
    public string mPositionPercentage { get; set; }
    public string mEntitlementStartDate { get; set; }
    public string mEntitlementEndDate { get; set; }
    public string mVacationType { get; set; }
    public string mRecordNumber { get; set; }
    public string mMail { get; set; }
    public List<Escort> mEscorts { get; set; }

    public IPSTraveller()
    {

    }

    public class Escort
    {
        public string mId { get; set; }
        public string mLastName { get; set; }
        public string mFirstName { get; set; }
        public bool mIsMain { get; set; }
    }
}