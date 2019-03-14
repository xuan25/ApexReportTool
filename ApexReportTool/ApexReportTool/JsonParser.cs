using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;

namespace Json
{
    /// <summary>
    /// Class <c>JsonParser</c> provides parsing methods for converting json strings to C# objects.
    /// Author: Xuan525
    /// Date: 21/02/2019
    /// </summary>
    class JsonParser
    {
        private static object ToValue(string str)
        {
            if (str.Trim().ToLower() == "null")
                return null;
            if (str.Trim().ToLower() == "true" || str.Trim().ToLower() == "false")
            {
                bool.TryParse(str, out bool result);
                return result;
            }
            else if (str.Contains("."))
            {
                double.TryParse(str, out double result);
                return result;
            }
            else
            {
                long.TryParse(str, out long result);
                return result;
            }
        }

        private static object ParseValue(StringReader stringReader)
        {
            while (stringReader.Peek() == ' ' || stringReader.Peek() == '\r' || stringReader.Peek() == '\n')
                stringReader.Read();
            if (stringReader.Peek() == '\"')
            {
                stringReader.Read();
                StringBuilder stringBuilder = new StringBuilder();
                while (stringReader.Peek() != -1)
                {
                    if (stringReader.Peek() == '\\')
                    {
                        stringBuilder.Append((char)stringReader.Read());
                        stringBuilder.Append((char)stringReader.Read());
                    }
                    else if (stringReader.Peek() == '\"')
                    {
                        string value = stringBuilder.ToString();
                        while (stringReader.Peek() != ',' && stringReader.Peek() != '}' && stringReader.Peek() != ']')
                            stringReader.Read();
                        if (stringReader.Peek() == ',')
                            stringReader.Read();
                        return value;
                    }
                    else
                        stringBuilder.Append((char)stringReader.Read());
                }
                return stringBuilder.ToString();
            }
            else if (stringReader.Peek() == '{')
            {
                JsonObject jsonObject = ParseObject(stringReader);
                while (stringReader.Peek() != -1 && stringReader.Read() != ',') ;
                return jsonObject;
            }
            else if (stringReader.Peek() == '[')
            {
                JsonArray jsonArray = ParseArray(stringReader);
                while (stringReader.Peek() != -1 && stringReader.Read() != ',') ;
                return jsonArray;
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                while (stringReader.Peek() != -1)
                {
                    if (stringReader.Peek() == '\\')
                    {
                        stringBuilder.Append((char)stringReader.Read());
                        stringBuilder.Append((char)stringReader.Read());
                    }
                    else if (stringReader.Peek() == ',')
                    {
                        string value = stringBuilder.ToString();
                        stringReader.Read();
                        return ToValue(value);
                    }
                    else if (stringReader.Peek() == '}' || stringReader.Peek() == ']')
                        return ToValue(stringBuilder.ToString());
                    else
                        stringBuilder.Append((char)stringReader.Read());
                }
                return stringBuilder.ToString();
            }
        }

        private static KeyValuePair<string, object> ParseKeyValuePaire(StringReader stringReader)
        {
            StringBuilder stringBuilder = new StringBuilder();
            while (stringReader.Peek() != -1 && stringReader.Read() != '\"') ;
            while (stringReader.Peek() > -1)
            {
                if (stringReader.Peek() == '\\')
                {
                    stringBuilder.Append((char)stringReader.Read());
                    stringBuilder.Append((char)stringReader.Read());
                }
                else if (stringReader.Peek() == '\"')
                {
                    stringReader.Read();
                    while (stringReader.Peek() != -1 && stringReader.Read() != ':') ;
                    string key = stringBuilder.ToString();
                    object value = ParseValue(stringReader);
                    return new KeyValuePair<string, object>(key, value);
                }
                else
                {
                    stringBuilder.Append((char)stringReader.Read());
                }
            }
            return new KeyValuePair<string, object>("UNKNOW", null);
        }

        private static JsonObject ParseObject(StringReader stringReader)
        {
            stringReader.Read();
            JsonObject jsonObject = new JsonObject();
            while (stringReader.Peek() > -1)
            {
                if (stringReader.Peek() == '{')
                    ParseObject(stringReader);
                else if (stringReader.Peek() == '[')
                    ParseArray(stringReader);
                else if (stringReader.Peek() == '\"')
                {
                    KeyValuePair<string, object> keyValuePair = ParseKeyValuePaire(stringReader);
                    jsonObject.Add(keyValuePair.Key, keyValuePair.Value);
                }
                else if (stringReader.Peek() == '}')
                {
                    stringReader.Read();
                    return jsonObject;
                }
                else
                    stringReader.Read();
            }
            return jsonObject;
        }

        private static JsonArray ParseArray(StringReader stringReader)
        {
            stringReader.Read();
            JsonArray jsonArray = new JsonArray();
            while (stringReader.Peek() > -1)
            {
                if (stringReader.Peek() == ']')
                {
                    stringReader.Read();
                    return jsonArray;
                }
                else
                    jsonArray.Add(ParseValue(stringReader));
            }
            return jsonArray;
        }

        /// <summary>
        /// Parse a json string to an object
        /// <example>For example:
        /// <code>
        ///    dynamic json = JsonParser.Parse(jsonStr);
        ///    string keyData = json.data;
        ///    string keyData1 = json["data"];
        ///    string arrayItem = json.arrayExample[0];
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="json">Json string</param>
        /// <returns>Json object</returns>
        public static object Parse(string json)
        {
            StringReader stringReader = new StringReader(json.Trim());
            if (stringReader.Peek() == -1)
                return null;
            else if (stringReader.Peek() == '{')
                return ParseObject(stringReader);
            else if (stringReader.Peek() == '[')
                return ParseArray(stringReader);
            else
                return null;
        }
    }

    /// <summary>
    /// Class <c>JsonObject</c> models an Object in json.
    /// Author: Xuan525
    /// Date: 21/02/2019
    /// </summary>
    public class JsonObject : DynamicObject
    {
        private Dictionary<string, object> dictionary = new Dictionary<string, object>();

        /// <summary>
        /// The number of items in the Object
        /// </summary>
        public int Count
        {
            get
            {
                return dictionary.Count;
            }
        }

        /// <summary>
        /// Add a key-value pair to the Object
        /// </summary>
        /// <param name="key">The Key of the key-value pair</param>
        /// <param name="value">The Value of the key-value pair</param>
        public void Add(string key, object value)
        {
            dictionary.Add(key.ToLower(), value);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name.ToLower();
            return dictionary.TryGetValue(name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            dictionary[binder.Name.ToLower()] = value;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (dictionary.ContainsKey((string)indexes[0]))
                result = dictionary[(string)indexes[0]];
            else
                throw new System.NullReferenceException();
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (dictionary.ContainsKey((string)indexes[0]))
                dictionary[(string)indexes[0]] = value;
            else
                dictionary.Add((string)indexes[0], value);
            return true;
        }

    }

    /// <summary>
    /// Class <c>JsonArray</c> models an Array in json.
    /// Author: Xuan525
    /// Date: 21/02/2019
    /// </summary>
    public class JsonArray : DynamicObject
    {
        private List<object> list = new List<object>();

        /// <summary>
        /// The number of items in the Array
        /// </summary>
        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        /// <summary>
        /// Add a value to the Array
        /// </summary>
        /// <param name="value">The Value</param>
        public void Add(object value)
        {
            list.Add(value);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (list.Count > (int)indexes[0])
                result = list[(int)indexes[0]];
            else
                throw new System.NullReferenceException();
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (list.Count > (int)indexes[0])
                list[(int)indexes[0]] = value;
            else
            {
                while (list.Count < (int)indexes[0])
                    list.Add(null);
                list.Add(value);
            }
            return true;
        }

    }
}
