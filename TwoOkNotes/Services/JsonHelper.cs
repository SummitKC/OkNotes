using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace TwoOkNotes.Services
{
    public class JsonHelper
    {
        public static bool IsJsonEmpty(string json)
        {
            if (json == null)
            {
                return true;
            }
            else if (json == "")
            {
                return true;
            }
            else if (json == "{}")
            {
                return true;
            }
            return false;
        }

        public static bool isAttributeEmtpy(string attribute)
        {
            if (attribute == null)
            {
                return true;
            }
            else if (attribute == "")
            {
                return true;
            }
            return false;
        }
    }
}
