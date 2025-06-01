using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Protocol;
using System.Globalization;
using Json;
using Json.Serialization;

public class Localizer
{
    private Dictionary<string, LocalizedString> cache = new Dictionary<string, LocalizedString>();
    private static Localizer instance = null;
    
    public static Locale Locale { get; set; }
    public static Localizer Instance {
        get {
            if (instance == null)
                instance = new Localizer();

            return instance;
        }
    }

    public void Load(string resourceName)
    {
        cache.Clear();

        var assembly = Assembly.GetExecutingAssembly();
        var strs = assembly.GetManifestResourceNames();

        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        using (StreamReader reader = new StreamReader(stream))
        {
            var cdbDoc = reader.ReadToEnd();
            var cardStrings = CardStringsJsonSerializer.Instance.Deserialize(JsonParser.Parse(cdbDoc));
            cache = cardStrings.Strings;
        }
    }

    public string Get(string tag)
    {
        LocalizedString result;

        if (cache.TryGetValue(tag, out result))
        {
            return result.ToString();
        }
        else
        {
            return tag;
        }
    }

    public string Format(string tag, params object[] args)
    {
        return string.Format(Get(tag), args);
    }
}
