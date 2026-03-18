namespace Utils
{
    public class ColorString
    {
        public static string GetColorString(string color, string content)
        {
            return $"<color={color}>{content}</color>";
        }
    }
}