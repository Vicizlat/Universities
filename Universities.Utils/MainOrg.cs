using System;
using System.Linq;

namespace Universities.Utils
{
    public static class MainOrg
    {
        public static int Id { get; set; } = 0;
        public static string Name { get; set; } = string.Empty;
        public static string Preffix { get; set; } = string.Empty;
        public static int OrgStartId { get; set; } = 0;
        public static int PeopleStartId { get; set; } = 0;

        public static bool CheckMainOrg(string[] mainOrgs, string mainOrgPref)
        {
            if (mainOrgs.Length == 0) return PromptBox.Error("No Main Organizations found.");
            string[] mainOrg = mainOrgs.Select(o => o.Split(";")).FirstOrDefault(mo => mo[2] == mainOrgPref)?.ToArray() ?? Array.Empty<string>();
            if (mainOrg.Length == 0) return PromptBox.Error("No Main Organization selected or matching your saved settings. Please, select one from the list.");
            Id = int.Parse(mainOrg[0]);
            Name = mainOrg[1];
            Preffix = mainOrg[2];
            OrgStartId = int.Parse(mainOrg[3]);
            PeopleStartId = int.Parse(mainOrg[4]);
            return true;
        }

        public static string[] ToArray()
        {
            return new string[]
            {
                $"{Id}",
                Name,
                Preffix,
                $"{OrgStartId}",
                $"{PeopleStartId}"
            };
        }
    }
}