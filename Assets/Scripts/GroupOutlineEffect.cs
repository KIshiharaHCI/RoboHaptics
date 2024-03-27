using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class GroupOutlineEffect : MonoBehaviour
{
    public List<GameObject> group1 = new List<GameObject>();
    public List<GameObject> group2 = new List<GameObject>();
    public List<GameObject> group3 = new List<GameObject>();

    private List<List<GameObject>> outlineGroups = new List<List<GameObject>>();
    private List<GameObject[]> outlineObjectsGroups = new List<GameObject[]>();

    public Material outlineMaterial;
    private Coroutine highlightSequenceCoroutine;

    private InputAction toggleGroup1Action;
    private InputAction toggleGroup2Action;
    private InputAction toggleGroup3Action;
    private InputAction startSequenceAction;
    private InputAction resetHighlightsAction;

    void Awake()
    {
        toggleGroup1Action = new InputAction(binding: "<Keyboard>/1");
        toggleGroup2Action = new InputAction(binding: "<Keyboard>/2");
        toggleGroup3Action = new InputAction(binding: "<Keyboard>/3");
        startSequenceAction = new InputAction(binding: "<Keyboard>/enter");
        resetHighlightsAction = new InputAction(binding: "<Keyboard>/q");

        toggleGroup1Action.performed += ctx => ToggleGroupHighlight(0);
        toggleGroup2Action.performed += ctx => ToggleGroupHighlight(1);
        toggleGroup3Action.performed += ctx => ToggleGroupHighlight(2);
        startSequenceAction.performed += ctx => StartHighlightSequence();
        resetHighlightsAction.performed += ctx => ResetHighlights();
        startSequenceAction.performed += ctx => StartHighlightSequence();

        toggleGroup1Action.Enable();
        toggleGroup2Action.Enable();
        toggleGroup3Action.Enable();
        startSequenceAction.Enable();
        resetHighlightsAction.Enable();
    }

    void Start()
    {
        outlineGroups.Add(group1);
        outlineGroups.Add(group2);
        outlineGroups.Add(group3);

        foreach (var group in outlineGroups)
        {
            GameObject[] outlineGroup = new GameObject[group.Count];
            for (int i = 0; i < group.Count; i++)
            {
                if (group[i] != null)
                {
                    outlineGroup[i] = CreateOutline(group[i]);
                }
            }
            outlineObjectsGroups.Add(outlineGroup);
        }
    }

    GameObject CreateOutline(GameObject original)
{
    GameObject outlineObject = Instantiate(original, original.transform.position, original.transform.rotation, original.transform.parent);
    outlineObject.name = original.name + "_Outline";

    MeshRenderer meshRenderer = outlineObject.GetComponent<MeshRenderer>();
    if (meshRenderer != null)
    {
        meshRenderer.material = outlineMaterial;
    }

    // Adjust this value to change the relative thickness of the outline
    float outlineThickness = 0.05f; // Example thickness

    // Calculate the new scale considering the original object's scale
    Vector3 originalScale = original.transform.localScale;
    Vector3 outlineScale = new Vector3(
        originalScale.x * (1 + outlineThickness),
        originalScale.y * (1 + outlineThickness),
        originalScale.z * (1 + outlineThickness));

    outlineObject.transform.localScale = outlineScale;
    outlineObject.SetActive(false);

    return outlineObject;
}


    IEnumerator HighlightSequence()
    {
        foreach (var group in outlineObjectsGroups)
        {
            SetGroupActive(group, true);
            yield return new WaitForSeconds(30);
            SetGroupActive(group, false);
            yield return new WaitForSeconds(3);
        }
    }

    void ToggleGroupHighlight(int groupIndex)
    {
        if (groupIndex >= 0 && groupIndex < outlineObjectsGroups.Count)
        {
            bool isActive = outlineObjectsGroups[groupIndex][0].activeSelf;
            SetGroupActive(outlineObjectsGroups[groupIndex], !isActive);
        }
    }

    void SetGroupActive(GameObject[] group, bool state)
    {
        foreach (var obj in group)
        {
            if (obj != null) obj.SetActive(state);
        }
    }

    void ResetHighlights()
    {
        foreach (var group in outlineObjectsGroups)
        {
            SetGroupActive(group, false);
        }
    }

    void OnDestroy()
    {
        toggleGroup1Action.Dispose();
        toggleGroup2Action.Dispose();
        toggleGroup3Action.Dispose();
        startSequenceAction.Dispose();
        resetHighlightsAction.Dispose();
    }
    private void StartHighlightSequence()
    {
        if (highlightSequenceCoroutine != null)
        {
            StopCoroutine(highlightSequenceCoroutine);
        }
        highlightSequenceCoroutine = StartCoroutine(HighlightSequence());
    }

}
