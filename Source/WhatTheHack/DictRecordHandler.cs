using GiddyUpCore.Utilities;
using HugsLib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GiddyUpCore
{
    public class DictRecordHandler : SettingHandleConvertible
    {
        public Dictionary<String, Record> inner = new Dictionary<String, Record>();
        public Dictionary<String, Record> InnerList { get { return inner; } set { inner = value; } }

        public override void FromString(string settingValue)
        {
            inner = new Dictionary<String, Record>();
            if (!settingValue.Equals(string.Empty))
            {
                foreach (string str in settingValue.Split('|'))
                {
                    string[] split = str.Split(',');
                    if(split.Count() < 4) //ensures that it works for users that still have old AnimalRecords saved. 
                    {
                        inner.Add(str.Split(',')[0], new Record(Convert.ToBoolean(str.Split(',')[1]), Convert.ToBoolean(str.Split(',')[2]), ""));
                    }
                    else
                    {
                        inner.Add(str.Split(',')[0], new Record(Convert.ToBoolean(str.Split(',')[1]), Convert.ToBoolean(str.Split(',')[2]), str.Split(',')[3]));
                    }
                }
            }
        }

        public override string ToString()
        {
            List<String> strings = new List<string>();
            foreach (KeyValuePair<string, Record> item in inner)
            {
                strings.Add(item.Key +","+item.Value.ToString());
            }

            return inner != null ? String.Join("|", strings.ToArray()) : "";
        }
    }

}
