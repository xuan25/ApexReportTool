using System.Text;

namespace ApexReportTool
{
    /// <summary>
    /// A class for Generating report details
    /// Author: Xuan525
    /// Date: 09/03/2019
    /// </summary>
    class ReportDetails
    {
        public string HackerName, Details;
        public bool WallHack, Aimbot;
        public ReportDetails(string hackerName, bool wallHack, bool aimbot, string details)
        {
            HackerName = hackerName;
            WallHack = wallHack;
            Aimbot = aimbot;
            Details = details;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Cheater's in-game Id: ");
            stringBuilder.AppendLine(HackerName);
            stringBuilder.AppendLine("--------");
            if (WallHack)
                stringBuilder.AppendLine("- Wall hack");
            if (Aimbot)
                stringBuilder.AppendLine("- Aimbot");
            if(Details != null && Details.Trim() != "")
            {
                stringBuilder.AppendLine("--------");
                stringBuilder.AppendLine(Details);
            }
            return stringBuilder.ToString();
        }
    }
}
