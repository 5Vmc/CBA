using System.IO;
using System.Xml;
using UnityEditor;

namespace Babu.Editor.Build.Resolver
{
    class AndroidManifestResolver
    {
        [MenuItem("Babu/Resolver/AndroidManifest Resolve")]
        static public void Resolve()
        {
            XmlDocument manifest = new XmlDocument();
            manifest.Load("Assets/Plugins/Android/AndroidManifest.xml");

            XmlNode mainNode = manifest.SelectSingleNode("manifest");
            Search(mainNode, new DirectoryInfo("Assets"));

            manifest.Save("Assets/Plugins/Android/AndroidManifest.xml");
        }

        static void Search(XmlNode mainNode, DirectoryInfo parent)
        {
            foreach (var childDir in parent.GetDirectories())
            {
                Search(mainNode, childDir);
            }

            foreach (var file in parent.GetFiles())
            {
                if (file.Name.EndsWith("Dependencies.xml"))
                {
                    Merge(mainNode, file.FullName);
                }
            }
        }

        static void Merge(XmlNode mainNode, string fileName)
        {
            XmlDocument dependence = new XmlDocument();
            dependence.Load(fileName);
            XmlNode androidPackages = dependence.SelectSingleNode("dependencies").SelectSingleNode("androidPackages");

            string comment = fileName.Replace("\\", "/");
            if (comment.IndexOf("Assets") != -1)
            {
                comment = comment.Substring(comment.IndexOf("Assets"));
            }

            bool permissionCommentAdded = false;
            bool otherCommentAdded = false;
            if (androidPackages != null)
            {
                XmlNode manifestDependence = androidPackages.SelectSingleNode("androidManifest");
                if (manifestDependence != null)
                {
                    foreach (var node in manifestDependence.ChildNodes)
                    {
                        XmlElement element = (XmlElement)node;
                        XmlNode insertNode = mainNode.OwnerDocument.ImportNode(element, true);
                        if (element.Name == "uses-permission")
                        {
                            if (HasNode(mainNode, insertNode) == false)
                            {
                                if (permissionCommentAdded == false)
                                {
                                    XmlComment commentNode = mainNode.OwnerDocument.CreateComment(comment + " Begin");
                                    mainNode.AppendChild(commentNode);
                                    permissionCommentAdded = true;
                                }
                                
                                mainNode.AppendChild(insertNode);
                            }
                        }
                        else
                        {
                            if (HasNode(mainNode.SelectSingleNode("application"), insertNode) == false)
                            {
                                if (otherCommentAdded == false)
                                {
                                    XmlComment commentNode = mainNode.OwnerDocument.CreateComment(comment + " Begin");
                                    mainNode.SelectSingleNode("application").AppendChild(commentNode);
                                    otherCommentAdded = true;
                                }
                                
                                mainNode.SelectSingleNode("application").AppendChild(insertNode);
                            }
                        }
                    }
                }

                if (permissionCommentAdded)
                {
                    XmlComment commentNode = mainNode.OwnerDocument.CreateComment(comment + " End");
                    mainNode.AppendChild(commentNode);
                }

                if (otherCommentAdded)
                {
                    XmlComment commentNode = mainNode.OwnerDocument.CreateComment(comment + " End");
                    mainNode.SelectSingleNode("application").AppendChild(commentNode);
                }
            }
        }

        static bool HasNode(XmlNode parent, XmlNode node)
        {
            foreach (XmlNode child in parent.ChildNodes)
            {
                if (SimpleCompareNode(child, node))
                {
                    return true;
                }
            }
            return false;
        }

        static bool SimpleCompareNode(XmlNode node1, XmlNode node2)
        {
            XmlAttributeCollection node1Attributes = node1.Attributes;
            XmlAttributeCollection node2Attributes = node2.Attributes;
            if (node1Attributes == null || node2Attributes == null)
            {
                return false;
            }

            XmlNode attrNode1 = node1Attributes.GetNamedItem("android:name");
            XmlNode attrNode2 = node2Attributes.GetNamedItem("android:name");
            if (attrNode1 == null || attrNode2 == null)
            {
                return false;
            }

            if (attrNode1.Value == attrNode2.Value)
            {
                return true;
            }
            return false;
        }

        static bool CompareNode(XmlNode node1, XmlNode node2)
        {
            XmlNodeList node1ChildNodes = node1.ChildNodes;
            XmlNodeList node2ChildNodes = node2.ChildNodes;
            XmlAttributeCollection node1Attributes = node1.Attributes;
            XmlAttributeCollection node2Attributes = node2.Attributes;

            if (node1.Name == node2.Name && node1ChildNodes.Count == node2ChildNodes.Count)
            {
                if (node1Attributes == null && node2Attributes != null || node1Attributes != null && node2Attributes == null)
                {
                    return false;
                }
                else if (node1Attributes != null && node2Attributes != null)
                {
                    if (node1Attributes.Count == node2Attributes.Count)
                    {
                        int m = 0;
                        if (node1Attributes.Count > 0)
                        {
                            while (m < node1Attributes.Count && node1Attributes.Item(m).Name == node2Attributes.Item(m).Name && node1Attributes.Item(m).Value == node2Attributes.Item(m).Value)
                            {
                                m++;
                            }
                            if (m < node1Attributes.Count)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                if (node1ChildNodes.Count == 0)
                {
                    if (node1.InnerText != node2.InnerText)
                    {
                        return false;
                    }
                }
                else
                {
                    int k = 0;
                    while (k < node1ChildNodes.Count && CompareNode(node1ChildNodes.Item(k), node2ChildNodes.Item(k)))
                    {
                        k++;
                    }
                    if (k < node1ChildNodes.Count)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
