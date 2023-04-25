using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using CitizenFX.Core;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;

namespace FrameworkLibraryServer
{
    public class Framework : BaseScript
    {
        public FrameworkConfig config =
            JsonConvert.DeserializeObject<FrameworkConfig>(LoadResourceFile(GetCurrentResourceName(), "framework.json"));

        private string[] frameworks = new string[] {"None", "ESX Legacy", "ESX Infinity", "QBCore", "Custom"};
        private dynamic framework;
        public Framework()
        {
            Msg("Checking Framework configuration...");
            if (config.AutoDetect)
            {
                Msg("AutoDetect Framework is enabled. Trying to find a framework...");
                AutoDetect();
            }
            else
            {
                Msg("AutoDetect is disabled and " + (config.Framework == "None" ? "no " : (frameworks.Contains(config.Framework) ? "" : "unsupported ")) + "Framework " + (config.Framework == "None" ? "" : config.Framework) + " is selected.");
                if (!frameworks.Contains(config.Framework))
                {
                    config.Framework = "None";
                    SaveResourceFile(GetCurrentResourceName(), "framework.json", JsonConvert.SerializeObject(config), -1);
                    Msg("Unsupported Framework has been detected and restored to default value \"None\"");
                }
                else
                {
                    InitializeFramework();
                }
            }
        }

        private void AutoDetect()
        {
            try
            {
                
                if (Exports["qb-core"].GetCoreObject() != null)
                {
                    Msg("Detected QBCore.");
                    config.Framework = "QBCore";
                    config.AutoDetect = false;
                    SaveResourceFile(GetCurrentResourceName(), "framework.json", JsonConvert.SerializeObject(config),
                        -1);
                }
                else if (Exports["es_extended"].getSharedObject() != null)
                {
                    Msg("Detected ESX Legacy.");
                    config.Framework = "ESX Legacy";
                    config.AutoDetect = false;
                    SaveResourceFile(GetCurrentResourceName(), "framework.json", JsonConvert.SerializeObject(config),
                        -1);
                }
                else
                {
                    TriggerEvent(config.ESXEvent, new object[]
                    {
                        new Action<dynamic>(esx =>
                        {
                            try
                            {
                                var pl = esx.GetPlayers();
                                if (pl != null)
                                {
                                    Msg("Detected ESX Infinity.");
                                    config.Framework = "ESX Infinity";
                                    config.AutoDetect = false;
                                    SaveResourceFile(GetCurrentResourceName(), "framework.json",
                                        JsonConvert.SerializeObject(config), -1);
                                }
                            }
                            catch (Exception ignored)
                            {

                            }
                        })
                    });
                }

                if (!config.AutoDetect)
                {
                    Msg("Trying to restart the resource...");
                    StopResource(GetCurrentResourceName());
                    StartResource(GetCurrentResourceName());
                }
            } catch(Exception ignored)
            {

            }
        }

        private void Msg(string message)
        {
            Debug.WriteLine("[" + GetCurrentResourceName() + "] Framework Detection: " + message);
        }

        private void InitializeFramework()
        {
            switch (config.Framework)
            {
                case "ESX Legacy":
                    framework = Exports["es_extended"].getSharedObject();
                    
                    break;
                case "ESX Infinity":
                    TriggerEvent(config.ESXEvent, new object[] { new Action<dynamic>(esx =>
                    {
                        framework = esx;
                    })});
                    break;
                case "QBCore":
                    framework = Exports["qb-core"].GetCoreObject();
                    break;
                case "Custom":
                    framework = new ExpandoObject();
                    break;
                case "None":
                    framework = new ExpandoObject();
                    break;
            }
            if (framework == null)
            {
                Msg("Failed to initialize selected framework " + config.Framework);
            }
        }

        public int GetPlayerWalletMoney(Player source)
        {
            if (config.Framework == "ESX Infinity")
            {
                return (int) framework.GetPlayerFromId(source.Handle).GetMoney();
            } else if (config.Framework == "ESX Legacy")
            {
                return (int) framework.GetPlayerFromId(source.Handle).getMoney();

            }
            else if (config.Framework == "Custom")
            {
                return (int) Exports[config.ExportResource].GetPlayerWalletMoney(source.Handle);
            }
            else if (config.Framework == "QBCore")
            {
                int amount;
                try
                {
                    amount = (int) framework.Functions.GetPlayer(int.Parse(source.Handle)).PlayerData.money["cash"];
                }
                catch (Exception e)
                {
                    Msg("Unusual money storage detected. Trying fallback method...");
                    amount = (int) framework.Functions.GetPlayer(int.Parse(source.Handle)).PlayerData.money.cash;
                }
                return amount;
            }
            else
            {
                return 0;
            }
        }

        public int GetPlayerAccountMoney(Player source, string account)
        {
            if (config.Framework == "ESX Infinity")
            {
                return (int) framework.GetPlayerFromId(source.Handle).GetAccountMoney(account);
            }
            else if (config.Framework == "ESX Legacy")
            {
                return (int) (framework.GetPlayerFromId(source.Handle).getAccount(account).money ?? framework.GetPlayerFromId(source.Handle).GetAccount(account).money);
            }
            else if (config.Framework == "Custom")
            {
                return (int)Exports[config.ExportResource].GetPlayerAccountMoney(source.Handle, account);
            }
            else if (config.Framework == "QBCore")
            {
                int amount;
                try
                {
                    amount = (int) framework.Functions.GetPlayer(int.Parse(source.Handle)).PlayerData.money[account];
                }
                catch (Exception e)
                {
                    Msg("Unusual money storage detected. Trying fallback method...");
                    amount = (int) Extension.GetPropertyValue(
                        framework.Functions.GetPlayer(int.Parse(source.Handle)).PlayerData.money, account);
                }
                return amount;
            }
            else
            {
                return 0;
            }
        }

        public void AddPlayerWalletMoney(Player source, int amount)
        {
            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                framework.GetPlayerFromId(source.Handle).addMoney(amount);
            }
            else if (config.Framework == "QBCore")
            {
                framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions.AddMoney("cash", amount);
            }
            else if (config.Framework == "Custom")
            {
                Exports[config.ExportResource].AddPlayerWalletMoney(source.Handle, amount);
            }
        }

        public void AddPlayerAccountMoney(Player source, int amount, string account)
        {
            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                framework.GetPlayerFromId(source.Handle).addAccountMoney(account, amount);
            }
            else if (config.Framework == "QBCore")
            {
                framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions.AddMoney(account, amount);
            }
            else if (config.Framework == "Custom")
            {
                Exports[config.ExportResource].AddPlayerAccountMoney(source.Handle, amount, account);
            }
        }

        public void RemovePlayerWalletMoney(Player source, int amount)
        {
            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                framework.GetPlayerFromId(source.Handle).removeMoney(amount);
            }
            else if (config.Framework == "QBCore")
            {
                framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions.RemoveMoney("cash", amount);
            }
            else if (config.Framework == "Custom")
            {
                Exports[config.ExportResource].RemovePlayerWalletMoney(source.Handle, amount);
            }
        }

        public void RemovePlayerAccountMoney(Player source, int amount, string account)
        {
            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                framework.GetPlayerFromId(source.Handle).removeAccountMoney(account, amount);
            }
            else if (config.Framework == "QBCore")
            {
                framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions.RemoveMoney(account, amount);
            }
            else if (config.Framework == "Custom")
            {
                Exports[config.ExportResource].RemovePlayerAccountMoney(source.Handle, amount, account);
            }
        }

        public void AddPlayerInventoryItem(Player source, string item, int amount)
        {
            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                framework.GetPlayerFromId(source.Handle).addInventoryItem(item, amount);
            }
            else if (config.Framework == "QBCore")
            {
                framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions.AddItem(item, amount);
            }
            else if (config.Framework == "Custom")
            {
                Exports[config.ExportResource].AddPlayerInventoryItem(source.Handle, item, amount);
            }
        }

        public void RemovePlayerInventoryItem(Player source, string item, int amount)
        {
            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                framework.GetPlayerFromId(source.Handle).removeInventoryItem(item, amount);
            }
            else if (config.Framework == "QBCore")
            {
                framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions.RemoveItem(item, amount);
            }
            else if (config.Framework == "Custom")
            {
                Exports[config.ExportResource].RemovePlayerInventoryItem(source.Handle, item, amount);
            }
        }
        public int GetPlayerInventoryItemCount(Player source, string item)
        {
            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                return framework.GetPlayerFromId(source.Handle).getInventoryItem(item).count;
            }
            else if (config.Framework == "QBCore")
            {
                return framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions.GetItemsByName(item).amount;
            }
            else if (config.Framework == "Custom")
            {
                return Exports[config.ExportResource].GetPlayerInventoryItemCount(source.Handle, item);
            }

            return 0;
        }
    }
}
