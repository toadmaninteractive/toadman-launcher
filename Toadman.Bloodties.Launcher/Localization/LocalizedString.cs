namespace Protocol
{
    public partial class LocalizedString
    {
        public override string ToString()
        {
            if (Localizer.Locale == Locale.En)
                return Text;

            LocalizedEntry res;

            if (Data.TryGetValue(Localizer.Locale, out res))
            {
                if (string.IsNullOrEmpty(res.Translation))
                    return Text;

                return res.Translation;
            }

            return Text;
        }

        public static implicit operator string(LocalizedString str)
        {
            return str.ToString();
        }
    }
}
