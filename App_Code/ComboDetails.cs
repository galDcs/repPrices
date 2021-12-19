using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

//namespace AgencyPricesWS
//{
    public class ComboDetails
    {
        public string mId;
        public string mName;

        public ComboDetails()
        {
            mId = string.Empty;
            mName = string.Empty;
        }

        public string getId()
        {
            return mId;
        }

        public string getName()
        {
            return mName;
        }

        protected void setId(string id)
        {
            mId = id;
        }

        protected void setName(string name)
        {
            mName = name;
        }
    }
//}