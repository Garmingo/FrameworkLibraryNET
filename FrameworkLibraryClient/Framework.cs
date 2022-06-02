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

        private string[] frameworks = new string[] {"None", "ESX Legacy", "ESX Infinity", "QBCore"};
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
                    if (config.Framework == "ESX Legacy")
                    {
                        EventHandlers["esx:playerLoaded"] += new Action<ExpandoObject>((playerData) =>
                        {
                            InitializeFramework();
                        });
                    }
                    else
                    {
                        InitializeFramework();
                    }
                }
            }
        }


        private void Msg(string message)
        {
            Debug.WriteLine("[" + GetCurrentResourceName() + "] Framework Detection: " + message);
        }

        private async void InitializeFramework()
        {
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
                    }



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
                        }
                        finished = jobName != null;
                    }
                    catch (Exception)
                    {
                        Msg("Waiting for initialization of selected framework " + config.Framework);
                    }
                }
                catch (Exception)
                {

                }

                await Delay(500);
            }
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
                return framework.Functions.GetPlayerData().job.grade;
            }
            else
            {
                return 0;
            }
        }

        

    }
}
