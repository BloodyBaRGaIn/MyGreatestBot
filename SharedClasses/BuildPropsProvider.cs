using System;
using System.Collections.Generic;
using System.Xml;

namespace SharedClasses
{
    public static class BuildPropsProvider
    {
        private static XmlDocument? solutionProperties = null;
        private static XmlNodeList? propertiesList = null;

        public static Exception? LastError { get; private set; } = null;

        public static bool Init()
        {
            if (solutionProperties != null && propertiesList != null)
            {
                return true;
            }

            solutionProperties = new XmlDocument();
            try
            {
                solutionProperties.Load("Directory.Build.props");
                XmlElement xRoot = solutionProperties.DocumentElement
                    ?? throw new XmlException("Cannot get XML root");
                if (xRoot.Name != "Project" || xRoot.ChildNodes.Count != 1)
                {
                    throw new XmlException("Invalid XML root");
                }
                XmlNode propertiesNode = xRoot.FirstChild
                    ?? throw new XmlException("Cannot get XML node");
                if (propertiesNode.Name != "PropertyGroup" || !propertiesNode.HasChildNodes)
                {
                    throw new XmlException("Invalid XML node");
                }
                propertiesList = propertiesNode.ChildNodes;
            }
            catch (Exception ex)
            {
                solutionProperties = null;
                propertiesList = null;
                LastError = ex;
                return false;
            }

            return true;
        }

        public static bool GetProperties(out Dictionary<string, string> propertiesDictionary)
        {
            propertiesDictionary = [];

            if (!Init())
            {
                return false;
            }

            if (propertiesList != null)
            {
                foreach (XmlNode prop in propertiesList)
                {
                    propertiesDictionary[prop.Name] = prop.InnerText;
                }
            }

            return propertiesDictionary.Count != 0;
        }
    }
}
