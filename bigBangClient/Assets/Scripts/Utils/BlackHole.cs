using deVoid.UIFramework;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class BlackHole : MonoBehaviour
{
    private Image img;

    private Image Img
    {
        get
        {
            if (img == null)
            {
                img = GetComponent<Image>();
                // 克隆材质
                img.material = Instantiate(img.material);
            }
            return img;
        }
    }

    public float Radius
    {
        get => Material.GetFloat("_Radius");
        set => Material.SetFloat("_Radius", value);
    }

    public Material Material
    {
        get => Img.material;
    }

    public float Alpha
    {
        get => Img.color.a;
        set => Img.SetAlpha(value);
    }

    /// <summary>
    /// 定位，用于跟随UI摄像机拍摄的物体
    /// </summary>
    /// <param name="target">UI物体</param>
    public void Locate(Transform target)
    {
        Material.SetVector("_Center", (UIController.Instance.GetCamera().WorldToScreenPoint(target.position)
            - new Vector3(Screen.width / 2f, Screen.height / 2f)) / UIController.Instance.Canvas.scaleFactor);
    }

    /// <summary>
    /// 定位，用于跟随3D摄像机拍摄的物体
    /// </summary>
    /// <param name="camera">3D摄像机（非UI摄像机）</param>
    /// <param name="target">3D物体</param>
    public void Locate(Camera camera, Transform target)
    {
        Material.SetVector("_Center", (UIFrame.Change3DScreenPointToUIScreenPoint(camera.WorldToScreenPoint(target.position))
            - new Vector3(Screen.width / 2f, Screen.height / 2f)) / UIController.Instance.Canvas.scaleFactor);
    }
}
