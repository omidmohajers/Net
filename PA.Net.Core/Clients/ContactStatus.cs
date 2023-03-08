using System.Text;

namespace PA.Net.Core
{
    public class ContactStatus
    {
        public StatusType Type{get;set;}
        public string Description { get; set; }
        public override string ToString()
        {
            string result = ((byte)Type).ToString();
            result += ":";
            result += Description;
            return result;
        }

        public static ContactStatus Parse(string val)
        {
            if (string.IsNullOrWhiteSpace(val))
                return new ContactStatus();
            else
            {
                ContactStatus cs = new ContactStatus();
                string[] t = val.Split(':');
                cs.Type = (StatusType)(int.Parse(t[0]));
                cs.Description = t[1];
                return cs;
            }
        }

        public byte[] ToByte()
        {
            return Encoding.UTF8.GetBytes(this.ToString());
        }

        public static ContactStatus FromBytes(byte[] data)
        {
            return ContactStatus.Parse(Encoding.UTF8.GetString(data));
        }
    }
}