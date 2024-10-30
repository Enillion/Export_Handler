using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Export_Handler
{
    class Program
    {
        static void Main(string[] args)
        {
            string folderPath = Directory.GetCurrentDirectory();
            var projectExport = new Dictionary<string, string>();

            // Loop through all .xml files in the folder
            foreach (var file in Directory.GetFiles(folderPath, "*.xml"))
            {
                var document = XDocument.Load(file);

                foreach (var conceptGrp in document.Descendants("conceptGrp"))
                {
                    var segmentID = conceptGrp.Descendants("descrip")
                                              .FirstOrDefault()?.Value;

                    if (string.IsNullOrEmpty(segmentID))
                        continue;

                    foreach (var languageGrp in conceptGrp.Descendants("languageGrp"))
                    {
                        var langCode = languageGrp.Descendants("language")
                                                  .FirstOrDefault()?.Attribute("lang")?.Value;
                        var segment = languageGrp.Descendants("term")
                                                 .FirstOrDefault()?.Value;

                        if (string.IsNullOrEmpty(langCode) || string.IsNullOrEmpty(segment))
                            continue;

                        var key = $"{segmentID}@{langCode}";

                        if (!projectExport.ContainsKey(key))
                        {
                            projectExport[key] = segment;
                        }
                    }
                }
            }

            string folderName = new DirectoryInfo(folderPath).Name;
            string outputFileName = Path.Combine(folderPath, $"{folderName}_Combined_Export.xml");
            var outputDocument = new XDocument(new XElement("root"));

            foreach (var entry in projectExport)
            {
                var keyParts = entry.Key.Split('@');
                if (keyParts.Length != 2)
                    continue;

                var stringID = keyParts[0];
                var languageContainer = keyParts[1];

                var idElement = outputDocument.Root.Elements("ID")
                                                   .FirstOrDefault(e => e.Attribute("type")?.Value == stringID);

                if (idElement == null)
                {
                    idElement = new XElement("ID", new XAttribute("type", stringID));
                    outputDocument.Root.Add(idElement);
                }

                var languageElement = idElement.Elements("language")
                                               .FirstOrDefault(e => e.Attribute("langCode")?.Value == languageContainer);

                if (languageElement == null)
                {
                    languageElement = new XElement("language",
                        new XAttribute("langCode", languageContainer),
                        entry.Value);
                    idElement.Add(languageElement);
                }
            }

            outputDocument.Save(outputFileName);
            Console.WriteLine($"Output saved to {outputFileName}");
            Console.ReadKey();
        }
    }

}
