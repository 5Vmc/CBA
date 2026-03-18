using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Babu
{
    public class CbaBundleResVersionData
    {
        public class BundleVersionData
        {
            public long bundleVersion = -1;
            public List<string> clientVersionList = new();
        }

        public long updateJsonVersion = -1;
        public string clientMinVersion = "";
        public string downloadClientUrl = "";
        public string forderUrl = "";
        public long fallbackResVersion = -1;
        public List<BundleVersionData> bundleVersionList = new();

        public string GetBundleUrl(string majorVersion)
        {

            long bundleVersion = -1;
            foreach (BundleVersionData bundleVersionData in bundleVersionList)
            {
                foreach (string clientVersion in bundleVersionData.clientVersionList)
                {
                    if (clientVersion == majorVersion)
                    {
                        bundleVersion = bundleVersionData.bundleVersion;
                        break;
                    }
                }
            }
            if (bundleVersion == -1)
            {
                bundleVersion = fallbackResVersion;
            }
            string url = forderUrl + bundleVersion.ToString();
            return url;
        }
    }
}
