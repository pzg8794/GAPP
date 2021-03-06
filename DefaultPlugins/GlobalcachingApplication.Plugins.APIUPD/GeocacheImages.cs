﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.APIUPD
{
    public class GeocacheImages : Utils.BasePlugin.BaseImportFilter
    {
        public const string STR_NOGEOCACHESELECTED = "No geocache selected for update";
        public const string STR_ERROR = "Error";
        public const string STR_UNABLELIVE = "Unable to access the Live API or process its data";
        public const string STR_UPDATINGGEOCACHES = "Updating geocaches...";
        public const string STR_UPDATINGGEOCACHE = "Updating geocache...";

        public const string ACTION_UPDATE_ALL = "Update geocache images|All";
        public const string ACTION_UPDATE_SELECTED = "Update geocache images|Selected";
        public const string ACTION_UPDATE_ACTIVE = "Update geocache images|Active";

        private List<Framework.Data.Geocache> _gcList = null;
        private string _errormessage = null;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            if (PluginSettings.Instance == null)
            {
                var p = new PluginSettings(core);
            }

            AddAction(ACTION_UPDATE_ALL);
            AddAction(ACTION_UPDATE_SELECTED);
            AddAction(ACTION_UPDATE_ACTIVE);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UNABLELIVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UPDATINGGEOCACHES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_UPDATINGGEOCACHE));

            return await base.InitializeAsync(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.LiveAPI;
            }
        }

        protected override void ImportMethod()
        {
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_UPDATINGGEOCACHES, STR_UPDATINGGEOCACHE, _gcList.Count, 0, true))
                {
                    int totalcount = _gcList.Count;
                    using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken)))
                    {
                        int index = 0;

                        while (_gcList.Count > 0)
                        {
                            if (PluginSettings.Instance.AdditionalDelayBetweenImageImport > 0)
                            {
                                Thread.Sleep(PluginSettings.Instance.AdditionalDelayBetweenImageImport);
                            }
                            var resp = client.Client.GetImagesForGeocache(client.Token, _gcList[0].Code);
                            if (resp.Status.StatusCode == 0)
                            {
                                if (resp.Images != null)
                                {
                                    List<string> ids = new List<string>();
                                    foreach (var img in resp.Images)
                                    {
                                        if (img.Url.IndexOf("/cache/log/") < 0)
                                        {
                                            Framework.Data.GeocacheImage gcImg = Utils.API.Convert.GeocacheImage(Core, img, _gcList[0].Code);
                                            AddGeocacheImage(gcImg);
                                            ids.Add(gcImg.ID);
                                        }
                                    }
                                    List<Framework.Data.GeocacheImage> allImages = Utils.DataAccess.GetGeocacheImages(Core.GeocacheImages, _gcList[0].Code);
                                    foreach (Framework.Data.GeocacheImage gim in allImages)
                                    {
                                        if (!ids.Contains(gim.ID))
                                        {
                                            Core.GeocacheImages.Remove(gim);
                                        }
                                    }
                                }

                                if (PluginSettings.Instance.DeselectGeocacheAfterUpdate)
                                {
                                    _gcList[0].Selected = false;
                                }

                                index++;
                                if (!progress.UpdateProgress(STR_UPDATINGGEOCACHES, STR_UPDATINGGEOCACHE, totalcount, index))
                                {
                                    break;
                                }
                                _gcList.RemoveAt(0);

                                if (_gcList.Count > 0)
                                {
                                    Thread.Sleep(3000);
                                }
                            }
                            else
                            {
                                _errormessage = resp.Status.StatusMessage;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _errormessage = e.Message;
            }
        }

        public async override Task<bool> ActionAsync(string action)
        {
            bool result = base.Action(action);
            if (result &&
                (
                 action == ACTION_UPDATE_ALL ||
                 action == ACTION_UPDATE_SELECTED ||
                 action == ACTION_UPDATE_ACTIVE
                ))
            {
                try
                {
                    //get from goundspeak
                    if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, false))
                    {
                        _gcList = null;
                        if (action == ACTION_UPDATE_ALL)
                        {
                            _gcList = (from Framework.Data.Geocache a in Core.Geocaches select a).ToList();
                        }
                        else if (action == ACTION_UPDATE_SELECTED)
                        {
                            _gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                        }
                        else if (action == ACTION_UPDATE_ACTIVE && Core.ActiveGeocache != null)
                        {
                            _gcList = new List<Framework.Data.Geocache>();
                            _gcList.Add(Core.ActiveGeocache);
                        }
                        if (_gcList != null && _gcList.Count > 0)
                        {
                            _errormessage = null;
                            await PerformImport();
                            if (!string.IsNullOrEmpty(_errormessage))
                            {
                                System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    }
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_UNABLELIVE), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            return result;
        }

    }
}
