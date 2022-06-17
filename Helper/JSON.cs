namespace Helper
{
    using System.Text.Json;

    public class JSON
    {
        public string JSONstring()
        {
            var x = new SomeDataObject() { SomeProperty1 = "some value", SomeProperty2 = 2 };
            return JsonSerializer.Serialize(x);
        }

        public SomeDataObject DeserializeFromJSON(string json)
        {
            return JsonSerializer.Deserialize<SomeDataObject>(json);
        }
    }
}