using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(Database))]
public class DatabaseEditor : Editor
{
    private string searchQuery = "";
    private int progressionFilter = -1; // -1 means no filter
    private Vector2 scrollPosition;
    private bool hasSearched = false;
    private List<Entry> filteredEntries = new List<Entry>();
    private string[] progressionOptions;
    private Dictionary<int, int> progressionLevelMap = new Dictionary<int, int>(); // Maps popup index to actual progression level
    private bool showProgressionFilter = false;
    private Dictionary<string, Vector2> entryScrollPositions = new Dictionary<string, Vector2>(); // For individual entry scrolling

    // Reference search fields
    private bool showReferenceSearch = false;
    private string referenceSearchQuery = "";
    private List<Entry> referenceSearchResults = new List<Entry>();
    private bool hasReferenceSearched = false;

    public override void OnInspectorGUI()
    {
        Database database = (Database)target;

        // Build progression options dynamically
        UpdateProgressionOptions(database);

        EditorGUILayout.Space();

        // Name search field
        EditorGUILayout.BeginHorizontal();
        GUI.SetNextControlName("SearchField");
        string newSearchQuery = EditorGUILayout.TextField("Search Entry:", searchQuery);

        // Handle Enter key and search button
        bool performSearch = false;
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "SearchField")
        {
            performSearch = true;
            Event.current.Use(); // Consume the event
        }

        if (GUILayout.Button("Find", GUILayout.Width(60)))
        {
            performSearch = true;
        }

        // Clear button
        if (GUILayout.Button("Clear", GUILayout.Width(60)))
        {
            newSearchQuery = "";
            hasSearched = false;
            filteredEntries.Clear();
        }
        EditorGUILayout.EndHorizontal();

        // Progression Level Filter
        EditorGUILayout.BeginHorizontal();
        showProgressionFilter = EditorGUILayout.Foldout(showProgressionFilter, "Progression Level Filter", true);
        EditorGUILayout.EndHorizontal();

        if (showProgressionFilter)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Only show the popup if we have options
            if (progressionOptions != null && progressionOptions.Length > 1)
            {
                // Find the current index in the popup based on our progression filter
                int currentPopupIndex = 0; // Default to "All Levels"
                if (progressionFilter >= 0)
                {
                    // Try to find the index of our current filter in the map
                    foreach (var kvp in progressionLevelMap)
                    {
                        if (kvp.Value == progressionFilter)
                        {
                            currentPopupIndex = kvp.Key + 1; // +1 because "All Levels" is at index 0
                            break;
                        }
                    }
                }

                int newPopupIndex = EditorGUILayout.Popup("Filter by Level:", currentPopupIndex, progressionOptions);

                // Convert popup index back to progression level
                int newProgressionFilter = -1; // Default to "All Levels"
                if (newPopupIndex > 0) // If not "All Levels"
                {
                    newProgressionFilter = progressionLevelMap[newPopupIndex - 1];
                }

                if (newProgressionFilter != progressionFilter)
                {
                    progressionFilter = newProgressionFilter;
                    hasSearched = true;
                    UpdateFilteredEntries(database);
                }
            }
            else
            {
                EditorGUILayout.LabelField("No progression levels found in database", EditorStyles.boldLabel);
            }

            // Show count of entries for each level that exists
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Entry counts by level:", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            var levelsInUse = database.entries
                .Select(e => e.progressionLevel)
                .Distinct()
                .OrderBy(level => level);

            foreach (int level in levelsInUse)
            {
                int count = database.entries.Count(e => e.progressionLevel == level);
                EditorGUILayout.LabelField($"Level {level}: {count} entries");
            }

            EditorGUILayout.EndVertical();
        }

        // Reference Search section
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        showReferenceSearch = EditorGUILayout.Foldout(showReferenceSearch, "Search References", true);
        EditorGUILayout.EndHorizontal();

        if (showReferenceSearch)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Find entries that reference a specific entry name", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("ReferenceSearchField");
            string newReferenceSearchQuery = EditorGUILayout.TextField("Entry Name:", referenceSearchQuery);

            bool performReferenceSearch = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return &&
                GUI.GetNameOfFocusedControl() == "ReferenceSearchField")
            {
                performReferenceSearch = true;
                Event.current.Use();
            }

            if (GUILayout.Button("Find References", GUILayout.Width(120)))
            {
                performReferenceSearch = true;
            }

            if (GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                newReferenceSearchQuery = "";
                hasReferenceSearched = false;
                referenceSearchResults.Clear();
            }
            EditorGUILayout.EndHorizontal();

            // Update reference search query
            if (newReferenceSearchQuery != referenceSearchQuery)
            {
                referenceSearchQuery = newReferenceSearchQuery;

                if (string.IsNullOrEmpty(referenceSearchQuery))
                {
                    hasReferenceSearched = false;
                    referenceSearchResults.Clear();
                }
            }

            // Perform reference search when requested
            if (performReferenceSearch && !string.IsNullOrEmpty(referenceSearchQuery))
            {
                hasReferenceSearched = true;
                UpdateReferenceSearchResults(database);
            }

            // Display reference search results
            if (hasReferenceSearched)
            {
                if (referenceSearchResults.Count > 0)
                {
                    EditorGUILayout.LabelField($"Found {referenceSearchResults.Count} entries that reference '{referenceSearchQuery}':",
                        EditorStyles.boldLabel);

                    foreach (Entry entry in referenceSearchResults)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        // Display name with highlight
                        GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
                        nameStyle.normal.textColor = Color.cyan;
                        EditorGUILayout.LabelField($"Name: {entry.name}", nameStyle);

                        // Display type
                        EditorGUILayout.LabelField($"Type: {entry.entryType}");
                        EditorGUILayout.LabelField($"Progression Level: {entry.progressionLevel}");

                        // Show output preview with highlighted reference
                        EditorGUILayout.LabelField("Output Preview:");
                        string outputPreview = entry.output;
                        EditorGUILayout.SelectableLabel(outputPreview, EditorStyles.textField, GUILayout.Height(60));

                        // Show alt output if it contains the reference
                        if (!string.IsNullOrEmpty(entry.altOutput) &&
                            entry.altOutput.IndexOf(referenceSearchQuery, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            EditorGUILayout.LabelField("Alt Output Preview:");
                            EditorGUILayout.SelectableLabel(entry.altOutput, EditorStyles.textField, GUILayout.Height(60));
                        }

                        // Button to select this entry in the full list
                        if (GUILayout.Button("Select This Entry"))
                        {
                            // Find the index of this entry in the main list
                            int entryIndex = database.entries.IndexOf(entry);
                            if (entryIndex >= 0)
                            {
                                // Schedule selection for next frame to allow the UI to update
                                EditorApplication.delayCall += () => {
                                    // Expand the entries array in the inspector
                                    SerializedProperty entriesProp = serializedObject.FindProperty("entries");
                                    entriesProp.isExpanded = true;

                                    // Expand the specific entry
                                    SerializedProperty entryProp = entriesProp.GetArrayElementAtIndex(entryIndex);
                                    entryProp.isExpanded = true;

                                    // Apply changes
                                    serializedObject.ApplyModifiedProperties();

                                    // Reset search
                                    searchQuery = "";
                                    progressionFilter = -1;
                                    hasSearched = false;
                                    filteredEntries.Clear();

                                    // Also reset reference search
                                    referenceSearchQuery = "";
                                    hasReferenceSearched = false;
                                    referenceSearchResults.Clear();
                                };
                            }
                        }

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox($"No entries found that reference '{referenceSearchQuery}'", MessageType.Info);
                }
            }

            EditorGUILayout.EndVertical();
        }

        // Focus the search field initially
        if (Event.current.type == EventType.Repaint && !EditorGUIUtility.editingTextField)
        {
            EditorGUI.FocusTextInControl("SearchField");
        }

        // Update search query
        if (newSearchQuery != searchQuery)
        {
            searchQuery = newSearchQuery;

            // If search is cleared, reset search state only if we're not filtering by progression
            if (string.IsNullOrEmpty(searchQuery) && progressionFilter == -1)
            {
                hasSearched = false;
                filteredEntries.Clear();
            }
            else
            {
                hasSearched = true;
                UpdateFilteredEntries(database);
            }
        }

        // Perform search when requested
        if (performSearch || (hasSearched && (progressionFilter != -1 || !string.IsNullOrEmpty(searchQuery))))
        {
            hasSearched = true;
            UpdateFilteredEntries(database);
        }

        EditorGUILayout.Space();

        // Display search results or default inspector
        if (hasSearched && filteredEntries.Count > 0)
        {
            string filterDescription = "";
            if (!string.IsNullOrEmpty(searchQuery) && progressionFilter >= 0)
                filterDescription = $"name containing '{searchQuery}' AND progression level {progressionFilter}";
            else if (!string.IsNullOrEmpty(searchQuery))
                filterDescription = $"name containing '{searchQuery}'";
            else if (progressionFilter >= 0)
                filterDescription = $"progression level {progressionFilter}";

            EditorGUILayout.LabelField($"Results: {filteredEntries.Count} entries with {filterDescription}", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (Entry entry in filteredEntries)
            {
                // Create a unique key for this entry
                string entryKey = entry.name; // Or use a unique ID if available
                if (!entryScrollPositions.ContainsKey(entryKey))
                {
                    entryScrollPositions[entryKey] = Vector2.zero;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Display name with highlight
                GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
                nameStyle.normal.textColor = Color.green;
                EditorGUILayout.LabelField($"Name: {entry.name}", nameStyle);

                // Display type
                EditorGUILayout.LabelField($"Type: {entry.entryType}");

                // Make progression level editable
                SerializedProperty entriesProp = serializedObject.FindProperty("entries");
                int entryIndex = database.entries.IndexOf(entry);
                if (entryIndex >= 0)
                {
                    SerializedProperty entryProp = entriesProp.GetArrayElementAtIndex(entryIndex);
                    SerializedProperty progressionProp = entryProp.FindPropertyRelative("progressionLevel");

                    EditorGUILayout.PropertyField(progressionProp, new GUIContent("Progression Level:"));

                    if (progressionProp.intValue != entry.progressionLevel)
                    {
                        serializedObject.ApplyModifiedProperties();
                        // Rebuild progression options since levels may have changed
                        UpdateProgressionOptions(database);
                        // Re-apply filter after changes
                        UpdateFilteredEntries(database);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField($"Progression Level: {entry.progressionLevel}");
                }

                // Show output preview with individual scroll position
                EditorGUILayout.LabelField("Output Preview:");
                entryScrollPositions[entryKey] = EditorGUILayout.BeginScrollView(
                    entryScrollPositions[entryKey],
                    GUILayout.Height(60)
                );
                EditorGUILayout.SelectableLabel(entry.output, EditorStyles.textField);
                EditorGUILayout.EndScrollView();

                // Add button to find references to this entry
                if (GUILayout.Button("Find References to this Entry"))
                {
                    referenceSearchQuery = entry.name;
                    showReferenceSearch = true;
                    hasReferenceSearched = true;
                    UpdateReferenceSearchResults(database);
                }

                // Button to select this entry in the full list
                if (GUILayout.Button("Select This Entry"))
                {
                    // Find the index of this entry in the main list
                    int entryIndex2 = database.entries.IndexOf(entry);
                    if (entryIndex2 >= 0)
                    {
                        // Schedule selection for next frame to allow the UI to update
                        EditorApplication.delayCall += () => {
                            // Expand the entries array in the inspector
                            SerializedProperty entriesProp2 = serializedObject.FindProperty("entries");
                            entriesProp2.isExpanded = true;

                            // Expand the specific entry
                            SerializedProperty entryProp = entriesProp2.GetArrayElementAtIndex(entryIndex2);
                            entryProp.isExpanded = true;

                            // Apply changes
                            serializedObject.ApplyModifiedProperties();

                            // Reset search
                            searchQuery = "";
                            progressionFilter = -1;
                            hasSearched = false;
                            filteredEntries.Clear();
                        };
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();

            // Button to show full inspector
            if (GUILayout.Button("Show All Entries"))
            {
                searchQuery = "";
                progressionFilter = -1;
                hasSearched = false;
                filteredEntries.Clear();
            }
        }
        else if (hasSearched)
        {
            string filterDescription = "";
            if (!string.IsNullOrEmpty(searchQuery) && progressionFilter >= 0)
                filterDescription = $"name containing '{searchQuery}' AND progression level {progressionFilter}";
            else if (!string.IsNullOrEmpty(searchQuery))
                filterDescription = $"name containing '{searchQuery}'";
            else if (progressionFilter >= 0)
                filterDescription = $"progression level {progressionFilter}";

            EditorGUILayout.HelpBox($"No entries found with {filterDescription}", MessageType.Info);
            EditorGUILayout.Space();

            // Show default inspector
            base.OnInspectorGUI();
        }
        else
        {
            // Show default inspector when not searching
            base.OnInspectorGUI();
        }

        // Mark the object as dirty if modified
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    private void UpdateProgressionOptions(Database database)
    {
        // Get all progression levels that are actually in use
        var levelsInUse = database.entries
            .Select(e => e.progressionLevel)
            .Distinct()
            .OrderBy(level => level)
            .ToList();

        // Only rebuild if needed
        if (progressionOptions == null || progressionOptions.Length != levelsInUse.Count + 1)
        {
            List<string> options = new List<string> { "All Levels" };
            progressionLevelMap.Clear();

            for (int i = 0; i < levelsInUse.Count; i++)
            {
                int level = levelsInUse[i];
                options.Add($"Level {level}");
                progressionLevelMap[i] = level;
            }

            progressionOptions = options.ToArray();
        }
    }

    private void UpdateFilteredEntries(Database database)
    {
        filteredEntries.Clear();

        // No filters applied
        if (string.IsNullOrEmpty(searchQuery) && progressionFilter == -1)
        {
            return;
        }

        // Apply filters
        IEnumerable<Entry> results = database.entries;

        // Apply name filter if provided
        if (!string.IsNullOrEmpty(searchQuery))
        {
            string lowercaseQuery = searchQuery.ToLower();

            // First try exact matches
            List<Entry> exactMatches = results
                .Where(e => e.name.ToLower() == lowercaseQuery)
                .ToList();

            if (exactMatches.Count > 0)
            {
                results = exactMatches;
            }
            else
            {
                // Then try contains matches
                results = results.Where(e => e.name.ToLower().Contains(lowercaseQuery));
            }
        }

        // Apply progression filter if provided
        if (progressionFilter >= 0)
        {
            results = results.Where(e => e.progressionLevel == progressionFilter);
        }

        filteredEntries = results.ToList();
    }

    private void UpdateReferenceSearchResults(Database database)
    {
        referenceSearchResults.Clear();

        if (string.IsNullOrEmpty(referenceSearchQuery))
        {
            return;
        }

        // Case insensitive search for entries that mention the reference query in output or alt output
        string lowercaseQuery = referenceSearchQuery.ToLower();

        foreach (Entry entry in database.entries)
        {
            bool isReferenced = false;

            // Check main output
            if (!string.IsNullOrEmpty(entry.output) &&
                entry.output.IndexOf(referenceSearchQuery, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isReferenced = true;
            }

            // Check alt output if exists
            if (!isReferenced && !string.IsNullOrEmpty(entry.altOutput) &&
                entry.altOutput.IndexOf(referenceSearchQuery, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isReferenced = true;
            }

            // Add to results if referenced
            if (isReferenced)
            {
                referenceSearchResults.Add(entry);
            }
        }
    }
}