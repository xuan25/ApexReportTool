using System.Text;

namespace ApexReportTool
{
    /// <summary>
    /// A class for Generating report details
    /// Author: Xuan525
    /// Date: 14/03/2019
    /// </summary>
    class ReportDetails
    {
        public string HackerName, Details;
        public bool WallHack, Aimbot, SpeedHacked, DamageHacked;
        public ReportDetails(string hackerName, bool wallHack, bool aimbot, bool speedHacked, bool damageHacked, string details)
        {
            HackerName = hackerName;
            WallHack = wallHack;
            Aimbot = aimbot;
            SpeedHacked = speedHacked;
            DamageHacked = damageHacked;
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
            if (SpeedHacked)
                stringBuilder.AppendLine("- Movement speed hacked");
            if (DamageHacked)
                stringBuilder.AppendLine("- Damage hacked");
            if (Details != null && Details.Trim() != "")
            {
                stringBuilder.AppendLine("--------");
                stringBuilder.AppendLine(Details);
            }
            return stringBuilder.ToString();
        }
    }
}
