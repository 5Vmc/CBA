using BigBang;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class ColorAddByDistance : MonoBehaviour
{
    List<Material> _materials = new List<Material>();
    List<Color> _originColors = new List<Color>();

    private Camera MainCamera;
    public CameraID cameraId = CameraID.Challenge;
    public Color TargetColor = new Color(224 / 255f, 241 / 255f, 243 / 255f);
    [Range(0f, 1f)]
    public float TargetPercent = 0.5f;
    public float DistanceIn = 11f;
    public float DistanceOut = 9.2f;

    void Awake()
    {
        //MainCamera = CameraManager.Instance.GetCamera(cameraId);
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr)
        {
            foreach (Material mat in mr.materials)
            {
                _materials.Add(mat);
                _originColors.Add(mat.color);
            }
        }
    }

    void Update()
    {
        UpdateColor();
    }

    void UpdateColor()
    {
        if (MainCamera == null)
            MainCamera = CameraManager.Instance.GetCamera(cameraId);
        Vector3 pos1 = new Vector3(transform.position.x, transform.position.y, 0);
        Vector3 pos2 = new Vector3(MainCamera.transform.position.x, MainCamera.transform.position.y, 0);
        float distance = (pos1 - pos2).magnitude;
        //float distance = Mathf.Abs(transform.position.z - MainCamera.transform.position.z);
        //Debug.Log("Distance: " + distance + "---" + transform.parent.name);
        if (distance < DistanceOut)
        {
            for (int i = 0; i < _materials.Count; ++i)
            {
                _materials[i].color = _originColors[i];
            }
        }
        else if (distance > DistanceIn)
        {
            for (int i = 0; i < _materials.Count; ++i)
            {
                _materials[i].color = _originColors[i] * (1 - TargetPercent) + TargetPercent * TargetColor;
            }
        }
        else
        {
            float percent = (distance - DistanceOut) / (DistanceIn - DistanceOut);
            percent = 1 - percent * TargetPercent;
            for (int i = 0; i < _materials.Count; ++i)
            {
                _materials[i].color = _originColors[i] * percent + (1 - percent) * TargetColor;
            }
        }
    }
}
