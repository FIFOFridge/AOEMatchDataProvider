using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOEMatchDataProvider.Models.RequestService
{
    public class RequestWrapper<TValue> where TValue : class
    {
        public RequestResponseWrapper RequestResponseWrapper { get; set; }
        public TValue Value { get; set; }
        public Exception Exception { get; set; }
        public string RequestUrl { get; set; }
        //public bool IsSuccess => RequestResponseWrapper.IsSuccess && Exception == null;
        public bool IsSuccess
        {
            get
            {
                if (RequestResponseWrapper == null)
                    throw new NotImplementedException("Wrong implementation of RequestResponseWrapper");

                return RequestResponseWrapper.IsSuccess && Exception == null;
            }
        }

    }
}
