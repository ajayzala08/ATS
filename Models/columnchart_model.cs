using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ATS2019_2.Models
{
    [DataContract]
    public class columnchart_model
    {
        //DataContract for Serializing Data - required to serve in JSON format
        public columnchart_model(string label, int y)
        {
            this.label = label;
            this.Y = y;
        }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "label")]
        public string label = null;

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "y")]
        public Nullable<int> Y = null;
    }
}