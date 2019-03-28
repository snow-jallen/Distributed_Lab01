using System;

namespace Shared
{
    public class MessageDto
    {
        public MessageDto(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public string Value { get; }
        public string Result { get; set; }
    }
}
