using System.Xml;

public class Settings
{
    private XmlDocument settings;

    public Settings(string file)
    {
        settings = new XmlDocument();
        settings.Load(file);
    }

    public bool GetBoolean(string setting)
    {
        if (settings.SelectSingleNode("Settings/" + setting) == null)
            return false;
        string value = settings.SelectSingleNode("Settings/" + setting).InnerText;
        return value == "true" || value == "1" ? true : false;
    }

    public int GetInt(string setting)
    {
        if (settings.SelectSingleNode("Settings/" + setting) == null)
            return 0;
        string value = settings.SelectSingleNode("Settings/" + setting).InnerText;
        return int.Parse(value);
    }

    public string GetString(string setting)
    {
        if (settings.SelectSingleNode("Settings/" + setting) == null)
            return "";
        return settings.SelectSingleNode("Settings/" + setting).InnerText;
    }
}
