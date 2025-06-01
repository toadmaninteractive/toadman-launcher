using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Json;

// Ported to C# by Maxim Pushkar from
// https://github.com/Autodesk/sjson/blob/master/index.js

namespace SJson
{
    public class SJsonParseException : Exception
    {
        public SJsonParseException() { }

        public SJsonParseException(string message)
            : base(message) { }

        public SJsonParseException(string message, Exception inner)
            : base(message, inner) { }

        public SJsonParseException(string token, string expected)
            : base($"Unexpected token '${token}', expected '${expected}'.") { }

        public SJsonParseException(string token, string expected, int line, string near)
            : base($"Unexpected token '${token}', expected '${expected}' on line ${line} near '${near}'.") { }
    }

    public class SJsonStringifyException : Exception
    {
        public SJsonStringifyException() { }

        public SJsonStringifyException(string message)
            : base(message) { }

        public SJsonStringifyException(string message, Exception inner)
            : base(message, inner) { }
    }

    public class SJsonCharacterMask
    {
        private int[] mask = { 0, 0, 0, 0 };

        public SJsonCharacterMask(string str)
        {
            if (!string.IsNullOrEmpty(str))
                foreach (char c in str)
                    mask[(c / 32) & 3] |= (1 << (c % 32));
        }

        public bool HasChar(char c)
        {
            return (mask[(c / 32) & 3] & (1 << (c % 32))) != 0;
        }
    }

    public class SJson
    {
        private readonly SJsonCharacterMask numberMask = new SJsonCharacterMask("-+0123456789");
        private readonly SJsonCharacterMask numberExpMask = new SJsonCharacterMask(".eE");
        private readonly SJsonCharacterMask idTermMask = new SJsonCharacterMask(" \t\n=:");
        private readonly SJsonCharacterMask whitespaceMask = new SJsonCharacterMask(" \n\r\t,");

        private byte[] buffer;
        private int index = 0;
        private ImmutableJson input;
        private int tabCount = 0;

        private void InitializeParser(string sjson)
        {
            buffer = Encoding.UTF8.GetBytes(sjson);
            index = 0;
        }

        private void SkipWhitespaces()
        {
            while (index < buffer.Length)
            {
                if (buffer[index] == 47) // "/"
                {
                    ++index;

                    if (buffer[index] == 47)
                        while (buffer[++index] != 10) { } // "\n"
                    else if (buffer[index] == 42) // "*"
                        while (buffer[++index] != 42) { }
                }
                else if (!whitespaceMask.HasChar(Convert.ToChar(buffer[index])))
                {
                    break;
                }

                ++index;
            }
        }

        private void ConsumeChar(byte code)
        {
            SkipWhitespaces();

            if (buffer[index++] != code)
                throw new SJsonParseException(buffer[index].ToString(), code.ToString()); // FIXME
        }

        private void ConsumeKeyword(string keyword)
        {
            SkipWhitespaces();

            var chars = Encoding.UTF8.GetBytes(keyword);

            foreach (char c in chars)
            {
                if (buffer[index++] != c)
                    throw new SJsonParseException(buffer[index - 1].ToString(), c.ToString()); // FIXME
            }
        }

        private ImmutableJson ParseValue()
        {
            SkipWhitespaces();

            var c = Convert.ToChar(buffer[index]);

            if (numberMask.HasChar(c))
                return ParseNumber();

            if (c == 123) // "{"
                return ParseObject();

            if (c == 91) // "["
                return ParseArray();

            if (c == 34) // "
                return ParseString();  

            if (c == 116) {
                ConsumeKeyword("true");
                return ImmutableJson.True;
            }

            if (c == 102) {
                ConsumeKeyword("false");
                return ImmutableJson.False;
            }

            if (c == 110) {
                ConsumeKeyword("null");
                return ImmutableJson.Null;
            }

            throw new SJsonParseException("Expects number, {, [, \", true, false or null."); // FIXME
        }

        private ImmutableJson ParseNumber()
        {
            SkipWhitespaces();

            var start = index;
            var isFloat = false;

            for (; index < buffer.Length; ++index)
            {
                var c = Convert.ToChar(buffer[index]);
                var expc = numberExpMask.HasChar(c);
                isFloat |= expc;

                if (!expc && !numberMask.HasChar(c))
                    break;
            }

            var num = Encoding.UTF8.GetString(buffer, start, index - start);

            if (isFloat)
                return ImmutableJson.Create(double.Parse(num, CultureInfo.InvariantCulture));

            try
            {
                long value = long.Parse(num);
                return (value >= int.MinValue && value <= int.MaxValue)
                    ? ImmutableJson.Create((int)value)
                    : ImmutableJson.Create(value);
            }
            catch (Exception)
            {
                return ImmutableJson.Create(double.Parse(num, CultureInfo.InvariantCulture));
            }
        }

        private ImmutableJson ParseString()
        {
            // Literal string
            int start;

            if (buffer[index] == 34 && buffer[index + 1] == 34 && buffer[index + 2] == 34)
            {
                index += 3;
                start = index;
                for (; buffer[index] != 34 || buffer[index + 1] != 34 || buffer[index + 2] != 34; ++index) ;
                index += 3;

                return ImmutableJson.Create(Encoding.UTF8.GetString(buffer, start, index - start - 3));
            }

            start = index;
            var escape = false;
            ConsumeChar(34);

            for (; buffer[index] != 34; ++index)
            { 
                if (buffer[index] == 92) // unescaped "
                {
                    ++index;
                    escape = true;
                }
            }

            ConsumeChar(34);

            if (!escape)
                return ImmutableJson.Create(Encoding.UTF8.GetString(buffer, start + 1, index - start - 2));

            index = start;

            var octets = new List<byte>();
            ConsumeChar(34);

            for (; buffer[index] != 34; ++index)
            {
                if (buffer[index] == 92) // unescaped "
                {
                    ++index;
                    if (buffer[index] == 98) octets.Add(8); // \b
                    else if (buffer[index] == 102) octets.Add(12); // \f
                    else if (buffer[index] == 110) octets.Add(10); // \n
                    else if (buffer[index] == 114) octets.Add(13); // \r
                    else if (buffer[index] == 116) octets.Add(9); // \t
                    else if (buffer[index] == 117)
                    { // \u
                        ++index;
                        octets.Add((byte)(16 * (buffer[index] - 48) + buffer[index + 1] - 48));
                        index += 2;
                        octets.Add((byte)(16 * (buffer[index] - 48) + buffer[index + 1] - 48));
                        index += 2;
                    }
                    else octets.Add(buffer[index]); // \" \\ \/
                }
                else
                    octets.Add(buffer[index]);
            }

            ConsumeChar(34);

            return ImmutableJson.Create(Encoding.UTF8.GetString(octets.ToArray()));
        }

        private ImmutableJson ParseArray()
        {
            var ar = new JsonArray();

            SkipWhitespaces();
            ConsumeChar(91); // "["
            SkipWhitespaces();

            for (; buffer[index] != 93; SkipWhitespaces()) // "]"
                ar.Add(ParseValue());

            ConsumeChar(93);

            return ImmutableJson.Create(ar);
        }

        private string ParseIdentifier()
        {
            SkipWhitespaces();

            if (index >= buffer.Length)  // Catch whitespace EOF
                return null;

            if (buffer[index] == 34)
                return ParseString().AsString;

            var start = index;
            for (; !idTermMask.HasChar(Convert.ToChar(buffer[index])); ++index) ;

            return Encoding.UTF8.GetString(buffer, start, index - start);
        }

        private ImmutableJson ParseObject()
        {
            var obj = new JsonObject();
            ConsumeChar(123); // "{"
            SkipWhitespaces();

            for (; buffer[index] != 125; SkipWhitespaces())
            { // "}"
                var key = ParseIdentifier();
                SkipWhitespaces();

                if (buffer[index] == 58)
                    ConsumeChar(58);
                else
                    ConsumeChar(61); // ":" or "="

                obj[key] = ParseValue();
            }

            ConsumeChar(125); // "}"

            return obj;
        }

        public ImmutableJson ParseRoot()
        {
            SkipWhitespaces();

            if (buffer[index] == 123)
                return ParseObject();

            var obj = new JsonObject();

            while (index < buffer.Length)
            { // "}"
                var key = ParseIdentifier();
                SkipWhitespaces();

                if (index == buffer.Length) // Catch whitespace EOF
                    break;

                if (buffer[index] == 58)
                    ConsumeChar(58);
                else
                    ConsumeChar(61); // ":" or "="

                obj[key] = ParseValue();
            }

            SkipWhitespaces();

            if (index != buffer.Length)
                throw new SJsonParseException("end-of-string");

            return obj;
        }

        private void InitializeStringifier(ImmutableJson obj)
        {
            tabCount = 0;
            input = obj;
        }

        private string EndLine()
        {
            var result = "\n";

            for (var i = 0; i < tabCount; i++)
                result += "\t";

            return result;
        }

        private string PackString(string value)
        {
            var regex = new Regex("\r|\n");

            if (regex.Matches(value).Count > 0)
            {
                return $"\"\"\"{value}\"\"\"";
            }
            else
            {
                var result  = "";

                foreach (char symbol in value)
                {
                    switch (symbol)
                    {
                        case '\\':
                            result += "\\\\";
                            break;
                        default:
                            result += symbol;
                            break;
                    }
                }

                return $"\"{result}\"";
            }
        }

        private string PackArray(ImmutableJsonArray value)
        {
            var result = "[";
            tabCount++; // indentation

            foreach (var element in value)
                result += EndLine() + PackValue(element);

            tabCount--; // end indentation

            return result + EndLine() + "]";
        }

        private string GetObjectKey(string key)
        {
            var regex = new Regex(" |=|/");
            return (regex.Matches(key).Count > 0 || key == string.Empty) ? PackString(key) : key;
        }

        private string PackObject(ImmutableJsonObject value)
        {
            var result = "{";
            tabCount++; // indentation

            foreach (var key in value.AsObject.Keys)
                result += EndLine() + GetObjectKey(key) + " = " + PackValue(value.AsObject[key]);

            tabCount--; // end indentation

            return result + EndLine() + "}";
        }

        private string PackValue(ImmutableJson value)
        {
            if (value.IsObject)
                return PackObject(value.AsObject);
            else if (value.IsArray)
                return PackArray(value.AsArray);
            else if (value.IsString)
                return PackString(value.AsString);
            else if (value.IsBool)
                return value.AsBool ? "true" : "false";
            else if (value.IsInt)
                return value.AsInt.ToString();
            else if (value.IsLong)
                return value.AsLong.ToString();
            else if (value.IsNumber)
                return value.AsNumber.ToString(CultureInfo.InvariantCulture);
            else if (value.IsNull)
                return "null";

            throw new SJsonStringifyException("Can not determine value type");
        }

        public string PackRoot()
        {
            // If the root is an object loop through key here to not add '{ }' and indentation
            var result = "";

            if (input.IsObject)
            {
                foreach (var key in input.AsObject.Keys)
                    result += GetObjectKey(key) + " = " + PackValue(input.AsObject[key]) + EndLine();
            }
            else
            {
                result = PackValue(input);
            }

            return result;            
        }

        public static ImmutableJson Parse(string sjson)
        {
            var parser = new SJson();
            parser.InitializeParser(sjson);
            return parser.ParseRoot();
        }

        public static string Stringify(ImmutableJson obj)
        {
            var packer = new SJson();
            packer.InitializeStringifier(obj);
            return packer.PackRoot();
        }
    }
}
