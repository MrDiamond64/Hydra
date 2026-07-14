using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HydraMenu.ui.sections
{
    internal class SearchSection : ISection
    {
        public SearchSection() : base("Search") { }

        public List<SearchResult> Results = new List<SearchResult>();
        public string SearchQuery = "";

        public struct SearchResult
        {
            public string FeatureName;
            public int TabIndex;
            public Action OnClick;
        }

        public override void Render()
        {
            if (Results.Count == 0)
            {
                GUILayout.Label("No features found matching your search.", Styles.MainBox);
                return;
            }

            // Group results by section for better readability
            var groupedResults = Results.GroupBy(r => r.TabIndex).OrderBy(g => g.Key);

            foreach (var group in groupedResults)
            {
                string sectionName = "Unknown Section";
                // We don't have direct access to allSections here, but we can infer it from the result name
                // or just use a generic header. Since we want it organized, let's use the first result's prefix.
                var firstResult = group.First();
                if (firstResult.FeatureName.Contains(" > "))
                {
                    sectionName = firstResult.FeatureName.Split(" > ")[0];
                }
                else if (firstResult.FeatureName.StartsWith("Section: "))
                {
                    sectionName = firstResult.FeatureName.Replace("Section: ", "");
                }

                GUILayout.Label(sectionName, Styles.SectionBox);
                GUILayout.Space(5);

                foreach (var result in group)
                {
                    string label = result.FeatureName.Contains(" > ") 
                        ? result.FeatureName.Split(" > ")[1] 
                        : result.FeatureName;

                    if (GUILayout.Button(label, Styles.PlayerBox))
                    {
                        result.OnClick?.Invoke();
                    }
                }
                GUILayout.Space(10);
            }
        }

        public void UpdateResults(ISection[] allSections, byte currentTabIndex, Action<byte> setTab)
        {
            Results.Clear();
            if (string.IsNullOrWhiteSpace(SearchQuery)) return;

            for (int i = 0; i < allSections.Length; i++)
            {
                var section = allSections[i];
                
                // Search in section names
                if (section.name.IndexOf(SearchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Results.Add(new SearchResult 
                    { 
                        FeatureName = $"Section: {section.name}", 
                        TabIndex = (byte)i, 
                        OnClick = () => setTab((byte)i) 
                    });
                }

                // Search in individual features
                foreach (var feature in section.Features)
                {
                    if (feature.Name.IndexOf(SearchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Results.Add(new SearchResult 
                        { 
                            FeatureName = $"{section.name} > {feature.Name}", 
                            TabIndex = (byte)i, 
                            OnClick = () => setTab((byte)i) 
                        });
                    }
                }
            }
        }
    }
}
