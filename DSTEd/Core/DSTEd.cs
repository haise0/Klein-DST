﻿using System;
using System.ComponentModel;
using System.Threading;
using DSTEd.Core.Klei.Games;
using DSTEd.Core.Steam;
using DSTEd.UI;

namespace DSTEd.Core {
    public class DSTEd : System.Windows.Application {
        private String version = "2.0.0";
        private String language = "en_US";
        private IDE ide = null;
        private Workspace workspace = null;
		private Loadingv2 loaderv2 = new Loadingv2();
        private Steam.Steam steam = null;
        private Configuration configuration = null;
        private Core.LUA.LUA lua = new LUA.LUA();

        public DSTEd() {
        }

        public void Start() {
            Logger.Info("Start DSTEd v" + GetVersion());

            this.configuration = new Configuration();

            // Init Language
            I18N.SetLanguage(this.GetLanguage());

            // Init classes
            this.steam = new Steam.Steam();
            this.ide = new IDE();
            this.workspace = new Workspace();
            //this.loading = new Loading();

            // Set the steam path by configuration
            this.steam.SetPath(this.configuration.Get("STEAM_PATH", null));
            this.workspace.SetPath(this.steam.GetPath());

            this.workspace.OnSelect(delegate (string path, Boolean save) {
                if (!this.steam.ValidatePath(path)) {
                    Dialog.Open(I18N.__("Bad steam path! Please select the directory of Steam."), I18N.__("Problem"), Dialog.Buttons.OK, Dialog.Icon.Warning, delegate (Dialog.Result result) {
                        return true;
                    });
                    return;
                }

                this.steam.SetPath(path);
                this.workspace.SetPath(path);

                if (save) {
                    this.configuration.Set("STEAM_PATH", path);
                    this.configuration.Save();
                }

                this.workspace.Close(true);
                
            });

            this.workspace.OnClose(delegate (CancelEventArgs e) {
                Dialog.Open(I18N.__("You must set the workspace path! If you cancel these, DSTEd will be closed."), I18N.__("Problem"), Dialog.Buttons.RetryCancel, Dialog.Icon.Warning, delegate (Dialog.Result result) {
                    if (result == Dialog.Result.Cancel) {
                        Environment.Exit(0);
                        return true;
                    }

                    e.Cancel = true;
                    return true;
                });
            });
			///////////////////////V2 Loader///////////////////////////
			//define workers
			void SteamPathInit(uint p)
			{
				if (!steam.IsInstalled())
				{
					Logger.Info("Steam is not installed? Ask for Workspace...");
					Dialog.Open(I18N.__("We can not find the path to STEAM. Please check the workspace settings."), I18N.__("Problem: Steam"), Dialog.Buttons.OK, Dialog.Icon.Warning,
						delegate (Dialog.Result r)
						{
							workspace.Show();
							return true;
						}
						);
				}
				p++;
			}
			void gameloading(uint p)
			{
				steam.LoadGame(new DSTC());//CL
				steam.LoadGame(new DSTM());//MT
				steam.LoadGame(new DSTS());//SV
				ide.Init();
				p++;
			}
			void modsloading(uint p)
			{
				//do nothing now
				p++;
			}
			void workshoploading(uint p)
			{
				steam.GetWorkShop().GetPublishedMods(322330, delegate (WorkshopItem[] items) {
					Logger.Info("You have " + items.Length + " published Mods on the Steam-Workshop!");

					for (int index = 0; index < items.Length; index++)
					{
						Logger.Info(items[index].ToString());
					}
				});
				p++;
			}
			//Add Worker
			var steampathinit = new STWorkUnits();
			steampathinit += SteamPathInit;
			var asyncloadingphase1 = new STWorkUnits();
			asyncloadingphase1 += gameloading;
			asyncloadingphase1 += modsloading;
			asyncloadingphase1 += workshoploading;
			//Start loading;
			loaderv2.Start();
			//////////////////Wait for Loading finished////////////////
            this.Run();
        }

        public IDE GetIDE() {
            return this.ide;
        }

        public LUA.LUA GetLUA() {
            return this.lua;
        }

        public Steam.Steam GetSteam() {
            return this.steam;
        }

        public Workspace GetWorkspace() {
            return this.workspace;
        }

        public Boolean IsWorkspaceReady() {
            return this.workspace != null;
        }

        public String GetVersion() {
            return this.version;
        }

        public String GetLanguage() {
            return this.configuration.Get("LANGUAGE", this.language);
        }
    }
}
