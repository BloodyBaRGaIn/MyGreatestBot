using System;
using System.Collections.Generic;
using System.Xml;

namespace SharedClasses
{
    /// <summary>
    /// Provides XML properties from MSBuild shared file.
    /// </summary>
    public static class BuildPropsProvider
    {
        /// <summary>
        /// Reserved MSBuild target file name.
        /// </summary>
        private const string BuildPropsFileName = "Directory.Build.props";

        private static XmlDocument? solutionProperties;
        private static XmlNodeList? propertiesList;

        /// <summary>
        /// Provides last exception thrown during the file process.<br/>
        /// Returns <c>null</c> if there were no exceptions.
        /// </summary>
        public static Exception? LastError { get; private set; }

        /// <summary>
        /// Starts file processing.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the processing completed successfully, otherwise <c>false</c>.
        /// </returns>
        public static bool Init()
        {
            if (solutionProperties != null && propertiesList != null)
            {
                return true;
            }

            solutionProperties = new XmlDocument();
            try
            {
                solutionProperties.Load(BuildPropsFileName);
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

        /// <summary>
        /// Fills the <paramref name="propertiesDictionary"/> with properties.
        /// </summary>
        /// <param name="propertiesDictionary">
        /// Properties dictionary.<br/>
        /// Will be empty if the operation failed.
        /// </param>
        /// <returns>
        /// <c>true</c> if the operation completed successfully, otherwise <c>false</c>.
        /// </returns>
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
