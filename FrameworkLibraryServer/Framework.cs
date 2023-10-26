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

        public static bool PropertyExists(dynamic obj, string name)
        {
            if (obj == null) return false;
            if (obj is IDictionary<string, object> dict)
            {
                return dict.ContainsKey(name);
            }
            return obj.GetType().GetProperty(name) != null;
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

        public dynamic GetRawFramework()
        {
            return framework;
        }

        public int GetPlayerWalletMoney(Player source)
        {
            if (config.Framework == "ESX Infinity")
            {
                if (PropertyExists(framework.GetPlayerFromId(source.Handle), "GetMoney"))
                {
                    return (int)framework.GetPlayerFromId(source.Handle).GetMoney();
                }
            }

            if (config.Framework == "ESX Legacy")
            {
                if (PropertyExists(framework.GetPlayerFromId(source.Handle), "getMoney"))
                {
                    return (int)framework.GetPlayerFromId(source.Handle).getMoney();
                }
            }
            
            if (config.Framework == "Custom")
            {
                if (PropertyExists(Exports[config.ExportResource], "GetPlayerWalletMoney"))
                {
                    return (int)Exports[config.ExportResource].GetPlayerWalletMoney(source.Handle);
                }
            }
            
            if (config.Framework == "QBCore")
            {
                int amount = 0;
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

            Msg("Could not find suitable Wallet Money implementation. Returning 0.");
            return 0;
        }

        public int GetPlayerAccountMoney(Player source, string account)
        {
            if (config.Framework == "ESX Infinity")
            {
                if (PropertyExists(framework.GetPlayerFromId(source.Handle), "GetAccountMoney"))
                {
                    return (int) framework.GetPlayerFromId(source.Handle).GetAccountMoney(account);
                }

                if (PropertyExists(framework.GetPlayerFromId(source.Handle), "accounts"))
                {
                    return (int)((List<dynamic>)framework.GetPlayerFromId(source.Handle).accounts).Where(acc => acc.name == account).ToArray()[0].money;
                }
            }
            
            if (config.Framework == "ESX Legacy")
            {
                if (PropertyExists(framework.GetPlayerFromId(source.Handle), "getAccount"))
                {
                    return (int) framework.GetPlayerFromId(source.Handle).getAccount(account).money;
                }

                if (PropertyExists(framework.GetPlayerFromId(source.Handle), "GetAccount"))
                {
                    return (int)framework.GetPlayerFromId(source.Handle).GetAccount(account).money;
                }
            }
            
            if (config.Framework == "Custom")
            {
                if (PropertyExists(Exports[config.ExportResource], "GetPlayerAccountMoney"))
                {
                    return (int)Exports[config.ExportResource].GetPlayerAccountMoney(source.Handle, account);
                }
            }
            
            if (config.Framework == "QBCore")
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

            Msg("Could not find suitable Account Money implementation. Returning 0.");
            return 0;
        }

        public void AddPlayerWalletMoney(Player source, int amount)
        {
            if (amount < 1)
            {
                Msg("Invalid amount passed to AddPlayerWalletMoney. Aborting.");
                return;
            }

            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                if (PropertyExists(framework.GetPlayerFromId(source.Handle), "addMoney"))
                {
                    framework.GetPlayerFromId(source.Handle).addMoney(amount);
                    return;
                }
            }

            if (config.Framework == "QBCore")
            {
                if (PropertyExists(framework.Functions.GetPlayer(int.Parse(source.Handle)), "Functions") && PropertyExists(framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions, "AddMoney"))
                {
                    framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions.AddMoney("cash", amount);
                    return;
                }
            }

            if (config.Framework == "Custom")
            {
                if (PropertyExists(Exports[config.ExportResource], "AddPlayerWalletMoney"))
                {
                    Exports[config.ExportResource].AddPlayerWalletMoney(source.Handle, amount);
                    return;
                }
            }

            Msg("Could not find suitable AddPlayerWalletMoney implementation. No money has been added.");
        }

        public void AddPlayerAccountMoney(Player source, int amount, string account)
        {
            if (amount < 1)
            {
                Msg("Invalid amount passed to AddPlayerAccountMoney. Aborting.");
                return;
            }

            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                if (PropertyExists(framework.GetPlayerFromId(source.Handle), "addAccountMoney"))
                {
                    framework.GetPlayerFromId(source.Handle).addAccountMoney(account, amount);
                    return;
                }
            }

            if (config.Framework == "QBCore")
            {
                if (PropertyExists(framework.Functions.GetPlayer(int.Parse(source.Handle)), "Functions") &&
                    PropertyExists(framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions, "AddMoney"))
                {
                    framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions.AddMoney(account, amount);
                    return;
                }
            }

            if (config.Framework == "Custom")
            {
                if (PropertyExists(Exports[config.ExportResource], "AddPlayerAccountMoney"))
                {
                    Exports[config.ExportResource].AddPlayerAccountMoney(source.Handle, amount, account);
                    return;
                }
            }

            Msg("Could not find suitable AddPlayerAccountMoney implementation. No money has been added.");
        }

        public void RemovePlayerWalletMoney(Player source, int amount)
        {
            if (amount < 1)
            {
                Msg("Invalid amount passed to RemovePlayerWalletMoney. Aborting.");
                return;
            }

            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                if (PropertyExists(framework.GetPlayerFromId(source.Handle), "removeMoney"))
                {
                    framework.GetPlayerFromId(source.Handle).removeMoney(amount);
                    return;
                }
            }
            
            if (config.Framework == "QBCore")
            {
                if (PropertyExists(framework.Functions.GetPlayer(int.Parse(source.Handle)), "Functions") &&
                    PropertyExists(framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions, "RemoveMoney"))
                {
                    framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions.RemoveMoney("cash", amount);
                    return;
                }
            }
            
            if (config.Framework == "Custom")
            {
                if (PropertyExists(Exports[config.ExportResource], "RemovePlayerWalletMoney"))
                {
                    Exports[config.ExportResource].RemovePlayerWalletMoney(source.Handle, amount);
                    return;
                }
            }

            Msg("Could not find suitable RemovePlayerWalletMoney implementation. No money has been removed.");
        }

        public void RemovePlayerAccountMoney(Player source, int amount, string account)
        {
            if (amount < 1)
            {
                Msg("Invalid amount passed to RemovePlayerAccountMoney. Aborting.");
                return;
            }

            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                if (PropertyExists(framework.GetPlayerFromId(source.Handle), "removeAccountMoney"))
                {
                    framework.GetPlayerFromId(source.Handle).removeAccountMoney(account, amount);
                    return;
                }
            }
            
            if (config.Framework == "QBCore")
            {
                if (PropertyExists(framework.Functions.GetPlayer(int.Parse(source.Handle)), "Functions") &&
                    PropertyExists(framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions, "RemoveMoney"))
                {
                    framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions.RemoveMoney(account, amount);
                    return;
                }
            }
            
            if (config.Framework == "Custom")
            {
                if (PropertyExists(Exports[config.ExportResource], "RemovePlayerAccountMoney"))
                {
                    Exports[config.ExportResource].RemovePlayerAccountMoney(source.Handle, amount, account);
                    return;
                }
            }

            Msg("Could not find suitable RemovePlayerAccountMoney implementation. No money has been removed.");
        }

        public void AddPlayerInventoryItem(Player source, string item, int amount)
        {
            if (amount < 1)
            {
                Msg("Invalid amount passed to AddPlayerInventoryItem. Aborting.");
                return;
            }

            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                if (PropertyExists(framework.GetPlayerFromId(source.Handle), "addInventoryItem"))
                {
                    framework.GetPlayerFromId(source.Handle).addInventoryItem(item, amount);
                    return;
                }
            }
            
            if (config.Framework == "QBCore")
            {
                if (PropertyExists(framework.Functions.GetPlayer(int.Parse(source.Handle)), "Functions") &&
                    PropertyExists(framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions, "AddItem"))
                {
                    framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions.AddItem(item, amount);
                    return;
                }
            }
            
            if (config.Framework == "Custom")
            {
                if (PropertyExists(Exports[config.ExportResource], "AddPlayerInventoryItem"))
                {
                    Exports[config.ExportResource].AddPlayerInventoryItem(source.Handle, item, amount);
                    return;
                }
            }

            Msg("Could not find suitable AddPlayerInventoryItem implementation. No item has been added.");
        }

        public void RemovePlayerInventoryItem(Player source, string item, int amount)
        {
            if (amount < 1)
            {
                Msg("Invalid amount passed to RemovePlayerInventoryItem. Aborting.");
                return;
            }

            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                if (PropertyExists(framework.GetPlayerFromId(source.Handle), "removeInventoryItem"))
                {
                    framework.GetPlayerFromId(source.Handle).removeInventoryItem(item, amount);
                    return;
                }
            }
            
            if (config.Framework == "QBCore")
            {
                if (PropertyExists(framework.Functions.GetPlayer(int.Parse(source.Handle)), "Functions") &&
                    PropertyExists(framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions, "RemoveItem"))
                {
                    framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions.RemoveItem(item, amount);
                    return;
                }
            }
            
            if (config.Framework == "Custom")
            {
                if (PropertyExists(Exports[config.ExportResource], "RemovePlayerInventoryItem"))
                {
                    Exports[config.ExportResource].RemovePlayerInventoryItem(source.Handle, item, amount);
                    return;
                }
            }

            Msg("Could not find suitable RemovePlayerInventoryItem implementation. No item has been removed.");
        }


        public int GetPlayerInventoryItemCount(Player source, string item)
        {
            if (config.Framework == "ESX Legacy" || config.Framework == "ESX Infinity")
            {
                if (PropertyExists(framework.GetPlayerFromId(source.Handle), "getInventoryItem"))
                {
                    return framework.GetPlayerFromId(source.Handle).getInventoryItem(item).count;
                }
            }

            if (config.Framework == "QBCore")
            {
                if (PropertyExists(framework.Functions.GetPlayer(int.Parse(source.Handle)), "Functions") &&
                    PropertyExists(framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions, "GetItemsByName"))
                {
                    return framework.Functions.GetPlayer(int.Parse(source.Handle)).Functions.GetItemsByName(item)
                        .amount;
                }
            }

            if (config.Framework == "Custom")
            {
                if (PropertyExists(Exports[config.ExportResource], "GetPlayerInventoryItemCount"))
                {
                    return Exports[config.ExportResource].GetPlayerInventoryItemCount(source.Handle, item);
                }
            }

            Msg("Could not find suitable GetPlayerInventoryItemCount implementation. Returning 0.");
            return 0;
        }
    }
}
