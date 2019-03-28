using System;
using System.Collections.Generic;

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
    }

    public class Job
    {
        public Job(IEnumerable<MessageDto> messages)
        {
            Messages = messages;
        }

        public string Result { get; set; }
        public IEnumerable<MessageDto> Messages { get; }
    }
}
