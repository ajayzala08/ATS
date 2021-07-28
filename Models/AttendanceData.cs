using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ATS2019_2.Models
{
    public class AttendanceData
    {
        public List<String> intime { get; set; }
        public List<String> outtime { get; set; }
        public List<String> totaltime { get; set; }
        public List<String> firstHalf { get; set; }
        public List<String> secondHalf { get; set; }
        public List<String> attendanceDate { get; set; }
        public List<String> shiftName { get; set; }
        public List<string> missingpunchstatus { get; set; }
        public List<string> daysname { get; set; }
        public float totalpresent { get; set; }
        public float totalabsent { get; set; }
        public int? totalweekoff { get; set; }
        public int totalLatecount { get; set; }
        public int totalMissingPunch { get; set; }
        public string status { get; set; }
    }
}