using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Sync
{
    static class Extensions
    {
        public static string ToJson(this object value, Formatting formatting = Formatting.None)
        {
            if (value == null) return null;
            try
            {
                return JsonConvert.SerializeObject(value, formatting);
            }
            catch
            {
                //log exception but dont throw one
            }

            return null;
        }


    }
}
