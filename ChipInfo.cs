using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace TamaSmartApp
{
    public class ChipInfo
    {
        public string Name { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public uint Size { get; set; }
        public uint Page { get; set; }
        public string SpiCmd { get; set; } = "25xx";
        public string Id { get; set; } = "";

        public string SizeFormatted => FormatSize(Size);
        public string PageFormatted => Page.ToString();

        private string FormatSize(uint bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024} KB";
            return $"{bytes / (1024 * 1024)} MB";
        }
    }

    public static class ChipDatabase
    {
        private static List<ChipInfo> _chips = new List<ChipInfo>();
        private static bool _loaded = false;

        public static void Load(string? xmlPath = null)
        {
            if (_loaded) return;

            try
            {
                XDocument doc;

                // Try to load from embedded resource first
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                
                // Try different possible resource names (LogicalName is "chiplist.xml" in csproj)
                string[] possibleResourceNames = new string[]
                {
                    "chiplist.xml",  // LogicalName from csproj
                    "TamaSmartApp.chiplist.xml",
                    "lib.chiplist.xml",
                    "TamaSmartApp.lib.chiplist.xml"
                };

                System.IO.Stream? stream = null;
                foreach (string resourceName in possibleResourceNames)
                {
                    stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Loaded chiplist.xml from embedded resource: {resourceName}");
                        break;
                    }
                }

                using (stream)
                {
                    if (stream != null)
                    {
                        doc = XDocument.Load(stream);
                    }
                    else
                    {
                        // Fallback to file system if embedded resource not found
                        if (xmlPath == null)
                        {
                            xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", "chiplist.xml");
                        }

                        if (!File.Exists(xmlPath))
                        {
                            // Try alternative paths
                            string[] paths = new string[]
                            {
                                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", "chiplist.xml"),
                                Path.Combine(Directory.GetCurrentDirectory(), "lib", "chiplist.xml"),
                                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chiplist.xml"),
                                xmlPath
                            };

                            foreach (string path in paths)
                            {
                                if (File.Exists(path))
                                {
                                    xmlPath = path;
                                    break;
                                }
                            }
                        }

                        if (!File.Exists(xmlPath))
                        {
                            return;
                        }

                        doc = XDocument.Load(xmlPath);
                    }
                }
                XElement root = doc.Element("chiplist");
                if (root == null) return;

                // Parse SPI chips
                XElement spiElement = root.Element("SPI");
                if (spiElement != null)
                {
                    foreach (XElement manufacturer in spiElement.Elements())
                    {
                        string manufacturerName = manufacturer.Name.LocalName;
                        foreach (XElement chip in manufacturer.Elements())
                        {
                            ChipInfo info = new ChipInfo
                            {
                                Manufacturer = manufacturerName,
                                Name = chip.Name.LocalName
                            };

                            XAttribute idAttr = chip.Attribute("id");
                            if (idAttr != null)
                            {
                                info.Id = idAttr.Value.ToUpper().Replace(" ", "");
                            }

                            XAttribute sizeAttr = chip.Attribute("size");
                            if (sizeAttr != null && uint.TryParse(sizeAttr.Value, out uint size))
                            {
                                info.Size = size;
                            }

                            XAttribute pageAttr = chip.Attribute("page");
                            if (pageAttr != null && uint.TryParse(pageAttr.Value, out uint page))
                            {
                                info.Page = page;
                            }

                            XAttribute spicmdAttr = chip.Attribute("spicmd");
                            if (spicmdAttr != null)
                            {
                                info.SpiCmd = spicmdAttr.Value;
                            }
                            else
                            {
                                // Default to 25xx for SPI flash chips
                                info.SpiCmd = "25xx";
                            }

                            _chips.Add(info);
                        }
                    }
                }

                _loaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading chip database: {ex.Message}");
            }
        }

        public static ChipInfo? FindByFlashId(byte[] flashId)
        {
            var matches = FindAllByFlashId(flashId);
            if (matches.Count == 0) return null;
            if (matches.Count == 1) return matches[0];
            
            // Multiple matches - return first one (caller should use FindAllByFlashId and show dialog)
            return matches[0];
        }

        public static List<ChipInfo> FindAllByFlashId(byte[] flashId)
        {
            List<ChipInfo> results = new List<ChipInfo>();
            
            if (flashId == null || flashId.Length < 3) return results;

            // Try to load if not loaded (from embedded resource)
            if (!_loaded)
            {
                Load();
            }

            // Format ID as hex string (e.g., "C84015")
            string idString = string.Join("", flashId.Select(b => b.ToString("X2")));

            // Find all exact matches
            var exactMatches = _chips.Where(c => 
                c.Id.Equals(idString, StringComparison.OrdinalIgnoreCase)).ToList();

            if (exactMatches.Count > 0)
            {
                return exactMatches;
            }

            // Try partial match (first 2 bytes)
            string partialId = flashId[0].ToString("X2") + flashId[1].ToString("X2");
            var partialMatches = _chips.Where(c =>
                c.Id.StartsWith(partialId, StringComparison.OrdinalIgnoreCase)).ToList();

            return partialMatches;
        }

        public static List<ChipInfo> GetAllChips()
        {
            if (!_loaded)
            {
                Load();
            }
            return _chips.ToList();
        }
    }
}
