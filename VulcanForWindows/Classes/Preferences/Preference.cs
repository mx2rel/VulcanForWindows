namespace VulcanForWindows.Preferences
{
    public class Preference
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public Preference(string _category, string _name, string _value)
        {
            Category = _category;
            Name = _name;
            Value = _value;
        }
    }
}
