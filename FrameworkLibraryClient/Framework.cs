using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;

namespace FrameworkLibraryClient
{
    public class Framework : BaseScript
    {
        public FrameworkConfig config =
            JsonConvert.DeserializeObject<FrameworkConfig>(LoadResourceFile(GetCurrentResourceName(), "framework.json"));

        private string[] frameworks = new string[] {"None", "ESX Legacy", "ESX Infinity", "QBCore", "Custom"};
        private dynamic framework;

        public Framework()
        {
            Msg((config.Framework == "None" ? "No " : (frameworks.Contains(config.Framework) ? "" : "Unsupported ")) + "Framework " + (config.Framework == "None" ? "" : (config.Framework + " ")) + "is selected.");
            if (!frameworks.Contains(config.Framework))
            {
                Msg("Unsupported Framework has been detected and restored to default value \"None\"");
            }
            else
            {
                if (config.Framework != "None")
                {
                    InitializeFramework();
                }
            }
        }


        private void Msg(string message)
        {
            if (config.ShowDebugInfo)
            {
                Debug.WriteLine("[" + GetCurrentResourceName() + "] Framework Detection: " + message);
            }
        }

        private async void InitializeFramework()
        {
            Msg("Starting initialization for Framework " + config.Framework);
            bool finished = false;
            while (!finished)
            {
                try
                {
                    switch (config.Framework)
                    {
                        case "ESX Legacy":
                            framework = Exports["es_extended"].getSharedObject();

                            break;
                        case "ESX Infinity":
                            TriggerEvent(config.ESXEvent,
                                new object[] {new Action<dynamic>(esx => { framework = esx; })});
                            break;
                        case "QBCore":
                            framework = Exports["qb-core"].GetCoreObject();
                            break;
                        case "Custom":
                            framework = new ExpandoObject();
                            break;
                    }



                    string jobName = null;
                    switch (config.Framework)
                    {
                        case "ESX Legacy":
                            jobName = framework.GetPlayerData().job.name;
                            break;
                        case "ESX Infinity":
                            jobName = framework.GetPlayerData().job.name;
                            break;
                        case "QBCore":
                            jobName = framework.Functions.GetPlayerData().job
                                .name;
                            break;
                        case "Custom":
                            jobName = Exports[config.ExportResource].GetPlayerJobName();
                            break;
                    }

                    finished = jobName != null;

                }
                catch (Exception)
                {

                }

                await Delay(1000);
            }

            Msg("Finished initialization for Framework " + config.Framework);
        }

        public bool IsInitialized()
        {
            try
            {
                string jobName = null;
                switch (config.Framework)
                {
                    case "ESX Legacy":
                        jobName = framework.GetPlayerData().job.name;
                        break;
                    case "ESX Infinity":
                        jobName = framework.GetPlayerData().job.name;
                        break;
                    case "QBCore":
                        jobName = framework.Functions.GetPlayerData().job
                            .name;
                        break;
                    case "None":
                        return true;
                }

                return jobName != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetPlayerJobName()
        {
            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                return framework.GetPlayerData().job.name;
            }
            else if (config.Framework == "QBCore")
            {
                return framework.Functions.GetPlayerData().job.name;
            }
            else if (config.Framework == "Custom")
            {
                return Exports[config.ExportResource].GetPlayerJobName();
            }
            else
            {
                return "";
            }
        }

        public int GetPlayerJobGrade()
        {
            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                return framework.GetPlayerData().job.grade;
            }
            else if (config.Framework == "QBCore")
            {
                return framework.Functions.GetPlayerData().job.grade is int ? framework.Functions.GetPlayerData().job.grade : framework.Functions.GetPlayerData().job.grade.level;
            }
            else if (config.Framework == "Custom")
            {
                return Exports[config.ExportResource].GetPlayerJobGrade();
            }
            else
            {
                return 0;
            }
        }

        public int GetInventoryItemCount(string item)
        {
            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                foreach (var i in framework.GetPlayerData().inventory)
                {
                    if (i.name == item)
                    {
                        return i.count;
                    }
                }
                return 0;
            }
            else if (config.Framework == "QBCore")
            {
                return framework.Functions.GetPlayerData().Functions.GetItemsByName(item).amount;
            }
            else if (config.Framework == "Custom")
            {
                return Exports[config.ExportResource].GetInventoryItemCount(item);
            }
            else
            {
                return 0;
            }
        }

        public void AddKeys(string plate)
        {
            if (config.Framework == "QBCore")
            {
                TriggerEvent("qb-vehiclekeys:client:AddKeys", plate);
            }
        }

    }
}
